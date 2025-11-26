

using Bobcode.Ussd.Arkesel.Attributes;
using Bobcode.Ussd.Arkesel.Extensions;
using Bobcode.Ussd.Arkesel.Models;

namespace Bobcode.Ussd.Arkesel.Actions;

/// <summary>
/// Base class for action handlers providing common functionality
/// </summary>
public abstract class BaseActionHandler : IActionHandler
{
    private string? _cachedKey;

    /// <summary>
    /// Unique key identifying this action handler.
    /// Auto-generated from class name or [UssdAction] attribute.
    /// </summary>
    public virtual string Key
    {
        get
        {
            if (_cachedKey == null)
            {
                _cachedKey = UssdActionAttribute.GetActionKey(GetType());
            }
            return _cachedKey;
        }
    }

    /// <summary>
    /// Handles the action and returns the result
    /// </summary>
    public abstract Task<UssdStepResult> HandleAsync(UssdContext context);

    /// <summary>
    /// Helper method to continue the session with a message
    /// </summary>
    protected UssdStepResult Continue(string message, string? nextStep = null)
        => new UssdStepResult
        {
            Message = message,
            ContinueSession = true,
            NextStep = nextStep
        };

    /// <summary>
    /// Helper method to end the session with a message
    /// </summary>
    protected UssdStepResult End(string message)
        => new UssdStepResult
        {
            Message = message,
            ContinueSession = false
        };

    /// <summary>
    /// Helper method to navigate to home menu
    /// </summary>
    protected UssdStepResult GoHome()
        => new UssdStepResult
        {
            GoHome = true,
            ContinueSession = true
        };

    /// <summary>
    /// Helper method to navigate to a specific menu page using enum
    /// </summary>
    protected UssdStepResult GoTo<TNode>(TNode page) where TNode : struct, Enum
        => new UssdStepResult
        {
            ContinueSession = true,
            NextStep = page.ToPageId()
        };

    /// <summary>
    /// Helper method to navigate to a specific menu node using string ID
    /// </summary>
    protected UssdStepResult GoTo(string nodeId)
        => new UssdStepResult
        {
            ContinueSession = true,
            NextStep = nodeId
        };

    /// <summary>
    /// Helper method to get session data using a string key (legacy)
    /// </summary>
    protected T? GetSessionData<T>(UssdContext context, string key)
    {
        if (context.Session.Data.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }

    /// <summary>
    /// Helper method to set session data using a string key (legacy)
    /// </summary>
    protected void SetSessionData(UssdContext context, string key, object? value)
    {
        context.Session.Data[key] = value;
    }

    /// <summary>
    /// Helper method to get strongly-typed session data
    /// </summary>
    protected T? Get<T>(UssdContext context, SessionKey<T> key)
    {
        return context.Session.Get(key);
    }

    /// <summary>
    /// Helper method to set strongly-typed session data
    /// </summary>
    protected void Set<T>(UssdContext context, SessionKey<T> key, T value)
    {
        context.Session.Set(key, value);
    }

    /// <summary>
    /// Helper method to get strongly-typed session data with default value
    /// </summary>
    protected T GetOrDefault<T>(UssdContext context, SessionKey<T> key, T defaultValue)
    {
        return context.Session.GetOrDefault(key, defaultValue);
    }

    /// <summary>
    /// Helper method to check if a session key exists
    /// </summary>
    protected bool Has<T>(UssdContext context, SessionKey<T> key)
    {
        return context.Session.Has(key);
    }

    /// <summary>
    /// Helper method to remove a session key
    /// </summary>
    protected void Remove<T>(UssdContext context, SessionKey<T> key)
    {
        context.Session.Remove(key);
    }
}
