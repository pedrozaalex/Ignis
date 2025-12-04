namespace Ignis.Core;

/// <summary>
/// Abstracts the platform (Windows, Console, Headless Test).
/// Handles the raw tick from the OS.
/// </summary>
public interface IGameHost
{
    /// <summary>Run the game loop until exit.</summary>
    void Run();
    
    /// <summary>Request the host to exit.</summary>
    void Exit();
    
    /// <summary>Width of the render target in pixels.</summary>
    int Width { get; }
    
    /// <summary>Height of the render target in pixels.</summary>
    int Height { get; }
    
    /// <summary>Fired when the render target is resized.</summary>
    event Action<int, int>? OnResize;
}

