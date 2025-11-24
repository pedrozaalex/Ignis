using Ignis.Engine.Input;
using Ignis.Engine.Reactive;
using Ignis.Engine.UI.Abstractions;
using Ignis.Engine.UI.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TextInputEventArgs = Ignis.Engine.Input.TextInputEventArgs;

namespace Ignis.Engine.UI.Input
{
    /// <summary>
    /// Manages input state and dispatches events to the UI tree.
    /// Handles focus, hover, and event bubbling.
    /// </summary>
    public class InputManager
    {
        private readonly Signal<long?> _focusedElementId = new(null);
        private readonly Signal<long?> _hoveredElementId = new(null);
        private readonly Signal<long?> _activeElementId = new(null);
        
        private readonly IInputProvider _input;
        
        private IView? _root;
        private readonly Dictionary<long, Rectangle> _bounds;
        
        // Drag state
        private bool _isDragging;
        private object? _dragPayload;
        private Vector2 _dragStartPosition;
        private const float DragThreshold = 5f;

        public Signal<long?> FocusedElementId => _focusedElementId;
        public Signal<long?> HoveredElementId => _hoveredElementId;
        public Signal<long?> ActiveElementId => _activeElementId;

        public InputManager(Dictionary<long, Rectangle> bounds, IInputProvider input)
        {
            _bounds = bounds;
            _input = input;
            
            // Subscribe to text input events
            _input.TextInput += OnTextInput;
        }
        
        private void OnTextInput(object? sender, TextInputEventArgs e)
        {
            // Forward text input to focused element
            if (_focusedElementId.Value.HasValue && _root != null)
            {
                var focusedView = FindViewById(_focusedElementId.Value.Value, _root);
                if (focusedView is ViewComponent component)
                {
                    component.EventHandlers.InvokeTextInput(e.Character);
                }
            }
        }

        public void SetRoot(IView root)
        {
            _root = root;
        }

        /// <summary>
        /// Process input for the current frame.
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
            
            // Debug logging
            var isJustPressed = _input.IsMouseButtonJustPressed(0);
            if (isJustPressed)
            {
                Console.WriteLine($"[InputManager] IsMouseButtonJustPressed(0) = TRUE at {mousePos}, bounds count: {_bounds.Count}, hitView: {(hitView != null ? $"ID {hitView.Layout.ElementId}" : "NULL")}");
            }
            
            // Update hover state
            var previousHovered = _hoveredElementId.Value;
            var currentHovered = hitView?.Layout.ElementId;

            if (previousHovered != currentHovered)
            {
                Console.WriteLine($"[InputManager] Hover changed from {previousHovered} to {currentHovered}");
                
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

                _hoveredElementId.Value = currentHovered;
            }

            // Mouse button events
            if (_input.IsMouseButtonJustPressed(0))
            {
                HandleMouseDown(mousePos, 0, hitView);
            }
            else if (_input.IsMouseButtonReleased(0))
            {
                HandleMouseUp(mousePos, 0, hitView);
            }

            // Mouse move
            HandleMouseMove(mousePos, hitView);
        }

        private void HandleMouseDown(Vector2 position, int button, IView? hitView)
        {
            if (hitView is ViewComponent component)
            {
                Console.WriteLine($"[InputManager] HandleMouseDown on element {component.Layout.ElementId}, focusable: {component.Layout.Focusable}");
                
                // Set active element (pressed/dragging state)
                _activeElementId.Value = component.Layout.ElementId;
                
                // Set focus if focusable
                if (component.Layout.Focusable)
                {
                    Console.WriteLine($"[InputManager] Setting focus to {component.Layout.ElementId}");
                    _focusedElementId.Value = component.Layout.ElementId;
                }

                // Fire pointer down event with bubbling
                var evt = new PointerEvent(position, button, PointerType.Mouse, PointerEventType.Down);
                Console.WriteLine($"[InputManager] Invoking PointerDown on {component.Layout.ElementId}");
                BubbleEvent(component, view =>
                {
                    if (view is ViewComponent vc)
                    {
                        vc.EventHandlers.InvokePointerDown(evt);
                    }
                }, evt);

                // Store drag start position
                _dragStartPosition = position;
            }
            else
            {
                Console.WriteLine($"[InputManager] HandleMouseDown but hitView is not a ViewComponent");
            }
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
                        if (view is ViewComponent vc)
                        {
                            vc.EventHandlers.InvokeDrop(dropEvt);
                        }
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
                    if (view is ViewComponent vc)
                    {
                        vc.EventHandlers.InvokePointerUp(evt);
                    }
                }, evt);
            }
            
            // Clear active element after processing mouse up
            _activeElementId.Value = null;
        }

        private void HandleMouseMove(Vector2 position, IView? hitView)
        {
            // Check for drag threshold
            if (!_isDragging && _input.IsMouseButtonPressed(0))
            {
                var distance = Vector2.Distance(position, _dragStartPosition);
                if (distance > DragThreshold && hitView is ViewComponent dragSource)
                {
                    // Start drag
                    _isDragging = true;
                    var dragEvt = new DragEvent(position, null, DragEventType.Start);
                    dragSource.EventHandlers.InvokeDragStart(dragEvt);
                    _dragPayload = dragEvt.Payload; // Handler should set payload
                }
            }

            if (_isDragging && hitView is ViewComponent dropTarget)
            {
                var dragOverEvt = new DragEvent(position, _dragPayload, DragEventType.Over);
                dropTarget.EventHandlers.InvokeDragOver(dragOverEvt);
            }
            else if (hitView is ViewComponent component)
            {
                var evt = new PointerEvent(position, 0, PointerType.Mouse, PointerEventType.Move);
                component.EventHandlers.InvokePointerMove(evt);
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
                if (_focusedElementId.Value.HasValue)
                {
                    var focusedView = FindViewById(_focusedElementId.Value.Value, _root!);
                    if (focusedView != null)
                    {
                        // Try to handle with shortcuts, bubbling up
                        BubbleShortcut(focusedView, key, modifiers, out var handled);
                        
                        if (!handled)
                        {
                            // Fire keyboard event
                            BubbleEvent(focusedView, view =>
                            {
                                if (view is ViewComponent vc)
                                {
                                    vc.EventHandlers.InvokeKeyDown(evt);
                                }
                            }, evt);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Bubbles an event up the tree until handled.
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
        /// Bubbles a shortcut up the tree until handled.
        /// </summary>
        private void BubbleShortcut(IView start, Keys key, ModifierKeys modifiers, out bool handled)
        {
            handled = false;
            var current = start;
            
            while (current != null && !handled)
            {
                if (current is ViewComponent component)
                {
                    handled = component.Shortcuts.TryHandle(key, modifiers);
                }
                
                if (!handled)
                {
                    current = FindParent(current, _root!);
                }
            }
        }

        /// <summary>
        /// Finds the view at the given position (depth-first, returns deepest match).
        /// </summary>
        private IView? FindViewAt(Vector2 position, IView root)
        {
            if (!_bounds.TryGetValue(root.Layout.ElementId, out var bounds))
            {
                if (_input.IsMouseButtonJustPressed(0))
                {
                    Console.WriteLine($"[FindViewAt] No bounds for element ID {root.Layout.ElementId}");
                }
                return null;
            }

            var contains = bounds.Contains(position);
            if (_input.IsMouseButtonJustPressed(0))
            {
                Console.WriteLine($"[FindViewAt] ID {root.Layout.ElementId}, bounds {bounds}, contains {position}: {contains}");
            }
            
            if (!contains)
                return null;

            // Check children first (depth-first)
            if (root is IViewContainer container)
            {
                // Reverse order to respect Z-index (last child is on top)
                foreach (var child in container.GetChildren().Reverse())
                {
                    var hit = FindViewAt(position, child);
                    if (hit != null)
                        return hit;
                }
            }

            return root;
        }

        /// <summary>
        /// Finds a view by its element ID.
        /// </summary>
        private IView? FindViewById(long id, IView root)
        {
            if (root.Layout.ElementId == id)
                return root;

            if (root is IViewContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    var found = FindViewById(id, child);
                    if (found != null)
                        return found;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the parent of a view in the tree.
        /// </summary>
        private IView? FindParent(IView target, IView root)
        {
            if (root is IViewContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    if (child == target)
                        return root;

                    var found = FindParent(target, child);
                    if (found != null)
                        return found;
                }
            }

            return null;
        }
    }
}

