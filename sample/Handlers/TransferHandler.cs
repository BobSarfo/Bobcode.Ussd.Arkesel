using OpenUSSD.Actions;
using OpenUSSD.Attributes;
using OpenUSSD.models;

namespace Sample.Handlers;

/// <summary>
/// Example action handler for processing money transfers
/// Demonstrates multi-step flow with strongly-typed session data
/// </summary>
[UssdAction] // Auto-generates key as "Transfer" from class name
public class TransferHandler : BaseActionHandler
{
    public override Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        var input = context.Request.UserData;

        // Get current step from session using strongly-typed key
        var transferStep = Get(context, SessionKeys.TransferStep) ?? "recipient";

        switch (transferStep)
        {
            case "recipient":
                // Store recipient phone number using strongly-typed key
                Set(context, SessionKeys.Recipient, input);
                Set(context, SessionKeys.TransferStep, "amount");
                return Task.FromResult(Continue("Enter amount to transfer:"));

            case "amount":
                // Validate and store amount
                if (!decimal.TryParse(input, out var amount) || amount <= 0)
                {
                    return Task.FromResult(Continue("Invalid amount. Please enter a valid amount:"));
                }

                Set(context, SessionKeys.Amount, amount);
                Set(context, SessionKeys.TransferStep, "confirm");

                var recipient = Get(context, SessionKeys.Recipient);
                return Task.FromResult(Continue(
                    $"Confirm transfer:\nTo: {recipient}\nAmount: GHS {amount:F2}\n1. Confirm\n2. Cancel"
                ));

            case "confirm":
                if (input == "1")
                {
                    // Process transfer
                    var recipientPhone = Get(context, SessionKeys.Recipient);
                    var transferAmount = Get(context, SessionKeys.Amount);

                    // In a real application, you would process the transfer here
                    // await _transferService.ProcessTransferAsync(context.Session.Msisdn, recipientPhone, transferAmount);

                    // Clear transfer session data
                    Remove(context, SessionKeys.TransferStep);
                    Remove(context, SessionKeys.Recipient);
                    Remove(context, SessionKeys.Amount);

                    return Task.FromResult(End($"Transfer of GHS {transferAmount:F2} to {recipientPhone} successful!\nThank you."));
                }
                else
                {
                    // Clear transfer session data
                    Remove(context, SessionKeys.TransferStep);
                    Remove(context, SessionKeys.Recipient);
                    Remove(context, SessionKeys.Amount);

                    return Task.FromResult(GoHome());
                }

            default:
                return Task.FromResult(End("An error occurred. Please try again."));
        }
    }
}

