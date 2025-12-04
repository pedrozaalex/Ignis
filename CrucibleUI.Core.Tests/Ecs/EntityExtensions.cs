using CrucibleUI.Core.Types;
using Friflo.Engine.ECS;

namespace CrucibleUI.Core.Tests.Ecs;

/// <summary>
/// Extension methods for Entity to provide a fluent API similar to the Rust morphorm tests.
/// </summary>
public static class EntityExtensions
{
    /// <summary>
    /// Gets or creates the LayoutProperties component for an entity.
    /// </summary>
    private static ref LayoutProperties GetOrAddLayoutProps(this Entity entity)
    {
        if (!entity.HasComponent<LayoutProperties>())
        {
            entity.AddComponent(new LayoutProperties());
        }
        return ref entity.GetComponent<LayoutProperties>();
    }

    public static Entity SetWidth(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().Width = value;
        return entity;
    }

    public static Entity SetHeight(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().Height = value;
        return entity;
    }

    public static Entity SetLayoutType(this Entity entity, LayoutType value)
    {
        entity.GetOrAddLayoutProps().LayoutType = value;
        return entity;
    }

    public static Entity SetPositionType(this Entity entity, PositionType value)
    {
        entity.GetOrAddLayoutProps().PositionType = value;
        return entity;
    }

    public static Entity SetAlignment(this Entity entity, Alignment value)
    {
        entity.GetOrAddLayoutProps().Alignment = value;
        return entity;
    }

    public static Entity SetLeft(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().Left = value;
        return entity;
    }

    public static Entity SetRight(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().Right = value;
        return entity;
    }

    public static Entity SetTop(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().Top = value;
        return entity;
    }

    public static Entity SetBottom(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().Bottom = value;
        return entity;
    }

    public static Entity SetPaddingLeft(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().PaddingLeft = value;
        return entity;
    }

    public static Entity SetPaddingRight(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().PaddingRight = value;
        return entity;
    }

    public static Entity SetPaddingTop(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().PaddingTop = value;
        return entity;
    }

    public static Entity SetPaddingBottom(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().PaddingBottom = value;
        return entity;
    }

    public static Entity SetPadding(this Entity entity, Units value)
    {
        ref var props = ref entity.GetOrAddLayoutProps();
        props.PaddingLeft = value;
        props.PaddingRight = value;
        props.PaddingTop = value;
        props.PaddingBottom = value;
        return entity;
    }

    public static Entity SetVerticalGap(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().VerticalGap = value;
        return entity;
    }

    public static Entity SetHorizontalGap(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().HorizontalGap = value;
        return entity;
    }

    public static Entity SetMinVerticalGap(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().MinVerticalGap = value;
        return entity;
    }

    public static Entity SetMinHorizontalGap(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().MinHorizontalGap = value;
        return entity;
    }

    public static Entity SetMaxVerticalGap(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().MaxVerticalGap = value;
        return entity;
    }

    public static Entity SetMaxHorizontalGap(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().MaxHorizontalGap = value;
        return entity;
    }

    public static Entity SetMinWidth(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().MinWidth = value;
        return entity;
    }

    public static Entity SetMinHeight(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().MinHeight = value;
        return entity;
    }

    public static Entity SetMaxWidth(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().MaxWidth = value;
        return entity;
    }

    public static Entity SetMaxHeight(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().MaxHeight = value;
        return entity;
    }

    public static Entity SetBorderLeft(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().BorderLeft = value;
        return entity;
    }

    public static Entity SetBorderRight(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().BorderRight = value;
        return entity;
    }

    public static Entity SetBorderTop(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().BorderTop = value;
        return entity;
    }

    public static Entity SetBorderBottom(this Entity entity, Units value)
    {
        entity.GetOrAddLayoutProps().BorderBottom = value;
        return entity;
    }

    public static Entity SetBorder(this Entity entity, Units value)
    {
        ref var props = ref entity.GetOrAddLayoutProps();
        props.BorderLeft = value;
        props.BorderRight = value;
        props.BorderTop = value;
        props.BorderBottom = value;
        return entity;
    }

    public static Entity SetVisibility(this Entity entity, bool visible)
    {
        if (visible)
        {
            entity.RemoveTag<InvisibleTag>();
        }
        else
        {
            entity.AddTag<InvisibleTag>();
        }
        return entity;
    }

    public static Entity SetGridColumns(this Entity entity, params Units[] columns)
    {
        if (!entity.HasComponent<GridDefinition>())
        {
            entity.AddComponent(new GridDefinition());
        }
        ref var grid = ref entity.GetComponent<GridDefinition>();
        grid.Columns = columns;
        return entity;
    }

    public static Entity SetGridRows(this Entity entity, params Units[] rows)
    {
        if (!entity.HasComponent<GridDefinition>())
        {
            entity.AddComponent(new GridDefinition());
        }
        ref var grid = ref entity.GetComponent<GridDefinition>();
        grid.Rows = rows;
        return entity;
    }

    public static Entity SetColumnStart(this Entity entity, int value)
    {
        entity.GetOrAddLayoutProps().ColumnStart = value;
        return entity;
    }

    public static Entity SetRowStart(this Entity entity, int value)
    {
        entity.GetOrAddLayoutProps().RowStart = value;
        return entity;
    }

    public static Entity SetColumnSpan(this Entity entity, int value)
    {
        entity.GetOrAddLayoutProps().ColumnSpan = value;
        return entity;
    }

    public static Entity SetRowSpan(this Entity entity, int value)
    {
        entity.GetOrAddLayoutProps().RowSpan = value;
        return entity;
    }

    public static Entity SetContentSize(this Entity entity, SubLayoutContext context, ContentSizeFunc.ContentSizeDelegate func)
    {
        context.SetContentSize(entity, func);
        return entity;
    }

    /// <summary>
    /// Creates a child entity under this entity.
    /// </summary>
    public static Entity CreateChild(this Entity parent)
    {
        var child = parent.Store.CreateEntity();
        parent.AddChild(child);
        return child;
    }
}
