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
    
    // Interaction state colors
    public Color ButtonHoverColor { get; init; }
    public Color ButtonActiveColor { get; init; }
    public Color InputFocusBorderColor { get; init; }
    public Color SliderThumbColor { get; init; }
    public Color SliderThumbHoverColor { get; init; }
    
    // Input/Field colors
    public Color InputBackground { get; init; }
    
    // Overlay colors
    public Color TooltipBackground { get; init; }
    
    // Generic accent colors (can be used for axes, tags, categories, etc.)
    public Color Accent1 { get; init; }  // Red-ish
    public Color Accent2 { get; init; }  // Green-ish
    public Color Accent3 { get; init; }  // Blue-ish
    
    // Status colors
    public Color ErrorColor { get; init; }
    public Color WarningColor { get; init; }
    public Color InfoColor { get; init; }
    
    public SpriteFontBase? DefaultFont { get; init; }

    public static Theme Dark => new()
    {
        PrimaryColor = new Color(0, 122, 204),
        BackgroundColor = new Color(30, 30, 30),
        SurfaceColor = new Color(45, 45, 48),
        BorderColor = new Color(63, 63, 70),
        TextColor = Color.White,
        ButtonHoverColor = new Color(0, 140, 230),
        ButtonActiveColor = new Color(0, 100, 180),
        InputFocusBorderColor = new Color(0, 122, 204),
        SliderThumbColor = new Color(180, 180, 180),
        SliderThumbHoverColor = new Color(220, 220, 220),
        InputBackground = new Color(51, 51, 55),
        TooltipBackground = new Color(30, 30, 30, 240),
        Accent1 = new Color(255, 100, 100),
        Accent2 = new Color(100, 255, 100),
        Accent3 = new Color(100, 150, 255),
        ErrorColor = new Color(255, 100, 100),
        WarningColor = new Color(255, 200, 100),
        InfoColor = new Color(100, 200, 255)
    };

    public static Theme Light => new()
    {
        PrimaryColor = new Color(0, 120, 215),
        BackgroundColor = new Color(240, 240, 240),
        SurfaceColor = new Color(255, 255, 255),
        BorderColor = new Color(200, 200, 200),
        TextColor = Color.Black,
        ButtonHoverColor = new Color(0, 140, 240),
        ButtonActiveColor = new Color(0, 100, 190),
        InputFocusBorderColor = new Color(0, 120, 215),
        SliderThumbColor = new Color(100, 100, 100),
        SliderThumbHoverColor = new Color(60, 60, 60),
        InputBackground = new Color(245, 245, 245),
        TooltipBackground = new Color(50, 50, 50, 240),
        Accent1 = new Color(230, 80, 80),
        Accent2 = new Color(80, 200, 80),
        Accent3 = new Color(80, 120, 230),
        ErrorColor = new Color(200, 50, 50),
        WarningColor = new Color(200, 150, 50),
        InfoColor = new Color(50, 150, 200)
    };
}

