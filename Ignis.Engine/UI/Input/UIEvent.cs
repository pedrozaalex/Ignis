using Microsoft.Xna.Framework;

namespace Ignis.Engine.UI.Input
{
    /// <summary>
    /// Base class for all UI events. Supports event bubbling with stop propagation.
    /// </summary>
    public abstract class UIEvent
    {
        public bool Handled { get; private set; }
        
        public void StopPropagation() => Handled = true;
    }

    /// <summary>
    /// Mouse/touch/pen pointer events.
    /// </summary>
    public class PointerEvent : UIEvent
    {
        public Vector2 Position { get; }
        public int Button { get; }
        public PointerType Type { get; }
        public PointerEventType EventType { get; }

        public PointerEvent(Vector2 position, int button, PointerType type, PointerEventType eventType)
        {
            Position = position;
            Button = button;
            Type = type;
            EventType = eventType;
        }
    }

    public enum PointerType
    {
        Mouse,
        Touch,
        Pen
    }

    public enum PointerEventType
    {
        Down,
        Up,
        Move,
        Enter,
        Leave
    }

    /// <summary>
    /// Keyboard events.
    /// </summary>
    public class KeyboardEvent : UIEvent
    {
        public Microsoft.Xna.Framework.Input.Keys Key { get; }
        public ModifierKeys Modifiers { get; }
        public KeyboardEventType EventType { get; }
        public char? Character { get; }

        public KeyboardEvent(
            Microsoft.Xna.Framework.Input.Keys key, 
            ModifierKeys modifiers, 
            KeyboardEventType eventType,
            char? character = null)
        {
            Key = key;
            Modifiers = modifiers;
            EventType = eventType;
            Character = character;
        }
    }

    public enum KeyboardEventType
    {
        Down,
        Up
    }

    [Flags]
    public enum ModifierKeys
    {
        None = 0,
        Control = 1,
        Shift = 2,
        Alt = 4
    }

    /// <summary>
    /// Drag and drop events.
    /// </summary>
    public class DragEvent : UIEvent
    {
        public Vector2 Position { get; }
        public object? Payload { get; }
        public DragEventType EventType { get; }
        
        private bool _accepted;
        
        public bool IsAccepted => _accepted;

        public DragEvent(Vector2 position, object? payload, DragEventType eventType)
        {
            Position = position;
            Payload = payload;
            EventType = eventType;
        }

        public void Accept() => _accepted = true;
    }

    public enum DragEventType
    {
        Start,
        Over,
        Drop,
        End
    }
}

