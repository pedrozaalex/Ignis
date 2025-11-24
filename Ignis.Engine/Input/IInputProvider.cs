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
}

