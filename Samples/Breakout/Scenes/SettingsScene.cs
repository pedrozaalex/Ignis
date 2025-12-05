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
/// Settings scene for audio and game options.
/// </summary>
public sealed class SettingsScene : Scene, IBreakoutScene
{
    private readonly BreakoutContext _context;
    private readonly SceneManager _sceneManager;
    private readonly UIRenderer _ui;

    private int _selectedIndex;
    private readonly string[] _menuItems = ["Master Volume", "SFX Volume", "Music Volume", "Back"];

    private int _width;
    private int _height;

    public SettingsScene(BreakoutContext context, SceneManager sceneManager)
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

        // Selection
        if (input.IsKeyPressed(Key.Enter) || input.IsKeyPressed(Key.Space))
        {
            if (_selectedIndex == _menuItems.Length - 1) // Back
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
        _ui.DrawCenteredText(commands, "SETTINGS", _width / 2f, 80f, 36f, Color4.White);

        // Menu items
        var menuStartY = _height * 0.35f;
        const float itemSpacing = 60f;
        var sliderX = _width / 2f + 20;
        var sliderWidth = 200f;

        for (var i = 0; i < _menuItems.Length; i++)
        {
            var y = menuStartY + i * itemSpacing;
            var isSelected = i == _selectedIndex;
            var color = isSelected ? Color4.Yellow : Color4.White;

            if (i < 3) // Volume sliders
            {
                var value = i switch
                {
                    0 => _context.Audio.MasterVolume,
                    1 => _context.Audio.SfxVolume,
                    2 => _context.Audio.MusicVolume,
                    _ => 0f
                };

                _ui.DrawSliderWithLabel(commands, _menuItems[i], _width / 2f - 200, sliderX, y,
                    sliderWidth, 20f, value, isSelected, color);
            }
            else // Back button
            {
                _ui.DrawMenuItem(commands, _menuItems[i], _width / 2f, y, isSelected);
            }
        }

        // Instructions
        _ui.DrawCenteredText(commands, "Up/Down to Navigate, Left/Right to Adjust, ESC to Go Back",
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
