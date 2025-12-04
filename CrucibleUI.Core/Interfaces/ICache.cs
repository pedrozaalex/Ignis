using CrucibleUI.Core.Types;

namespace CrucibleUI.Core.Interfaces;

/// <summary>
/// The Cache is a store which contains the computed size and position of nodes after a layout calculation.
/// </summary>
/// <typeparam name="TNode">A type which represents a layout node.</typeparam>
public interface ICache<in TNode> where TNode : notnull
{
    /// <summary>
    /// Sets the cached position and size of the given node.
    /// </summary>
    void SetBounds(TNode node, float posX, float posY, float width, float height);

    /// <summary>
    /// Returns the cached bounds of the given node.
    /// </summary>
    Rect? Bounds(TNode node);
}


