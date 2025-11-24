using FontStashSharp;
using Ignis.Engine.Assets;
using Ignis.Engine.Graphics.Systems;
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
    private RenderSystem? _renderSystem;

    /// <summary>
    /// The headless core application
    /// </summary>
    public IgnisApp App { get; }
    
    /// <summary>
    /// FontSystem for dynamic font loading using FontStashSharp.
    /// </summary>
    public FontSystem? FontSystem { get; private set; }
    
    /// <summary>
    /// The default font for UI rendering. Automatically loaded during LoadContent().
    /// </summary>
    public SpriteFontBase? DefaultFont { get; private set; }

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
        Window.AllowUserResizing = true;

    }

    protected override void Initialize()
    {
        base.Initialize();
        
        // Hook up text input events to InputService
        Window.TextInput += (sender, args) =>
        {
            App.Input.RaiseTextInput(args.Character, args.Key);
        };
        
        App.Initialize();
    }

    protected override void LoadContent()
    {
        App.SimulationRoot.Add(new CameraSystem(GraphicsDevice));
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _renderSystem = new RenderSystem(GraphicsDevice);

        // Create FontSystem with optimal scaling parameters
        FontSystem = DefaultFontProvider.CreateDefaultFontSystem();
        if (FontSystem != null)
        {
            DefaultFont = DefaultFontProvider.GetDefaultFont(FontSystem);
        }

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
        GraphicsDevice.Clear(Color.Black);

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
        // Render all 3D meshes using RenderSystem (Phase 3)
        _renderSystem?.Draw(App.World);
    }

    /// <summary>
    /// Override this to render 2D UI overlay
    /// </summary>
    protected virtual void OnRenderUI(SpriteBatch spriteBatch)
    {
        // Override in derived classes for UI rendering
    }
}