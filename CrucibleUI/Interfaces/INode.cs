using CrucibleUI.Types;

namespace CrucibleUI.Interfaces;

/// <summary>
/// A Node represents a layout element which can be sized and positioned based on a number of layout properties.
/// </summary>
/// <remarks>
/// The getter methods in this interface allow for the layout function to retrieve the layout properties of the node.
/// The node stores its reference to the property store internally.
/// The children of the node can be optionally stored externally using the Tree type.
/// </remarks>
/// <typeparam name="TTree">A type representing a tree structure where the children of the node can be stored.</typeparam>
/// <typeparam name="TSubLayout">A type representing a context for computing content size.</typeparam>
/// <typeparam name="TCacheKey">A type representing a key to store and retrieve values from the Cache.</typeparam>
public interface INode<in TTree, TSubLayout, out TCacheKey>
{
    /// <summary>
    /// Returns a key which can be used to set/get computed layout data from the cache.
    /// </summary>
    TCacheKey Key { get; }

    /// <summary>
    /// Returns an enumerable of the children of the node.
    /// </summary>
    IEnumerable<INode<TTree, TSubLayout, TCacheKey>> Children(TTree tree);

    /// <summary>
    /// Returns a boolean representing whether the node is visible to layout.
    /// </summary>
    bool Visible { get; }

    /// <summary>
    /// Returns the layout type of the node.
    /// </summary>
    LayoutType? LayoutType { get; }

    /// <summary>
    /// Returns the position type of the node.
    /// </summary>
    PositionType? PositionType { get; }

    /// <summary>
    /// Returns the alignment of the node.
    /// </summary>
    Alignment? Alignment { get; }

    /// <summary>
    /// Returns the desired width of the node.
    /// </summary>
    Units? Width { get; }

    /// <summary>
    /// Returns the desired height of the node.
    /// </summary>
    Units? Height { get; }

    /// <summary>
    /// Returns the desired left-side space of the node.
    /// </summary>
    Units? Left { get; }

    /// <summary>
    /// Returns the desired right-side space of the node.
    /// </summary>
    Units? Right { get; }

    /// <summary>
    /// Returns the desired top-side space of the node.
    /// </summary>
    Units? Top { get; }

    /// <summary>
    /// Returns the desired bottom-side space of the node.
    /// </summary>
    Units? Bottom { get; }

    /// <summary>
    /// Returns the width and height of the node if its desired width and/or desired height are auto and the node has no children.
    /// This can be used to size the node based on visual content (such as text), or to apply an aspect ratio size constraint.
    /// </summary>
    (float Width, float Height)? ContentSize(ref TSubLayout subLayout, float? parentWidth, float? parentHeight);

    /// <summary>
    /// Returns the desired left-side child-space (padding) of the node.
    /// </summary>
    Units? PaddingLeft { get; }

    /// <summary>
    /// Returns the desired right-side child-space (padding) of the node.
    /// </summary>
    Units? PaddingRight { get; }

    /// <summary>
    /// Returns the desired top-side child-space (padding) of the node.
    /// </summary>
    Units? PaddingTop { get; }

    /// <summary>
    /// Returns the desired bottom-side child-space (padding) of the node.
    /// </summary>
    Units? PaddingBottom { get; }

    /// <summary>
    /// Returns the desired space to be applied between the children of the node on the vertical axis.
    /// </summary>
    Units? VerticalGap { get; }

    /// <summary>
    /// Returns the desired space to be applied between the children of the node on the horizontal axis.
    /// </summary>
    Units? HorizontalGap { get; }

    /// <summary>
    /// Returns the desired minimum space to be applied between the children of the node on the vertical axis.
    /// </summary>
    Units? MinVerticalGap { get; }

    /// <summary>
    /// Returns the desired minimum space to be applied between the children of the node on the horizontal axis.
    /// </summary>
    Units? MinHorizontalGap { get; }

    /// <summary>
    /// Returns the desired maximum space to be applied between the children of the node on the vertical axis.
    /// </summary>
    Units? MaxVerticalGap { get; }

    /// <summary>
    /// Returns the desired maximum space to be applied between the children of the node on the horizontal axis.
    /// </summary>
    Units? MaxHorizontalGap { get; }

    /// <summary>
    /// Returns the minimum width of the node.
    /// </summary>
    Units? MinWidth { get; }

    /// <summary>
    /// Returns the minimum height of the node.
    /// </summary>
    Units? MinHeight { get; }

    /// <summary>
    /// Returns the maximum width of the node.
    /// </summary>
    Units? MaxWidth { get; }

    /// <summary>
    /// Returns the maximum height of the node.
    /// </summary>
    Units? MaxHeight { get; }

    /// <summary>
    /// Returns the left-side border width of the node.
    /// </summary>
    Units? BorderLeft { get; }

    /// <summary>
    /// Returns the right-side border width of the node.
    /// </summary>
    Units? BorderRight { get; }

    /// <summary>
    /// Returns the top-side border width of the node.
    /// </summary>
    Units? BorderTop { get; }

    /// <summary>
    /// Returns the bottom-side border width of the node.
    /// </summary>
    Units? BorderBottom { get; }

    /// <summary>
    /// Returns the vertical scroll offset of the node.
    /// </summary>
    float? VerticalScroll { get; }

    /// <summary>
    /// Returns the horizontal scroll offset of the node.
    /// </summary>
    float? HorizontalScroll { get; }

    /// <summary>
    /// Returns the grid column definitions.
    /// </summary>
    IReadOnlyList<Units>? GridColumns { get; }

    /// <summary>
    /// Returns the grid row definitions.
    /// </summary>
    IReadOnlyList<Units>? GridRows { get; }

    /// <summary>
    /// Returns the column start index for grid children.
    /// </summary>
    int? ColumnStart { get; }

    /// <summary>
    /// Returns the row start index for grid children.
    /// </summary>
    int? RowStart { get; }

    /// <summary>
    /// Returns the column span for grid children.
    /// </summary>
    int? ColumnSpan { get; }

    /// <summary>
    /// Returns the row span for grid children.
    /// </summary>
    int? RowSpan { get; }
}
