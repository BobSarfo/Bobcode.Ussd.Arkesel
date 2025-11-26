using Bobcode.Ussd.Arkesel.Models;

namespace Bobcode.Ussd.Arkesel.Actions;

/// <summary>
/// Interface for handling USSD actions triggered by menu options
/// </summary>
public interface IActionHandler
{
    /// <summary>
    /// Unique key identifying this action handler
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Handles the action and returns the result
    /// </summary>
    /// <param name="context">The USSD context containing request and session information</param>
    /// <returns>The result of the action execution</returns>
    Task<UssdStepResult> HandleAsync(UssdContext context);
}
