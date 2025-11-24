namespace Ignis.Engine.UI.Input
{
    /// <summary>
    /// Delegate types for event handlers.
    /// </summary>
    public delegate void PointerEventHandler(PointerEvent evt);
    public delegate void KeyboardEventHandler(KeyboardEvent evt);
    public delegate void DragEventHandler(DragEvent evt);
    public delegate void TextInputEventHandler(char character);

    /// <summary>
    /// Collection of event handlers for a view.
    /// </summary>
    public class EventHandlers
    {
        public PointerEventHandler? OnPointerDown { get; set; }
        public PointerEventHandler? OnPointerUp { get; set; }
        public PointerEventHandler? OnPointerMove { get; set; }
        public PointerEventHandler? OnPointerEnter { get; set; }
        public PointerEventHandler? OnPointerLeave { get; set; }
        
        public KeyboardEventHandler? OnKeyDown { get; set; }
        public KeyboardEventHandler? OnKeyUp { get; set; }
        
        public TextInputEventHandler? OnTextInput { get; set; }
        
        public DragEventHandler? OnDragStart { get; set; }
        public DragEventHandler? OnDragOver { get; set; }
        public DragEventHandler? OnDrop { get; set; }
        public DragEventHandler? OnDragEnd { get; set; }

        public void InvokePointerDown(PointerEvent evt) => OnPointerDown?.Invoke(evt);
        public void InvokePointerUp(PointerEvent evt) => OnPointerUp?.Invoke(evt);
        public void InvokePointerMove(PointerEvent evt) => OnPointerMove?.Invoke(evt);
        public void InvokePointerEnter(PointerEvent evt) => OnPointerEnter?.Invoke(evt);
        public void InvokePointerLeave(PointerEvent evt) => OnPointerLeave?.Invoke(evt);
        
        public void InvokeKeyDown(KeyboardEvent evt) => OnKeyDown?.Invoke(evt);
        public void InvokeKeyUp(KeyboardEvent evt) => OnKeyUp?.Invoke(evt);
        
        public void InvokeTextInput(char character) => OnTextInput?.Invoke(character);
        
        public void InvokeDragStart(DragEvent evt) => OnDragStart?.Invoke(evt);
        public void InvokeDragOver(DragEvent evt) => OnDragOver?.Invoke(evt);
        public void InvokeDrop(DragEvent evt) => OnDrop?.Invoke(evt);
        public void InvokeDragEnd(DragEvent evt) => OnDragEnd?.Invoke(evt);
    }
}

