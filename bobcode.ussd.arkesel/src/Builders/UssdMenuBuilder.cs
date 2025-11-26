using Bobcode.Ussd.Arkesel.Actions;
using Bobcode.Ussd.Arkesel.Extensions;
using Bobcode.Ussd.Arkesel.Models;

namespace Bobcode.Ussd.Arkesel.Builders;

/// <summary>
/// Strongly-typed fluent builder for creating USSD menu structures
/// </summary>
/// <typeparam name="TNode">The enum type representing menu pages</typeparam>
public class UssdMenuBuilder<TNode> where TNode : struct, Enum
{
    private readonly Menu _menu;
    private TNode? _rootNode;

    public UssdMenuBuilder(string menuId)
    {
        _menu = new Menu(menuId);
    }

    /// <summary>
    /// Sets the root page for the menu
    /// </summary>
    public UssdMenuBuilder<TNode> Root(TNode page)
    {
        _rootNode = page;
        _menu.RootId = page.ToPageId();
        return this;
    }

    /// <summary>
    /// Adds a menu page with fluent configuration
    /// </summary>
    public UssdMenuBuilder<TNode> Page(TNode page, Action<PageBuilder<TNode>> configure)
    {
        var pageBuilder = new PageBuilder<TNode>(page, _menu);
        configure(pageBuilder);
        return this;
    }

    /// <summary>
    /// Builds and returns the menu
    /// </summary>
    public Menu Build()
    {
        if (_rootNode == null)
            throw new InvalidOperationException("Root page must be set before building the menu.");

        if (!_menu.Pages.ContainsKey(_menu.RootId))
            throw new InvalidOperationException($"Root page '{_rootNode}' has not been configured.");

        return _menu;
    }
}

/// <summary>
/// Builder for configuring a single menu page
/// </summary>
public class PageBuilder<TNode> where TNode : struct, Enum
{
    private readonly TNode _page;
    private readonly Menu _menu;
    private readonly MenuPage _menuPage;
    private readonly List<string> _messageLines = new();

    internal PageBuilder(TNode page, Menu menu)
    {
        _page = page;
        _menu = menu;
        var pageId = page.ToPageId();
        
        // Create or get existing page
        if (!_menu.Pages.TryGetValue(pageId, out var existingPage))
        {
            _menuPage = new MenuPage(pageId, "");
            _menu.Pages[pageId] = _menuPage;
        }
        else
        {
            _menuPage = existingPage;
        }
    }

    /// <summary>
    /// Sets the message text for this page
    /// </summary>
    public PageBuilder<TNode> Title(string message)
    {
        _messageLines.Add(message);
        UpdateTitle();
        return this;
    }

    /// <summary>
    /// Adds a line to the message
    /// </summary>
    public PageBuilder<TNode> Line(string line)
    {
        _messageLines.Add(line);
        UpdateTitle();
        return this;
    }

    /// <summary>
    /// Marks this page as terminal (ends the session)
    /// </summary>
    public PageBuilder<TNode> Terminal()
    {
        _menuPage.IsTerminal = true;
        return this;
    }

    /// <summary>
    /// Adds an option to this page
    /// </summary>
    public OptionBuilder<TNode> Option(string input, string label)
    {
        return new OptionBuilder<TNode>(this, input, label);
    }

    /// <summary>
    /// Adds a paginated list of items as options
    /// </summary>
    public PageBuilder<TNode> OptionList<T>(
        IEnumerable<T> items,
        Func<T, string> labelSelector,
        bool autoPaginate = true,
        int itemsPerPage = 5)
    {
        var itemList = items.ToList();
        
        if (!autoPaginate || itemList.Count <= itemsPerPage)
        {
            // Add all items without pagination
            for (int i = 0; i < itemList.Count; i++)
            {
                var item = itemList[i];
                var input = (i + 1).ToString();
                var label = labelSelector(item);
                _messageLines.Add($"{input}. {label}");
            }
        }
        else
        {
            // Mark this page for pagination
            _menuPage.IsPaginated = true;
            _menuPage.ItemsPerPage = itemsPerPage;
            
            // Store items for pagination (will be handled by UssdApp)
            for (int i = 0; i < itemList.Count; i++)
            {
                var item = itemList[i];
                var label = labelSelector(item);
                _messageLines.Add($"{i + 1}. {label}");
            }
        }
        
        UpdateTitle();
        return this;
    }

    /// <summary>
    /// Adds an input field that accepts any user input (wildcard).
    /// This is useful for collecting free-form text like phone numbers, amounts, names, etc.
    /// The wildcard will match any input that doesn't match other specific options.
    /// The actual user input will be passed to the action handler via context.Request.UserData.
    /// </summary>
    public OptionBuilder<TNode> Input()
    {
        return new OptionBuilder<TNode>(this, "*", "", isWildcard: true);
    }

    internal void AddOption(MenuOption option)
    {
        _menuPage.Options.Add(option);
    }

    private void UpdateTitle()
    {
        _menuPage.Title = string.Join("\n", _messageLines);
    }

    internal PageBuilder<TNode> GetThis() => this;
}

/// <summary>
/// Builder for configuring a menu option
/// </summary>
public class OptionBuilder<TNode> where TNode : struct, Enum
{
    private readonly PageBuilder<TNode> _pageBuilder;
    private readonly string _input;
    private readonly string _label;
    private readonly bool _isWildcard;
    private string? _targetStep;
    private string? _actionKey;

    internal OptionBuilder(PageBuilder<TNode> pageBuilder, string input, string label, bool isWildcard = false)
    {
        _pageBuilder = pageBuilder;
        _input = input;
        _label = label;
        _isWildcard = isWildcard;
    }

    /// <summary>
    /// Navigates to another page when this option is selected
    /// </summary>
    public PageBuilder<TNode> GoTo(TNode targetPage)
    {
        _targetStep = targetPage.ToPageId();
        Commit();
        return _pageBuilder;
    }

    /// <summary>
    /// Executes an action handler when this option is selected
    /// </summary>
    public PageBuilder<TNode> Action<THandler>() where THandler : IActionHandler
    {
        _actionKey = Attributes.UssdActionAttribute.GetActionKey(typeof(THandler));
        Commit();
        return _pageBuilder;
    }

    /// <summary>
    /// Executes an action handler with a specific key
    /// </summary>
    public PageBuilder<TNode> Action(string actionKey)
    {
        _actionKey = actionKey;
        Commit();
        return _pageBuilder;
    }

    /// <summary>
    /// Navigates to a page and executes an action
    /// </summary>
    public PageBuilder<TNode> GoToAndAction<THandler>(TNode targetPage) where THandler : IActionHandler
    {
        _targetStep = targetPage.ToPageId();
        _actionKey = Attributes.UssdActionAttribute.GetActionKey(typeof(THandler));
        Commit();
        return _pageBuilder;
    }

    private void Commit()
    {
        var option = new MenuOption
        {
            Input = _input,
            Label = _label,
            TargetStep = _targetStep,
            ActionKey = _actionKey,
            IsWildcard = _isWildcard
        };
        _pageBuilder.AddOption(option);
    }
}

