using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Engine.Core;

/// <summary>
/// MonoGame Wrapper - Manages GraphicsDevice and visual rendering
/// Inherits from MonoGame's Game class
/// </summary>
public class IgnisGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    
    /// <summary>
    /// The headless core application
    /// </summary>
    public IgnisApp App { get; }
    
    public IgnisGame(IgnisApp? app = null)
    {
        App = app ?? new IgnisApp();
        
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        
        // Apply settings
        _graphics.PreferredBackBufferWidth = App.Settings.WindowWidth;
        _graphics.PreferredBackBufferHeight = App.Settings.WindowHeight;
        _graphics.SynchronizeWithVerticalRetrace = App.Settings.VSync;
        
        IsMouseVisible = true;
        Window.Title = App.Settings.WindowTitle;
    }
    
    protected override void Initialize()
    {
        base.Initialize();
        App.Initialize();
    }
    
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        App.LoadContent();
    }
    
    protected override void Update(GameTime gameTime)
    {
        // Poll input (TODO: Phase 5)
        
        // Step the headless core
        double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;
        App.Update(deltaTime);
        
        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        // Clear screen
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        // Reset render state
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        
        // Render 3D World
        OnRender3D();
        
        // Render 2D UI Overlay
        if (_spriteBatch != null)
        {
            OnRenderUI(_spriteBatch);
        }
        
        base.Draw(gameTime);
    }
    
    /// <summary>
    /// Override this to render 3D content
    /// </summary>
    protected virtual void OnRender3D()
    {
        // Override in derived classes for 3D rendering
    }
    
    /// <summary>
    /// Override this to render 2D UI overlay
    /// </summary>
    protected virtual void OnRenderUI(SpriteBatch spriteBatch)
    {
        // Override in derived classes for UI rendering
    }
}

