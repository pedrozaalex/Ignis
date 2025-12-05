using System.Numerics;
using Ignis.Core;
using Ignis.Core.Scenery;
using Ignis.Core.Timing;
using Ignis.Graphics;
using Samples.Breakout.Core;
using Silk.NET.Input;

namespace Samples.Breakout.Scenes;

/// <summary>
/// Main menu scene with Play, Leaderboard, and Settings options.
/// </summary>
public sealed class MainMenuScene : Scene, IBreakoutScene
{
    private readonly BreakoutContext _context;
    private readonly SceneManager _sceneManager;
    
    private int _selectedIndex;
    private readonly string[] _menuItems = ["Play", "Level Select", "Leaderboard", "Settings", "Exit"];
    private float _animTime;
    
    private int _width;
    private int _height;
    
    public MainMenuScene(BreakoutContext context, SceneManager sceneManager)
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
            _context.Audio.PlaySfx(Services.AudioService.SfxMenuSelect);
        }
        else if (input.IsKeyPressed(Key.Down) || input.IsKeyPressed(Key.S))
        {
            _selectedIndex = (_selectedIndex + 1) % _menuItems.Length;
            _context.Audio.PlaySfx(Services.AudioService.SfxMenuSelect);
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
        switch (_selectedIndex)
        {
            case 0: // Play
                _context.ResetGame();
                _sceneManager.LoadScene(new GameScene(_context, _sceneManager));
                break;
            case 1: // Level Select
                _sceneManager.LoadScene(new LevelSelectScene(_context, _sceneManager));
                break;
            case 2: // Leaderboard
                _sceneManager.LoadScene(new LeaderboardScene(_context, _sceneManager, false, 0, 0));
                break;
            case 3: // Settings
                _sceneManager.LoadScene(new SettingsScene(_context, _sceneManager));
                break;
            case 4: // Exit
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
        
        // Animated background pattern
        for (var i = 0; i < 5; i++)
        {
            var yOffset = ((_animTime * 20 + i * 150) % (_height + 100)) - 50;
            commands.DrawQuad(
                new Vector2(100 + i * 200, yOffset),
                new Vector2(80, 20),
                new Color4(0.1f, 0.1f, 0.15f)
            );
        }
        
        // Title
        var titleY = _height * 0.2f;
        DrawCenteredText(commands, "BREAKOUT", _width / 2f, titleY, 48f, Color4.White);
        DrawCenteredText(commands, "An Ignis Sample Game", _width / 2f, titleY + 60f, 16f, Color4.Gray);
        
        // Menu items
        var menuStartY = _height * 0.45f;
        const float itemSpacing = 50f;
        
        for (var i = 0; i < _menuItems.Length; i++)
        {
            var y = menuStartY + i * itemSpacing;
            var isSelected = i == _selectedIndex;
            
            var color = isSelected ? Color4.Yellow : Color4.White;
            var size = isSelected ? 28f : 24f;
            
            if (isSelected)
            {
                var pulse = MathF.Sin(_animTime * 5f) * 0.2f + 0.8f;
                color = new Color4(1f, 1f, pulse, 1f);
                DrawCenteredText(commands, ">", _width / 2f - 100f, y, size, color);
                DrawCenteredText(commands, "<", _width / 2f + 100f, y, size, color);
            }
            
            DrawCenteredText(commands, _menuItems[i], _width / 2f, y, size, color);
        }
        
        // Instructions
        DrawCenteredText(commands, "Use Arrow Keys to Navigate, Enter to Select", 
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
