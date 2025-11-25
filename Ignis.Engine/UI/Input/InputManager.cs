using Ignis.Engine.Input;
using Ignis.Engine.Reactive;
using Ignis.Engine.UI.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TextInputEventArgs = Ignis.Engine.Input.TextInputEventArgs;

namespace Ignis.Engine.UI.Input;

/// <summary>
///     Manages input state and dispatches events to the UI tree.
///     Handles focus, hover, and event bubbling.
/// </summary>
public class InputManager
{
    private const float DragThreshold = 5f;
    private readonly Dictionary<long, Rectangle> _bounds;
    private readonly IInputProvider _input;
    private object? _dragPayload;
    private Vector2 _dragStartPosition;

    // Drag state
    private bool _isDragging;

    private IView? _root;

    public InputManager(Dictionary<long, Rectangle> bounds, IInputProvider input)
    {
        _bounds = bounds;
        _input = input;

        // Subscribe to text input events
        _input.TextInput += OnTextInput;
    }

    public Signal<long?> FocusedElementId { get; } = new(null);

    public Signal<long?> HoveredElementId { get; } = new(null);

    public Signal<long?> ActiveElementId { get; } = new(null);

    private void OnTextInput(object? sender, TextInputEventArgs e)
    {
        // Forward text input to focused element AND bubble up
        if (FocusedElementId.Value.HasValue && _root != null)
        {
            var focusedView = FindViewById(FocusedElementId.Value.Value, _root);

            // Bubble text input event up the tree
            var current = focusedView;
            while (current != null)
            {
                if (current is ViewComponent component) component.EventHandlers.InvokeTextInput(e.Character);
                current = FindParent(current, _root);
            }
        }
    }

    public void SetRoot(IView root)
    {
        _root = root;
    }

    /// <summary>
    ///     Process input for the current frame.
    /// </summary>
    public void Update()
    {
        if (_root == null) return;

        ProcessMouseInput();
        ProcessKeyboardInput();
    }

    private void ProcessMouseInput()
    {
        var mousePos = _input.MousePosition;

        // Find the topmost view under the cursor (depth-first search, reverse order for Z-index)
        var hitView = FindViewAt(mousePos, _root!);

        // Update hover state
        var previousHovered = HoveredElementId.Value;
        var currentHovered = hitView?.Layout.ElementId;

        if (previousHovered != currentHovered)
        {
            // Fire leave event on previous
            if (previousHovered.HasValue)
            {
                var prevView = FindViewById(previousHovered.Value, _root!);
                if (prevView is ViewComponent prevComponent)
                {
                    var leaveEvt = new PointerEvent(mousePos, 0, PointerType.Mouse, PointerEventType.Leave);
                    prevComponent.EventHandlers.InvokePointerLeave(leaveEvt);
                }
            }

            // Fire enter event on current
            if (currentHovered.HasValue && hitView is ViewComponent hitComponent)
            {
                var enterEvt = new PointerEvent(mousePos, 0, PointerType.Mouse, PointerEventType.Enter);
                hitComponent.EventHandlers.InvokePointerEnter(enterEvt);
            }

            HoveredElementId.Value = currentHovered;
        }

        // Mouse button events
        if (_input.IsMouseButtonJustPressed(0))
            HandleMouseDown(mousePos, 0, hitView);
        else if (_input.IsMouseButtonReleased(0)) HandleMouseUp(mousePos, 0, hitView);

        // Mouse move
        HandleMouseMove(mousePos, hitView);
    }

    private void HandleMouseDown(Vector2 position, int button, IView? hitView)
    {
        var focusHandled = false;

        // 1. Handle Active State & Events (PointerDown bubbles from the HIT element)
        if (hitView is ViewComponent component)
        {
            // Set active element (pressed/dragging state)
            ActiveElementId.Value = component.Layout.ElementId;

            // Store drag start position
            _dragStartPosition = position;

            // Fire pointer down event with bubbling
            var evt = new PointerEvent(position, button, PointerType.Mouse, PointerEventType.Down);
            BubbleEvent(component, view =>
            {
                if (view is ViewComponent vc) vc.EventHandlers.InvokePointerDown(evt);
            }, evt);
        }

        // 2. Handle Focus - Walk up the tree to find the nearest focusable ancestor
        // This ensures clicking a label inside a button/input focuses the container
        var currentFocusCandidate = hitView;
        while (currentFocusCandidate != null)
        {
            if (currentFocusCandidate.Layout.Focusable)
            {
                FocusedElementId.Value = currentFocusCandidate.Layout.ElementId;
                focusHandled = true;
                break;
            }

            currentFocusCandidate = FindParent(currentFocusCandidate, _root!);
        }

        // 3. Clear focus if we clicked something that has no focusable ancestor
        if (!focusHandled) FocusedElementId.Value = null;
    }

    private void HandleMouseUp(Vector2 position, int button, IView? hitView)
    {
        if (_isDragging)
        {
            // End drag
            if (hitView is ViewComponent dropTarget)
            {
                var dropEvt = new DragEvent(position, _dragPayload, DragEventType.Drop);
                BubbleEvent(dropTarget, view =>
                {
                    if (view is ViewComponent vc) vc.EventHandlers.InvokeDrop(dropEvt);
                }, dropEvt);
            }

            _isDragging = false;
            _dragPayload = null;
        }
        else if (hitView is ViewComponent component)
        {
            var evt = new PointerEvent(position, button, PointerType.Mouse, PointerEventType.Up);
            BubbleEvent(component, view =>
            {
                if (view is ViewComponent vc) vc.EventHandlers.InvokePointerUp(evt);
            }, evt);
        }

        // Clear active element after processing mouse up
        ActiveElementId.Value = null;
    }

    private void HandleMouseMove(Vector2 position, IView? hitView)
    {
        // Check for drag threshold
        if (!_isDragging && _input.IsMouseButtonPressed(0))
        {
            var distance = Vector2.Distance(position, _dragStartPosition);
            if (distance > DragThreshold)
            {
                // Prioritize active element as source
                IView? sourceView = null;
                if (ActiveElementId.Value.HasValue && _root != null)
                    sourceView = FindViewById(ActiveElementId.Value.Value, _root);

                if (sourceView == null) sourceView = hitView;

                if (sourceView is ViewComponent sourceComponent)
                    // FIX: Only enter drag mode if the component actually handles DragStart
                    // This prevents Sliders from triggering DnD state
                    if (sourceComponent.EventHandlers.OnDragStart != null)
                    {
                        _isDragging = true;
                        var dragEvt = new DragEvent(position, null, DragEventType.Start);
                        sourceComponent.EventHandlers.InvokeDragStart(dragEvt);
                        _dragPayload = dragEvt.Payload;
                    }
            }
        }

        if (_isDragging && hitView is ViewComponent dropTarget)
        {
            var dragOverEvt = new DragEvent(position, _dragPayload, DragEventType.Over);
            dropTarget.EventHandlers.InvokeDragOver(dragOverEvt);
        }
        else
        {
            // FIX: Mouse Capture Logic
            // If we have an active element, it receives moves regardless of mouse position
            if (ActiveElementId.Value.HasValue && _root != null)
            {
                var activeView = FindViewById(ActiveElementId.Value.Value, _root);
                if (activeView is ViewComponent activeComponent)
                {
                    var evt = new PointerEvent(position, 0, PointerType.Mouse, PointerEventType.Move);
                    activeComponent.EventHandlers.InvokePointerMove(evt);
                }
            }
            // Otherwise, send to view under cursor
            else if (hitView is ViewComponent component)
            {
                var evt = new PointerEvent(position, 0, PointerType.Mouse, PointerEventType.Move);
                component.EventHandlers.InvokePointerMove(evt);
            }
        }
    }

    private void ProcessKeyboardInput()
    {
        // Get pressed keys from input service
        var pressedKeys = (_input as InputService)?.GetPressedKeys() ?? Array.Empty<Keys>();
        var previousKeys = (_input as InputService)?.GetPreviousPressedKeys() ?? Array.Empty<Keys>();

        // Detect newly pressed keys
        var newKeys = pressedKeys.Except(previousKeys).ToList();

        foreach (var key in newKeys)
        {
            var modifiers = _input.GetModifiers();
            var evt = new KeyboardEvent(key, modifiers, KeyboardEventType.Down);

            // Try shortcuts first on focused element, then bubble
            if (!FocusedElementId.Value.HasValue) continue;

            var focusedView = FindViewById(FocusedElementId.Value.Value, _root!);

            if (focusedView == null) continue;

            // Try to handle with shortcuts, bubbling up
            BubbleShortcut(focusedView, key, modifiers, out var handled);

            if (!handled)
                // Fire keyboard event
                BubbleEvent(focusedView, view =>
                {
                    if (view is ViewComponent vc) vc.EventHandlers.InvokeKeyDown(evt);
                }, evt);
        }
    }


    /// <summary>
    ///     Bubbles an event up the tree until handled.
    /// </summary>
    private void BubbleEvent(IView start, Action<IView> invoker, UIEvent evt)
    {
        var current = start;
        while (current != null && !evt.Handled)
        {
            invoker(current);
            current = FindParent(current, _root!);
        }
    }

    /// <summary>
    ///     Bubbles a shortcut up the tree until handled.
    /// </summary>
    private void BubbleShortcut(IView start, Keys key, ModifierKeys modifiers, out bool handled)
    {
        handled = false;
        var current = start;

        while (current != null && !handled)
        {
            if (current is ViewComponent component) handled = component.Shortcuts.TryHandle(key, modifiers);

            if (!handled) current = FindParent(current, _root!);
        }
    }

    /// <summary>
    ///     Finds the view at the given position (depth-first, returns deepest match).
    /// </summary>
    private IView? FindViewAt(Vector2 position, IView root)
    {
        if (!_bounds.TryGetValue(root.Layout.ElementId, out var bounds)) return null;

        var contains = bounds.Contains(position);

        if (!contains)
            return null;

        // Check children first (depth-first)
        if (root is not IViewContainer container) return root;

        // Reverse order to respect Z-index (last child is on top)
        foreach (var child in container.GetChildren().Reverse())
        {
            var hit = FindViewAt(position, child);
            if (hit != null)
                return hit;
        }

        return root;
    }

    /// <summary>
    ///     Finds a view by its element ID.
    /// </summary>
    private IView? FindViewById(long id, IView root)
    {
        if (root.Layout.ElementId == id)
            return root;

        return root is not IViewContainer container
            ? null
            : container
                .GetChildren()
                .Select(child => FindViewById(id, child))
                .OfType<IView>()
                .FirstOrDefault();
    }

    /// <summary>
    ///     Finds the parent of a view in the tree using iterative approach.
    /// </summary>
    private static IView? FindParent(IView target, IView root)
    {
        if (root is not IViewContainer) return null;

        // Use a stack to iteratively traverse the tree
        var stack = new Stack<IView>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            if (current is not IViewContainer container) continue;

            foreach (var child in container.GetChildren())
            {
                // If this child is our target, current is the parent
                if (child == target)
                    return current;

                // Otherwise, add child to stack to search its subtree
                stack.Push(child);
            }
        }

        return null;
    }
}