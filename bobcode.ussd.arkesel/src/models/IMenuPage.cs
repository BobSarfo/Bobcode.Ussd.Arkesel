namespace Bobcode.Ussd.Arkesel.Models;

/// <summary>
/// Interface for menu page identifiers.
/// Implement this with an enum to create strongly-typed menu pages.
/// </summary>
public interface IMenuPage
{
    /// <summary>
    /// Gets the string representation of the menu page
    /// </summary>
    string ToPageId();
}
