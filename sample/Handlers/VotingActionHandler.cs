using Bobcode.Ussd.Arkesel.Actions;
using Bobcode.Ussd.Arkesel.Attributes;
using Bobcode.Ussd.Arkesel.Models;

namespace Bobcode.Ussd.Arkesel.Sample.Handlers;

/// <summary>
/// Example action handler for processing voting selections
/// </summary>
[UssdAction] // Auto-generates key as "VotingAction" from class name
public class VotingActionHandler : BaseActionHandler
{
    public override Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        // Get the candidate from user input
        var candidate = context.Request.UserData;

        // Store the vote in session using strongly-typed keys
        Set(context, SessionKeys.Vote, candidate);
        Set(context, SessionKeys.Timestamp, DateTime.UtcNow);

        // In a real application, you would save the vote to a database here
        // await _voteRepository.SaveVoteAsync(context.Session.Msisdn, candidate);

        return Task.FromResult(End($"Thank you for voting for candidate {candidate}!\nYour vote has been recorded."));
    }
}

