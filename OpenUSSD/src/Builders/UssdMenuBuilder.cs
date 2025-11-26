using OpenUSSD.Actions;
using OpenUSSD.Extensions;
using OpenUSSD.models;

namespace OpenUSSD.Builders;

/// <summary>
/// Strongly-typed fluent builder for creating USSD menu structures
/// </summary>
/// <typeparam name="TNode">The enum type representing menu nodes</typeparam>
public class UssdMenuBuilder<TNode> where TNode : struct, Enum
{
    private readonly Menu _menu;
    private TNode? _rootNode;

    public UssdMenuBuilder(string menuId)
    {
        _menu = new Menu(menuId);
    }

    /// <summary>
    /// Sets the root node for the menu
    /// </summary>
    public UssdMenuBuilder<TNode> Root(TNode node)
    {
        _rootNode = node;
        _menu.RootId = node.ToNodeId();
        return this;
    }

    /// <summary>
    /// Adds a menu node with fluent configuration
    /// </summary>
    public UssdMenuBuilder<TNode> Node(TNode node, Action<NodeBuilder<TNode>> configure)
    {
        var nodeBuilder = new NodeBuilder<TNode>(node, _menu);
        configure(nodeBuilder);
        return this;
    }

    /// <summary>
    /// Builds and returns the menu
    /// </summary>
    public Menu Build()
    {
        if (_rootNode == null)
            throw new InvalidOperationException("Root node must be set before building the menu.");

        if (!_menu.Nodes.ContainsKey(_menu.RootId))
            throw new InvalidOperationException($"Root node '{_rootNode}' has not been configured.");

        return _menu;
    }
}

/// <summary>
/// Builder for configuring a single menu node
/// </summary>
public class NodeBuilder<TNode> where TNode : struct, Enum
{
    private readonly TNode _node;
    private readonly Menu _menu;
    private readonly MenuNode _menuNode;
    private readonly List<string> _messageLines = new();

    internal NodeBuilder(TNode node, Menu menu)
    {
        _node = node;
        _menu = menu;
        var nodeId = node.ToNodeId();
        
        // Create or get existing node
        if (!_menu.Nodes.TryGetValue(nodeId, out var existingNode))
        {
            _menuNode = new MenuNode(nodeId, "");
            _menu.Nodes[nodeId] = _menuNode;
        }
        else
        {
            _menuNode = existingNode;
        }
    }

    /// <summary>
    /// Sets the message text for this node
    /// </summary>
    public NodeBuilder<TNode> Message(string message)
    {
        _messageLines.Add(message);
        UpdateTitle();
        return this;
    }

    /// <summary>
    /// Adds a line to the message
    /// </summary>
    public NodeBuilder<TNode> Line(string line)
    {
        _messageLines.Add(line);
        UpdateTitle();
        return this;
    }

    /// <summary>
    /// Marks this node as terminal (ends the session)
    /// </summary>
    public NodeBuilder<TNode> Terminal()
    {
        _menuNode.IsTerminal = true;
        return this;
    }

    /// <summary>
    /// Adds an option to this node
    /// </summary>
    public OptionBuilder<TNode> Option(string input, string label)
    {
        return new OptionBuilder<TNode>(this, input, label);
    }

    /// <summary>
    /// Adds a paginated list of items as options
    /// </summary>
    public NodeBuilder<TNode> OptionList<T>(
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
            // Mark this node for pagination
            _menuNode.IsPaginated = true;
            _menuNode.ItemsPerPage = itemsPerPage;
            
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
        _menuNode.Options.Add(option);
    }

    private void UpdateTitle()
    {
        _menuNode.Title = string.Join("\n", _messageLines);
    }

    internal NodeBuilder<TNode> GetThis() => this;
}

/// <summary>
/// Builder for configuring a menu option
/// </summary>
public class OptionBuilder<TNode> where TNode : struct, Enum
{
    private readonly NodeBuilder<TNode> _nodeBuilder;
    private readonly string _input;
    private readonly string _label;
    private readonly bool _isWildcard;
    private string? _targetStep;
    private string? _actionKey;

    internal OptionBuilder(NodeBuilder<TNode> nodeBuilder, string input, string label, bool isWildcard = false)
    {
        _nodeBuilder = nodeBuilder;
        _input = input;
        _label = label;
        _isWildcard = isWildcard;
    }

    /// <summary>
    /// Navigates to another node when this option is selected
    /// </summary>
    public NodeBuilder<TNode> GoTo(TNode targetNode)
    {
        _targetStep = targetNode.ToNodeId();
        Commit();
        return _nodeBuilder;
    }

    /// <summary>
    /// Executes an action handler when this option is selected
    /// </summary>
    public NodeBuilder<TNode> Action<THandler>() where THandler : IActionHandler
    {
        _actionKey = Attributes.UssdActionAttribute.GetActionKey(typeof(THandler));
        Commit();
        return _nodeBuilder;
    }

    /// <summary>
    /// Executes an action handler with a specific key
    /// </summary>
    public NodeBuilder<TNode> Action(string actionKey)
    {
        _actionKey = actionKey;
        Commit();
        return _nodeBuilder;
    }

    /// <summary>
    /// Navigates to a node and executes an action
    /// </summary>
    public NodeBuilder<TNode> GoToAndAction<THandler>(TNode targetNode) where THandler : IActionHandler
    {
        _targetStep = targetNode.ToNodeId();
        _actionKey = Attributes.UssdActionAttribute.GetActionKey(typeof(THandler));
        Commit();
        return _nodeBuilder;
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
        _nodeBuilder.AddOption(option);
    }
}

