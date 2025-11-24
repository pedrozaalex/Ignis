namespace Ignis.Engine.UI.Abstractions;

[Flags]
public enum WidgetState
{
    Normal = 0,
    Hovered = 1,
    Active = 2,   // Pressed / Dragging
    Focused = 4,
    Disabled = 8
}

