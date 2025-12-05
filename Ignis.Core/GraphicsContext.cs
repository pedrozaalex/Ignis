namespace Ignis.Core;

/// <summary>
/// Base context for games/samples that need graphics rendering.
/// Extends EngineContext with window, dimensions, and input access.
/// </summary>
public class GraphicsContext : EngineContext
{
    /// <summary>The window instance.</summary>
    public Window Window { get; }

    /// <summary>Current viewport width in pixels.</summary>
    public int Width { get; set; }

    /// <summary>Current viewport height in pixels.</summary>
    public int Height { get; set; }

    public GraphicsContext(Window window, int width, int height)
    {
        Window = window;
        Width = width;
        Height = height;
    }

    /// <summary>Get the current input state from the window.</summary>
    public InputState? GetInput() => Window.InputState;

    /// <summary>Update dimensions (call on window resize).</summary>
    public void UpdateDimensions(int width, int height)
    {
        Width = width;
        Height = height;
    }
}
