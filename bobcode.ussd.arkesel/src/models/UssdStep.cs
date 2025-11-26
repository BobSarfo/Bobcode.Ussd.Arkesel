using Bobcode.Ussd.Arkesel.Actions;

namespace Bobcode.Ussd.Arkesel.Models;

public abstract class UssdStep
{
    public string Name { get; set; } = string.Empty;
    public abstract Task<UssdStepResult> ExecuteAsync(UssdContext ctx);
}