using Ignis.Engine.Core;
using Ignis.Engine.Reactive;
using Ignis.Engine.UI;
using Ignis.Engine.UI.Abstractions;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Widgets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReactiveEffect = Ignis.Engine.Reactive.Effect;
using static Ignis.Engine.UI.Elements.Elements;
using Console = System.Console;

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
        // Create UIContext early so it's available in LoadContent
        // Note: GraphicsDevice won't be available yet, will be set in Initialize
    }

    protected override void LoadContent()
    {
        base.LoadContent(); // This loads the default font automatically
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
            Console.WriteLine("[BasicWidgetsSample] Using automatic default font");
        }
        else
        {
            Console.WriteLine("[BasicWidgetsSample] WARNING: No default font available");
        }

        Console.WriteLine("=== Basic Widgets Sample ===");
        Console.WriteLine("Initializing UI...");

        // Build UI
        var ui = BuildUI();
        _uiContext.SetRoot(ui);

        Console.WriteLine("Demonstrating reactive UI updates.");
        Console.WriteLine("Watch console for reactive changes!");
        Console.WriteLine("================================");

        // Demonstrate reactive effects
        SetupReactiveLogging();
    }


    private IView BuildUI()
    {
        // Content panel
        var contentPanel = Panel()
            .Background(new Color(37, 37, 38))
            .Border(new Color(63, 63, 70), 2f)
            .Width(500)
            .Height(Units.Auto)
            .Padding(30)
            .Children(
                Column(
                    // Title
                    Label("Basic Widgets Demo", null, new Color(100, 200, 255)),

                    // Player Name Row
                    Row(
                        Label("Player Name:", null, Color.LightGray).Width(120),
                        new TextField(_playerName).Width(300)
                    ),

                    // Health Field
                    new NumberField<int>(
                        "Health",
                        _health,
                        x => Math.Min(100, x + 10),
                        x => Math.Max(0, x - 10)
                    ).Width(300),

                    // Alive Checkbox
                    new Checkbox("Is Alive", _isAlive),

                    // Volume Label
                    Label(Computed<string>.From(() => $"Volume: {(_volume.Value * 100):F0}%")),

                    // Volume Slider
                    new Slider(_volume)
                        .Width(300),

                    // Status Panel
                    Panel()
                        .Background(new Color(45, 45, 48))
                        .Border(new Color(0, 122, 204))
                        .Padding(15)
                        .Gap(5)
                        .Children(
                            Label("Current State (Reactive)", null, new Color(255, 200, 100)),
                            Label(Computed<string>.From(() => $"Name: {_playerName.Value ?? "(empty)"}")),
                            Label(Computed<string>.From(() => $"Health: {_health.Value}/100")),
                            Label(Computed<string>.From(() => $"Alive: {(_isAlive.Value ? "Yes" : "No")}")),
                            Label(Computed<string>.From(() => $"Volume: {(_volume.Value * 100):F0}%"))
                        )
                )
            );

        // Wrapper to center content
        return Panel()
            .Background(new Color(25, 25, 28))
            .Width(Units.Stretch(1))
            .Height(Units.Stretch(1))
            .Align(Alignment.Center)
            .Children(contentPanel);
    }

    private void SetupReactiveLogging()
    {
        // Log when player name changed
        // new ReactiveEffect(() => { Console.WriteLine($"[REACTIVE] Player name changed to: {_playerName.Value}"); });

        // Log when health changes
        new ReactiveEffect(() =>
        {
            var health = _health.Value;
            Console.WriteLine($"[REACTIVE] Health changed to: {health}");

            // Auto-update alive status when health reaches 0
            if (health <= 0 && _isAlive.Value)
            {
                Console.WriteLine("[REACTIVE] Health is 0, setting alive to false!");
                _isAlive.Value = false;
            }
        });

        // Log when alive status changes
        new ReactiveEffect(() => { Console.WriteLine($"[REACTIVE] Alive status changed to: {_isAlive.Value}"); });

        // Log when volume changes
        new ReactiveEffect(() => { Console.WriteLine($"[REACTIVE] Volume changed to: {(_volume.Value * 100):F0}%"); });
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
            Console.WriteLine("\n[AUTO] Triggering automatic update...");

            // Cycle through some changes
            var cycle = (int)(totalSeconds / 3.0) % 4;
            switch (cycle)
            {
                case 0:
                    // _playerName.Value = "Auto-Player-" + new Random().Next(1, 100);
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
                        Console.WriteLine("[AUTO] Resurrecting player!");
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