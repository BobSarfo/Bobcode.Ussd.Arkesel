namespace OpenUSSD.Extensions;

/// <summary>
/// Extension methods for enum-based menu nodes
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Converts an enum value to a node ID string
    /// </summary>
    public static string ToNodeId<TEnum>(this TEnum enumValue) where TEnum : struct, Enum
    {
        return enumValue.ToString();
    }

    /// <summary>
    /// Tries to parse a node ID string to an enum value
    /// </summary>
    public static bool TryParseNodeId<TEnum>(string nodeId, out TEnum result) where TEnum : struct, Enum
    {
        return Enum.TryParse(nodeId, true, out result);
    }

    /// <summary>
    /// Parses a node ID string to an enum value
    /// </summary>
    public static TEnum ParseNodeId<TEnum>(string nodeId) where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(nodeId, true, out var result))
            return result;
        
        throw new ArgumentException($"Invalid node ID '{nodeId}' for enum type {typeof(TEnum).Name}");
    }
}

