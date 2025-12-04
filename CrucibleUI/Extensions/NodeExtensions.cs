using CrucibleUI.Interfaces;
using CrucibleUI.Types;

namespace CrucibleUI.Extensions;

/// <summary>
/// Extension methods for INode providing direction-agnostic layout property access.
/// </summary>
public static class NodeExtensions
{
    /// <summary>
    /// Gets the main axis size based on the parent layout type.
    /// </summary>
    public static Units Main<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row or LayoutType.Grid => node.Width ?? Units.Stretch(1.0f),
            LayoutType.Column => node.Height ?? Units.Stretch(1.0f),
            _ => Units.Stretch(1.0f)
        };
    }

    /// <summary>
    /// Gets the minimum main axis size based on the parent layout type.
    /// </summary>
    public static Units MinMain<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.MinWidth ?? Units.Pixels(0.0f),
            LayoutType.Column or LayoutType.Grid => node.MinHeight ?? Units.Pixels(0.0f),
            _ => Units.Pixels(0.0f)
        };
    }

    /// <summary>
    /// Gets the maximum main axis size based on the parent layout type.
    /// </summary>
    public static Units MaxMain<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.MaxWidth ?? Units.Pixels(float.MaxValue),
            LayoutType.Column or LayoutType.Grid => node.MaxHeight ?? Units.Pixels(float.MaxValue),
            _ => Units.Pixels(float.MaxValue)
        };
    }

    /// <summary>
    /// Gets the cross axis size based on the parent layout type.
    /// </summary>
    public static Units Cross<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row or LayoutType.Grid => node.Height ?? Units.Stretch(1.0f),
            LayoutType.Column => node.Width ?? Units.Stretch(1.0f),
            _ => Units.Stretch(1.0f)
        };
    }

    /// <summary>
    /// Gets the minimum cross axis size based on the parent layout type.
    /// </summary>
    public static Units MinCross<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.MinHeight ?? Units.Pixels(0.0f),
            LayoutType.Column or LayoutType.Grid => node.MinWidth ?? Units.Pixels(0.0f),
            _ => Units.Pixels(0.0f)
        };
    }

    /// <summary>
    /// Gets the maximum cross axis size based on the parent layout type.
    /// </summary>
    public static Units MaxCross<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.MaxHeight ?? Units.Pixels(float.MaxValue),
            LayoutType.Column or LayoutType.Grid => node.MaxWidth ?? Units.Pixels(float.MaxValue),
            _ => Units.Pixels(float.MaxValue)
        };
    }

    /// <summary>
    /// Gets the content size in direction-agnostic terms.
    /// </summary>
    public static (float Main, float Cross)? ContentSizing<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        ref TSubLayout subLayout,
        LayoutType parentLayoutType,
        float? parentMain,
        float? parentCross)
    {
        var result = parentLayoutType switch
        {
            LayoutType.Row or LayoutType.Grid => node.ContentSize(ref subLayout, parentMain, parentCross),
            LayoutType.Column => node.ContentSize(ref subLayout, parentCross, parentMain),
            _ => node.ContentSize(ref subLayout, parentMain, parentCross)
        };

        if (result is null) return null;

        return parentLayoutType switch
        {
            LayoutType.Row or LayoutType.Grid => (result.Value.Width, result.Value.Height),
            LayoutType.Column => (result.Value.Height, result.Value.Width),
            _ => null
        };
    }
}
