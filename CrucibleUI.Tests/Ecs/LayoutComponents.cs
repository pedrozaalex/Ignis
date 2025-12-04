using CrucibleUI.Types;
using Friflo.Engine.ECS;

namespace CrucibleUI.Tests.Ecs;

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
    public Units? MinVerticalGap;
    public Units? MinHorizontalGap;
    public Units? MaxVerticalGap;
    public Units? MaxHorizontalGap;
    
    public Units? MinWidth;
    public Units? MinHeight;
    public Units? MaxWidth;
    public Units? MaxHeight;
    
    public Units? BorderLeft;
    public Units? BorderRight;
    public Units? BorderTop;
    public Units? BorderBottom;
    
    public float? VerticalScroll;
    public float? HorizontalScroll;
    
    public int? ColumnStart;
    public int? RowStart;
    public int? ColumnSpan;
    public int? RowSpan;
}

/// <summary>
/// Component storing grid layout definitions.
/// </summary>
public struct GridDefinition : IComponent
{
    public Units[]? Columns;
    public Units[]? Rows;
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
/// Tag for invisible entities (excluded from layout).
/// </summary>
public struct InvisibleTag : ITag { }

/// <summary>
/// Component for custom content size calculation.
/// </summary>
public struct ContentSizeFunc : IComponent
{
    public delegate (float Width, float Height) ContentSizeDelegate(float? parentWidth, float? parentHeight);
    
    // Note: Friflo components should be blittable, so we store delegate separately
    // This component just marks that the entity has a content size function
    public bool HasContentSize;
}
