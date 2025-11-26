namespace Bobcode.Ussd.Arkesel.Models;

/// <summary>
/// Represents a USSD session with strongly-typed data access
/// </summary>
public class UssdSession
{
    public string SessionId { get; init; }
    public string Msisdn { get; init; }
    public string UserId { get; init; }
    public string Network { get; init; }
    public int Level { get; set; } = 1;
    public int Part { get; set; } = 1;
    public string CurrentStep { get; set; }
    public DateTime ExpireAt { get; set; }
    public IDictionary<string, object?> Data { get; } = new Dictionary<string, object?>();

    /// <summary>
    /// Indicates if the session is waiting for resume/fresh choice
    /// </summary>
    public bool AwaitingResumeChoice { get; set; } = false;

    /// <summary>
    /// Stores the previous state before showing resume prompt
    /// </summary>
    public string? PreviousStep { get; set; }

    public UssdSession(string sessionId, string msisdn, string userId, string network, string? initialStep = null)
    {
        SessionId = sessionId;
        Msisdn = msisdn;
        UserId = userId;
        Network = network;
        CurrentStep = initialStep ?? "main"; // Fallback to "main" for backward compatibility
        ExpireAt = DateTime.UtcNow.AddMinutes(2);
    }

    /// <summary>
    /// Sets a strongly-typed value in the session
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="key">The strongly-typed session key</param>
    /// <param name="value">The value to store</param>
    public void Set<T>(SessionKey<T> key, T value)
    {
        Data[key.Key] = value;
    }

    /// <summary>
    /// Gets a strongly-typed value from the session
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="key">The strongly-typed session key</param>
    /// <returns>The value, or default(T) if not found</returns>
    public T? Get<T>(SessionKey<T> key)
    {
        if (Data.TryGetValue(key.Key, out var value) && value != null)
        {
            if (value is T typedValue)
                return typedValue;

            // Try to convert if possible
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default;
            }
        }
        return default;
    }

    /// <summary>
    /// Gets a strongly-typed value from the session with a default value
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="key">The strongly-typed session key</param>
    /// <param name="defaultValue">The default value to return if not found</param>
    /// <returns>The value, or the default value if not found</returns>
    public T GetOrDefault<T>(SessionKey<T> key, T defaultValue)
    {
        return Get(key) ?? defaultValue;
    }

    /// <summary>
    /// Checks if a key exists in the session
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="key">The strongly-typed session key</param>
    /// <returns>True if the key exists, false otherwise</returns>
    public bool Has<T>(SessionKey<T> key)
    {
        return Data.ContainsKey(key.Key);
    }

    /// <summary>
    /// Removes a value from the session
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="key">The strongly-typed session key</param>
    public void Remove<T>(SessionKey<T> key)
    {
        Data.Remove(key.Key);
    }

    /// <summary>
    /// Clears all session data
    /// </summary>
    public void Clear()
    {
        Data.Clear();
    }
}