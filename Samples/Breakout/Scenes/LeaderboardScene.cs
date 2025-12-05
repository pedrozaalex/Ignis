using System.Numerics;
using Ignis.Core;
using Ignis.Core.Scenery;
using Ignis.Core.Timing;
using Ignis.Graphics;
using Samples.Breakout.Core;
using Samples.Common;
using Silk.NET.Input;

namespace Samples.Breakout.Scenes;

/// <summary>
/// Leaderboard display scene.
/// </summary>
public sealed class LeaderboardScene : Scene, IBreakoutScene
{
    private readonly BreakoutContext _context;
    private readonly SceneManager _sceneManager;

    private readonly bool _isPostGame;
    private readonly int _finalScore;
    private readonly int _finalLevel;

    private bool _enteringName;
    private string _playerName = "";
    private int _newEntryIndex = -1;

    private int _width;
    private int _height;

    public LeaderboardScene(BreakoutContext context, SceneManager sceneManager, bool isPostGame, int score, int level)
    {
        _context = context;
        _sceneManager = sceneManager;
        _isPostGame = isPostGame;
        _finalScore = score;
        _finalLevel = level;
        _width = context.Width;
        _height = context.Height;
    }

    public override void OnEnter(EngineContext context)
    {
        // Check if score qualifies for leaderboard
        if (_isPostGame && _context.Leaderboard.IsHighScore(_finalScore))
        {
            _enteringName = true;
        }
    }

    public override void OnExit()
    {
    }

    public override void Update(GameTime time)
    {
        var input = _context.GetInput();
        if (input == null) return;

        if (_enteringName)
        {
            HandleNameEntry(input);
        }
        else
        {
            if (input.IsKeyPressed(Key.Escape) || input.IsKeyPressed(Key.Enter))
            {
                _sceneManager.LoadScene(new MainMenuScene(_context, _sceneManager));
            }
        }
    }

    private void HandleNameEntry(InputState input)
    {
        // Handle backspace
        if (input.IsKeyPressed(Key.Backspace) && _playerName.Length > 0)
        {
            _playerName = _playerName[..^1];
        }

        // Handle enter to confirm
        if (input.IsKeyPressed(Key.Enter) && _playerName.Length > 0)
        {
            _newEntryIndex = _context.Leaderboard.AddEntry(_playerName, _finalScore, _finalLevel);
            _context.Leaderboard.Save();
            _enteringName = false;
        }

        // Handle letter input (simple A-Z)
        if (_playerName.Length < 10)
        {
            for (var key = Key.A; key <= Key.Z; key++)
            {
                if (input.IsKeyPressed(key))
                {
                    var letter = (char)('A' + (key - Key.A));
                    _playerName += letter;
                }
            }
        }
    }

    public void Render(float alpha)
    {
        var server = _context.RenderingServer;

        var pass = new RenderPass
        {
            Target = RenderTargetHandle.Screen,
            ClearColor = new Color4(0.05f, 0.05f, 0.1f),
            ClearDepth = true,
            Viewport = new Rect(0, 0, _width, _height)
        };

        server.BeginPass(pass);

        var projection = Matrix4x4.CreateOrthographicOffCenter(0, _width, _height, 0, -1, 1);
        var commands = server.CreateCommandList();
        commands.SetPipeline(server.DefaultShader2D);
        commands.SetProjectionMatrix(projection);
        commands.SetViewMatrix(Matrix4x4.Identity);

        // Title
        UIRenderer.DrawCenteredText(commands, server, _context.Font, "HIGH SCORES", _width / 2f, 60f, 36f, Color4.Yellow);

        if (_enteringName)
        {
            DrawNameEntryUI(commands);
        }
        else
        {
            DrawLeaderboard(commands);
        }

        server.Submit(commands);
        server.EndPass();
    }

    private void DrawNameEntryUI(IRenderCommandList commands)
    {
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "NEW HIGH SCORE!", _width / 2f, 120f, 28f, new Color4(0.2f, 0.9f, 0.2f, 1f));
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, $"Score: {_finalScore}", _width / 2f, 160f, 24f, Color4.White);

        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "Enter Your Name:", _width / 2f, 220f, 20f, Color4.Gray);

        // Name input box
        var boxWidth = 200f;
        var boxHeight = 40f;
        var boxX = (_width - boxWidth) / 2;
        var boxY = 250f;

        commands.DrawQuad(new Vector2(boxX, boxY), new Vector2(boxWidth, boxHeight), new Color4(0.1f, 0.1f, 0.15f, 1f));
        UIRenderer.DrawBorder(commands, boxX, boxY, boxWidth, boxHeight, 2f, Color4.Yellow);

        var displayName = _playerName + "_";
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, displayName, _width / 2f, boxY + boxHeight / 2, 24f, Color4.White);

        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "Press ENTER to Confirm", _width / 2f, _height - 40f, 14f, Color4.Gray);
    }

    private void DrawLeaderboard(IRenderCommandList commands)
    {
        var entries = _context.Leaderboard.GetTopEntries(10);

        if (entries.Count == 0)
        {
            UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "No high scores yet!", _width / 2f, _height / 2f, 24f, Color4.Gray);
        }
        else
        {
            var startY = 120f;
            const float rowHeight = 40f;

            // Header
            UIRenderer.DrawText(commands, _context.Font, "Rank", _width / 2f - 200, startY, 18f, Color4.Gray);
            UIRenderer.DrawText(commands, _context.Font, "Name", _width / 2f - 100, startY, 18f, Color4.Gray);
            UIRenderer.DrawText(commands, _context.Font, "Score", _width / 2f + 80, startY, 18f, Color4.Gray);
            UIRenderer.DrawText(commands, _context.Font, "Level", _width / 2f + 180, startY, 18f, Color4.Gray);

            startY += 30f;

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var y = startY + i * rowHeight;
                var isNew = i == _newEntryIndex;
                var color = isNew ? Color4.Yellow : Color4.White;

                UIRenderer.DrawText(commands, _context.Font, $"{i + 1}.", _width / 2f - 200, y, 20f, color);
                UIRenderer.DrawText(commands, _context.Font, entry.Name, _width / 2f - 100, y, 20f, color);
                UIRenderer.DrawText(commands, _context.Font, entry.Score.ToString(), _width / 2f + 80, y, 20f, color);
                UIRenderer.DrawText(commands, _context.Font, entry.Level.ToString(), _width / 2f + 180, y, 20f, color);
            }
        }

        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "Press ENTER or ESC to Return", _width / 2f, _height - 40f, 14f, Color4.Gray);
    }

    public void OnResize(int width, int height)
    {
        _width = width;
        _height = height;
    }
}
