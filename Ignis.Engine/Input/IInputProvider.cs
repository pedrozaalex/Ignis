using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Ignis.Engine.UI.Input;

namespace Ignis.Engine.Input;

public interface IInputProvider
{
    Vector2 MousePosition { get; }
    bool IsMouseButtonPressed(int buttonIndex);
    bool IsMouseButtonReleased(int buttonIndex);
    bool IsMouseButtonJustPressed(int buttonIndex);
    
    bool IsKeyDown(Keys key);
    bool IsKeyPressed(Keys key);
    
    ModifierKeys GetModifiers();
    
    event EventHandler<TextInputEventArgs>? TextInput;
}

public class TextInputEventArgs : EventArgs
{
    public char Character { get; }
    public Keys Key { get; }
    
    public TextInputEventArgs(char character, Keys key)
    {
        Character = character;
        Key = key;
    }
}

