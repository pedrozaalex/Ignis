using Ignis.Core.Timing;

namespace Ignis.Core.Scenery;

/// <summary>
/// Manages scene transitions and updates.
/// </summary>
public sealed class SceneManager
{
    private readonly EngineContext _context;
    private Scene? _currentScene;
    
    /// <summary>The currently active scene.</summary>
    public Scene? CurrentScene => _currentScene;
    
    public SceneManager(EngineContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Load a new scene, replacing the current one.
    /// </summary>
    public void LoadScene(Scene scene)
    {
        _currentScene?.OnExit();
        _currentScene = scene;
        _currentScene.OnEnter(_context);
    }
    
    /// <summary>
    /// Update the current scene.
    /// </summary>
    public void Update(GameTime time)
    {
        _currentScene?.Update(time);
    }
}

