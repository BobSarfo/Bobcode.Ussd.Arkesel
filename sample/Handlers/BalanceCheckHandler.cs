using Bobcode.Ussd.Arkesel.Actions;
using Bobcode.Ussd.Arkesel.Attributes;
using Bobcode.Ussd.Arkesel.Models;

namespace Bobcode.Ussd.Arkesel.Sample.Handlers;

/// <summary>
/// Example action handler for checking account balance
/// </summary>
[UssdAction] // Auto-generates key as "BalanceCheck" from class name
public class BalanceCheckHandler : BaseActionHandler
{
    public override Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        // In a real application, you would fetch the balance from a database or API
        // var balance = await _accountService.GetBalanceAsync(context.Session.Msisdn);

        // For demo purposes, we'll use a mock balance
        var balance = 150.50m;

        // Store in session for potential future use using strongly-typed key
        Set(context, SessionKeys.LastBalanceCheck, DateTime.UtcNow);

        return Task.FromResult(End($"Your current balance is: GHS {balance:F2}\nThank you for using our service."));
    }
}

