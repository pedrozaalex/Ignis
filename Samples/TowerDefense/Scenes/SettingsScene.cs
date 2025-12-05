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

public sealed class SettingsScene : Scene, ITowerDefenseScene
{
    private readonly TowerDefenseContext _context;
    private readonly SceneManager _sceneManager;
    private readonly CrucibleRenderer _renderer;

    private Widget _root = null!;
    private WidgetInputHandler _inputHandler = null!;
    private int _width;
    private int _height;

    public SettingsScene(TowerDefenseContext context, SceneManager sceneManager)
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
        var panel = new Panel()
            .Column<Panel>()
            .Gap<Panel>(Units.Pixels(30))
            .Alignment<Panel>(Alignment.Center);

        // Title
        panel.Children<Panel>(
            new Label("SETTINGS")
                .FontSize(36f)
                .Alignment<Label>(Alignment.Center)
                .Padding<Label>(Units.Pixels(20))
        );

        // Sliders
        AddSlider(panel, "Master Volume", _context.Audio.MasterVolume, v =>
        {
            _context.Audio.MasterVolume = v;
            _context.Audio.PlaySfx(AudioService.SfxMenuSelect);
        });

        AddSlider(panel, "SFX Volume", _context.Audio.SfxVolume, v =>
        {
            _context.Audio.SfxVolume = v;
            _context.Audio.PlaySfx(AudioService.SfxMenuSelect);
        });

        AddSlider(panel, "Music Volume", _context.Audio.MusicVolume, v =>
        {
            _context.Audio.MusicVolume = v;
        });

        // Back Button
        var backBtn = new CrucibleUI.Widgets.Button("Back")
            .FontSize(24f)
            .Width<CrucibleUI.Widgets.Button>(Units.Pixels(200))
            .Height<CrucibleUI.Widgets.Button>(Units.Pixels(45))
            .OnClick(() =>
            {
                _sceneManager.LoadScene(new MainMenuScene(_context, _sceneManager));
            });

        backBtn.OnFocus += (_) => _context.Audio.PlaySfx(AudioService.SfxMenuSelect);

        panel.Children<Panel>(backBtn);

        // Instructions
        panel.Children<Panel>(
            new Label("Left/Right to Adjust, Escape to Return")
                .FontSize(14f)
                .Color(0.5f, 0.5f, 0.6f)
                .Alignment<Label>(Alignment.Center)
                .Padding<Label>(Units.Pixels(40))
        );

        _root = new Panel()
            .Width<Panel>(Units.Stretch(1))
            .Height<Panel>(Units.Stretch(1))
            .Alignment<Panel>(Alignment.Center)
            .Children<Panel>(panel);

        _inputHandler = new WidgetInputHandler(_root);
    }

    private void AddSlider(Panel parent, string label, float initialValue, Action<float> onChange)
    {
        var container = new Panel()
            .Column<Panel>()
            .Gap<Panel>(Units.Pixels(5))
            .Width<Panel>(Units.Pixels(300));

        var labelWidget = new Label(label).FontSize(20f);
        var valueLabel = new Label($"{(int)(initialValue * 100)}%").FontSize(16f).Color(0.7f, 0.7f, 0.7f);

        var slider = new Slider(0f, 1f, initialValue)
            .Width<Slider>(Units.Stretch(1))
            .Height<Slider>(Units.Pixels(20))
            .OnValueChanged(v =>
            {
                onChange(v);
                valueLabel.Text = $"{(int)(v * 100)}%";
            });

        slider.OnFocus += (_) => _context.Audio.PlaySfx(AudioService.SfxMenuSelect);

        container.Children<Panel>(
            new Panel().Row<Panel>().Children<Panel>(labelWidget, valueLabel),
            slider
        );

        parent.Children<Panel>(container);
    }

    public override void OnEnter(EngineContext context)
    {
        _root.ComputeBounds(0, 0, _width, _height);
        _root.ComputeLayout();
    }

    public override void OnExit()
    {
        _context.SaveSettings();
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

        // Slider adjustment with keys
        if (_inputHandler.FocusedWidget is Slider slider)
        {
            if (input.IsKeyPressed(Key.Left) || input.IsKeyPressed(Key.A))
                slider.SetValue(slider.Value - 0.1f);
            else if (input.IsKeyPressed(Key.Right) || input.IsKeyPressed(Key.D))
                slider.SetValue(slider.Value + 0.1f);
        }

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
