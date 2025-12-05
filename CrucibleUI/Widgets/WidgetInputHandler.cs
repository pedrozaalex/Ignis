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

    /// <summary>
    /// Triggers the submit action on the currently focused widget.
    /// </summary>
    public void HandleSubmit()
    {
        _focusedWidget?.HandleSubmit();
    }

    /// <summary>
    /// Navigates focus in the specified direction.
    /// </summary>
    public void HandleNavigation(float dx, float dy)
    {
        if (_focusedWidget == null)
        {
            // If nothing focused, focus the first focusable widget
            var first = FindFirstFocusable(_root);
            if (first != null)
            {
                _focusedWidget = first;
                _focusedWidget.SetFocused(true);
            }
            return;
        }

        var allFocusable = new List<Widget>();
        CollectFocusable(_root, allFocusable);

        var best = FindBestCandidate(_focusedWidget, allFocusable, dx, dy);
        if (best != null)
        {
            _focusedWidget.SetFocused(false);
            _focusedWidget = best;
            _focusedWidget.SetFocused(true);
        }
    }

    private void CollectFocusable(Widget root, List<Widget> list)
    {
        if (root.IsFocusable && root.IsVisible && !root.IsDisabled)
        {
            list.Add(root);
        }
        foreach (var child in root.ChildWidgets)
        {
            CollectFocusable(child, list);
        }
    }

    private Widget? FindFirstFocusable(Widget root)
    {
        if (root.IsFocusable && root.IsVisible && !root.IsDisabled) return root;
        foreach (var child in root.ChildWidgets)
        {
            var found = FindFirstFocusable(child);
            if (found != null) return found;
        }
        return null;
    }

    private Widget? FindBestCandidate(Widget current, List<Widget> candidates, float dx, float dy)
    {
        Widget? best = null;
        float bestDist = float.MaxValue;

        var cx = current.ComputedX + current.ComputedWidth / 2;
        var cy = current.ComputedY + current.ComputedHeight / 2;

        foreach (var candidate in candidates)
        {
            if (candidate == current) continue;

            var tx = candidate.ComputedX + candidate.ComputedWidth / 2;
            var ty = candidate.ComputedY + candidate.ComputedHeight / 2;

            var vx = tx - cx;
            var vy = ty - cy;

            bool valid = false;
            if (Math.Abs(dx) > Math.Abs(dy)) // Horizontal
            {
                if (dx > 0) valid = vx > 0; // Right
                else valid = vx < 0; // Left
            }
            else // Vertical
            {
                if (dy > 0) valid = vy > 0; // Down
                else valid = vy < 0; // Up
            }

            if (valid)
            {
                var dist = vx * vx + vy * vy;
                // Weight distance to prefer aligned items
                if (Math.Abs(dx) > Math.Abs(dy)) // Horizontal movement
                {
                    dist += Math.Abs(vy) * Math.Abs(vy) * 10; // Penalize vertical offset
                }
                else
                {
                    dist += Math.Abs(vx) * Math.Abs(vx) * 10; // Penalize horizontal offset
                }

                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = candidate;
                }
            }
        }
        return best;
    }
}
