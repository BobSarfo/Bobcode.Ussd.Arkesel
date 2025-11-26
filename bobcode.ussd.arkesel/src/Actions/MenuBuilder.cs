using Bobcode.Ussd.Arkesel.Models;

namespace Bobcode.Ussd.Arkesel.Actions;

/// <summary>
/// Fluent builder for creating USSD menu structures
/// </summary>
public class MenuBuilder
{
    private readonly Menu _menu;

    public MenuBuilder(string menuId) => _menu = new Menu(menuId);

    /// <summary>
    /// Sets the root page ID for the menu
    /// </summary>
    public MenuBuilder SetRoot(string rootId)
    {
        _menu.RootId = rootId;
        return this;
    }

    /// <summary>
    /// Adds a menu page
    /// </summary>
    /// <param name="id">Unique identifier for the page</param>
    /// <param name="title">Title/header text displayed to the user</param>
    /// <param name="isTerminal">Whether this page ends the session</param>
    public MenuBuilder AddPage(string id, string title, bool isTerminal = false)
    {
        var page = new MenuPage(id, title) { IsTerminal = isTerminal };
        _menu.Pages[id] = page;
        return this;
    }

    /// <summary>
    /// Adds an option to a menu page
    /// </summary>
    /// <param name="pageId">The page to add the option to</param>
    /// <param name="input">The input value the user enters (use "*" for wildcard/any input)</param>
    /// <param name="label">The label displayed to the user</param>
    /// <param name="targetStep">The page to navigate to when selected</param>
    /// <param name="actionKey">The action handler key to execute when selected</param>
    /// <param name="isWildcard">If true, this option accepts any user input</param>
    public MenuBuilder AddOption(
        string pageId,
        string input,
        string label,
        string? targetStep = null,
        string? actionKey = null,
        bool isWildcard = false)
    {
        if (!_menu.Pages.ContainsKey(pageId))
        {
            throw new InvalidOperationException($"Page '{pageId}' does not exist. Add the page before adding options.");
        }

        var page = _menu.Pages[pageId];
        page.Options.Add(new MenuOption
        {
            Input = input,
            Label = label,
            TargetStep = targetStep,
            ActionKey = actionKey,
            IsWildcard = isWildcard || input == "*" // Auto-detect wildcard from "*" input
        });
        return this;
    }

    /// <summary>
    /// Adds multiple options to a menu page
    /// </summary>
    public MenuBuilder AddOptions(string pageId, params (string input, string label, string? targetStep, string? actionKey)[] options)
    {
        foreach (var (input, label, targetStep, actionKey) in options)
        {
            AddOption(pageId, input, label, targetStep, actionKey);
        }
        return this;
    }

    /// <summary>
    /// Builds and returns the menu
    /// </summary>
    public Menu Build()
    {
        // Validate that root page exists
        if (!_menu.Pages.ContainsKey(_menu.RootId))
        {
            throw new InvalidOperationException($"Root page '{_menu.RootId}' does not exist in the menu.");
        }

        return _menu;
    }
}
