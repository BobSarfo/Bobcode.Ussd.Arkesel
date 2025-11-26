using System.Text;
using Bobcode.Ussd.Arkesel.Actions;
using Bobcode.Ussd.Arkesel.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bobcode.Ussd.Arkesel.Core
{
    /// <summary>
    /// Main USSD application handler that processes incoming USSD requests
    /// </summary>
    public class UssdApp
    {
        private readonly IUssdSessionStore _sessionStore;
        private readonly ILogger<UssdApp> _logger;
        private readonly Menu _menu;
        private readonly IServiceProvider _serviceProvider;
        private readonly UssdOptions _options;

        public UssdApp(
            IUssdSessionStore sessionStore,
            ILogger<UssdApp> logger,
            Menu menu,
            IServiceProvider serviceProvider,
            UssdOptions? options = null)
        {
            _sessionStore = sessionStore;
            _logger = logger;
            _menu = menu;
            _serviceProvider = serviceProvider;
            _options = options ?? new UssdOptions();
        }

        /// <summary>
        /// Handles an incoming USSD request and returns the appropriate response
        /// </summary>
        public async Task<UssdResponseDto> HandleRequestAsync(UssdRequestDto request)
        {
            var existingSession = await _sessionStore.GetAsync(request.SessionID);

            UssdStepResult response;

            // Handle session resumption logic
            if (request.NewSession && existingSession != null && _options.EnableSessionResumption)
            {
                // Check if session is not expired and not at home
                if (existingSession.ExpireAt > DateTime.UtcNow && existingSession.CurrentStep != _menu.RootId)
                {
                    // Store the current state and ask user to resume or start fresh
                    existingSession.AwaitingResumeChoice = true;
                    existingSession.PreviousStep = existingSession.CurrentStep;

                    // Build the resume prompt message
                    var resumeMessage = $"{_options.ResumeSessionPrompt}\n1. {_options.ResumeOptionLabel}\n2. {_options.StartFreshOptionLabel}";

                    response = new UssdStepResult
                    {
                        Message = resumeMessage,
                        ContinueSession = true
                    };

                    await _sessionStore.SetAsync(existingSession, _options.SessionTimeout);

                    return new UssdResponseDto
                    {
                        SessionID = request.SessionID,
                        UserID = request.UserID,
                        ContinueSession = response.ContinueSession,
                        Msisdn = request.Msisdn,
                        Message = response.Message
                    };
                }
            }

            var session = existingSession ?? new UssdSession(
                request.SessionID,
                request.Msisdn,
                request.UserID,
                request.Network,
                _menu.RootId  // Pass the correct root ID from the menu
            );

            if (request.NewSession)
            {
                session.CurrentStep = _menu.RootId;
                session.AwaitingResumeChoice = false;
                session.PreviousStep = null;
                response = RenderMenu(session);
            }
            else
            {
                response = await ProcessRequest(session, request.UserData ?? "");
            }

            // Handle GoHome navigation
            if (response.GoHome)
            {
                session.CurrentStep = _menu.RootId;
                session.Level = 1;
                response = RenderMenu(session);
            }

            // Handle NextStep navigation
            if (!string.IsNullOrEmpty(response.NextStep))
            {
                session.CurrentStep = response.NextStep;
                response = RenderMenu(session);
            }

            await _sessionStore.SetAsync(session, _options.SessionTimeout);

            return new UssdResponseDto
            {
                SessionID = request.SessionID,
                UserID = request.UserID,
                ContinueSession = response.ContinueSession,
                Msisdn = request.Msisdn,
                Message = response.Message
            };
        }

        private async Task<UssdStepResult> ProcessRequest(UssdSession session, string input)
        {
            // Handle resume choice if awaiting
            if (session.AwaitingResumeChoice)
            {
                if (input == "1") // Resume
                {
                    session.AwaitingResumeChoice = false;
                    if (!string.IsNullOrEmpty(session.PreviousStep))
                    {
                        session.CurrentStep = session.PreviousStep;
                        session.PreviousStep = null;
                    }
                    return RenderMenu(session, "Resuming your session...\n");
                }
                else if (input == "2") // Start Fresh
                {
                    session.AwaitingResumeChoice = false;
                    session.PreviousStep = null;
                    session.CurrentStep = _menu.RootId;
                    session.Level = 1;
                    session.Data.Clear(); // Clear all session data
                    return RenderMenu(session);
                }
                else
                {
                    // Invalid choice, show prompt again
                    var resumeMessage = $"{_options.ResumeSessionPrompt}\n1. {_options.ResumeOptionLabel}\n2. {_options.StartFreshOptionLabel}";
                    return new UssdStepResult
                    {
                        Message = $"{_options.InvalidInputMessage}\n{resumeMessage}",
                        ContinueSession = true
                    };
                }
            }

            var currentPage = _menu.GetPage(session.CurrentStep);

            // Handle back navigation
            if (input == _options.BackCommand && session.Level > 1)
            {
                return GoBack(session);
            }

            // Handle home navigation
            if (input == _options.HomeCommand)
            {
                return UssdStepResult.Home();
            }

            // First try exact match (non-wildcard options)
            var option = currentPage.Options.FirstOrDefault(o => !o.IsWildcard && o.Input == input);

            // If no exact match, try wildcard option (accepts any input)
            if (option == null)
            {
                option = currentPage.Options.FirstOrDefault(o => o.IsWildcard);
            }

            if (option == null)
            {
                return RenderMenu(session, _options.InvalidInputMessage);
            }

            // Execute action handler if specified
            if (!string.IsNullOrEmpty(option.ActionKey))
            {
                var handler = _serviceProvider.GetServices<IActionHandler>()
                    .FirstOrDefault(h => h.Key == option.ActionKey);

                if (handler != null)
                {
                    var context = new UssdContext(
                        new UssdRequestDto
                        {
                            SessionID = session.SessionId,
                            UserID = session.UserId,
                            NewSession = false,
                            Msisdn = session.Msisdn,
                            UserData = input,
                            Network = session.Network
                        },
                        session
                    )
                    {
                        ContextActionKey = option.ActionKey
                    };

                    var result = await handler.HandleAsync(context);
                    return result;
                }

                _logger.LogWarning("No action handler found for key {ActionKey}", option.ActionKey);
            }

            // Navigate to target step if specified
            if (!string.IsNullOrEmpty(option.TargetStep))
            {
                session.CurrentStep = option.TargetStep;
                return RenderMenu(session);
            }

            // Terminal option with no action or target
            return new UssdStepResult
            {
                Message = _options.DefaultEndMessage,
                ContinueSession = false
            };
        }

        private UssdStepResult GoBack(UssdSession session)
        {
            // Simple back navigation - in a real implementation, you'd maintain a navigation stack
            session.Level--;
            if (session.Level <= 1)
            {
                session.CurrentStep = _menu.RootId;
                session.Level = 1;
            }
            return RenderMenu(session, "Going back...");
        }

        private UssdStepResult RenderMenu(UssdSession session, string? prefix = null)
        {
            var page = _menu.GetPage(session.CurrentStep);
            var sb = new StringBuilder();

            if (prefix != null)
            {
                sb.AppendLine(prefix);
            }

            sb.AppendLine(page.Title);

            // Check if pagination is needed
            var options = page.Options.ToList();
            if (_options.EnablePagination && options.Count > _options.ItemsPerPage)
            {
                var currentPage = session.Part;
                var paginatedOptions = PaginateOptions(options, currentPage, _options.ItemsPerPage);

                foreach (var opt in paginatedOptions)
                {
                    sb.AppendLine($"{opt.Input}. {opt.Label}");
                }

                // Add pagination controls
                var totalPages = (int)Math.Ceiling((double)options.Count / _options.ItemsPerPage);
                if (currentPage < totalPages)
                {
                    sb.AppendLine($"{_options.NextPageCommand}. Next");
                }
                if (currentPage > 1)
                {
                    sb.AppendLine($"{_options.PreviousPageCommand}. Previous");
                }
            }
            else
            {
                foreach (var opt in options)
                {
                    sb.AppendLine($"{opt.Input}. {opt.Label}");
                }
            }

            session.Level++;
            return new UssdStepResult
            {
                Message = sb.ToString().TrimEnd(),
                ContinueSession = !page.IsTerminal
            };
        }

        private IEnumerable<MenuOption> PaginateOptions(List<MenuOption> options, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            return options.Skip(skip).Take(pageSize);
        }
    }
}