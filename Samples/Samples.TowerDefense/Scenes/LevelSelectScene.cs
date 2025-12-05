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
/// Level selection scene with a grid of levels.
/// </summary>
public sealed class LevelSelectScene : Scene, ITowerDefenseScene
{
    private readonly TowerDefenseContext _context;
    private readonly SceneManager _sceneManager;

    private int _selectedLevel = 1;
    private int _width;
    private int _height;

    public LevelSelectScene(TowerDefenseContext context, SceneManager sceneManager)
    {
        _context = context;
        _sceneManager = sceneManager;
        _width = context.Width;
        _height = context.Height;
    }

    public override void OnEnter(EngineContext context)
    {
    }

    public override void OnExit()
    {
    }

    public override void Update(GameTime time)
    {
        var input = _context.GetInput();
        if (input == null) return;

        var levelCount = _context.Levels.LevelCount;
        var cols = 5;

        // Grid navigation
        if (input.IsKeyPressed(Key.Left) || input.IsKeyPressed(Key.A))
        {
            _selectedLevel = Math.Max(1, _selectedLevel - 1);
            _context.Audio.PlaySfx(AudioService.SfxMenuSelect);
        }
        else if (input.IsKeyPressed(Key.Right) || input.IsKeyPressed(Key.D))
        {
            _selectedLevel = Math.Min(levelCount, _selectedLevel + 1);
            _context.Audio.PlaySfx(AudioService.SfxMenuSelect);
        }
        else if (input.IsKeyPressed(Key.Up) || input.IsKeyPressed(Key.W))
        {
            _selectedLevel = Math.Max(1, _selectedLevel - cols);
            _context.Audio.PlaySfx(AudioService.SfxMenuSelect);
        }
        else if (input.IsKeyPressed(Key.Down) || input.IsKeyPressed(Key.S))
        {
            _selectedLevel = Math.Min(levelCount, _selectedLevel + cols);
            _context.Audio.PlaySfx(AudioService.SfxMenuSelect);
        }

        // Selection
        if (input.IsKeyPressed(Key.Enter) || input.IsKeyPressed(Key.Space))
        {
            if (_selectedLevel <= _context.Settings.HighestLevelUnlocked)
            {
                _context.ResetGame();
                _context.StartLevel(_selectedLevel);
                _sceneManager.LoadScene(new GameScene(_context, _sceneManager));
            }
            else
            {
                _context.Audio.PlaySfx(AudioService.SfxNotEnoughGold);
            }
        }

        // Back
        if (input.IsKeyPressed(Key.Escape))
        {
            _sceneManager.LoadScene(new MainMenuScene(_context, _sceneManager));
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

        // Title
        DrawCenteredText(commands, "SELECT LEVEL", _width / 2f, 60f, 36f, Color4.White);

        // Level grid
        var levelCount = _context.Levels.LevelCount;
        var cols = 5;
        var rows = (levelCount + cols - 1) / cols;

        const float boxSize = 100f;
        const float spacing = 20f;

        var gridWidth = cols * boxSize + (cols - 1) * spacing;
        var gridHeight = rows * boxSize + (rows - 1) * spacing;
        var startX = (_width - gridWidth) / 2f;
        var startY = 140f;

        for (var i = 0; i < levelCount; i++)
        {
            var row = i / cols;
            var col = i % cols;
            var level = i + 1;

            var x = startX + col * (boxSize + spacing);
            var y = startY + row * (boxSize + spacing);

            var isSelected = level == _selectedLevel;
            var isUnlocked = level <= _context.Settings.HighestLevelUnlocked;

            // Box background
            Color4 boxColor;
            if (isSelected)
                boxColor = isUnlocked ? new Color4(0.3f, 0.5f, 0.8f, 1f) : new Color4(0.4f, 0.2f, 0.2f, 1f);
            else if (isUnlocked)
                boxColor = new Color4(0.15f, 0.2f, 0.3f, 1f);
            else
                boxColor = new Color4(0.1f, 0.1f, 0.15f, 0.5f);

            commands.DrawQuad(new Vector2(x, y), new Vector2(boxSize, boxSize), boxColor);

            // Border
            if (isSelected)
            {
                var borderColor = isUnlocked ? new Color4(0.5f, 0.8f, 1f, 1f) : new Color4(0.8f, 0.3f, 0.3f, 1f);
                DrawBorder(commands, x, y, boxSize, boxSize, 3f, borderColor);
            }

            // Level number
            var numColor = isUnlocked ? Color4.White : new Color4(0.4f, 0.4f, 0.4f, 1f);
            DrawCenteredText(commands, level.ToString(), x + boxSize / 2, y + 25f, 32f, numColor);

            // Level name
            var levelData = _context.Levels.GetLevel(level);
            if (levelData != null)
            {
                var nameColor = isUnlocked ? new Color4(0.7f, 0.7f, 0.8f, 1f) : new Color4(0.3f, 0.3f, 0.35f, 1f);
                DrawCenteredText(commands, levelData.Name, x + boxSize / 2, y + 60f, 12f, nameColor);
            }

            // Lock icon for locked levels
            if (!isUnlocked)
            {
                DrawCenteredText(commands, "[LOCKED]", x + boxSize / 2, y + 80f, 10f, new Color4(0.5f, 0.3f, 0.3f, 1f));
            }
        }

        // Selected level info
        var selectedData = _context.Levels.GetLevel(_selectedLevel);
        if (selectedData != null)
        {
            var infoY = startY + gridHeight + 40f;
            var isUnlocked = _selectedLevel <= _context.Settings.HighestLevelUnlocked;

            DrawCenteredText(commands, selectedData.Name, _width / 2f, infoY, 24f,
                isUnlocked ? Color4.White : new Color4(0.5f, 0.5f, 0.5f, 1f));

            var wavesText = $"{selectedData.Waves.Count} Waves";
            DrawCenteredText(commands, wavesText, _width / 2f, infoY + 35f, 16f, new Color4(0.6f, 0.6f, 0.7f, 1f));
        }

        // Instructions
        DrawCenteredText(commands, "Arrow Keys to Navigate, Enter to Play, Escape to Return",
            _width / 2f, _height - 40f, 14f, new Color4(0.5f, 0.5f, 0.6f, 1f));

        server.Submit(commands);
        server.EndPass();
    }

    private void DrawBorder(IRenderCommandList commands, float x, float y, float w, float h, float thickness, Color4 color)
    {
        commands.DrawQuad(new Vector2(x, y), new Vector2(w, thickness), color); // Top
        commands.DrawQuad(new Vector2(x, y + h - thickness), new Vector2(w, thickness), color); // Bottom
        commands.DrawQuad(new Vector2(x, y), new Vector2(thickness, h), color); // Left
        commands.DrawQuad(new Vector2(x + w - thickness, y), new Vector2(thickness, h), color); // Right
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
