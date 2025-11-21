using Ignis.Engine.Core;
using Ignis.Engine.UI.Core;
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
    private UIContext? _uiContext;
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
        _uiContext = new UIContext(GraphicsDevice);
        
        // Use the automatically loaded default font
        if (DefaultFont != null)
        {
            _uiContext.SetDefaultFont(DefaultFont);
        }
        
        // Create the full editor layout
        var editorLayout = new EditorLayout();
        _uiContext.SetRoot(editorLayout);
        
        Console.WriteLine("=== Complete Editor Layout Sample ===");
        Console.WriteLine("This demonstrates a full game engine editor UI including:");
        Console.WriteLine("  - Menu Bar (File, Edit, GameObject menus)");
        Console.WriteLine("  - Hierarchy Panel (Scene tree with expand/collapse)");
        Console.WriteLine("  - Scene View (3D viewport placeholder)");
        Console.WriteLine("  - Inspector Panel (Property editor with various widgets)");
        Console.WriteLine("  - Console Panel (Log viewer with filtering)");
        Console.WriteLine();
        Console.WriteLine("All panels are reactive and update automatically!");
        Console.WriteLine("Watch the console as the editor reacts to changes.");
        Console.WriteLine("=====================================");
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