using Ignis.Core;
using Ignis.Core.Scenery;
using Ignis.Graphics;
using Samples.TowerDefense.Services;

namespace Samples.TowerDefense.Core;

/// <summary>
/// Extended context for Tower Defense with game-specific services.
/// </summary>
public sealed class TowerDefenseContext : EngineContext
{
    public IRenderingServer RenderingServer { get; }
    public int Width { get; set; }
    public int Height { get; set; }

    public Window Window { get; }
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
    {
        RenderingServer = server;
        Width = width;
        Height = height;
        Window = window;

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
        string[] fontPaths =
        [
            @"C:\Windows\Fonts\segoeui.ttf",
            @"C:\Windows\Fonts\arial.ttf",
            @"C:\Windows\Fonts\tahoma.ttf",
            "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
            "/System/Library/Fonts/Helvetica.ttc"
        ];

        foreach (var path in fontPaths)
        {
            if (File.Exists(path))
            {
                var handle = RenderingServer.CreateFontFromFile(path);
                if (handle.Id != 0) return handle;
            }
        }

        return FontHandle.Invalid;
    }

    public InputState? GetInput() => Window.InputState;
}

/// <summary>
/// Interface for Tower Defense scenes that can render.
/// </summary>
public interface ITowerDefenseScene
{
    void Render(float alpha);
    void OnResize(int width, int height);
}
