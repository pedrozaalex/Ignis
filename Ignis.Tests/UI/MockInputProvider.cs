using Ignis.Engine.Input;
using Ignis.Engine.UI.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TextInputEventArgs = Ignis.Engine.Input.TextInputEventArgs;

namespace Ignis.Tests.UI;

public class MockInputProvider : IInputProvider
{
    private Vector2 _mousePosition;
    private readonly Dictionary<int, bool> _mouseButtons = new();
    private readonly Dictionary<int, bool> _prevMouseButtons = new();
    private readonly HashSet<Keys> _keysDown = new();
    private readonly HashSet<Keys> _prevKeysDown = new();

    public event EventHandler<TextInputEventArgs>? TextInput;

    public Vector2 MousePosition
    {
        get => _mousePosition;
        set => _mousePosition = value;
    }

    public void SetMouseButton(int buttonIndex, bool isPressed)
    {
        _mouseButtons[buttonIndex] = isPressed;
    }

    public void SetKey(Keys key, bool isDown)
    {
        if (isDown)
            _keysDown.Add(key);
        else
            _keysDown.Remove(key);
    }

    public void Update()
    {
        _prevMouseButtons.Clear();
        foreach (var kvp in _mouseButtons)
        {
            _prevMouseButtons[kvp.Key] = kvp.Value;
        }

        _prevKeysDown.Clear();
        foreach (var key in _keysDown)
        {
            _prevKeysDown.Add(key);
        }
    }

    public bool IsMouseButtonPressed(int buttonIndex)
    {
        return _mouseButtons.TryGetValue(buttonIndex, out var pressed) && pressed;
    }

    public bool IsMouseButtonReleased(int buttonIndex)
    {
        var current = _mouseButtons.TryGetValue(buttonIndex, out var c) && c;
        var prev = _prevMouseButtons.TryGetValue(buttonIndex, out var p) && p;
        return !current && prev;
    }

    public bool IsMouseButtonJustPressed(int buttonIndex)
    {
        var current = _mouseButtons.TryGetValue(buttonIndex, out var c) && c;
        var prev = _prevMouseButtons.TryGetValue(buttonIndex, out var p) && p;
        return current && !prev;
    }

    public bool IsKeyDown(Keys key)
    {
        return _keysDown.Contains(key);
    }

    public bool IsKeyPressed(Keys key)
    {
        return _keysDown.Contains(key) && !_prevKeysDown.Contains(key);
    }

    public ModifierKeys GetModifiers()
    {
        var modifiers = ModifierKeys.None;
        if (IsKeyDown(Keys.LeftControl) || IsKeyDown(Keys.RightControl))
            modifiers |= ModifierKeys.Control;
        if (IsKeyDown(Keys.LeftShift) || IsKeyDown(Keys.RightShift))
            modifiers |= ModifierKeys.Shift;
        if (IsKeyDown(Keys.LeftAlt) || IsKeyDown(Keys.RightAlt))
            modifiers |= ModifierKeys.Alt;
        return modifiers;
    }
    
    public void SimulateTextInput(char character, Keys key = Keys.None)
    {
        TextInput?.Invoke(this, new TextInputEventArgs(character, key));
    }
}

