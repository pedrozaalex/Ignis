using System.Numerics;
using CrucibleUI.Types;
using CrucibleUI.Widgets;
using Ignis.Core;
using Ignis.Core.Scenery;
using Ignis.Core.Timing;
using Ignis.Graphics;
using Samples.Common;
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
    private readonly CrucibleRenderer _renderer;

    private Widget _root = null!;
    private WidgetInputHandler _inputHandler = null!;
    private float _animTime;
    private int _width;
    private int _height;

    public MainMenuScene(TowerDefenseContext context, SceneManager sceneManager)
    {
        _context = context;
        _sceneManager = sceneManager;
        _renderer = new CrucibleRenderer(context.RenderingServer, context.Font);
        _width = context.Width;
        _height = context.Height;

        BuildUI();
    }

    private void BuildUI()
    {
        var menuPanel = new Panel()
            .Column<Panel>()
            .Gap<Panel>(Units.Pixels(20))
            .Alignment<Panel>(Alignment.Center);

        // Title
        menuPanel.Children<Panel>(
            new Label("TOWER DEFENSE")
                .FontSize(48f)
                .Color(0.8f, 0.3f, 1f)
                .Alignment<Label>(Alignment.Center)
        );

        // Menu Items
        AddMenuItem(menuPanel, "New Game", () =>
        {
            _context.ResetGame();
            _sceneManager.LoadScene(new GameScene(_context, _sceneManager));
        });

        AddMenuItem(menuPanel, "Level Select", () =>
        {
            _sceneManager.LoadScene(new LevelSelectScene(_context, _sceneManager));
        });

        AddMenuItem(menuPanel, "Settings", () =>
        {
            _sceneManager.LoadScene(new SettingsScene(_context, _sceneManager));
        });

        AddMenuItem(menuPanel, "Exit", () =>
        {
            _context.Window.Close();
        });

        // Instructions
        menuPanel.Children<Panel>(
            new Label("Use Arrow Keys to Navigate, Enter to Select")
                .FontSize(16f)
                .Color(0.5f, 0.5f, 0.6f)
                .Alignment<Label>(Alignment.Center)
                .Padding<Label>(Units.Pixels(40)) // Add some space before instructions
        );

        _root = new Panel()
            .Width<Panel>(Units.Stretch(1))
            .Height<Panel>(Units.Stretch(1))
            .Alignment<Panel>(Alignment.Center)
            .Children<Panel>(menuPanel);

        _inputHandler = new WidgetInputHandler(_root);
    }

    private void AddMenuItem(Panel parent, string text, Action action)
    {
        var btn = new CrucibleUI.Widgets.Button(text)
            .FontSize(24f)
            .Width<CrucibleUI.Widgets.Button>(Units.Pixels(250))
            .Height<CrucibleUI.Widgets.Button>(Units.Pixels(45))
            .OnClick(() =>
            {
                _context.Audio.PlaySfx(AudioService.SfxMenuSelect);
                action();
            });

        // Add sound on focus
        btn.OnFocus += (_) => _context.Audio.PlaySfx(AudioService.SfxMenuSelect);

        parent.Children<Panel>(btn);
    }

    public override void OnEnter(EngineContext context)
    {
        _context.Audio.PlayMusic("menu_music");

        // Layout
        _root.ComputeBounds(0, 0, _width, _height);
        _root.ComputeLayout();
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

        // Mouse Input
        var pos = input.MousePosition;
        _inputHandler.HandleMouseMove(pos.X, pos.Y);

        if (input.IsMousePressed(MouseButton.Left))
            _inputHandler.HandleMouseDown(pos.X, pos.Y);

        if (input.IsMouseReleased(MouseButton.Left))
            _inputHandler.HandleMouseUp(pos.X, pos.Y);

        // Keyboard Navigation
        if (input.IsKeyPressed(Key.Up) || input.IsKeyPressed(Key.W))
            _inputHandler.HandleNavigation(0, -1);
        else if (input.IsKeyPressed(Key.Down) || input.IsKeyPressed(Key.S))
            _inputHandler.HandleNavigation(0, 1);

        if (input.IsKeyPressed(Key.Enter) || input.IsKeyPressed(Key.Space))
            _inputHandler.HandleSubmit();

        if (input.IsKeyPressed(Key.Escape))
            _context.Window.Close();
    }

    public void Render(float alpha)
    {
        var server = _context.RenderingServer;

        var pass = new RenderPass
        {
            Target = RenderTargetHandle.Screen,
            ClearColor = new Color4(0.05f, 0.02f, 0.1f),
            ClearDepth = true,
            Viewport = new Ignis.Graphics.Rect(0, 0, _width, _height)
        };

        server.BeginPass(pass);

        var projection = Matrix4x4.CreateOrthographicOffCenter(0, _width, _height, 0, -1, 1);
        var commands = server.CreateCommandList();
        commands.SetPipeline(server.DefaultShader2D);
        commands.SetProjectionMatrix(projection);
        commands.SetViewMatrix(Matrix4x4.Identity);

        // Animated background particles
        DrawBackgroundEffects(commands);

        // Render UI
        _renderer.Render(_root, commands);

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

            var particleAlpha = 0.1f + (float)random.NextDouble() * 0.2f;
            commands.DrawQuad(
                new Vector2(x, y),
                new Vector2(size, size),
                new Color4(0.4f, 0.2f, 0.8f, particleAlpha)
            );
        }
    }

    public void OnResize(int width, int height)
    {
        _width = width;
        _height = height;
        _root.ComputeBounds(0, 0, width, height);
        _root.ComputeLayout();
    }
}
