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
/// Settings scene for audio options.
/// </summary>
public sealed class SettingsScene : Scene, ITowerDefenseScene
{
    private readonly TowerDefenseContext _context;
    private readonly SceneManager _sceneManager;

    private int _selectedIndex;
    private readonly string[] _menuItems = ["Master Volume", "SFX Volume", "Music Volume", "Back"];

    private int _width;
    private int _height;

    public SettingsScene(TowerDefenseContext context, SceneManager sceneManager)
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
        DrawCenteredText(commands, "SETTINGS", _width / 2f, 80f, 36f, Color4.White);

        // Settings items
        var startY = 200f;
        var spacing = 70f;

        for (var i = 0; i < _menuItems.Length; i++)
        {
            var isSelected = i == _selectedIndex;
            var y = startY + i * spacing;

            var color = isSelected ? Color4.White : new Color4(0.6f, 0.6f, 0.7f, 1f);

            if (i < 3) // Volume sliders
            {
                // Label
                DrawText(commands, _menuItems[i], _width / 2f - 200f, y, 20f, color);

                // Slider background
                var sliderX = _width / 2f + 20f;
                var sliderWidth = 200f;
                commands.DrawQuad(new Vector2(sliderX, y - 8f), new Vector2(sliderWidth, 16f), new Color4(0.2f, 0.2f, 0.3f, 1f));

                // Slider fill
                var value = i switch
                {
                    0 => _context.Audio.MasterVolume,
                    1 => _context.Audio.SfxVolume,
                    2 => _context.Audio.MusicVolume,
                    _ => 0f
                };

                var fillWidth = sliderWidth * value;
                if (fillWidth > 0)
                {
                    var fillColor = isSelected ? new Color4(0.4f, 0.7f, 1f, 1f) : new Color4(0.3f, 0.5f, 0.8f, 1f);
                    commands.DrawQuad(new Vector2(sliderX, y - 8f), new Vector2(fillWidth, 16f), fillColor);
                }

                // Value text
                DrawText(commands, $"{(int)(value * 100)}%", sliderX + sliderWidth + 20f, y, 16f, color);

                // Selection indicator
                if (isSelected)
                {
                    DrawText(commands, "<", sliderX - 25f, y, 20f, new Color4(1f, 0.8f, 0.2f, 1f));
                    DrawText(commands, ">", sliderX + sliderWidth + 70f, y, 20f, new Color4(1f, 0.8f, 0.2f, 1f));
                }
            }
            else // Back button
            {
                var size = isSelected ? 24f : 20f;
                DrawCenteredText(commands, _menuItems[i], _width / 2f, y, size, color);

                if (isSelected)
                {
                    DrawCenteredText(commands, "> ", _width / 2f - 60f, y, 24f, new Color4(1f, 0.8f, 0.2f, 1f));
                    DrawCenteredText(commands, " <", _width / 2f + 60f, y, 24f, new Color4(1f, 0.8f, 0.2f, 1f));
                }
            }
        }

        // Instructions
        DrawCenteredText(commands, "Left/Right to Adjust, Escape to Return",
            _width / 2f, _height - 60f, 14f, new Color4(0.5f, 0.5f, 0.6f, 1f));

        server.Submit(commands);
        server.EndPass();
    }

    private void DrawText(IRenderCommandList commands, string text, float x, float y, float size, Color4 color)
    {
        commands.DrawText(_context.Font, text, new Vector2(x, y - size / 2), size, color);
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
