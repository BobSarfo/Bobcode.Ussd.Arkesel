namespace Bobcode.Ussd.Arkesel.Models;

/// <summary>
/// Represents a USSD menu structure with multiple pages
/// </summary>
public class Menu
{
    /// <summary>
    /// Unique identifier for this menu
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Collection of menu pages indexed by their ID
    /// </summary>
    public Dictionary<string, MenuPage> Pages { get; } = new();

    /// <summary>
    /// ID of the root/starting page
    /// </summary>
    public string RootId { get; set; } = "main";

    public Menu(string id) => Id = id;

    /// <summary>
    /// Gets a menu page by ID
    /// </summary>
    /// <param name="id">The page ID</param>
    /// <returns>The menu page</returns>
    /// <exception cref="KeyNotFoundException">Thrown when page ID doesn't exist</exception>
    public MenuPage GetPage(string id)
    {
        if (!Pages.TryGetValue(id, out var page))
        {
            throw new KeyNotFoundException($"Menu page '{id}' not found in menu '{Id}'.");
        }
        return page;
    }

    /// <summary>
    /// Tries to get a menu page by ID
    /// </summary>
    /// <param name="id">The page ID</param>
    /// <param name="page">The menu page if found</param>
    /// <returns>True if page exists, false otherwise</returns>
    public bool TryGetPage(string id, out MenuPage? page)
    {
        return Pages.TryGetValue(id, out page);
    }

    /// <summary>
    /// Checks if a page exists in the menu
    /// </summary>
    public bool HasPage(string id) => Pages.ContainsKey(id);
}
