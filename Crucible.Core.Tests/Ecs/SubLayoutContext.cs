using Friflo.Engine.ECS;

namespace Crucible.Core.Tests.Ecs;

/// <summary>
/// Sublayout context for content size calculations.
/// Stores delegates for entities that need custom content sizing.
/// </summary>
public class SubLayoutContext
{
    private readonly Dictionary<int, ContentSizeFunc.ContentSizeDelegate> _contentSizeFuncs = new();

    public void SetContentSize(Entity entity, ContentSizeFunc.ContentSizeDelegate func)
    {
        _contentSizeFuncs[entity.Id] = func;
        
        if (!entity.HasComponent<ContentSizeFunc>())
        {
            entity.AddComponent(new ContentSizeFunc { HasContentSize = true });
        }
    }

    public ContentSizeFunc.ContentSizeDelegate? GetContentSize(Entity entity)
    {
        return _contentSizeFuncs.GetValueOrDefault(entity.Id);
    }
}
