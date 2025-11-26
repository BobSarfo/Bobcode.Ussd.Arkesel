using Bobcode.Ussd.Arkesel.Actions;
using Bobcode.Ussd.Arkesel.Models;

namespace Bobcode.Ussd.Arkesel.Tests;

// Test action handler implementation
public class TestActionHandler : BaseActionHandler
{
    public override string Key => "test_action";

    public override Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        // Store some data in session
        SetSessionData(context, "test_key", "test_value");
        
        return Task.FromResult(End("Action executed successfully!"));
    }
}

public class VoteActionHandler : BaseActionHandler
{
    public override string Key => "vote";

    public override Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        var choice = context.Request.UserData;
        SetSessionData(context, "vote", choice);
        
        return Task.FromResult(End($"Thank you for voting for option {choice}!"));
    }
}

public class NavigationActionHandler : BaseActionHandler
{
    public override string Key => "navigate";

    public override Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        return Task.FromResult(Continue("Processing...", "next_step"));
    }
}

public class ActionHandlerTests
{
    [Fact]
    public async Task TestActionHandler_ExecutesSuccessfully()
    {
        // Arrange
        var handler = new TestActionHandler();
        var session = new UssdSession("session1", "233123456789", "user1", "MTN");
        var request = new UssdRequestDto
        {
            SessionID = "session1",
            UserID = "user1",
            Msisdn = "233123456789",
            UserData = "1",
            Network = "MTN"
        };
        var context = new UssdContext(request, session);

        // Act
        var result = await handler.HandleAsync(context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Action executed successfully!", result.Message);
        Assert.False(result.ContinueSession);
        Assert.Equal("test_value", session.Data["test_key"]);
    }

    [Fact]
    public async Task VoteActionHandler_StoresVoteInSession()
    {
        // Arrange
        var handler = new VoteActionHandler();
        var session = new UssdSession("session1", "233123456789", "user1", "MTN");
        var request = new UssdRequestDto
        {
            SessionID = "session1",
            UserID = "user1",
            Msisdn = "233123456789",
            UserData = "2",
            Network = "MTN"
        };
        var context = new UssdContext(request, session);

        // Act
        var result = await handler.HandleAsync(context);

        // Assert
        Assert.Contains("Thank you for voting", result.Message);
        Assert.Equal("2", session.Data["vote"]);
    }

    [Fact]
    public async Task NavigationActionHandler_ReturnsNextStep()
    {
        // Arrange
        var handler = new NavigationActionHandler();
        var session = new UssdSession("session1", "233123456789", "user1", "MTN");
        var request = new UssdRequestDto
        {
            SessionID = "session1",
            UserID = "user1",
            Msisdn = "233123456789",
            UserData = "1",
            Network = "MTN"
        };
        var context = new UssdContext(request, session);

        // Act
        var result = await handler.HandleAsync(context);

        // Assert
        Assert.True(result.ContinueSession);
        Assert.Equal("next_step", result.NextStep);
    }

    [Fact]
    public void SessionData_CanBeAccessedDirectly()
    {
        // Arrange
        var session = new UssdSession("session1", "233123456789", "user1", "MTN");
        session.Data["count"] = 42;
        session.Data["name"] = "John";

        // Act & Assert
        Assert.Equal(42, session.Data["count"]);
        Assert.Equal("John", session.Data["name"]);
    }

    [Fact]
    public void ActionHandler_HasCorrectKey()
    {
        // Arrange & Act
        var testHandler = new TestActionHandler();
        var voteHandler = new VoteActionHandler();
        var navHandler = new NavigationActionHandler();

        // Assert
        Assert.Equal("test_action", testHandler.Key);
        Assert.Equal("vote", voteHandler.Key);
        Assert.Equal("navigate", navHandler.Key);
    }
}

