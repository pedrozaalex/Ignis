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

public sealed class LevelSelectScene : Scene, ITowerDefenseScene
{
    private readonly TowerDefenseContext _context;
    private readonly SceneManager _sceneManager;
    private readonly CrucibleRenderer _renderer;

    private Widget _root = null!;
    private WidgetInputHandler _inputHandler = null!;
    private int _width;
    private int _height;

    private Label _infoLabel = null!;
    private Label _wavesLabel = null!;

    public LevelSelectScene(TowerDefenseContext context, SceneManager sceneManager)
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
        var mainPanel = new Panel()
            .Column<Panel>()
            .Gap<Panel>(Units.Pixels(20))
            .Alignment<Panel>(Alignment.Center);

        // Title
        mainPanel.Children<Panel>(
            new Label("SELECT LEVEL")
                .FontSize(36f)
                .Alignment<Label>(Alignment.Center)
                .Padding<Label>(Units.Pixels(20))
        );

        // Level Grid
        var grid = new Panel()
            .Width<Panel>(Units.Pixels(600))
            .Height<Panel>(Units.Pixels(400))
            .Gap<Panel>(Units.Pixels(20));

        var levelCount = _context.Levels.LevelCount;
        var cols = 5;
        var rows = (levelCount + cols - 1) / cols;

        for (var r = 0; r < rows; r++)
        {
            var rowPanel = new Panel().Row<Panel>().Gap<Panel>(Units.Pixels(20)).Alignment<Panel>(Alignment.Center);
            for (var c = 0; c < cols; c++)
            {
                var levelIndex = r * cols + c + 1;
                if (levelIndex > levelCount) break;

                var isUnlocked = levelIndex <= _context.Settings.HighestLevelUnlocked;
                var levelData = _context.Levels.GetLevel(levelIndex);

                var btn = new CrucibleUI.Widgets.Button(levelIndex.ToString())
                    .FontSize(24f)
                    .Width<CrucibleUI.Widgets.Button>(Units.Pixels(80))
                    .Height<CrucibleUI.Widgets.Button>(Units.Pixels(80))
                    .OnClick(() =>
                    {
                        if (isUnlocked)
                        {
                            _context.ResetGame();
                            _context.StartLevel(levelIndex);
                            _sceneManager.LoadScene(new GameScene(_context, _sceneManager));
                        }
                        else
                        {
                            _context.Audio.PlaySfx(AudioService.SfxNotEnoughGold);
                        }
                    });

                if (!isUnlocked)
                {
                    btn.Color(0.5f, 0.5f, 0.5f);
                    btn.Background<CrucibleUI.Widgets.Button>(0.2f, 0.2f, 0.2f);
                }

                btn.OnFocus += (_) =>
                {
                    _context.Audio.PlaySfx(AudioService.SfxMenuSelect);
                    UpdateInfo(levelIndex);
                };

                rowPanel.Children<Panel>(btn);
            }
            mainPanel.Children<Panel>(rowPanel);
        }

        // Info Panel
        _infoLabel = new Label("").FontSize(24f).Alignment<Label>(Alignment.Center);
        _wavesLabel = new Label("").FontSize(16f).Color(0.7f, 0.7f, 0.7f).Alignment<Label>(Alignment.Center);

        mainPanel.Children<Panel>(_infoLabel, _wavesLabel);

        // Instructions
        mainPanel.Children<Panel>(
            new Label("Arrow Keys to Navigate, Enter to Play, Escape to Return")
                .FontSize(14f)
                .Color(0.5f, 0.5f, 0.6f)
                .Alignment<Label>(Alignment.Center)
                .Padding<Label>(Units.Pixels(20))
        );

        _root = new Panel()
            .Width<Panel>(Units.Stretch(1))
            .Height<Panel>(Units.Stretch(1))
            .Alignment<Panel>(Alignment.Center)
            .Children<Panel>(mainPanel);

        _inputHandler = new WidgetInputHandler(_root);
    }

    private void UpdateInfo(int levelIndex)
    {
        var levelData = _context.Levels.GetLevel(levelIndex);
        if (levelData != null)
        {
            var isUnlocked = levelIndex <= _context.Settings.HighestLevelUnlocked;
            _infoLabel.Text = levelData.Name;
            _infoLabel.Color(isUnlocked ? 1f : 0.5f, isUnlocked ? 1f : 0.5f, isUnlocked ? 1f : 0.5f);
            _wavesLabel.Text = $"{levelData.Waves.Count} Waves";
        }
    }

    public override void OnEnter(EngineContext context)
    {
        _root.ComputeBounds(0, 0, _width, _height);
        _root.ComputeLayout();

        // Focus first level
        _inputHandler.HandleNavigation(0, 0); // Will focus first focusable
    }

    public override void OnExit()
    {
    }

    public override void Update(GameTime time)
    {
        var input = _context.GetInput();
        if (input == null) return;

        // Mouse
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
        else if (input.IsKeyPressed(Key.Left) || input.IsKeyPressed(Key.A))
            _inputHandler.HandleNavigation(-1, 0);
        else if (input.IsKeyPressed(Key.Right) || input.IsKeyPressed(Key.D))
            _inputHandler.HandleNavigation(1, 0);

        if (input.IsKeyPressed(Key.Enter) || input.IsKeyPressed(Key.Space))
            _inputHandler.HandleSubmit();

        if (input.IsKeyPressed(Key.Escape))
            _sceneManager.LoadScene(new MainMenuScene(_context, _sceneManager));
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

        _renderer.Render(_root, commands);

        server.Submit(commands);
        server.EndPass();
    }

    public void OnResize(int width, int height)
    {
        _width = width;
        _height = height;
        _root.ComputeBounds(0, 0, width, height);
        _root.ComputeLayout();
    }
}
