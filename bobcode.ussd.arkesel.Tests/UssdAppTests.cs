using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Bobcode.Ussd.Arkesel.Actions;
using Bobcode.Ussd.Arkesel.Core;
using Bobcode.Ussd.Arkesel.Models;

namespace bobcode.ussd.arkesel.Tests;

public class UssdAppTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IUssdSessionStore _sessionStore;
    private readonly ILogger<UssdApp> _logger;
    private readonly Menu _menu;

    public UssdAppTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMemoryCache();
        _serviceProvider = services.BuildServiceProvider();

        _sessionStore = new MemorySessionStore(_serviceProvider.GetRequiredService<IMemoryCache>());
        _logger = _serviceProvider.GetRequiredService<ILogger<UssdApp>>();

        _menu = new MenuBuilder("test-menu")
            .AddPage("main", "Welcome to Test App")
            .AddOption("main", "1", "Option 1", "step1")
            .AddOption("main", "2", "Option 2", "step2")
            .AddPage("step1", "You selected Option 1", isTerminal: true)
            .AddPage("step2", "You selected Option 2")
            .AddOption("step2", "1", "Sub Option 1", actionKey: "test_action")
            .Build();
    }

    [Fact]
    public async Task HandleRequestAsync_NewSession_ReturnsRootMenu()
    {
        // Arrange
        var app = new UssdApp(_sessionStore, _logger, _menu, _serviceProvider);
        var request = new UssdRequestDto
        {
            SessionID = "session1",
            UserID = "user1",
            NewSession = true,
            Msisdn = "233123456789",
            Network = "MTN"
        };

        // Act
        var response = await app.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.ContinueSession);
        Assert.Contains("Welcome to Test App", response.Message);
        Assert.Contains("1. Option 1", response.Message);
        Assert.Contains("2. Option 2", response.Message);
    }

    [Fact]
    public async Task HandleRequestAsync_ValidInput_NavigatesToTargetStep()
    {
        // Arrange
        var app = new UssdApp(_sessionStore, _logger, _menu, _serviceProvider);
        
        // Create new session
        var newSessionRequest = new UssdRequestDto
        {
            SessionID = "session1",
            UserID = "user1",
            NewSession = true,
            Msisdn = "233123456789",
            Network = "MTN"
        };
        await app.HandleRequestAsync(newSessionRequest);

        // Act - Select option 1
        var request = new UssdRequestDto
        {
            SessionID = "session1",
            UserID = "user1",
            NewSession = false,
            Msisdn = "233123456789",
            UserData = "1",
            Network = "MTN"
        };
        var response = await app.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Contains("You selected Option 1", response.Message);
        Assert.False(response.ContinueSession); // Terminal node
    }

    [Fact]
    public async Task HandleRequestAsync_InvalidInput_ShowsErrorMessage()
    {
        // Arrange
        var options = new UssdOptions { InvalidInputMessage = "Invalid choice!" };
        var app = new UssdApp(_sessionStore, _logger, _menu, _serviceProvider, options);
        
        // Create new session
        var newSessionRequest = new UssdRequestDto
        {
            SessionID = "session1",
            UserID = "user1",
            NewSession = true,
            Msisdn = "233123456789",
            Network = "MTN"
        };
        await app.HandleRequestAsync(newSessionRequest);

        // Act - Invalid input
        var request = new UssdRequestDto
        {
            SessionID = "session1",
            UserID = "user1",
            NewSession = false,
            Msisdn = "233123456789",
            UserData = "99",
            Network = "MTN"
        };
        var response = await app.HandleRequestAsync(request);

        // Assert
        Assert.Contains("Invalid choice!", response.Message);
        Assert.True(response.ContinueSession);
    }

    [Fact]
    public async Task HandleRequestAsync_SessionPersists_AcrossRequests()
    {
        // Arrange
        var app = new UssdApp(_sessionStore, _logger, _menu, _serviceProvider);
        var sessionId = "session1";

        // Act - Create session
        await app.HandleRequestAsync(new UssdRequestDto
        {
            SessionID = sessionId,
            UserID = "user1",
            NewSession = true,
            Msisdn = "233123456789",
            Network = "MTN"
        });

        // Navigate to step2
        await app.HandleRequestAsync(new UssdRequestDto
        {
            SessionID = sessionId,
            UserID = "user1",
            NewSession = false,
            Msisdn = "233123456789",
            UserData = "2",
            Network = "MTN"
        });

        // Retrieve session
        var session = await _sessionStore.GetAsync(sessionId);

        // Assert
        Assert.NotNull(session);
        Assert.Equal("step2", session.CurrentStep);
        Assert.Equal(3, session.Level); // Incremented twice
    }

    [Fact]
    public async Task HandleRequestAsync_BackCommand_NavigatesBack()
    {
        // Arrange
        var options = new UssdOptions { BackCommand = "0" };
        var app = new UssdApp(_sessionStore, _logger, _menu, _serviceProvider, options);

        // Create session and navigate
        await app.HandleRequestAsync(new UssdRequestDto
        {
            SessionID = "session1",
            UserID = "user1",
            NewSession = true,
            Msisdn = "233123456789",
            Network = "MTN"
        });

        await app.HandleRequestAsync(new UssdRequestDto
        {
            SessionID = "session1",
            UserID = "user1",
            NewSession = false,
            Msisdn = "233123456789",
            UserData = "2",
            Network = "MTN"
        });

        // Act - Go back
        var response = await app.HandleRequestAsync(new UssdRequestDto
        {
            SessionID = "session1",
            UserID = "user1",
            NewSession = false,
            Msisdn = "233123456789",
            UserData = "0",
            Network = "MTN"
        });

        // Assert
        Assert.Contains("Going back", response.Message);
    }

    [Fact]
    public async Task HandleRequestAsync_HomeCommand_NavigatesToRoot()
    {
        // Arrange
        var options = new UssdOptions { HomeCommand = "00" };
        var app = new UssdApp(_sessionStore, _logger, _menu, _serviceProvider, options);

        // Navigate to a sub-menu
        await app.HandleRequestAsync(new UssdRequestDto
        {
            SessionID = "session1",
            UserID = "user1",
            NewSession = true,
            Msisdn = "233123456789",
            Network = "MTN"
        });

        await app.HandleRequestAsync(new UssdRequestDto
        {
            SessionID = "session1",
            UserID = "user1",
            NewSession = false,
            Msisdn = "233123456789",
            UserData = "2",
            Network = "MTN"
        });

        // Act - Go home
        var response = await app.HandleRequestAsync(new UssdRequestDto
        {
            SessionID = "session1",
            UserID = "user1",
            NewSession = false,
            Msisdn = "233123456789",
            UserData = "00",
            Network = "MTN"
        });

        // Assert
        Assert.Contains("Welcome to Test App", response.Message);
        
        var session = await _sessionStore.GetAsync("session1");
        Assert.Equal("main", session!.CurrentStep);
    }
}

