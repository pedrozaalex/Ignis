using Ignis.Engine.Reactive;
using Ignis.Engine.UI.Abstractions;
using Ignis.Engine.UI.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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
        
        private MouseState _previousMouseState;
        private KeyboardState _previousKeyboardState;
        
        private IView? _root;
        private readonly Dictionary<long, Rectangle> _bounds;
        
        // Drag state
        private bool _isDragging;
        private object? _dragPayload;
        private Vector2 _dragStartPosition;
        private const float DragThreshold = 5f;

        public Signal<long?> FocusedElementId => _focusedElementId;
        public Signal<long?> HoveredElementId => _hoveredElementId;

        public InputManager(Dictionary<long, Rectangle> bounds)
        {
            _bounds = bounds;
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

            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();

            ProcessMouseInput(mouseState);
            ProcessKeyboardInput(keyboardState);

            _previousMouseState = mouseState;
            _previousKeyboardState = keyboardState;
        }

        private void ProcessMouseInput(MouseState mouseState)
        {
            var mousePos = new Vector2(mouseState.X, mouseState.Y);

            // Find the topmost view under the cursor (depth-first search, reverse order for Z-index)
            var hitView = FindViewAt(mousePos, _root!);
            
            // Update hover state
            var previousHovered = _hoveredElementId.Value;
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

                _hoveredElementId.Value = currentHovered;
            }

            // Mouse button events
            if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
            {
                HandleMouseDown(mousePos, 0, hitView);
            }
            else if (mouseState.LeftButton == ButtonState.Released && _previousMouseState.LeftButton == ButtonState.Pressed)
            {
                HandleMouseUp(mousePos, 0, hitView);
            }

            // Mouse move
            if (mouseState.Position != _previousMouseState.Position)
            {
                HandleMouseMove(mousePos, hitView);
            }
        }

        private void HandleMouseDown(Vector2 position, int button, IView? hitView)
        {
            if (hitView is ViewComponent component)
            {
                // Set focus if focusable
                if (component.Layout.Focusable)
                {
                    _focusedElementId.Value = component.Layout.ElementId;
                }

                // Fire pointer down event with bubbling
                var evt = new PointerEvent(position, button, PointerType.Mouse, PointerEventType.Down);
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
        }

        private void HandleMouseMove(Vector2 position, IView? hitView)
        {
            // Check for drag threshold
            if (!_isDragging && Mouse.GetState().LeftButton == ButtonState.Pressed)
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

        private void ProcessKeyboardInput(KeyboardState keyboardState)
        {
            var pressedKeys = keyboardState.GetPressedKeys();
            var previousKeys = _previousKeyboardState.GetPressedKeys();

            // Detect newly pressed keys
            var newKeys = pressedKeys.Except(previousKeys).ToList();

            foreach (var key in newKeys)
            {
                var modifiers = GetModifiers(keyboardState);
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

        private static ModifierKeys GetModifiers(KeyboardState keyboardState)
        {
            var modifiers = ModifierKeys.None;
            if (keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl))
                modifiers |= ModifierKeys.Control;
            if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
                modifiers |= ModifierKeys.Shift;
            if (keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt))
                modifiers |= ModifierKeys.Alt;
            return modifiers;
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
                return null;

            if (!bounds.Contains(position))
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

