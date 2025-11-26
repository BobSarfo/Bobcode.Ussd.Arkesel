using Bobcode.Ussd.Arkesel.Actions;
using Bobcode.Ussd.Arkesel.Attributes;
using Bobcode.Ussd.Arkesel.Extensions;
using Bobcode.Ussd.Arkesel.Models;

namespace Bobcode.Ussd.Arkesel.Sample.Handlers;

/// <summary>
/// Handler for collecting recipient phone number
/// </summary>
[UssdAction]
public class TransferRecipientHandler : BaseActionHandler
{
    public override Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        var input = context.Request.UserData;

        // Basic validation for phone number (you can add more sophisticated validation)
        if (string.IsNullOrWhiteSpace(input) || input.Length < 10)
        {
            // Stay on current node (TransferRecipient) and show error
            return Task.FromResult(Continue("Invalid phone number. Please enter a valid phone number:"));
        }

        // Store recipient phone number using strongly-typed key
        Set(context, SessionKeys.Recipient, input);

        // Navigate to amount collection node
        return Task.FromResult(GoTo(BankMenuNode.TransferAmount));
    }
}

/// <summary>
/// Handler for collecting transfer amount
/// </summary>
[UssdAction]
public class TransferAmountHandler : BaseActionHandler
{
    public override Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        var input = context.Request.UserData;

        // Validate and store amount
        if (!decimal.TryParse(input, out var amount) || amount <= 0)
        {
            return Task.FromResult(Continue("Invalid amount. Please enter a valid amount:"));
        }

        Set(context, SessionKeys.Amount, amount);

        // Build confirmation message with collected data
        var recipient = Get(context, SessionKeys.Recipient);
        var confirmMessage = $"Confirm transfer:\nTo: {recipient}\nAmount: GHS {amount:F2}\n\n1. Confirm\n2. Cancel";

        // Navigate to confirmation page with custom message
        return Task.FromResult(Continue(confirmMessage, BankMenuNode.TransferConfirm.ToPageId()));
    }
}

/// <summary>
/// Handler for confirming and processing the transfer
/// </summary>
[UssdAction]
public class TransferConfirmHandler : BaseActionHandler
{
    public override Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        var input = context.Request.UserData;

        if (input == "1")
        {
            // Process transfer
            var recipientPhone = Get(context, SessionKeys.Recipient);
            var transferAmount = Get(context, SessionKeys.Amount);

            // In a real application, you would process the transfer here
            // await _transferService.ProcessTransferAsync(context.Session.Msisdn, recipientPhone, transferAmount);

            // Clear transfer session data (End() and GoHome() clear session automatically)
            Remove(context, SessionKeys.Recipient);
            Remove(context, SessionKeys.Amount);

            return Task.FromResult(End($"Transfer of GHS {transferAmount:F2} to {recipientPhone} successful!\nThank you."));
        }
        else
        {
            // Clear transfer session data (GoHome() clears session automatically)
            Remove(context, SessionKeys.Recipient);
            Remove(context, SessionKeys.Amount);

            return Task.FromResult(GoHome());
        }
    }
}

