using System.Numerics;
using Friflo.Engine.ECS;
using Ignis.Engine.ECS;
using Ignis.Engine.ECS.Bridge;
using Ignis.Engine.Reactive;
using Xunit;

namespace Ignis.Tests.ECS.Bridge;

/// <summary>
/// Tests for ComponentSignal&lt;T&gt; - Reactive bridge between ECS components and Signals
/// </summary>
public class ComponentSignalTests
{
    [Fact]
    public void ComponentSignal_ReadPropagation_ReadsFromECS()
    {
        // Arrange
        var store = new EntityStore();
        var entity = store.CreateGameObject();
        entity.Position.value = new Vector3(5, 10, 15);

        // Act
        var signal = entity.ComponentSignal<Position>();
        var position = signal.Value;

        // Assert
        Assert.Equal(new Vector3(5, 10, 15), position.value);
    }

    [Fact]
    public void ComponentSignal_WritePropagation_WritesToECS()
    {
        // Arrange
        var store = new EntityStore();
        var entity = store.CreateGameObject();
        entity.Position.value = new Vector3(0, 0, 0);

        var signal = entity.ComponentSignal<Position>();

        // Act
        signal.Value = new Position(10, 20, 30);

        // Assert
        Assert.Equal(new Vector3(10, 20, 30), entity.Position.value);
    }

    [Fact]
    public void ComponentSignal_Polling_DetectsExternalECSChanges()
    {
        // Arrange
        var store = new EntityStore();
        var entity = store.CreateGameObject();
        entity.Position.value = new Vector3(0, 0, 0);

        var signal = entity.ComponentSignal<Position>();
        var notificationCount = 0;

        var effect = new Effect(() =>
        {
            _ = signal.Value;
            notificationCount++;
        });

        Assert.Equal(1, notificationCount);

        // Act - Modify directly in ECS
        entity.Position.value = new Vector3(100, 200, 300);
        signal.Poll();

        // Assert
        Assert.Equal(new Vector3(100, 200, 300), signal.Value.value);
        Assert.Equal(2, notificationCount);
    }

    [Fact]
    public void ComponentSignal_DeletedEntity_ReturnsDefault()
    {
        // Arrange
        var store = new EntityStore();
        var entity = store.CreateGameObject();
        entity.Position.value = new Vector3(1, 2, 3);

        var signal = entity.ComponentSignal<Position>();

        // Act
        entity.DeleteEntity();
        var position = signal.Value;

        // Assert - Should return default and not crash
        Assert.Equal(default, position);
    }

    [Fact]
    public void ComponentSignal_DeletedEntity_WritesDoNotCrash()
    {
        // Arrange
        var store = new EntityStore();
        var entity = store.CreateGameObject();
        var signal = entity.ComponentSignal<Position>();

        entity.DeleteEntity();

        // Act & Assert - Should not throw
        signal.Value = new Position(1, 2, 3);
        var result = signal.Value;
        Assert.Equal(default, result);
    }
}

