using System.Numerics;
using Friflo.Engine.ECS;
using Ignis.Engine.ECS;
using Ignis.Engine.ECS.Bridge;
using Xunit;

namespace Ignis.Tests.ECS.Bridge;

/// <summary>
/// Tests for ReactiveQuery - SignalList that synchronizes with ECS queries
/// </summary>
public class ReactiveQueryTests
{
    [Fact]
    public void ReactiveQuery_InitialPopulation_ContainsMatchingEntities()
    {
        // Arrange
        var store = new EntityStore();
        for (int i = 0; i < 5; i++)
        {
            var entity = store.CreateGameObject();
            entity.Position.value = new Vector3(i, i, i);
        }

        // Act
        var query = store.Query<Position>();
        var reactiveQuery = new ReactiveQuery(query);

        // Assert
        Assert.Equal(5, reactiveQuery.Count);
    }

    [Fact]
    public void ReactiveQuery_Update_DetectsNewEntities()
    {
        // Arrange
        var store = new EntityStore();
        var query = store.Query<Position>();
        var reactiveQuery = new ReactiveQuery(query);
        var addedEntities = new List<Entity>();

        reactiveQuery.ItemAdded += (entity, index) => addedEntities.Add(entity);

        Assert.Equal(0, reactiveQuery.Count);

        // Act
        var newEntity = store.CreateGameObject();
        newEntity.Position.value = new Vector3(1, 1, 1);
        reactiveQuery.Update();

        // Assert
        Assert.Contains(newEntity, addedEntities);
        Assert.Contains(newEntity, reactiveQuery.Items);
    }

    [Fact]
    public void ReactiveQuery_Update_DetectsRemovedEntities()
    {
        // Arrange
        var store = new EntityStore();
        var entity = store.CreateGameObject();
        entity.Position.value = new Vector3(1, 1, 1);

        var query = store.Query<Position>();
        var reactiveQuery = new ReactiveQuery(query);
        reactiveQuery.Update();

        var removedEntities = new List<Entity>();
        reactiveQuery.ItemRemoved += (e, index) => removedEntities.Add(e);

        Assert.Single(reactiveQuery.Items);

        // Act
        entity.DeleteEntity();
        reactiveQuery.Update();

        // Assert
        Assert.Contains(entity, removedEntities);
        Assert.Empty(reactiveQuery.Items);
    }

    [Fact]
    public void ReactiveQuery_MultipleUpdates_MaintainsConsistency()
    {
        // Arrange
        var store = new EntityStore();
        var query = store.Query<Position>();
        var reactiveQuery = new ReactiveQuery(query);

        // Act & Assert - Add entities
        var e1 = store.CreateGameObject();
        e1.Position.value = Vector3.One;
        reactiveQuery.Update();
        Assert.Single(reactiveQuery.Items);

        var e2 = store.CreateGameObject();
        e2.Position.value = Vector3.One;
        reactiveQuery.Update();
        Assert.Equal(2, reactiveQuery.Count);

        // Remove one
        e1.DeleteEntity();
        reactiveQuery.Update();
        Assert.Single(reactiveQuery.Items);
        Assert.Contains(e2, reactiveQuery.Items);
        Assert.DoesNotContain(e1, reactiveQuery.Items);
    }
}

