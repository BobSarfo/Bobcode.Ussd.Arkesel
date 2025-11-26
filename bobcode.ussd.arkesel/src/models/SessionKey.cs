namespace Bobcode.Ussd.Arkesel.Models;

/// <summary>
/// Strongly-typed session key for type-safe session data access
/// </summary>
/// <typeparam name="T">The type of value stored under this key</typeparam>
public sealed class SessionKey<T>
{
    /// <summary>
    /// The internal string key used for storage
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Creates a new strongly-typed session key
    /// </summary>
    /// <param name="key">The internal string key</param>
    public SessionKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Session key cannot be null or empty", nameof(key));
        
        Key = key;
    }

    /// <summary>
    /// Implicit conversion to string for backward compatibility
    /// </summary>
    public static implicit operator string(SessionKey<T> sessionKey) => sessionKey.Key;

    public override string ToString() => Key;

    public override bool Equals(object? obj)
    {
        if (obj is SessionKey<T> other)
            return Key == other.Key;
        if (obj is string str)
            return Key == str;
        return false;
    }

    public override int GetHashCode() => Key.GetHashCode();
}

