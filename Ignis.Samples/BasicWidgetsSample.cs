using Ignis.Engine.Core;
using Ignis.Engine.Reactive;
using Ignis.Engine.UI;
using Ignis.Engine.UI.Abstractions;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Widgets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReactiveEffect = Ignis.Engine.Reactive.Effect;

namespace Ignis.Samples;

/// <summary>
/// BasicWidgetsSample - Demonstrates basic widget functionality and reactive updates.
/// Shows TextField, NumberField, Checkbox, Slider, and how they update reactively.
/// </summary>
public class BasicWidgetsSample : IgnisGame
{
    private UIContext? _uiContext;
    private SpriteBatch? _spriteBatch;

    // Reactive state
    private readonly Signal<string?> _playerName = new("Player");
    private readonly Signal<int> _health = new(100);
    private readonly Signal<bool> _isAlive = new(true);
    private readonly Signal<float> _volume = new(0.75f);

    public BasicWidgetsSample() : base(new IgnisApp(new EngineSettings
    {
        WindowTitle = "Ignis UI - Basic Widgets Sample",
        WindowWidth = 800,
        WindowHeight = 600
    }))
    {
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        // Get content path
        var contentPath = Path.Combine(Directory.GetCurrentDirectory(), Content.RootDirectory);
        var fontSpriteFontPath = Path.Combine(contentPath, "DefaultFont.spritefont");
        var fontXnbPath = Path.Combine(contentPath, "DefaultFont.xnb");

        // Generate the .spritefont file if it doesn't exist
        if (!File.Exists(fontSpriteFontPath))
        {
            System.Console.WriteLine("Generating DefaultFont.spritefont...");
            GenerateDefaultFontFile(fontSpriteFontPath);
        }

        // Build the content if .xnb doesn't exist
        if (!File.Exists(fontXnbPath))
        {
            System.Console.WriteLine($"Building DefaultFont.xnb with MGCB...");
            System.Console.WriteLine($"  Source: {fontSpriteFontPath}");
            System.Console.WriteLine($"  Target: {fontXnbPath}");
            var success = ContentBuilder.BuildFont(contentPath, "DefaultFont.spritefont");

            if (!success)
            {
                System.Console.WriteLine("⚠ Warning: Could not build .xnb file automatically.");
                System.Console.WriteLine("  Text will not be visible.");
                return;
            }
            
            System.Console.WriteLine($"✓ Build complete. Checking for XNB at: {fontXnbPath}");
            System.Console.WriteLine($"  XNB exists: {File.Exists(fontXnbPath)}");
        }
        else
        {
            System.Console.WriteLine($"✓ Font XNB already exists at: {fontXnbPath}");
        }

        // Try to load the compiled font
        try
        {
            System.Console.WriteLine($"Loading font from Content.RootDirectory: {Content.RootDirectory}");
            var font = Content.Load<SpriteFont>("DefaultFont");
            _uiContext?.SetDefaultFont(font);
            System.Console.WriteLine($"✓ Font loaded successfully! LineSpacing: {font.LineSpacing}, DefaultChar: {font.DefaultCharacter}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"⚠ Warning: Could not load DefaultFont: {ex.Message}");
            System.Console.WriteLine($"  Stack trace: {ex.StackTrace}");
            System.Console.WriteLine("  Text will not be visible.");
        }
    }

    private static void GenerateDefaultFontFile(string path)
    {
        var fontContent = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<XnaContent xmlns:Graphics=""Microsoft.Xna.Framework.Content.Pipeline.Graphics"">
  <Asset Type=""Graphics:FontDescription"">
    <FontName>Arial</FontName>
    <Size>14</Size>
    <Spacing>0</Spacing>
    <UseKerning>true</UseKerning>
    <Style>Regular</Style>
    <CharacterRegions>
      <CharacterRegion>
        <Start>&#32;</Start>
        <End>&#126;</End>
      </CharacterRegion>
    </CharacterRegions>
  </Asset>
</XnaContent>";
        File.WriteAllText(path, fontContent);
        System.Console.WriteLine($"Created {path}");
    }

    protected override void Initialize()
    {
        base.Initialize();

        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _uiContext = new UIContext(GraphicsDevice);

        System.Console.WriteLine("=== Basic Widgets Sample ===");
        System.Console.WriteLine("Initializing UI...");

        // Build UI
        var ui = BuildUI();
        _uiContext.SetRoot(ui);

        System.Console.WriteLine("Demonstrating reactive UI updates.");
        System.Console.WriteLine("Watch console for reactive changes!");
        System.Console.WriteLine("================================");

        // Demonstrate reactive effects
        SetupReactiveLogging();
    }

    private IView BuildUI()
    {
        var titleLabel = new Label("Basic Widgets Demo", null, new Color(100, 200, 255));
        titleLabel.Layout.PaddingBottom = Units.Pixels(20);

        // TextField
        var nameField = new TextField(_playerName);
        nameField.Layout.Width = Units.Pixels(300);

        var nameRow = CreateRow("Player Name:", nameField);

        // NumberField
        var healthField = new NumberField<int>(
            "Health",
            _health,
            x => Math.Min(100, x + 10), // Increment (max 100)
            x => Math.Max(0, x - 10) // Decrement (min 0)
        );
        healthField.Layout.Width = Units.Pixels(300);

        // Checkbox
        var aliveCheckbox = new Checkbox("Is Alive", _isAlive);
        aliveCheckbox.Layout.PaddingTop = Units.Pixels(10);

        // Slider
        var volumeLabel = new Label(
            Computed<string>.From(() => $"Volume: {(_volume.Value * 100):F0}%")
        );
        volumeLabel.Layout.PaddingTop = Units.Pixels(10);

        var volumeSlider = new Slider(_volume, 0f, 1f);
        volumeSlider.Layout.Width = Units.Pixels(300);
        volumeSlider.Layout.PaddingTop = Units.Pixels(5);

        // Status panel showing current state
        var statusPanel = CreateStatusPanel();
        statusPanel.Layout.PaddingTop = Units.Pixels(30);

        // Main content panel
        var contentPanel = new Panel(
            titleLabel,
            nameRow,
            healthField,
            aliveCheckbox,
            volumeLabel,
            volumeSlider,
            statusPanel
        )
        {
            BackgroundColor = new Color(37, 37, 38),
            BorderColor = new Color(63, 63, 70),
            BorderThickness = 2f
        };

        contentPanel.Layout.LayoutType = LayoutType.Column;
        contentPanel.Layout.Width = Units.Pixels(500);
        contentPanel.Layout.Height = Units.Auto; // Size to content
        contentPanel.Layout.PaddingLeft = Units.Pixels(30);
        contentPanel.Layout.PaddingRight = Units.Pixels(30);
        contentPanel.Layout.PaddingTop = Units.Pixels(30);
        contentPanel.Layout.PaddingBottom = Units.Pixels(30);

        // Wrapper to center content
        var wrapper = new Panel(contentPanel)
        {
            BackgroundColor = new Color(25, 25, 28) // Slightly darker background
        };
        wrapper.Layout.LayoutType = LayoutType.Column;
        wrapper.Layout.Width = Units.Stretch(1); // Fill viewport width
        wrapper.Layout.Height = Units.Stretch(1); // Fill viewport height
        wrapper.Layout.Alignment = Alignment.Center; // Center children

        return wrapper;
    }

    private IView CreateRow(string label, IView content)
    {
        var labelView = new Label(label, null, Color.LightGray);
        labelView.Layout.Width = Units.Pixels(120);
        labelView.Layout.PaddingTop = Units.Pixels(6);
        labelView.Layout.PaddingBottom = Units.Pixels(10);

        var row = new Panel(labelView, content)
        {
            BackgroundColor = Color.Transparent
        };
        row.Layout.LayoutType = LayoutType.Row;

        return row;
    }

    private IView CreateStatusPanel()
    {
        var title = new Label("Current State (Reactive)", null, new Color(255, 200, 100));
        title.Layout.PaddingBottom = Units.Pixels(10);

        // These labels automatically update when signals change!
        var nameStatus = new Label(
            Computed<string>.From(() => $"Name: {_playerName.Value ?? "(empty)"}")
        );
        nameStatus.Layout.PaddingBottom = Units.Pixels(5);

        var healthStatus = new Label(
            Computed<string>.From(() => $"Health: {_health.Value}/100")
        );
        healthStatus.Layout.PaddingBottom = Units.Pixels(5);

        var aliveStatus = new Label(
            Computed<string>.From(() => $"Alive: {(_isAlive.Value ? "Yes" : "No")}")
        );
        aliveStatus.Layout.PaddingBottom = Units.Pixels(5);

        var volumeStatus = new Label(
            Computed<string>.From(() => $"Volume: {(_volume.Value * 100):F0}%")
        );

        var statusPanel = new Panel(
            title,
            nameStatus,
            healthStatus,
            aliveStatus,
            volumeStatus
        )
        {
            BackgroundColor = new Color(45, 45, 48),
            BorderColor = new Color(0, 122, 204),
            BorderThickness = 1f
        };
        statusPanel.Layout.LayoutType = LayoutType.Column;
        statusPanel.Layout.PaddingLeft = Units.Pixels(15);
        statusPanel.Layout.PaddingRight = Units.Pixels(15);
        statusPanel.Layout.PaddingTop = Units.Pixels(10);
        statusPanel.Layout.PaddingBottom = Units.Pixels(10);

        return statusPanel;
    }

    private void SetupReactiveLogging()
    {
        // Log when player name changed
        new ReactiveEffect(() =>
        {
            System.Console.WriteLine($"[REACTIVE] Player name changed to: {_playerName.Value}");
        });

        // Log when health changes
        new ReactiveEffect(() =>
        {
            var health = _health.Value;
            System.Console.WriteLine($"[REACTIVE] Health changed to: {health}");

            // Auto-update alive status when health reaches 0
            if (health <= 0 && _isAlive.Value)
            {
                System.Console.WriteLine("[REACTIVE] Health is 0, setting alive to false!");
                _isAlive.Value = false;
            }
        });

        // Log when alive status changes
        new ReactiveEffect(() =>
        {
            System.Console.WriteLine($"[REACTIVE] Alive status changed to: {_isAlive.Value}");
        });

        // Log when volume changes
        new ReactiveEffect(() =>
        {
            System.Console.WriteLine($"[REACTIVE] Volume changed to: {(_volume.Value * 100):F0}%");
        });
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _uiContext?.Update(gameTime);

        // Simulate some automatic updates for demo purposes
        var totalSeconds = gameTime.TotalGameTime.TotalSeconds;

        // Every 3 seconds, change something automatically
        if (totalSeconds % 3.0 < 0.016 && totalSeconds > 1.0)
        {
            System.Console.WriteLine("\n[AUTO] Triggering automatic update...");

            // Cycle through some changes
            var cycle = (int)(totalSeconds / 3.0) % 4;
            switch (cycle)
            {
                case 0:
                    _playerName.Value = "Auto-Player-" + new Random().Next(1, 100);
                    break;
                case 1:
                    _health.Value = Math.Max(0, _health.Value - 15);
                    break;
                case 2:
                    _volume.Value = (float)new Random().NextDouble();
                    break;
                case 3:
                    if (_health.Value <= 0)
                    {
                        _health.Value = 100;
                        _isAlive.Value = true;
                        System.Console.WriteLine("[AUTO] Resurrecting player!");
                    }

                    break;
            }
        }
    }

    protected override void OnRenderUI(SpriteBatch spriteBatch)
    {
        base.OnRenderUI(spriteBatch);

        if (_uiContext != null)
        {
            // Draw UI - PrimitiveBatch handles its own Begin/End internally
            // SpriteBatch is used for text rendering within the draw calls
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