using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Ignis.Engine.Assets;
using Ignis.Engine.Input;

namespace Ignis.Engine.Core;

/// <summary>
///     Headless Core - Manages the ECS World and simulation loop
///     Used by IgnisGame (Visual) and Ignis.Tests (Headless)
/// </summary>
public class IgnisApp
{
    public IgnisApp(EngineSettings? settings = null)
    {
        Settings = settings ?? new EngineSettings();

        // Create ECS World
        World = new EntityStore();

        // Create Asset Manager
        AssetManager = new AssetManager();

        // Create Input Service
        Input = new InputService();

        // TODO: Event-driven reactivity will be implemented when we understand Friflo's event system better
        // For now, TransformSystem will check all entities with WorldTransform component

        // Create System Root
        SimulationRoot = new SystemRoot(World);

        TotalTime = 0.0;
    }

    /// <summary>
    ///     The ECS World containing all entities and components
    /// </summary>
    public EntityStore World { get; }

    /// <summary>
    ///     Root system group for all simulation systems
    /// </summary>
    public SystemRoot SimulationRoot { get; }

    /// <summary>
    ///     Engine configuration
    /// </summary>
    public EngineSettings Settings { get; }

    /// <summary>
    ///     Total elapsed time since engine start (in seconds)
    /// </summary>
    public double TotalTime { get; private set; }

    /// <summary>
    ///     Asset manager for loading and managing game assets
    /// </summary>
    public AssetManager AssetManager { get; private set; }

    /// <summary>
    ///     Input service for handling keyboard and mouse input
    /// </summary>
    public InputService Input { get; }

    /// <summary>
    ///     Update the simulation by one step
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update (in seconds)</param>
    public void Update(double deltaTime)
    {
        Input.Update();

        TotalTime += deltaTime;

        // Execute all systems in the simulation root
        SimulationRoot.Update(default);
    }

    /// <summary>
    ///     Initialize the app (called once at startup)
    /// </summary>
    public virtual void Initialize()
    {
        // Override in derived classes for custom initialization
    }

    /// <summary>
    ///     Load content (called once after Initialize)
    /// </summary>
    public virtual void LoadContent()
    {
        // Override in derived classes for content loading
    }
}