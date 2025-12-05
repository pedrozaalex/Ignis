using System.Numerics;
using Ignis.Core;
using Ignis.Core.Scenery;
using Ignis.Core.Timing;
using Ignis.Graphics;
using Samples.Breakout.Core;
using Samples.Breakout.Services;
using Samples.Common;
using Silk.NET.Input;

namespace Samples.Breakout.Scenes;

/// <summary>
/// Level selection scene.
/// </summary>
public sealed class LevelSelectScene : Scene, IBreakoutScene
{
    private readonly BreakoutContext _context;
    private readonly SceneManager _sceneManager;
    private readonly UIRenderer _ui;

    private int _selectedLevel = 1;
    private int _width;
    private int _height;

    public LevelSelectScene(BreakoutContext context, SceneManager sceneManager)
    {
        _context = context;
        _sceneManager = sceneManager;
        _ui = new UIRenderer(context.RenderingServer, context.Font);
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

        if (input.IsKeyPressed(Key.Left) || input.IsKeyPressed(Key.A))
        {
            _selectedLevel = (_selectedLevel - 2 + levelCount) % levelCount + 1;
            _context.Audio.PlaySfx(AudioService.SfxMenuSelect);
        }
        else if (input.IsKeyPressed(Key.Right) || input.IsKeyPressed(Key.D))
        {
            _selectedLevel = _selectedLevel % levelCount + 1;
            _context.Audio.PlaySfx(AudioService.SfxMenuSelect);
        }

        if (input.IsKeyPressed(Key.Enter) || input.IsKeyPressed(Key.Space))
        {
            _context.ResetGame();
            _context.CurrentLevel = _selectedLevel;
            _sceneManager.LoadScene(new GameScene(_context, _sceneManager));
        }

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
        _ui.DrawCenteredText(commands, "SELECT LEVEL", _width / 2f, 80f, 36f, Color4.White);

        // Level grid
        var levelCount = _context.Levels.LevelCount;
        var cols = Math.Min(levelCount, 5);
        var rows = (levelCount + cols - 1) / cols;

        const float boxSize = 80f;
        const float spacing = 20f;

        var totalWidth = cols * boxSize + (cols - 1) * spacing;
        var totalHeight = rows * boxSize + (rows - 1) * spacing;
        var startX = (_width - totalWidth) / 2;
        var startY = (_height - totalHeight) / 2;

        for (var i = 0; i < levelCount; i++)
        {
            var row = i / cols;
            var col = i % cols;
            var x = startX + col * (boxSize + spacing);
            var y = startY + row * (boxSize + spacing);
            var levelNum = i + 1;

            var isSelected = levelNum == _selectedLevel;

            // All levels are unlocked in Breakout
            _ui.DrawLevelBox(commands, x, y, boxSize, levelNum, null, isSelected, isUnlocked: true);
        }

        // Selected level info
        var level = _context.Levels.GetLevel(_selectedLevel);
        if (level != null)
        {
            _ui.DrawCenteredText(commands, level.Name, _width / 2f, _height - 100f, 24f, Color4.Yellow);
        }

        // Instructions
        _ui.DrawCenteredText(commands, "Arrow Keys to Select, Enter to Play, ESC to Go Back",
            _width / 2f, _height - 40f, 14f, Color4.Gray);

        server.Submit(commands);
        server.EndPass();
    }

    public void OnResize(int width, int height)
    {
        _width = width;
        _height = height;
    }
}
