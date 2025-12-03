using Crucible.Core.Interfaces;
using Crucible.Core.Types;

namespace Crucible.Core.Extensions;

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
    /// Gets the main axis before space (left for Row, top for Column).
    /// </summary>
    public static Units MainBefore<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.Left ?? Units.Auto,
            LayoutType.Column or LayoutType.Grid => node.Top ?? Units.Auto,
            _ => Units.Auto
        };
    }

    /// <summary>
    /// Gets the main axis after space (right for Row, bottom for Column).
    /// </summary>
    public static Units MainAfter<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.Right ?? Units.Auto,
            LayoutType.Column or LayoutType.Grid => node.Bottom ?? Units.Auto,
            _ => Units.Auto
        };
    }

    /// <summary>
    /// Gets the cross axis before space (top for Row, left for Column).
    /// </summary>
    public static Units CrossBefore<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.Top ?? Units.Auto,
            LayoutType.Column or LayoutType.Grid => node.Left ?? Units.Auto,
            _ => Units.Auto
        };
    }

    /// <summary>
    /// Gets the cross axis after space (bottom for Row, right for Column).
    /// </summary>
    public static Units CrossAfter<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.Bottom ?? Units.Auto,
            LayoutType.Column or LayoutType.Grid => node.Right ?? Units.Auto,
            _ => Units.Auto
        };
    }

    /// <summary>
    /// Gets the padding on the main axis before (left for Row, top for Column).
    /// </summary>
    public static Units PaddingMainBefore<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.PaddingLeft ?? default,
            LayoutType.Column or LayoutType.Grid => node.PaddingTop ?? default,
            _ => default
        };
    }

    /// <summary>
    /// Gets the padding on the main axis after (right for Row, bottom for Column).
    /// </summary>
    public static Units PaddingMainAfter<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.PaddingRight ?? default,
            LayoutType.Column or LayoutType.Grid => node.PaddingBottom ?? default,
            _ => default
        };
    }

    /// <summary>
    /// Gets the padding on the cross axis before (top for Row, left for Column).
    /// </summary>
    public static Units PaddingCrossBefore<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.PaddingTop ?? default,
            LayoutType.Column or LayoutType.Grid => node.PaddingLeft ?? default,
            _ => default
        };
    }

    /// <summary>
    /// Gets the padding on the cross axis after (bottom for Row, right for Column).
    /// </summary>
    public static Units PaddingCrossAfter<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.PaddingBottom ?? default,
            LayoutType.Column or LayoutType.Grid => node.PaddingRight ?? default,
            _ => default
        };
    }

    /// <summary>
    /// Gets the gap between children on the main axis.
    /// </summary>
    public static Units MainBetween<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.HorizontalGap ?? default,
            LayoutType.Column or LayoutType.Grid => node.VerticalGap ?? default,
            _ => default
        };
    }

    /// <summary>
    /// Gets the minimum gap between children on the main axis.
    /// </summary>
    public static Units MinMainBetween<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.MinHorizontalGap ?? default,
            LayoutType.Column or LayoutType.Grid => node.MinVerticalGap ?? default,
            _ => default
        };
    }

    /// <summary>
    /// Gets the maximum gap between children on the main axis.
    /// </summary>
    public static Units MaxMainBetween<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.MaxHorizontalGap ?? default,
            LayoutType.Column or LayoutType.Grid => node.MaxVerticalGap ?? default,
            _ => default
        };
    }

    /// <summary>
    /// Gets the gap between children on the cross axis.
    /// </summary>
    public static Units CrossBetween<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.VerticalGap ?? default,
            LayoutType.Column or LayoutType.Grid => node.HorizontalGap ?? default,
            _ => default
        };
    }

    /// <summary>
    /// Gets the border on the main axis before.
    /// </summary>
    public static Units BorderMainBefore<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.BorderLeft ?? default,
            LayoutType.Column or LayoutType.Grid => node.BorderTop ?? default,
            _ => default
        };
    }

    /// <summary>
    /// Gets the border on the main axis after.
    /// </summary>
    public static Units BorderMainAfter<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.BorderRight ?? default,
            LayoutType.Column or LayoutType.Grid => node.BorderBottom ?? default,
            _ => default
        };
    }

    /// <summary>
    /// Gets the border on the cross axis before.
    /// </summary>
    public static Units BorderCrossBefore<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.BorderTop ?? default,
            LayoutType.Column or LayoutType.Grid => node.BorderLeft ?? default,
            _ => default
        };
    }

    /// <summary>
    /// Gets the border on the cross axis after.
    /// </summary>
    public static Units BorderCrossAfter<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.BorderBottom ?? default,
            LayoutType.Column or LayoutType.Grid => node.BorderRight ?? default,
            _ => default
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

    /// <summary>
    /// Gets the scroll offset on the cross axis.
    /// </summary>
    public static float? CrossScroll<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.VerticalScroll,
            LayoutType.Column or LayoutType.Grid => node.HorizontalScroll,
            _ => null
        };
    }

    /// <summary>
    /// Gets the scroll offset on the main axis.
    /// </summary>
    public static float? MainScroll<TTree, TSubLayout, TCacheKey>(
        this INode<TTree, TSubLayout, TCacheKey> node,
        LayoutType parentLayoutType)
    {
        return parentLayoutType switch
        {
            LayoutType.Row => node.HorizontalScroll,
            LayoutType.Column or LayoutType.Grid => node.VerticalScroll,
            _ => null
        };
    }
}
