using Ignis.Core;
using Ignis.Core.Assets;
using Ignis.Graphics;
using Samples.Breakout.Services;

namespace Samples.Breakout.Core;

/// <summary>
/// Extended context for Breakout with game-specific services.
/// </summary>
public sealed class BreakoutContext : GraphicsContext
{
    public IRenderingServer RenderingServer { get; }
    public LeaderboardService Leaderboard { get; }
    public LevelService Levels { get; }
    public SettingsService Settings { get; }
    public AudioService Audio { get; }

    public FontHandle Font { get; private set; }

    // Game state (synced from ECS BreakoutState)
    public int CurrentLevel { get; set; } = 1;
    public int Score { get; set; }
    public int Lives { get; set; } = 3;

    public BreakoutContext(IRenderingServer server, int width, int height, Window window)
        : base(window, width, height)
    {
        RenderingServer = server;

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
/// Interface for Breakout scenes that can render.
/// </summary>
public interface IBreakoutScene
{
    void Render(float alpha);
    void OnResize(int width, int height);
}
