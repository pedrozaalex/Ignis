using CrucibleUI.Interfaces;
using CrucibleUI.Types;
using Friflo.Engine.ECS;

namespace Samples.Layout;

/// <summary>
/// Wrapper around Entity that implements INode interface for layout calculations.
/// </summary>
public readonly struct LayoutNode : INode<EntityStore, SubLayoutContext, Entity>
{
    private readonly Entity _entity;

    public LayoutNode(Entity entity)
    {
        _entity = entity;
    }

    public Entity Key => _entity;

    public IEnumerable<INode<EntityStore, SubLayoutContext, Entity>> Children(EntityStore tree)
    {
        foreach (var child in _entity.ChildEntities)
        {
            yield return new LayoutNode(child);
        }
    }

    public bool Visible => !_entity.Tags.Has<InvisibleTag>();

    private ref readonly LayoutProperties Props
    {
        get
        {
            if (_entity.TryGetComponent<LayoutProperties>(out _))
                return ref _entity.GetComponent<LayoutProperties>();
            return ref DefaultProps;
        }
    }

    private static readonly LayoutProperties DefaultProps = default;

    public LayoutType? LayoutType => Props.LayoutType;
    public PositionType? PositionType => Props.PositionType;
    public Alignment? Alignment => Props.Alignment;
    
    public Units? Width => Props.Width;
    public Units? Height => Props.Height;
    
    public Units? Left => Props.Left;
    public Units? Right => Props.Right;
    public Units? Top => Props.Top;
    public Units? Bottom => Props.Bottom;
    
    public Units? PaddingLeft => Props.PaddingLeft;
    public Units? PaddingRight => Props.PaddingRight;
    public Units? PaddingTop => Props.PaddingTop;
    public Units? PaddingBottom => Props.PaddingBottom;
    
    public Units? VerticalGap => Props.VerticalGap;
    public Units? HorizontalGap => Props.HorizontalGap;
    public Units? MinVerticalGap => null;
    public Units? MinHorizontalGap => null;
    public Units? MaxVerticalGap => null;
    public Units? MaxHorizontalGap => null;
    
    public Units? MinWidth => Props.MinWidth;
    public Units? MinHeight => Props.MinHeight;
    public Units? MaxWidth => Props.MaxWidth;
    public Units? MaxHeight => Props.MaxHeight;
    
    public Units? BorderLeft => null;
    public Units? BorderRight => null;
    public Units? BorderTop => null;
    public Units? BorderBottom => null;
    
    public float? VerticalScroll => null;
    public float? HorizontalScroll => null;

    public IReadOnlyList<Units>? GridColumns => null;
    public IReadOnlyList<Units>? GridRows => null;

    public int? ColumnStart => null;
    public int? RowStart => null;
    public int? ColumnSpan => null;
    public int? RowSpan => null;

    public (float Width, float Height)? ContentSize(ref SubLayoutContext subLayout, float? parentWidth, float? parentHeight)
    {
        return null;
    }

    public static implicit operator LayoutNode(Entity entity) => new(entity);
    public Entity Entity => _entity;
}

/// <summary>
/// Context for content size calculations (unused in this sample).
/// </summary>
public struct SubLayoutContext { }

