namespace Bobcode.Ussd.Arkesel.Models;

public class MenuPage
{
    public string Id { get; init; }
    public string Title { get; set; }
    public IList<MenuOption> Options { get; } = new List<MenuOption>();
    public bool IsTerminal { get; set; }
    public bool IsPaginated { get; set; }
    public int ItemsPerPage { get; set; } = 5;

    public MenuPage(string id, string title)
    {
        Id = id;
        Title = title;
    }
}