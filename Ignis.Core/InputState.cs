using System.Numerics;
using Silk.NET.Input;

namespace Ignis.Core;

/// <summary>
/// Helper class for polling keyboard and mouse input state.
/// 
/// Frame lifecycle:
/// 1. Events are collected between frames (KeyDown, MouseMove, etc.)
/// 2. During the frame, call IsKeyPressed/IsMousePressed to check one-shot events
/// 3. At the end of the frame, EndFrame() is called automatically by Window
/// 
/// Use Window.InputState - do not create InputState manually.
/// </summary>
public sealed class InputState : IDisposable
{
    private readonly IInputContext _input;
    private readonly HashSet<Key> _keysDown = [];
    private readonly HashSet<Key> _keysPressed = [];
    private readonly HashSet<Key> _keysReleased = [];
    private readonly HashSet<MouseButton> _mouseDown = [];
    private readonly HashSet<MouseButton> _mousePressed = [];
    private readonly HashSet<MouseButton> _mouseReleased = [];
    
    private Vector2 _mousePosition;
    private Vector2 _mouseDelta;
    private Vector2 _lastMousePosition;
    private float _scrollDelta;
    private bool _firstMouse = true;
    
    /// <summary>Current mouse position in screen coordinates.</summary>
    public Vector2 MousePosition => _mousePosition;
    
    /// <summary>Mouse movement since last frame.</summary>
    public Vector2 MouseDelta => _mouseDelta;
    
    /// <summary>Mouse scroll wheel delta since last frame.</summary>
    public float ScrollDelta => _scrollDelta;
    
    internal InputState(IInputContext input)
    {
        _input = input;
        
        foreach (var keyboard in input.Keyboards)
        {
            keyboard.KeyDown += OnKeyDown;
            keyboard.KeyUp += OnKeyUp;
        }
        
        foreach (var mouse in input.Mice)
        {
            mouse.MouseDown += OnMouseDown;
            mouse.MouseUp += OnMouseUp;
            mouse.MouseMove += OnMouseMove;
            mouse.Scroll += OnScroll;
        }
    }
    
    /// <summary>
    /// Clears per-frame input states. Called automatically by Window after OnUpdate.
    /// </summary>
    internal void EndFrame()
    {
        _keysPressed.Clear();
        _keysReleased.Clear();
        _mousePressed.Clear();
        _mouseReleased.Clear();
        _scrollDelta = 0;
        
        if (_firstMouse)
        {
            _lastMousePosition = _mousePosition;
            _firstMouse = false;
        }
        
        _mouseDelta = _mousePosition - _lastMousePosition;
        _lastMousePosition = _mousePosition;
    }
    
    // Keyboard
    
    /// <summary>Returns true while the key is held down.</summary>
    public bool IsKeyDown(Key key) => _keysDown.Contains(key);
    
    /// <summary>Returns true on the frame the key was pressed.</summary>
    public bool IsKeyPressed(Key key) => _keysPressed.Contains(key);
    
    /// <summary>Returns true on the frame the key was released.</summary>
    public bool IsKeyReleased(Key key) => _keysReleased.Contains(key);
    
    // Mouse
    
    /// <summary>Returns true while the mouse button is held down.</summary>
    public bool IsMouseDown(MouseButton button) => _mouseDown.Contains(button);
    
    /// <summary>Returns true on the frame the mouse button was pressed.</summary>
    public bool IsMousePressed(MouseButton button) => _mousePressed.Contains(button);
    
    /// <summary>Returns true on the frame the mouse button was released.</summary>
    public bool IsMouseReleased(MouseButton button) => _mouseReleased.Contains(button);
    
    // Convenience methods
    
    /// <summary>Returns a normalized direction vector based on WASD keys.</summary>
    public Vector2 GetWASDDirection()
    {
        var dir = Vector2.Zero;
        if (IsKeyDown(Key.W) || IsKeyDown(Key.Up)) dir.Y -= 1;
        if (IsKeyDown(Key.S) || IsKeyDown(Key.Down)) dir.Y += 1;
        if (IsKeyDown(Key.A) || IsKeyDown(Key.Left)) dir.X -= 1;
        if (IsKeyDown(Key.D) || IsKeyDown(Key.Right)) dir.X += 1;
        
        if (dir.LengthSquared() > 0)
            dir = Vector2.Normalize(dir);
        
        return dir;
    }
    
    /// <summary>Sets whether the mouse cursor is visible.</summary>
    public void SetCursorVisible(bool visible)
    {
        foreach (var mouse in _input.Mice)
        {
            mouse.Cursor.CursorMode = visible ? CursorMode.Normal : CursorMode.Disabled;
        }
    }
    
    /// <summary>Sets the cursor position.</summary>
    public void SetCursorPosition(Vector2 position)
    {
        foreach (var mouse in _input.Mice)
        {
            mouse.Position = position;
        }
    }
    
    // Event handlers
    
    private void OnKeyDown(IKeyboard keyboard, Key key, int scancode)
    {
        if (!_keysDown.Contains(key))
        {
            _keysPressed.Add(key);
        }
        _keysDown.Add(key);
    }
    
    private void OnKeyUp(IKeyboard keyboard, Key key, int scancode)
    {
        _keysDown.Remove(key);
        _keysReleased.Add(key);
    }
    
    private void OnMouseDown(IMouse mouse, MouseButton button)
    {
        if (!_mouseDown.Contains(button))
        {
            _mousePressed.Add(button);
        }
        _mouseDown.Add(button);
    }
    
    private void OnMouseUp(IMouse mouse, MouseButton button)
    {
        _mouseDown.Remove(button);
        _mouseReleased.Add(button);
    }
    
    private void OnMouseMove(IMouse mouse, Vector2 position)
    {
        _mousePosition = position;
    }
    
    private void OnScroll(IMouse mouse, ScrollWheel wheel)
    {
        _scrollDelta += wheel.Y;
    }
    
    public void Dispose()
    {
        foreach (var keyboard in _input.Keyboards)
        {
            keyboard.KeyDown -= OnKeyDown;
            keyboard.KeyUp -= OnKeyUp;
        }
        
        foreach (var mouse in _input.Mice)
        {
            mouse.MouseDown -= OnMouseDown;
            mouse.MouseUp -= OnMouseUp;
            mouse.MouseMove -= OnMouseMove;
            mouse.Scroll -= OnScroll;
        }
    }
}

