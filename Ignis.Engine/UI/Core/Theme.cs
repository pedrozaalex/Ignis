using Microsoft.Xna.Framework;
using FontStashSharp;

namespace Ignis.Engine.UI.Core;

public record Theme
{
    // Brand & Action
    public Color Primary { get; init; }
    public Color OnPrimary { get; init; }
    
    // Backgrounds & Surfaces
    public Color Background { get; init; }
    public Color Surface { get; init; }
    public Color SurfaceOverlay { get; init; }  // Elevated or hovered surfaces
    
    // Content (Typography & Icons)
    public Color TextMain { get; init; }
    public Color TextMuted { get; init; }
    public Color OnSurface { get; init; }  // Text on surface elements
    
    // Structural Elements
    public Color Border { get; init; }
    public Color BorderFocus { get; init; }
    
    // Functional (Feedback)
    public Color Success { get; init; }
    public Color Error { get; init; }
    public Color Warning { get; init; }
    public Color Info { get; init; }
    
    // Input/Field specific
    public Color InputBackground { get; init; }
    
    // Interactive states (calculated from Primary)
    public Color PrimaryHover { get; init; }
    public Color PrimaryActive { get; init; }
    public Color SurfaceHover { get; init; }
    public Color SurfaceActive { get; init; }
    
    // Slider specific
    public Color SliderTrack { get; init; }
    public Color SliderThumb { get; init; }
    public Color SliderThumbHover { get; init; }
    
    // Overlay colors
    public Color TooltipBackground { get; init; }
    
    public SpriteFontBase? DefaultFont { get; init; }
    
    // Legacy compatibility properties (deprecated - use semantic names)
    [Obsolete("Use Primary instead")]
    public Color PrimaryColor => Primary;
    [Obsolete("Use Background instead")]
    public Color BackgroundColor => Background;
    [Obsolete("Use Surface instead")]
    public Color SurfaceColor => Surface;
    [Obsolete("Use Border instead")]
    public Color BorderColor => Border;
    [Obsolete("Use TextMain instead")]
    public Color TextColor => TextMain;

    public static Theme Dark => new()
    {
        // Brand & Action
        Primary = new Color(0, 122, 204),
        OnPrimary = Color.White,
        
        // Backgrounds & Surfaces
        Background = new Color(30, 30, 30),
        Surface = new Color(45, 45, 48),
        SurfaceOverlay = new Color(55, 55, 58),
        
        // Content
        TextMain = Color.White,
        TextMuted = new Color(180, 180, 180),
        OnSurface = Color.White,
        
        // Structural
        Border = new Color(63, 63, 70),
        BorderFocus = new Color(0, 122, 204),
        
        // Functional
        Success = new Color(100, 220, 100),
        Error = new Color(255, 100, 100),
        Warning = new Color(255, 200, 100),
        Info = new Color(100, 200, 255),
        
        // Input
        InputBackground = new Color(51, 51, 55),
        
        // Interactive states
        PrimaryHover = new Color(0, 140, 230),
        PrimaryActive = new Color(0, 100, 180),
        SurfaceHover = new Color(60, 60, 65),
        SurfaceActive = new Color(35, 35, 38),
        
        // Slider
        SliderTrack = new Color(63, 63, 70),
        SliderThumb = new Color(180, 180, 180),
        SliderThumbHover = new Color(220, 220, 220),
        
        // Overlay
        TooltipBackground = new Color(30, 30, 30, 240)
    };

    public static Theme Light => new()
    {
        // Brand & Action
        Primary = new Color(0, 120, 215),
        OnPrimary = Color.White,
        
        // Backgrounds & Surfaces
        Background = new Color(240, 240, 240),
        Surface = Color.White,
        SurfaceOverlay = new Color(245, 245, 245),
        
        // Content
        TextMain = Color.Black,
        TextMuted = new Color(100, 100, 100),
        OnSurface = Color.Black,
        
        // Structural
        Border = new Color(200, 200, 200),
        BorderFocus = new Color(0, 120, 215),
        
        // Functional
        Success = new Color(80, 200, 80),
        Error = new Color(200, 50, 50),
        Warning = new Color(200, 150, 50),
        Info = new Color(50, 150, 200),
        
        // Input
        InputBackground = new Color(250, 250, 250),
        
        // Interactive states
        PrimaryHover = new Color(0, 140, 240),
        PrimaryActive = new Color(0, 100, 190),
        SurfaceHover = new Color(235, 235, 235),
        SurfaceActive = new Color(220, 220, 220),
        
        // Slider
        SliderTrack = new Color(200, 200, 200),
        SliderThumb = new Color(100, 100, 100),
        SliderThumbHover = new Color(60, 60, 60),
        
        // Overlay
        TooltipBackground = new Color(50, 50, 50, 240)
    };
}

