using System.Numerics;
using Ignis.Core;
using Ignis.Core.Scenery;
using Ignis.Core.Timing;
using Ignis.Graphics;
using Samples.TowerDefense.Core;
using Samples.TowerDefense.Services;
using Silk.NET.Input;

namespace Samples.TowerDefense.Scenes;

/// <summary>
/// Main menu scene with Play, Level Select, Leaderboard, and Settings options.
/// </summary>
public sealed class MainMenuScene : Scene, ITowerDefenseScene
{
    private readonly TowerDefenseContext _context;
    private readonly SceneManager _sceneManager;

    private int _selectedIndex;
    private readonly string[] _menuItems = ["New Game", "Level Select", "Settings", "Exit"];
    private float _animTime;

    private int _width;
    private int _height;

    public MainMenuScene(TowerDefenseContext context, SceneManager sceneManager)
    {
        _context = context;
        _sceneManager = sceneManager;
        _width = context.Width;
        _height = context.Height;
    }

    public override void OnEnter(EngineContext context)
    {
        _context.Audio.PlayMusic("menu_music");
    }

    public override void OnExit()
    {
        _context.Audio.StopMusic();
    }

    public override void Update(GameTime time)
    {
        _animTime += time.DeltaTime;

        var input = _context.GetInput();
        if (input == null) return;

        // Navigation
        if (input.IsKeyPressed(Key.Up) || input.IsKeyPressed(Key.W))
        {
            _selectedIndex = (_selectedIndex - 1 + _menuItems.Length) % _menuItems.Length;
            _context.Audio.PlaySfx(AudioService.SfxMenuSelect);
        }
        else if (input.IsKeyPressed(Key.Down) || input.IsKeyPressed(Key.S))
        {
            _selectedIndex = (_selectedIndex + 1) % _menuItems.Length;
            _context.Audio.PlaySfx(AudioService.SfxMenuSelect);
        }

        // Selection
        if (input.IsKeyPressed(Key.Enter) || input.IsKeyPressed(Key.Space))
        {
            HandleSelection();
        }

        // Quick exit
        if (input.IsKeyPressed(Key.Escape))
        {
            _context.Window.Close();
        }
    }

    private void HandleSelection()
    {
        _context.Audio.PlaySfx(AudioService.SfxMenuSelect);

        switch (_selectedIndex)
        {
            case 0: // New Game
                _context.ResetGame();
                _sceneManager.LoadScene(new GameScene(_context, _sceneManager));
                break;
            case 1: // Level Select
                _sceneManager.LoadScene(new LevelSelectScene(_context, _sceneManager));
                break;
            case 2: // Settings
                _sceneManager.LoadScene(new SettingsScene(_context, _sceneManager));
                break;
            case 3: // Exit
                _context.Window.Close();
                break;
        }
    }

    public void Render(float alpha)
    {
        var server = _context.RenderingServer;

        var pass = new RenderPass
        {
            Target = RenderTargetHandle.Screen,
            ClearColor = new Color4(0.05f, 0.02f, 0.1f),
            ClearDepth = true,
            Viewport = new Rect(0, 0, _width, _height)
        };

        server.BeginPass(pass);

        var projection = Matrix4x4.CreateOrthographicOffCenter(0, _width, _height, 0, -1, 1);
        var commands = server.CreateCommandList();
        commands.SetPipeline(server.DefaultShader2D);
        commands.SetProjectionMatrix(projection);
        commands.SetViewMatrix(Matrix4x4.Identity);

        // Animated background particles
        DrawBackgroundEffects(commands);

        // Title with glow effect
        var titleY = 100f + MathF.Sin(_animTime * 2f) * 5f;
        DrawCenteredText(commands, "TOWER DEFENSE", _width / 2f, titleY, 48f, new Color4(0.8f, 0.3f, 1f, 1f));

        // Menu items
        var menuStartY = 280f;
        var menuSpacing = 60f;

        for (var i = 0; i < _menuItems.Length; i++)
        {
            var isSelected = i == _selectedIndex;
            var y = menuStartY + i * menuSpacing;

            if (isSelected)
            {
                var pulse = 1f + MathF.Sin(_animTime * 8f) * 0.1f;
                var boxWidth = 250f * pulse;
                var boxHeight = 45f;

                // Selection highlight
                commands.DrawQuad(
                    new Vector2(_width / 2f - boxWidth / 2f, y - boxHeight / 2f),
                    new Vector2(boxWidth, boxHeight),
                    new Color4(0.4f, 0.2f, 0.6f, 0.5f)
                );

                // Selection arrows
                DrawCenteredText(commands, "> ", _width / 2f - 120f, y, 28f, new Color4(1f, 0.8f, 0.2f, 1f));
                DrawCenteredText(commands, " <", _width / 2f + 120f, y, 28f, new Color4(1f, 0.8f, 0.2f, 1f));
            }

            var color = isSelected ? new Color4(1f, 1f, 1f, 1f) : new Color4(0.6f, 0.6f, 0.7f, 1f);
            var size = isSelected ? 28f : 24f;
            DrawCenteredText(commands, _menuItems[i], _width / 2f, y, size, color);
        }

        // Instructions
        DrawCenteredText(commands, "Use Arrow Keys to Navigate, Enter to Select", _width / 2f, _height - 80f, 16f, new Color4(0.5f, 0.5f, 0.6f, 1f));

        // Version
        DrawText(commands, "v1.0", 10f, _height - 30f, 14f, new Color4(0.3f, 0.3f, 0.4f, 1f));

        server.Submit(commands);
        server.EndPass();
    }

    private void DrawBackgroundEffects(IRenderCommandList commands)
    {
        // Draw some floating particles for visual interest
        var random = new Random(42);
        for (var i = 0; i < 30; i++)
        {
            var baseX = (float)(random.NextDouble() * _width);
            var baseY = (float)(random.NextDouble() * _height);
            var speed = 0.5f + (float)random.NextDouble();
            var size = 2f + (float)random.NextDouble() * 4f;

            var x = (baseX + _animTime * 20f * speed) % _width;
            var y = baseY + MathF.Sin(_animTime + i) * 30f;

            var alpha = 0.1f + (float)random.NextDouble() * 0.2f;
            commands.DrawQuad(
                new Vector2(x, y),
                new Vector2(size, size),
                new Color4(0.4f, 0.2f, 0.8f, alpha)
            );
        }
    }

    private void DrawText(IRenderCommandList commands, string text, float x, float y, float size, Color4 color)
    {
        commands.DrawText(_context.Font, text, new Vector2(x, y), size, color);
    }

    private void DrawCenteredText(IRenderCommandList commands, string text, float x, float y, float size, Color4 color)
    {
        var (textWidth, textHeight) = _context.RenderingServer.MeasureText(_context.Font, text, size);
        commands.DrawText(_context.Font, text, new Vector2(x - textWidth / 2, y - textHeight / 2), size, color);
    }

    public void OnResize(int width, int height)
    {
        _width = width;
        _height = height;
    }
}
