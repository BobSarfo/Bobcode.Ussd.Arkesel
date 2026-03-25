using System.Text.Json;
using Bobcode.Ussd.Arkesel.Models;

namespace bobcode.ussd.arkesel.Tests;

public class UssdSessionJsonTests
{
    [Fact]
    public void Serialize_Deserialize_RoundTrips_ConstructorAndMutableFields()
    {
        var session = new UssdSession("session1", "233123456789", "user1", "MTN", "menu:root");
        session.Level = 3;
        session.Part = 2;
        session.AwaitingResumeChoice = true;
        session.PreviousStep = "prev";
        session.Data["note"] = "hello";

        var json = JsonSerializer.Serialize(session);
        var restored = JsonSerializer.Deserialize<UssdSession>(json);

        Assert.NotNull(restored);
        Assert.Equal(session.SessionId, restored.SessionId);
        Assert.Equal(session.Msisdn, restored.Msisdn);
        Assert.Equal(session.UserId, restored.UserId);
        Assert.Equal(session.Network, restored.Network);
        Assert.Equal(session.CurrentStep, restored.CurrentStep);
        Assert.Equal(session.Level, restored.Level);
        Assert.Equal(session.Part, restored.Part);
        Assert.Equal(session.AwaitingResumeChoice, restored.AwaitingResumeChoice);
        Assert.Equal(session.PreviousStep, restored.PreviousStep);
        Assert.True(restored.Data.ContainsKey("note"));
    }
}
