namespace Ignis.Engine.Core;

/// <summary>
///     Configuration POCO for engine settings
/// </summary>
public record struct EngineSettings()
{
    /// <summary>
    ///     Target frames per second for the simulation
    /// </summary>
    public int TargetFPS { get; set; } = 60;

    /// <summary>
    ///     Window width (for visual mode)
    /// </summary>
    public int WindowWidth { get; set; } = 1280;

    /// <summary>
    ///     Window height (for visual mode)
    /// </summary>
    public int WindowHeight { get; set; } = 720;

    /// <summary>
    ///     Window title
    /// </summary>
    public string WindowTitle { get; set; } = "Ignis Engine";

    /// <summary>
    ///     Enable VSync
    /// </summary>
    public bool VSync { get; set; } = true;

    /// <summary>
    ///     Enable UI debug visualization (draws bounds around all UI elements, highlights zero-size elements)
    /// </summary>
    public bool DebugUI { get; set; } = false;
}