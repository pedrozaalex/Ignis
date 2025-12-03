using Crucible.Core.Interfaces;
using Crucible.Core.Types;
using Friflo.Engine.ECS;

namespace Crucible.Core.Ecs;

/// <summary>
/// Wrapper around Entity that implements INode interface for layout calculations.
/// </summary>
public readonly struct EntityNode : INode<EntityStore, SubLayoutContext, Entity>
{
    private readonly Entity _entity;

    public EntityNode(Entity entity)
    {
        _entity = entity;
    }

    public Entity Key => _entity;

    public IEnumerable<INode<EntityStore, SubLayoutContext, Entity>> Children(EntityStore tree)
    {
        foreach (var child in _entity.ChildEntities)
        {
            yield return new EntityNode(child);
        }
    }

    public bool Visible => !_entity.Tags.Has<InvisibleTag>();

    private ref readonly LayoutProperties Props
    {
        get
        {
            if (_entity.TryGetComponent<LayoutProperties>(out var props))
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
    public Units? MinVerticalGap => Props.MinVerticalGap;
    public Units? MinHorizontalGap => Props.MinHorizontalGap;
    public Units? MaxVerticalGap => Props.MaxVerticalGap;
    public Units? MaxHorizontalGap => Props.MaxHorizontalGap;
    
    public Units? MinWidth => Props.MinWidth;
    public Units? MinHeight => Props.MinHeight;
    public Units? MaxWidth => Props.MaxWidth;
    public Units? MaxHeight => Props.MaxHeight;
    
    public Units? BorderLeft => Props.BorderLeft;
    public Units? BorderRight => Props.BorderRight;
    public Units? BorderTop => Props.BorderTop;
    public Units? BorderBottom => Props.BorderBottom;
    
    public float? VerticalScroll => Props.VerticalScroll;
    public float? HorizontalScroll => Props.HorizontalScroll;

    public IReadOnlyList<Units>? GridColumns
    {
        get
        {
            if (_entity.TryGetComponent<GridDefinition>(out var grid))
                return grid.Columns;
            return null;
        }
    }

    public IReadOnlyList<Units>? GridRows
    {
        get
        {
            if (_entity.TryGetComponent<GridDefinition>(out var grid))
                return grid.Rows;
            return null;
        }
    }

    public int? ColumnStart => Props.ColumnStart;
    public int? RowStart => Props.RowStart;
    public int? ColumnSpan => Props.ColumnSpan;
    public int? RowSpan => Props.RowSpan;

    public (float Width, float Height)? ContentSize(ref SubLayoutContext subLayout, float? parentWidth, float? parentHeight)
    {
        var func = subLayout.GetContentSize(_entity);
        return func?.Invoke(parentWidth, parentHeight);
    }

    public static implicit operator EntityNode(Entity entity) => new(entity);
    public Entity Entity => _entity;
}

