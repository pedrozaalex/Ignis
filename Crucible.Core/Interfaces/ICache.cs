using Crucible.Core.Types;

namespace Crucible.Core.Interfaces;

/// <summary>
/// The Cache is a store which contains the computed size and position of nodes after a layout calculation.
/// </summary>
/// <typeparam name="TNode">A type which represents a layout node.</typeparam>
public interface ICache<in TNode> where TNode : notnull
{
    /// <summary>
    /// Returns the cached width of the given node.
    /// </summary>
    float Width(TNode node);
    
    /// <summary>
    /// Returns the cached height of the given node.
    /// </summary>
    float Height(TNode node);
    
    /// <summary>
    /// Returns the cached horizontal position of the given node.
    /// </summary>
    float PosX(TNode node);
    
    /// <summary>
    /// Returns the cached vertical position of the given node.
    /// </summary>
    float PosY(TNode node);

    /// <summary>
    /// Sets the cached position and size of the given node.
    /// </summary>
    void SetBounds(TNode node, float posX, float posY, float width, float height);

    /// <summary>
    /// Returns the cached bounds of the given node.
    /// </summary>
    Rect? Bounds(TNode node);
}

/// <summary>
/// Extension methods for ICache providing direction-agnostic operations.
/// </summary>
public static class CacheExtensions
{
    /// <summary>
    /// Sets the cached rectangle using main/cross axis values based on parent layout type.
    /// </summary>
    public static void SetRect<TNode>(
        this ICache<TNode> cache,
        TNode node,
        LayoutType parentLayoutType,
        float mainPos,
        float crossPos,
        float main,
        float cross) where TNode : notnull
    {
        switch (parentLayoutType)
        {
            case LayoutType.Row:
                cache.SetBounds(node, mainPos, crossPos, main, cross);
                break;
            case LayoutType.Column:
                cache.SetBounds(node, crossPos, mainPos, cross, main);
                break;
            // Grid doesn't use SetRect
        }
    }
}
