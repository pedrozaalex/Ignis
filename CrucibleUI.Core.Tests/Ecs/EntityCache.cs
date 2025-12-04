using CrucibleUI.Core.Interfaces;
using CrucibleUI.Core.Types;
using Friflo.Engine.ECS;

namespace CrucibleUI.Core.Tests.Ecs;

/// <summary>
/// Cache implementation using Friflo ECS components.
/// </summary>
public class EntityCache : ICache<Entity>
{
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
