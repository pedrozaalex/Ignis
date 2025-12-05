using System.Numerics;
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
/// Settings scene for audio options.
/// </summary>
public sealed class SettingsScene : Scene, ITowerDefenseScene
{
    private readonly TowerDefenseContext _context;
    private readonly SceneManager _sceneManager;
    private readonly UIRenderer _ui;

    private int _selectedIndex;
    private readonly string[] _menuItems = ["Master Volume", "SFX Volume", "Music Volume", "Back"];

    private int _width;
    private int _height;

    public SettingsScene(TowerDefenseContext context, SceneManager sceneManager)
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
        _context.SaveSettings();
    }

    public override void Update(GameTime time)
    {
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

        // Adjust values
        if (input.IsKeyPressed(Key.Left) || input.IsKeyPressed(Key.A))
        {
            AdjustSetting(-0.1f);
        }
        else if (input.IsKeyPressed(Key.Right) || input.IsKeyPressed(Key.D))
        {
            AdjustSetting(0.1f);
        }

        // Selection (for Back button)
        if (input.IsKeyPressed(Key.Enter) || input.IsKeyPressed(Key.Space))
        {
            if (_selectedIndex == _menuItems.Length - 1)
            {
                _sceneManager.LoadScene(new MainMenuScene(_context, _sceneManager));
            }
        }

        // Quick back
        if (input.IsKeyPressed(Key.Escape))
        {
            _sceneManager.LoadScene(new MainMenuScene(_context, _sceneManager));
        }
    }

    private void AdjustSetting(float delta)
    {
        switch (_selectedIndex)
        {
            case 0: // Master Volume
                _context.Audio.MasterVolume = Math.Clamp(_context.Audio.MasterVolume + delta, 0f, 1f);
                _context.Audio.PlaySfx(AudioService.SfxMenuSelect);
                break;
            case 1: // SFX Volume
                _context.Audio.SfxVolume = Math.Clamp(_context.Audio.SfxVolume + delta, 0f, 1f);
                _context.Audio.PlaySfx(AudioService.SfxMenuSelect);
                break;
            case 2: // Music Volume
                _context.Audio.MusicVolume = Math.Clamp(_context.Audio.MusicVolume + delta, 0f, 1f);
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

        // Title
        _ui.DrawCenteredText(commands, "SETTINGS", _width / 2f, 80f, 36f, Color4.White);

        // Settings items
        var startY = 200f;
        var spacing = 70f;
        var sliderX = _width / 2f + 20f;
        var sliderWidth = 200f;

        for (var i = 0; i < _menuItems.Length; i++)
        {
            var isSelected = i == _selectedIndex;
            var y = startY + i * spacing;
            var color = isSelected ? Color4.White : new Color4(0.6f, 0.6f, 0.7f, 1f);

            if (i < 3) // Volume sliders
            {
                var value = i switch
                {
                    0 => _context.Audio.MasterVolume,
                    1 => _context.Audio.SfxVolume,
                    2 => _context.Audio.MusicVolume,
                    _ => 0f
                };

                _ui.DrawSliderWithLabel(commands, _menuItems[i], _width / 2f - 200f, sliderX, y,
                    sliderWidth, 16f, value, isSelected, color);
            }
            else // Back button
            {
                _ui.DrawMenuItem(commands, _menuItems[i], _width / 2f, y, isSelected);
            }
        }

        // Instructions
        _ui.DrawCenteredText(commands, "Left/Right to Adjust, Escape to Return",
            _width / 2f, _height - 60f, 14f, new Color4(0.5f, 0.5f, 0.6f, 1f));

        server.Submit(commands);
        server.EndPass();
    }

    public void OnResize(int width, int height)
    {
        _width = width;
        _height = height;
    }
}
