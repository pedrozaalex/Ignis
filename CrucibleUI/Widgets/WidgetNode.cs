using CrucibleUI.Interfaces;
using CrucibleUI.Types;

namespace CrucibleUI.Widgets;

/// <summary>
/// Adapts a Widget to the INode interface for layout computation.
/// </summary>
public readonly struct WidgetNode : INode<Widget, WidgetSubLayout, Widget>
{
    private readonly Widget _widget;

    public WidgetNode(Widget widget)
    {
        _widget = widget;
    }

    public Widget Key => _widget;

    public IEnumerable<INode<Widget, WidgetSubLayout, Widget>> Children(Widget tree)
    {
        foreach (var child in _widget.ChildWidgets)
        {
            yield return new WidgetNode(child);
        }
    }

    public bool Visible => _widget.IsVisible;

    public LayoutType? LayoutType => _widget.LayoutTypeValue;
    public PositionType? PositionType => null;
    public Alignment? Alignment => _widget.AlignmentValue;

    public Units? Width => _widget.WidthValue;
    public Units? Height => _widget.HeightValue;

    public Units? Left => null;
    public Units? Right => null;
    public Units? Top => null;
    public Units? Bottom => null;

    public Units? PaddingLeft => _widget.PaddingLeftValue;
    public Units? PaddingRight => _widget.PaddingRightValue;
    public Units? PaddingTop => _widget.PaddingTopValue;
    public Units? PaddingBottom => _widget.PaddingBottomValue;

    public Units? VerticalGap => _widget.VerticalGapValue;
    public Units? HorizontalGap => _widget.HorizontalGapValue;
    public Units? MinVerticalGap => null;
    public Units? MinHorizontalGap => null;
    public Units? MaxVerticalGap => null;
    public Units? MaxHorizontalGap => null;

    public Units? MinWidth => _widget.MinWidthValue;
    public Units? MinHeight => _widget.MinHeightValue;
    public Units? MaxWidth => _widget.MaxWidthValue;
    public Units? MaxHeight => _widget.MaxHeightValue;

    public Units? BorderLeft => null;
    public Units? BorderRight => null;
    public Units? BorderTop => null;
    public Units? BorderBottom => null;

    public float? VerticalScroll => null;
    public float? HorizontalScroll => null;

    public IReadOnlyList<Units>? GridColumns => null;
    public IReadOnlyList<Units>? GridRows => null;

    public int? ColumnStart => null;
    public int? RowStart => null;
    public int? ColumnSpan => null;
    public int? RowSpan => null;

    public (float Width, float Height)? ContentSize(ref WidgetSubLayout subLayout, float? parentWidth, float? parentHeight)
    {
        // Allow widgets to provide their own content size (e.g., for Label text measurement)
        return _widget.GetContentSize(parentWidth, parentHeight);
    }
}

/// <summary>
/// Context for widget content size calculations.
/// </summary>
public struct WidgetSubLayout
{
    // Can be extended with text measurement, font info, etc.
}
