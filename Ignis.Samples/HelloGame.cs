using System.Numerics;
using Ignis.Engine.Core;
using Ignis.Engine.ECS;
using Ignis.Engine.ECS.Components;
using Ignis.Engine.ECS.Systems;
using Friflo.Engine.ECS;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Samples;

/// <summary>
/// Sample Game demonstrating Phase 1 implementation
/// Creates a simple hierarchy: Root -> Child -> GrandChild
/// Rotates the root to demonstrate transform propagation
/// </summary>
public class HelloGame() : IgnisGame(new IgnisApp(new EngineSettings
{
    WindowTitle = "Ignis Engine - Phase 1 Sample",
    WindowWidth = 1280,
    WindowHeight = 720
}))
{
    private Entity _rootEntity;
    private Entity _childEntity;
    private Entity _grandChildEntity;

    protected override void Initialize()
    {
        base.Initialize();
        
        // Add TransformSystem to the simulation root
        App.SimulationRoot.Add(new TransformSystem());
        
        // Setup the entity hierarchy
        SetupHierarchy();
    }
    
    private void SetupHierarchy()
    {
        // Create Root entity at origin using archetype
        _rootEntity = App.World.CreateGameObject();
        // Default values are already set by CreateGameObject
        
        // Create Child entity at offset (10, 0, 0) from Root
        _childEntity = App.World.CreateGameObject();
        _childEntity.Position.value = new Vector3(10, 0, 0);
        
        // Create GrandChild entity at offset (0, 5, 0) from Child
        _grandChildEntity = App.World.CreateGameObject();
        _grandChildEntity.Position.value = new Vector3(0, 5, 0);
        
        // Build hierarchy: Root -> Child -> GrandChild
        _rootEntity.AddChild(_childEntity);
        _childEntity.AddChild(_grandChildEntity);
        
        Console.WriteLine("=== Phase 1 Sample: Hierarchy Setup ===");
        Console.WriteLine($"Root Entity: {_rootEntity.Id}");
        Console.WriteLine($"Child Entity: {_childEntity.Id} (Parent: {_childEntity.Parent.Id})");
        Console.WriteLine($"GrandChild Entity: {_grandChildEntity.Id} (Parent: {_grandChildEntity.Parent.Id})");
        Console.WriteLine("=======================================");
    }
    
    protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
    {
        // Rotate the root entity every frame
        // Rotate around Y-axis (up)
        float rotationSpeed = 0.5f; // radians per second
        float deltaRotation = rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        var currentRotation = _rootEntity.Rotation.value;
        var deltaQuat = Quaternion.CreateFromAxisAngle(Vector3.UnitY, deltaRotation);
        _rootEntity.Rotation.value = Quaternion.Multiply(currentRotation, deltaQuat);
        // Note: Setting Rotation triggers the OnComponentChanged event which marks the entity as dirty
        
        base.Update(gameTime);
        
        // Log world matrices every 60 frames (1 second at 60 FPS)
        if (gameTime.TotalGameTime.TotalSeconds % 1.0 < 0.016)
        {
            LogWorldMatrices();
        }
    }
    
    private void LogWorldMatrices()
    {
        var rootWorld = _rootEntity.GetComponent<WorldTransform>().Value;
        var childWorld = _childEntity.GetComponent<WorldTransform>().Value;
        var grandChildWorld = _grandChildEntity.GetComponent<WorldTransform>().Value;
        
        Console.WriteLine($"\n[{App.TotalTime:F2}s] World Matrices:");
        Console.WriteLine($"  Root Position: {rootWorld.Translation}");
        Console.WriteLine($"  Child Position: {childWorld.Translation}");
        Console.WriteLine($"  GrandChild Position: {grandChildWorld.Translation}");
    }
    
    protected override void OnRenderUI(SpriteBatch spriteBatch)
    {
        // TODO: Phase 4 - Render UI with instructions
        base.OnRenderUI(spriteBatch);
    }
}

