

using Bobcode.Ussd.Arkesel.Models;

namespace Bobcode.Ussd.Arkesel.Utilities;

/// <summary>
/// Helper class for paginating menu options
/// </summary>
public static class PaginationHelper
{
    /// <summary>
    /// Paginates a list of items
    /// </summary>
    /// <typeparam name="T">The type of items to paginate</typeparam>
    /// <param name="items">The full list of items</param>
    /// <param name="currentPage">The current page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated result containing items and metadata</returns>
    public static PaginatedResult<T> Paginate<T>(IEnumerable<T> items, int currentPage, int pageSize)
    {
        var itemList = items.ToList();
        var totalItems = itemList.Count;
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        
        // Ensure current page is within valid range
        currentPage = Math.Max(1, Math.Min(currentPage, totalPages));
        
        var skip = (currentPage - 1) * pageSize;
        var pageItems = itemList.Skip(skip).Take(pageSize).ToList();

        return new PaginatedResult<T>
        {
            Items = pageItems,
            CurrentPage = currentPage,
            TotalPages = totalPages,
            TotalItems = totalItems,
            PageSize = pageSize,
            HasNextPage = currentPage < totalPages,
            HasPreviousPage = currentPage > 1
        };
    }

    /// <summary>
    /// Creates a paginated menu from a list of options
    /// </summary>
    /// <param name="options">The menu options to paginate</param>
    /// <param name="currentPage">The current page number</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="nextPageCommand">Command for next page (e.g., "#")</param>
    /// <param name="previousPageCommand">Command for previous page (e.g., "*")</param>
    /// <returns>List of options including pagination controls</returns>
    public static List<MenuOption> CreatePaginatedMenu(
        List<MenuOption> options,
        int currentPage,
        int pageSize,
        string nextPageCommand = "#",
        string previousPageCommand = "*")
    {
        var result = Paginate(options, currentPage, pageSize);
        var paginatedOptions = new List<MenuOption>(result.Items);

        // Add pagination controls
        if (result.HasNextPage)
        {
            paginatedOptions.Add(new MenuOption
            {
                Input = nextPageCommand,
                Label = "Next Page",
                ActionKey = "_pagination_next"
            });
        }

        if (result.HasPreviousPage)
        {
            paginatedOptions.Add(new MenuOption
            {
                Input = previousPageCommand,
                Label = "Previous Page",
                ActionKey = "_pagination_previous"
            });
        }

        return paginatedOptions;
    }
}

/// <summary>
/// Result of a pagination operation
/// </summary>
/// <typeparam name="T">The type of items being paginated</typeparam>
public class PaginatedResult<T>
{
    /// <summary>
    /// Items on the current page
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage { get; set; }
}

