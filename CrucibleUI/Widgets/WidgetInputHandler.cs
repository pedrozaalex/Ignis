namespace CrucibleUI.Widgets;

/// <summary>
/// Handles input events and dispatches them to the appropriate widgets in a widget tree.
/// </summary>
public class WidgetInputHandler
{
    private readonly Widget _root;
    private Widget? _hoveredWidget;
    private Widget? _pressedWidget;
    private Widget? _focusedWidget;
    private Widget? _draggedWidget;

    public WidgetInputHandler(Widget root)
    {
        _root = root;
    }

    /// <summary>
    /// Call when mouse position changes.
    /// </summary>
    public void HandleMouseMove(float x, float y)
    {
        // Update hover state
        var newHovered = _root.HitTest(x, y);

        if (newHovered != _hoveredWidget)
        {
            _hoveredWidget?.SetHovered(false);
            newHovered?.SetHovered(true);
            _hoveredWidget = newHovered;
        }

        // Forward to dragged widget
        if (_draggedWidget != null)
        {
            _draggedWidget.HandleMouseMove(x, y);
        }
    }

    /// <summary>
    /// Call when mouse button is pressed.
    /// </summary>
    public void HandleMouseDown(float x, float y)
    {
        var widget = _root.HitTest(x, y);
        if (widget != null)
        {
            _pressedWidget = widget;
            _draggedWidget = widget;
            widget.SetPressed(true);
            widget.HandleMouseDown(x, y);

            // Update focus
            if (_focusedWidget != widget)
            {
                _focusedWidget?.SetFocused(false);
                widget.SetFocused(true);
                _focusedWidget = widget;
            }
        }
    }

    /// <summary>
    /// Call when mouse button is released.
    /// </summary>
    public void HandleMouseUp(float x, float y)
    {
        if (_pressedWidget != null)
        {
            _pressedWidget.SetPressed(false);
            _pressedWidget.HandleMouseUp(x, y);
            _pressedWidget = null;
        }

        _draggedWidget = null;
    }

    /// <summary>
    /// Gets the currently hovered widget.
    /// </summary>
    public Widget? HoveredWidget => _hoveredWidget;

    /// <summary>
    /// Gets the currently focused widget.
    /// </summary>
    public Widget? FocusedWidget => _focusedWidget;
}
