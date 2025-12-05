using CrucibleUI.Types;

namespace CrucibleUI.Widgets;

/// <summary>
/// Base class for all UI widgets. Provides layout properties, styling, input state, and hit testing.
/// </summary>
public abstract class Widget
{
    // Layout properties
    public Units? WidthValue { get; protected set; }
    public Units? HeightValue { get; protected set; }
    public Units? PaddingLeftValue { get; protected set; }
    public Units? PaddingRightValue { get; protected set; }
    public Units? PaddingTopValue { get; protected set; }
    public Units? PaddingBottomValue { get; protected set; }
    public Units? MinWidthValue { get; protected set; }
    public Units? MaxWidthValue { get; protected set; }
    public Units? MinHeightValue { get; protected set; }
    public Units? MaxHeightValue { get; protected set; }
    public Units? VerticalGapValue { get; protected set; }
    public Units? HorizontalGapValue { get; protected set; }
    public LayoutType? LayoutTypeValue { get; protected set; }
    public Alignment? AlignmentValue { get; protected set; }

    // Style properties
    public WidgetColor BackgroundColor { get; protected set; } = WidgetColor.Transparent;
    public WidgetColor BorderColorValue { get; protected set; } = WidgetColor.Transparent;
    public float CornerRadiusValue { get; protected set; }

    // Input state
    public bool IsHovered { get; private set; }
    public bool IsPressed { get; private set; }
    public bool IsFocused { get; private set; }
    public bool IsFocusable { get; protected set; }
    public bool IsDisabled { get; private set; }
    public bool IsVisible { get; private set; } = true;

    // Events
    public event Action<Widget>? OnFocus;
    public event Action<Widget>? OnBlur;
    public event Action<Widget>? OnSubmit;

    // Computed bounds (set by layout pass)
    public float ComputedX { get; private set; }
    public float ComputedY { get; private set; }
    public float ComputedWidth { get; private set; }
    public float ComputedHeight { get; private set; }

    // Parent-relative bounds (used during layout)
    private float _boundsX, _boundsY, _boundsW, _boundsH;

    // Children
    private readonly List<Widget> _children = new();
    public IReadOnlyList<Widget> ChildWidgets => _children;

    // Parent reference
    public Widget? Parent { get; private set; }

    // --- Fluent Builders ---

    public T Width<T>(Units value) where T : Widget
    {
        WidthValue = value;
        return (T)this;
    }

    public T Height<T>(Units value) where T : Widget
    {
        HeightValue = value;
        return (T)this;
    }

    public T Padding<T>(Units value) where T : Widget
    {
        PaddingLeftValue = value;
        PaddingRightValue = value;
        PaddingTopValue = value;
        PaddingBottomValue = value;
        return (T)this;
    }

    public T PaddingHorizontal<T>(Units value) where T : Widget
    {
        PaddingLeftValue = value;
        PaddingRightValue = value;
        return (T)this;
    }

    public T PaddingVertical<T>(Units value) where T : Widget
    {
        PaddingTopValue = value;
        PaddingBottomValue = value;
        return (T)this;
    }

    public T Gap<T>(Units value) where T : Widget
    {
        VerticalGapValue = value;
        HorizontalGapValue = value;
        return (T)this;
    }

    public T Row<T>() where T : Widget
    {
        LayoutTypeValue = LayoutType.Row;
        return (T)this;
    }

    public T Column<T>() where T : Widget
    {
        LayoutTypeValue = LayoutType.Column;
        return (T)this;
    }

    public T Stretch<T>() where T : Widget
    {
        WidthValue = Units.Stretch(1);
        HeightValue = Units.Stretch(1);
        return (T)this;
    }

    public T Alignment<T>(Alignment value) where T : Widget
    {
        AlignmentValue = value;
        return (T)this;
    }

    public T Background<T>(float r, float g, float b, float a = 1f) where T : Widget
    {
        BackgroundColor = new WidgetColor(r, g, b, a);
        return (T)this;
    }

    public T BorderColor<T>(float r, float g, float b, float a = 1f) where T : Widget
    {
        BorderColorValue = new WidgetColor(r, g, b, a);
        return (T)this;
    }

    public T CornerRadius<T>(float radius) where T : Widget
    {
        CornerRadiusValue = radius;
        return (T)this;
    }

    public T Visible<T>(bool visible) where T : Widget
    {
        IsVisible = visible;
        return (T)this;
    }

    public T Disabled<T>(bool disabled) where T : Widget
    {
        IsDisabled = disabled;
        return (T)this;
    }

    public T Focusable<T>(bool focusable = true) where T : Widget
    {
        IsFocusable = focusable;
        return (T)this;
    }

    public T Children<T>(params Widget[] children) where T : Widget
    {
        foreach (var child in children)
        {
            child.Parent = this;
            _children.Add(child);
        }
        return (T)this;
    }

    // --- Input State ---

    public void SetHovered(bool hovered) => IsHovered = hovered;
    public void SetPressed(bool pressed) => IsPressed = pressed;
    public void SetFocused(bool focused)
    {
        if (IsFocused == focused) return;
        IsFocused = focused;
        if (focused) OnFocus?.Invoke(this);
        else OnBlur?.Invoke(this);
    }

    public void TriggerSubmit() => OnSubmit?.Invoke(this);

    // --- Layout ---

    /// <summary>
    /// Sets computed bounds for this widget (used during layout pass).
    /// </summary>
    public void ComputeBounds(float x, float y, float width, float height)
    {
        _boundsX = x;
        _boundsY = y;
        _boundsW = width;
        _boundsH = height;

        // Compute absolute position from parent chain
        ComputeAbsolutePosition();
    }

    private void ComputeAbsolutePosition()
    {
        ComputedWidth = _boundsW;
        ComputedHeight = _boundsH;

        if (Parent != null)
        {
            ComputedX = Parent.ComputedX + _boundsX;
            ComputedY = Parent.ComputedY + _boundsY;
        }
        else
        {
            ComputedX = _boundsX;
            ComputedY = _boundsY;
        }
    }

    /// <summary>
    /// Computes layout for this widget and all children using the CrucibleUI layout engine.
    /// </summary>
    public void ComputeLayout()
    {
        // Create a WidgetNode wrapper and run the layout engine
        var node = new WidgetNode(this);
        var cache = new WidgetCache();
        var subLayout = new WidgetSubLayout();

        // Pass current computed size as the constraint for the root
        float? w = ComputedWidth > 0 ? ComputedWidth : null;
        float? h = ComputedHeight > 0 ? ComputedHeight : null;

        LayoutEngine.Compute<WidgetNode, Widget, WidgetSubLayout, Widget, WidgetCache>(
            node, cache, this, ref subLayout, w, h);
    }

    /// <summary>
    /// Converts this widget tree to a flat list of layout nodes.
    /// </summary>
    public List<Widget> ToLayoutNodes()
    {
        var result = new List<Widget>();
        CollectNodes(result);
        return result;
    }

    private void CollectNodes(List<Widget> list)
    {
        list.Add(this);
        foreach (var child in _children)
        {
            child.CollectNodes(list);
        }
    }

    // --- Hit Testing ---

    /// <summary>
    /// Finds the deepest widget at the given screen coordinates.
    /// </summary>
    public Widget? HitTest(float x, float y)
    {
        if (!IsVisible) return null;

        // Check if point is inside this widget's bounds
        if (x < ComputedX || x > ComputedX + ComputedWidth ||
            y < ComputedY || y > ComputedY + ComputedHeight)
        {
            return null;
        }

        // Check children in reverse order (last drawn = on top)
        for (var i = _children.Count - 1; i >= 0; i--)
        {
            var childHit = _children[i].HitTest(x, y);
            if (childHit != null)
                return childHit;
        }

        return this;
    }

    // --- Input Handling (virtual for subclasses) ---

    public virtual void HandleMouseDown(float x, float y) { }
    public virtual void HandleMouseUp(float x, float y) { }
    public virtual void HandleMouseMove(float x, float y) { }
    public virtual void HandleSubmit() { TriggerSubmit(); }

    // --- Content Size (virtual for widgets that need to measure content like Label) ---

    /// <summary>
    /// Returns the intrinsic content size for this widget (e.g., text dimensions for Label).
    /// </summary>
    public virtual (float Width, float Height)? GetContentSize(float? parentWidth, float? parentHeight)
    {
        return null;
    }
}
