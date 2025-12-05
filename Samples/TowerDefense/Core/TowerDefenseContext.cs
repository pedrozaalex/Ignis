using Ignis.Core;
using Ignis.Core.Assets;
using Ignis.Graphics;
using Samples.TowerDefense.Services;

namespace Samples.TowerDefense.Core;

/// <summary>
/// Extended context for Tower Defense with game-specific services.
/// </summary>
public sealed class TowerDefenseContext : GraphicsContext
{
    public IRenderingServer RenderingServer { get; }
    public LevelService Levels { get; }
    public SettingsService Settings { get; }
    public AudioService Audio { get; }

    public FontHandle Font { get; private set; }

    // Game state persisted across scenes
    public int CurrentLevel { get; set; } = 1;
    public int TotalScore { get; set; }
    public int Gold { get; set; } = 100;
    public int Lives { get; set; } = 20;

    public TowerDefenseContext(IRenderingServer server, int width, int height, Window window)
        : base(window, width, height)
    {
        RenderingServer = server;

        Levels = new LevelService();
        Settings = new SettingsService();
        Audio = new AudioService();
    }

    public void Initialize()
    {
        Font = LoadFont();
        Settings.Load();
        Levels.LoadLevels();
    }

    public void SaveSettings()
    {
        Settings.MasterVolume = Audio.MasterVolume;
        Settings.SfxVolume = Audio.SfxVolume;
        Settings.MusicVolume = Audio.MusicVolume;
        Settings.Save();
    }

    public void ResetGame()
    {
        TotalScore = 0;
        Gold = 100;
        Lives = 20;
        CurrentLevel = 1;
    }

    public void StartLevel(int level)
    {
        CurrentLevel = level;
        Gold = 100 + (level - 1) * 25; // More starting gold on later levels
        Lives = 20;
    }

    private FontHandle LoadFont()
    {
        var fontPath = SystemFontLocator.FindSystemFont();
        if (fontPath != null)
        {
            var handle = RenderingServer.CreateFontFromFile(fontPath);
            if (handle.Id != 0) return handle;
        }
        return FontHandle.Invalid;
    }
}

/// <summary>
/// Interface for Tower Defense scenes that can render.
/// </summary>
public interface ITowerDefenseScene
{
    void Render(float alpha);
    void OnResize(int width, int height);
}
