using Ignis.Core;
using Ignis.Core.Scenery;
using Ignis.Graphics;
using Samples.Breakout.Services;

namespace Samples.Breakout.Core;

/// <summary>
/// Extended context for Breakout with game-specific services.
/// </summary>
public sealed class BreakoutContext : EngineContext
{
    public IRenderingServer RenderingServer { get; }
    public int Width { get; set; }
    public int Height { get; set; }

    public Window Window { get; }
    public LeaderboardService Leaderboard { get; }
    public LevelService Levels { get; }
    public SettingsService Settings { get; }
    public AudioService Audio { get; }

    public FontHandle Font { get; private set; }

    // Game state
    public int CurrentLevel { get; set; } = 1;
    public int Score { get; set; }
    public int Lives { get; set; } = 3;

    public BreakoutContext(IRenderingServer server, int width, int height, Window window)
    {
        RenderingServer = server;
        Width = width;
        Height = height;
        Window = window;

        Leaderboard = new LeaderboardService();
        Levels = new LevelService();
        Settings = new SettingsService();
        Audio = new AudioService();
    }

    public void Initialize()
    {
        Font = LoadFont();
        Leaderboard.Load();
        Settings.Load();
        Levels.LoadLevels();
    }

    public void SaveSettings()
    {
        Settings.Save();
        Leaderboard.Save();
    }

    public void ResetGame()
    {
        Score = 0;
        Lives = 3;
        CurrentLevel = 1;
    }

    private FontHandle LoadFont()
    {
        string[] fontPaths =
        {
            @"C:\Windows\Fonts\segoeui.ttf",
            @"C:\Windows\Fonts\arial.ttf",
            @"C:\Windows\Fonts\tahoma.ttf",
            "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
            "/System/Library/Fonts/Helvetica.ttc"
        };

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
/// Interface for Breakout scenes that can render.
/// </summary>
public interface IBreakoutScene
{
    void Render(float alpha);
    void OnResize(int width, int height);
}
