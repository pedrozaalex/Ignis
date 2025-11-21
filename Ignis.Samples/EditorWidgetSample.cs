
using Ignis.Engine.Core;
using Ignis.Engine.Reactive;
using Ignis.Engine.UI.Examples;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Samples;

/// <summary>
/// EditorWidgetSample - Demonstrates the complete EditorLayout with all panels.
/// Shows a full game editor UI with MenuBar, Hierarchy, Scene View, Inspector, and Console.
/// This is the most comprehensive widget demonstration.
/// </summary>
public class EditorWidgetSample : IgnisGame
{
    private Engine.UI.Core.UIContext? _uiContext;
    private SpriteBatch? _spriteBatch;

    public EditorWidgetSample() : base(new IgnisApp(new EngineSettings
    {
        WindowTitle = "Ignis UI - Complete Editor Layout",
        WindowWidth = 1600,
        WindowHeight = 900
    }))
    {
    }

    protected override void LoadContent()
    {
        base.LoadContent(); // Automatically loads default font
    }

    protected override void Initialize()
    {
        base.Initialize();
        
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _uiContext = new Engine.UI.Core.UIContext(GraphicsDevice);
        
        // Use the automatically loaded default font
        if (DefaultFont != null)
        {
            _uiContext.SetDefaultFont(DefaultFont);
        }
        
        // Create the full editor layout
        var editorLayout = new EditorLayout();
        _uiContext.SetRoot(editorLayout);
        
        System.Console.WriteLine("=== Complete Editor Layout Sample ===");
        System.Console.WriteLine("This demonstrates a full game engine editor UI including:");
        System.Console.WriteLine("  - Menu Bar (File, Edit, GameObject menus)");
        System.Console.WriteLine("  - Hierarchy Panel (Scene tree with expand/collapse)");
        System.Console.WriteLine("  - Scene View (3D viewport placeholder)");
        System.Console.WriteLine("  - Inspector Panel (Property editor with various widgets)");
        System.Console.WriteLine("  - Console Panel (Log viewer with filtering)");
        System.Console.WriteLine();
        System.Console.WriteLine("All panels are reactive and update automatically!");
        System.Console.WriteLine("Watch the console as the editor reacts to changes.");
        System.Console.WriteLine("=====================================");
    }


    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _uiContext?.Update(gameTime);
    }

    protected override void OnRenderUI(SpriteBatch spriteBatch)
    {
        base.OnRenderUI(spriteBatch);
        
        if (_uiContext != null)
        {
            // UIContext.Draw handles Begin/End internally now to ensure correct draw order
            // of primitives vs text. Do NOT wrap this in spriteBatch.Begin/End.
            _uiContext.Draw(spriteBatch);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _spriteBatch?.Dispose();
            _uiContext?.Dispose();
        }
        base.Dispose(disposing);
    }
}