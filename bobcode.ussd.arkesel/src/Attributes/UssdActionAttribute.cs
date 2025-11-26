namespace Bobcode.Ussd.Arkesel.Attributes;

/// <summary>
/// Marks a class as a USSD action handler and optionally specifies its action key.
/// If no key is provided, the class name (without "Handler" suffix) is used.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class UssdActionAttribute : Attribute
{
    /// <summary>
    /// The action key used to identify this handler.
    /// If null, the class name (without "Handler" suffix) will be used.
    /// </summary>
    public string? Key { get; }

    /// <summary>
    /// Creates a new UssdAction attribute with auto-generated key from class name
    /// </summary>
    public UssdActionAttribute()
    {
        Key = null;
    }

    /// <summary>
    /// Creates a new UssdAction attribute with a specific key
    /// </summary>
    /// <param name="key">The action key to use</param>
    public UssdActionAttribute(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Action key cannot be null or empty", nameof(key));
        
        Key = key;
    }

    /// <summary>
    /// Gets the action key for a handler type, either from the attribute or derived from the class name
    /// </summary>
    public static string GetActionKey(Type handlerType)
    {
        var attribute = handlerType.GetCustomAttributes(typeof(UssdActionAttribute), false)
            .FirstOrDefault() as UssdActionAttribute;

        if (attribute?.Key != null)
            return attribute.Key;

        // Auto-generate from class name: "BalanceCheckHandler" -> "BalanceCheck"
        var className = handlerType.Name;
        if (className.EndsWith("Handler", StringComparison.OrdinalIgnoreCase))
            return className.Substring(0, className.Length - 7);
        
        return className;
    }
}

