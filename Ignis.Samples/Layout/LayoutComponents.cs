using CrucibleUI.Core.Interfaces;
using CrucibleUI.Core.Types;
using Friflo.Engine.ECS;

namespace Ignis.Samples.Layout;

/// <summary>
/// Component storing layout properties for an entity.
/// </summary>
public struct LayoutProperties : IComponent
{
    public LayoutType? LayoutType;
    public PositionType? PositionType;
    public Alignment? Alignment;
    
    public Units? Width;
    public Units? Height;
    
    public Units? Left;
    public Units? Right;
    public Units? Top;
    public Units? Bottom;
    
    public Units? PaddingLeft;
    public Units? PaddingRight;
    public Units? PaddingTop;
    public Units? PaddingBottom;
    
    public Units? VerticalGap;
    public Units? HorizontalGap;
    
    public Units? MinWidth;
    public Units? MinHeight;
    public Units? MaxWidth;
    public Units? MaxHeight;
}

/// <summary>
/// Component storing computed layout bounds.
/// </summary>
public struct LayoutBounds : IComponent
{
    public float PosX;
    public float PosY;
    public float Width;
    public float Height;

    public readonly Rect ToRect() => new(PosX, PosY, Width, Height);
}

/// <summary>
/// Component storing the color for rendering.
/// </summary>
public struct ShapeColor : IComponent
{
    public float R, G, B, A;
    
    public ShapeColor(float r, float g, float b, float a = 1f)
    {
        R = r; G = g; B = b; A = a;
    }
    
    public static ShapeColor Red => new(1f, 0.3f, 0.3f);
    public static ShapeColor Green => new(0.3f, 1f, 0.3f);
    public static ShapeColor Blue => new(0.3f, 0.3f, 1f);
    public static ShapeColor Yellow => new(1f, 1f, 0.3f);
    public static ShapeColor Cyan => new(0.3f, 1f, 1f);
    public static ShapeColor Magenta => new(1f, 0.3f, 1f);
    public static ShapeColor White => new(1f, 1f, 1f);
    public static ShapeColor Gray => new(0.5f, 0.5f, 0.5f);
}

/// <summary>
/// Tag for invisible entities.
/// </summary>
public struct InvisibleTag : ITag { }

