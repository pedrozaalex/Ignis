using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Input;

namespace Ignis.Engine.UI
{
    /// <summary>
    /// Fluent extension methods for attaching event handlers to views.
    /// </summary>
    public static class InteractivityExtensions
    {
        // Pointer Events
        public static T OnPointerDown<T>(this T view, PointerEventHandler handler) where T : ViewComponent
        {
            view.EventHandlers.OnPointerDown += handler;
            return view;
        }

        public static T OnPointerUp<T>(this T view, PointerEventHandler handler) where T : ViewComponent
        {
            view.EventHandlers.OnPointerUp += handler;
            return view;
        }

        public static T OnPointerMove<T>(this T view, PointerEventHandler handler) where T : ViewComponent
        {
            view.EventHandlers.OnPointerMove += handler;
            return view;
        }

        public static T OnPointerEnter<T>(this T view, PointerEventHandler handler) where T : ViewComponent
        {
            view.EventHandlers.OnPointerEnter += handler;
            return view;
        }

        public static T OnPointerLeave<T>(this T view, PointerEventHandler handler) where T : ViewComponent
        {
            view.EventHandlers.OnPointerLeave += handler;
            return view;
        }

        // Convenience: Click = PointerUp
        public static T OnClick<T>(this T view, Action handler) where T : ViewComponent
        {
            view.EventHandlers.OnPointerUp += _ => handler();
            return view;
        }

        // Keyboard Events
        public static T OnKeyDown<T>(this T view, KeyboardEventHandler handler) where T : ViewComponent
        {
            view.EventHandlers.OnKeyDown += handler;
            return view;
        }

        public static T OnKeyUp<T>(this T view, KeyboardEventHandler handler) where T : ViewComponent
        {
            view.EventHandlers.OnKeyUp += handler;
            return view;
        }
        
        // Text Input
        public static T OnTextInput<T>(this T view, TextInputEventHandler handler) where T : ViewComponent
        {
            view.EventHandlers.OnTextInput += handler;
            return view;
        }

        // Drag Events
        public static T OnDragStart<T>(this T view, DragEventHandler handler) where T : ViewComponent
        {
            view.EventHandlers.OnDragStart += handler;
            return view;
        }

        public static T OnDragOver<T>(this T view, DragEventHandler handler) where T : ViewComponent
        {
            view.EventHandlers.OnDragOver += handler;
            return view;
        }

        public static T OnDrop<T>(this T view, DragEventHandler handler) where T : ViewComponent
        {
            view.EventHandlers.OnDrop += handler;
            return view;
        }

        public static T OnDragEnd<T>(this T view, DragEventHandler handler) where T : ViewComponent
        {
            view.EventHandlers.OnDragEnd += handler;
            return view;
        }

        // Focus
        public static T Focusable<T>(this T view) where T : ViewComponent
        {
            view.Layout.Focusable = true;
            return view;
        }

        // Shortcuts
        public static T Shortcuts<T>(this T view, Action<ShortcutBuilder> configure) where T : ViewComponent
        {
            var builder = new ShortcutBuilder();
            configure(builder);
            foreach (var shortcut in builder.Build())
            {
                view.Shortcuts.Add(shortcut);
            }
            return view;
        }

        // Draggable (simplified - creates a drag start handler)
        public static T Draggable<T>(this T view, object payload, Func<IView>? visualBuilder = null) where T : ViewComponent
        {
            view.OnPointerDown(evt =>
            {
                // Store initial position for drag threshold detection
                // This would be expanded in a full implementation
                var dragEvent = new DragEvent(evt.Position, payload, DragEventType.Start);
                view.EventHandlers.InvokeDragStart(dragEvent);
            });
            return view;
        }
    }
}

