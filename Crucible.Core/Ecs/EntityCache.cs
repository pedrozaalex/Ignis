using Crucible.Core.Interfaces;
using Crucible.Core.Types;
using Friflo.Engine.ECS;

namespace Crucible.Core.Ecs;

/// <summary>
/// Cache implementation using Friflo ECS components.
/// </summary>
public class EntityCache : ICache<Entity>
{
    public float Width(Entity node)
    {
        if (node.TryGetComponent<LayoutBounds>(out var bounds))
            return bounds.Width;
        return 0;
    }

    public float Height(Entity node)
    {
        if (node.TryGetComponent<LayoutBounds>(out var bounds))
            return bounds.Height;
        return 0;
    }

    public float PosX(Entity node)
    {
        if (node.TryGetComponent<LayoutBounds>(out var bounds))
            return bounds.PosX;
        return 0;
    }

    public float PosY(Entity node)
    {
        if (node.TryGetComponent<LayoutBounds>(out var bounds))
            return bounds.PosY;
        return 0;
    }

    public void SetBounds(Entity node, float posX, float posY, float width, float height)
    {
        var bounds = new LayoutBounds
        {
            PosX = posX,
            PosY = posY,
            Width = width,
            Height = height
        };
        
        if (node.HasComponent<LayoutBounds>())
        {
            node.GetComponent<LayoutBounds>() = bounds;
        }
        else
        {
            node.AddComponent(bounds);
        }
    }

    public Rect? Bounds(Entity node)
    {
        if (node.TryGetComponent<LayoutBounds>(out var bounds))
            return bounds.ToRect();
        return null;
    }
}
