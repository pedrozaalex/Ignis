using Microsoft.Xna.Framework;
using FontStashSharp;

namespace Ignis.Engine.UI.Core;

public record Theme
{
    public Color PrimaryColor { get; init; }
    public Color BackgroundColor { get; init; }
    public Color SurfaceColor { get; init; }
    public Color BorderColor { get; init; }
    public Color TextColor { get; init; }
    
    public SpriteFontBase? DefaultFont { get; init; }

    public static Theme Dark => new()
    {
        PrimaryColor = new Color(0, 122, 204),
        BackgroundColor = new Color(30, 30, 30),
        SurfaceColor = new Color(45, 45, 48),
        BorderColor = new Color(63, 63, 70),
        TextColor = Color.White
    };

    public static Theme Light => new()
    {
        PrimaryColor = new Color(0, 120, 215),
        BackgroundColor = new Color(240, 240, 240),
        SurfaceColor = new Color(255, 255, 255),
        BorderColor = new Color(200, 200, 200),
        TextColor = Color.Black
    };
}

