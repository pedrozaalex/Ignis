using Crucible.Core.Tests.Ecs;
using Friflo.Engine.ECS;
using CrucibleEntityNode = Crucible.Core.Tests.Ecs.EntityNode;

namespace Crucible.Core.Tests;

/// <summary>
/// Test world that mimics the Rust morphorm_ecs World API.
/// Provides a simple way to create and manage entities for layout testing.
/// </summary>
public class TestWorld : IDisposable
{
    public EntityStore Store { get; }
    public EntityCache Cache { get; }
    private SubLayoutContext _subLayout;

    public SubLayoutContext SubLayout => _subLayout;

    public TestWorld()
    {
        Store = new EntityStore();
        Cache = new EntityCache();
        _subLayout = new SubLayoutContext();
    }

    /// <summary>
    /// Creates a new entity, optionally as a child of the specified parent.
    /// </summary>
    public Entity Add(Entity? parent = null)
    {
        var entity = Store.CreateEntity();
        if (parent.HasValue)
        {
            parent.Value.AddChild(entity);
        }
        return entity;
    }

    /// <summary>
    /// Performs layout on the specified root entity.
    /// </summary>
    public void Layout(Entity root)
    {
        var node = new CrucibleEntityNode(root);
        LayoutEngine.Compute<CrucibleEntityNode, EntityStore, SubLayoutContext, Entity, EntityCache>(node, Cache, Store, ref _subLayout);
    }

    public void Dispose()
    {
        // EntityStore doesn't implement IDisposable, but we keep the pattern
        // for potential future cleanup
    }
}

