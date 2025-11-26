namespace Bobcode.Ussd.Arkesel.Extensions;

/// <summary>
/// Extension methods for enum-based menu pages
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Converts an enum value to a page ID string
    /// </summary>
    public static string ToPageId<TEnum>(this TEnum enumValue) where TEnum : struct, Enum
    {
        return enumValue.ToString();
    }

    /// <summary>
    /// Tries to parse a page ID string to an enum value
    /// </summary>
    public static bool TryParsePageId<TEnum>(string pageId, out TEnum result) where TEnum : struct, Enum
    {
        return Enum.TryParse(pageId, true, out result);
    }

    /// <summary>
    /// Parses a page ID string to an enum value
    /// </summary>
    public static TEnum ParsePageId<TEnum>(string pageId) where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(pageId, true, out var result))
            return result;
        
        throw new ArgumentException($"Invalid page ID '{pageId}' for enum type {typeof(TEnum).Name}");
    }
}

