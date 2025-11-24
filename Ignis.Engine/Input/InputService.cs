using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Ignis.Engine.UI.Input;

namespace Ignis.Engine.Input;

public class InputService : IInputProvider
{
    private MouseState _currentMouse;
    private MouseState _prevMouse;
    private KeyboardState _currentKey;
    private KeyboardState _prevKey;
    private static int _frameCount = 0;

    public event EventHandler<TextInputEventArgs>? TextInput;

    public Vector2 MousePosition => new(_currentMouse.X, _currentMouse.Y);

    public void Update()
    {
        _frameCount++;
        _prevMouse = _currentMouse;
        _currentMouse = Mouse.GetState();
        
        _prevKey = _currentKey;
        _currentKey = Keyboard.GetState();
        
        // Debug: Log ALL button state changes
        if (_currentMouse.LeftButton != _prevMouse.LeftButton)
        {
            Console.WriteLine($"[InputService Frame {_frameCount}] Left button state changed: {_prevMouse.LeftButton} -> {_currentMouse.LeftButton} at {MousePosition}");
        }
    }

    public bool IsMouseButtonPressed(int buttonIndex)
    {
        return buttonIndex switch
        {
            0 => _currentMouse.LeftButton == ButtonState.Pressed,
            1 => _currentMouse.RightButton == ButtonState.Pressed,
            2 => _currentMouse.MiddleButton == ButtonState.Pressed,
            _ => false
        };
    }

    public bool IsMouseButtonJustPressed(int buttonIndex)
    {
        return buttonIndex switch
        {
            0 => _currentMouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released,
            1 => _currentMouse.RightButton == ButtonState.Pressed && _prevMouse.RightButton == ButtonState.Released,
            2 => _currentMouse.MiddleButton == ButtonState.Pressed && _prevMouse.MiddleButton == ButtonState.Released,
            _ => false
        };
    }

    public bool IsMouseButtonReleased(int buttonIndex)
    {
        return buttonIndex switch
        {
            0 => _currentMouse.LeftButton == ButtonState.Released && _prevMouse.LeftButton == ButtonState.Pressed,
            1 => _currentMouse.RightButton == ButtonState.Released && _prevMouse.RightButton == ButtonState.Pressed,
            2 => _currentMouse.MiddleButton == ButtonState.Released && _prevMouse.MiddleButton == ButtonState.Pressed,
            _ => false
        };
    }

    public bool IsKeyDown(Keys key) => _currentKey.IsKeyDown(key);
    
    public bool IsKeyPressed(Keys key) => _currentKey.IsKeyDown(key) && !_prevKey.IsKeyDown(key);

    public ModifierKeys GetModifiers()
    {
        var modifiers = ModifierKeys.None;
        if (_currentKey.IsKeyDown(Keys.LeftControl) || _currentKey.IsKeyDown(Keys.RightControl))
            modifiers |= ModifierKeys.Control;
        if (_currentKey.IsKeyDown(Keys.LeftShift) || _currentKey.IsKeyDown(Keys.RightShift))
            modifiers |= ModifierKeys.Shift;
        if (_currentKey.IsKeyDown(Keys.LeftAlt) || _currentKey.IsKeyDown(Keys.RightAlt))
            modifiers |= ModifierKeys.Alt;
        return modifiers;
    }
    
    public MouseState GetCurrentMouseState() => _currentMouse;
    public MouseState GetPreviousMouseState() => _prevMouse;
    public KeyboardState GetCurrentKeyboardState() => _currentKey;
    public KeyboardState GetPreviousKeyboardState() => _prevKey;
    
    public Keys[] GetPressedKeys() => _currentKey.GetPressedKeys();
    public Keys[] GetPreviousPressedKeys() => _prevKey.GetPressedKeys();
    
    internal void RaiseTextInput(char character, Keys key)
    {
        TextInput?.Invoke(this, new TextInputEventArgs(character, key));
    }
}

