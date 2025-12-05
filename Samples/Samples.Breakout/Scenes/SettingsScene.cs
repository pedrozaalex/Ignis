using System.Numerics;
using Ignis.Core;
using Ignis.Core.Scenery;
using Ignis.Core.Timing;
using Ignis.Graphics;
using Samples.Breakout.Core;
using Samples.Breakout.Services;
using Silk.NET.Input;

namespace Samples.Breakout.Scenes;

/// <summary>
/// Settings scene for audio and game options.
/// </summary>
public sealed class SettingsScene : Scene, IBreakoutScene
{
    private readonly BreakoutContext _context;
    private readonly SceneManager _sceneManager;

    private int _selectedIndex;
    private readonly string[] _menuItems = ["Master Volume", "SFX Volume", "Music Volume", "Back"];

    private int _width;
    private int _height;

    public SettingsScene(BreakoutContext context, SceneManager sceneManager)
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
        DrawCenteredText(commands, "SETTINGS", _width / 2f, 80f, 36f, Color4.White);

        // Menu items
        var menuStartY = _height * 0.35f;
        const float itemSpacing = 60f;

        for (var i = 0; i < _menuItems.Length; i++)
        {
            var y = menuStartY + i * itemSpacing;
            var isSelected = i == _selectedIndex;
            var color = isSelected ? Color4.Yellow : Color4.White;

            if (i < 3) // Volume sliders
            {
                // Label
                if (_context.Font.IsValid)
                {
                    commands.DrawText(_context.Font, _menuItems[i],
                        new Vector2(_width / 2f - 200, y - 10), 20f, color);
                }

                // Slider background
                var sliderX = _width / 2f + 20;
                var sliderWidth = 200f;
                var sliderHeight = 20f;

                commands.DrawQuad(
                    new Vector2(sliderX, y - sliderHeight / 2),
                    new Vector2(sliderWidth, sliderHeight),
                    new Color4(0.2f, 0.2f, 0.3f, 1f)
                );

                // Slider fill
                var value = i switch
                {
                    0 => _context.Audio.MasterVolume,
                    1 => _context.Audio.SfxVolume,
                    2 => _context.Audio.MusicVolume,
                    _ => 0f
                };

                var fillColor = isSelected
                    ? new Color4(0.9f, 0.7f, 0.2f, 1f)
                    : new Color4(0.3f, 0.5f, 0.9f, 1f);

                commands.DrawQuad(
                    new Vector2(sliderX, y - sliderHeight / 2),
                    new Vector2(sliderWidth * value, sliderHeight),
                    fillColor
                );

                // Value text
                if (_context.Font.IsValid)
                {
                    commands.DrawText(_context.Font, $"{(int)(value * 100)}%",
                        new Vector2(sliderX + sliderWidth + 20, y - 10), 18f, color);
                }

                // Selection indicator
                if (isSelected)
                {
                    DrawCenteredText(commands, "<", sliderX - 30, y, 20f, Color4.Yellow);
                    DrawCenteredText(commands, ">", sliderX + sliderWidth + 80, y, 20f, Color4.Yellow);
                }
            }
            else // Back button
            {
                DrawCenteredText(commands, _menuItems[i], _width / 2f, y, 24f, color);

                if (isSelected)
                {
                    DrawCenteredText(commands, ">", _width / 2f - 60, y, 24f, Color4.Yellow);
                    DrawCenteredText(commands, "<", _width / 2f + 60, y, 24f, Color4.Yellow);
                }
            }
        }

        // Instructions
        DrawCenteredText(commands, "Up/Down to Navigate, Left/Right to Adjust, ESC to Go Back",
            _width / 2f, _height - 40f, 14f, Color4.Gray);

        server.Submit(commands);
        server.EndPass();
    }

    private void DrawCenteredText(IRenderCommandList commands, string text, float x, float y, float fontSize, Color4 color)
    {
        if (!_context.Font.IsValid) return;

        var (textWidth, textHeight) = _context.RenderingServer.MeasureText(_context.Font, text, fontSize);
        commands.DrawText(_context.Font, text,
            new Vector2(x - textWidth / 2, y - textHeight / 2), fontSize, color);
    }

    public void OnResize(int width, int height)
    {
        _width = width;
        _height = height;
    }
}
