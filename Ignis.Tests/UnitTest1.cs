using System.Numerics;
using Ignis.Engine.Core;
using Ignis.Engine.ECS;
using Ignis.Engine.ECS.Components;
using Ignis.Engine.ECS.Systems;

namespace Ignis.Tests;

/// <summary>
/// Unit tests for Phase 1: Core Skeleton & Transform System
/// </summary>
public class TransformSystemTests
{
    /// <summary>
    /// Test that IgnisApp can be created and updated in headless mode
    /// </summary>
    [Fact]
    public void IgnisApp_CanBeCreatedAndUpdated_Headless()
    {
        // Arrange
        var app = new IgnisApp();
        
        // Act
        app.Initialize();
        app.Update(0.016); // Simulate one frame
        
        // Assert
        Assert.NotNull(app.World);
        Assert.NotNull(app.SimulationRoot);
        Assert.True(app.TotalTime > 0);
    }
    
    /// <summary>
    /// Test that child transforms inherit parent transformations
    /// </summary>
    [Fact]
    public void TransformSystem_ChildInheritsParentTransform()
    {
        // Arrange
        var app = new IgnisApp();
        app.SimulationRoot.Add(new TransformSystem());
        
        // Create parent at (10, 0, 0) using archetype
        var parent = app.World.CreateGameObject();
        parent.Position.value = new Vector3(10, 0, 0);
        
        // Create child at (5, 0, 0) relative to parent
        var child = app.World.CreateGameObject();
        child.Position.value = new Vector3(5, 0, 0);
        
        // Build hierarchy
        parent.AddChild(child);
        
        // Act
        app.Update(0.016); // Run transform system
        
        // Assert
        var parentWorld = parent.GetComponent<WorldTransform>().Value;
        var childWorld = child.GetComponent<WorldTransform>().Value;
        
        // Parent should be at (10, 0, 0)
        Assert.Equal(10f, parentWorld.Translation.X, 3);
        Assert.Equal(0f, parentWorld.Translation.Y, 3);
        Assert.Equal(0f, parentWorld.Translation.Z, 3);
        
        // Child should be at parent position + local offset = (10, 0, 0) + (5, 0, 0) = (15, 0, 0)
        Assert.Equal(15f, childWorld.Translation.X, 3);
        Assert.Equal(0f, childWorld.Translation.Y, 3);
        Assert.Equal(0f, childWorld.Translation.Z, 3);
    }
    
    /// <summary>
    /// Test that grandchildren correctly inherit multi-level hierarchy
    /// </summary>
    [Fact]
    public void TransformSystem_MultiLevelHierarchy()
    {
        // Arrange
        var app = new IgnisApp();
        app.SimulationRoot.Add(new TransformSystem());
        
        // Create Root at origin
        var root = app.World.CreateGameObject();
        // Default position is already Vector3.Zero
        
        // Create Child at (10, 0, 0)
        var child = app.World.CreateGameObject();
        child.Position.value = new Vector3(10, 0, 0);
        
        // Create GrandChild at (0, 5, 0)
        var grandChild = app.World.CreateGameObject();
        grandChild.Position.value = new Vector3(0, 5, 0);
        
        // Build hierarchy: Root -> Child -> GrandChild
        root.AddChild(child);
        child.AddChild(grandChild);
        
        // Act
        app.Update(0.016);
        
        // Assert
        var rootWorld = root.GetComponent<WorldTransform>().Value;
        var childWorld = child.GetComponent<WorldTransform>().Value;
        var grandChildWorld = grandChild.GetComponent<WorldTransform>().Value;
        
        // Root at (0, 0, 0)
        Assert.Equal(0f, rootWorld.Translation.X, 3);
        
        // Child at (10, 0, 0)
        Assert.Equal(10f, childWorld.Translation.X, 3);
        Assert.Equal(0f, childWorld.Translation.Y, 3);
        
        // GrandChild at (10, 5, 0)
        Assert.Equal(10f, grandChildWorld.Translation.X, 3);
        Assert.Equal(5f, grandChildWorld.Translation.Y, 3);
        Assert.Equal(0f, grandChildWorld.Translation.Z, 3);
    }
    
    /// <summary>
    /// Test that changing parent transform propagates to children
    /// </summary>
    [Fact]
    public void TransformSystem_ParentChange_PropagagesToChildren()
    {
        // Arrange
        var app = new IgnisApp();
        app.SimulationRoot.Add(new TransformSystem());
        
        var parent = app.World.CreateGameObject();
        parent.Position.value = new Vector3(10, 0, 0);
        
        var child = app.World.CreateGameObject();
        child.Position.value = new Vector3(5, 0, 0);
        
        parent.AddChild(child);
        
        // Initial update
        app.Update(0.016);
        
        var initialChildWorld = child.GetComponent<WorldTransform>().Value.Translation;
        Assert.Equal(15f, initialChildWorld.X, 3); // 10 + 5
        
        // Act: Move parent
        parent.Position.value = new Vector3(20, 0, 0);
        // Setting Position triggers event which marks entity as dirty
        
        app.Update(0.016);
        
        // Assert: Child should have moved with parent
        var newChildWorld = child.GetComponent<WorldTransform>().Value.Translation;
        Assert.Equal(25f, newChildWorld.X, 3); // 20 + 5
    }
    
    /// <summary>
    /// Test that rotation propagates correctly
    /// </summary>
    [Fact]
    public void TransformSystem_RotationPropagates()
    {
        // Arrange
        var app = new IgnisApp();
        app.SimulationRoot.Add(new TransformSystem());
        
        var parent = app.World.CreateGameObject();
        var rotation90Y = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 2); // 90 degrees around Y
        parent.Rotation.value = rotation90Y;
        
        var child = app.World.CreateGameObject();
        child.Position.value = new Vector3(10, 0, 0); // Child at X=10 (relative to parent)
        
        parent.AddChild(child);
        
        // Act
        app.Update(0.016);
        
        // Assert
        var childWorld = child.GetComponent<WorldTransform>().Value.Translation;
        
        // After 90-degree rotation around Y, child at (10,0,0) should be at approximately (0,0,-10)
        Assert.Equal(0f, childWorld.X, 1);
        Assert.Equal(0f, childWorld.Y, 1);
        Assert.Equal(-10f, childWorld.Z, 1);
    }
}

