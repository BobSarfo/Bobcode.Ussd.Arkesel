namespace OpenUSSD.models;

/// <summary>
/// Interface for menu node identifiers.
/// Implement this with an enum to create strongly-typed menu nodes.
/// </summary>
public interface IMenuNode
{
    /// <summary>
    /// Gets the string representation of the menu node
    /// </summary>
    string ToNodeId();
}

