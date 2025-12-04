using Ignis.Core.Timing;

namespace Ignis.Core.Scenery;

/// <summary>
/// Represents a self-contained state of the game (e.g., Menu, Level1, Level2).
/// Override this to create your game scenes.
/// </summary>
public abstract class Scene
{
    /// <summary>
    /// Called when the scene becomes active. Set up your ECS systems here.
    /// </summary>
    public abstract void OnEnter(EngineContext context);
    
    /// <summary>
    /// Called when the scene is being replaced. Dispose resources here.
    /// </summary>
    public abstract void OnExit();
    
    /// <summary>
    /// Called each frame to update the scene.
    /// </summary>
    public abstract void Update(GameTime time);
}

