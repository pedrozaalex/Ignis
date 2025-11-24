using Ignis.Engine.Core;
using Ignis.Engine.Reactive;
using Ignis.Engine.UI;
using Ignis.Engine.UI.Abstractions;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Elements;
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
    private readonly Signal<string?> _playerName = new("Player Name");
    private readonly Signal<int> _health = new(100);
    private readonly Signal<bool> _isAlive = new(true);
    private readonly Signal<float> _volume = new(0.75f);

    public BasicWidgetsSample() : base(new IgnisApp(new EngineSettings
    {
        WindowTitle = "Ignis UI - Basic Widgets Sample",
        WindowWidth = 1200,
        WindowHeight = 800
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
        _uiContext = new UIContext(GraphicsDevice, App.Input);
        _uiContext.SetGame(this); // Set game reference for FontSystem access

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
        // Get theme-aware colors - will resolve when context is available
        Color titleColor = Color.White; // Will be overridden in OnMount via theme
        Color warningColor = Color.White; // Will be overridden in OnMount via theme

        // Content panel
        var contentPanel = Panel()
            .Border(Color.Transparent, 2f)
            .Width(Units.Auto)
            .Height(Units.Auto)
            .Padding(30)
            .Children(
                Column(
                    // Title using the new Title() helper (32pt) - use theme InfoColor
                    CreateThemedTitle("Basic Widgets Demo"),
                    Rule(),

                    // Section Heading (24pt)
                    Heading("User Profile", Color.LightGray),
                    Column(
                            // Player Name Row
                            Row(
                                Label("Player Name:", null, Color.LightGray),
                                new TextField(_playerName) { Placeholder = "Type here..." }.Width(300)
                            ).AlignCenter(),

                            // Health Field with custom label
                            Row(
                                Label("Health:", null, Color.LightGray),
                                new NumberField<int>(
                                    _health,
                                    x => Math.Min(100, x + 10),
                                    x => Math.Max(0, x - 10)
                                ).Width(300)
                            ).AlignCenter(),

                            // Alive Checkbox
                            new Checkbox("Is Alive", _isAlive))
                        .Gap(5),
                    Rule(),

                    // Another section with Heading (24pt)
                    Heading("Settings", Color.LightGray),

                    // Volume Label
                    Label(Computed<string>.From(() => $"Volume: {_volume.Value * 100:F0}%")),

                    // Volume Slider
                    new Slider(_volume)
                        .Width(300),
                    Rule(),

                    // Status Panel with Subheading (18pt)
                    CreateStatusPanel()
                )
            );

        // Wrapper to center content - uses theme BackgroundColor by default
        return Panel()
            .Width(Units.Stretch(1))
            .Height(Units.Stretch(1))
            .Align(Alignment.Center)
            .Children(contentPanel);
    }

    private IView CreateThemedTitle(string text)
    {
        // Create a custom component that resolves color from theme after mount
        var titleView = new ThemedTitleView(text);
        return titleView;
    }

    private IView CreateStatusPanel()
    {
        var panel = Panel()
            .Padding(15)
            .Gap(5)
            .Children(
                CreateThemedSubheading("Current State (Reactive)"),
                Label(Computed<string>.From(() => $"Name: {_playerName.Value ?? "(empty)"}")),
                Label(Computed<string>.From(() => $"Health: {_health.Value}/100")),
                Label(Computed<string>.From(() => $"Alive: {(_isAlive.Value ? "Yes" : "No")}")),
                Label(Computed<string>.From(() => $"Volume: {_volume.Value * 100:F0}%"))
            );

        // Set border to use PrimaryColor after mount
        var wrapper = new ThemedPanelView(panel);
        return wrapper;
    }

    private IView CreateThemedSubheading(string text)
    {
        return new ThemedSubheadingView(text);
    }

    // Helper components that resolve colors from theme after mounting
    private class ThemedTitleView : ViewComponent, IViewContainer
    {
        private readonly IView _title;

        public ThemedTitleView(string text)
        {
            _title = Title(text, Color.White); // Will be updated in OnMount
        }

        protected override void OnMount()
        {
            _title.Mount(Context!);
            // Update color from theme
            if (_title is IViewContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    if (child is Text textView)
                    {
                        textView.Color = Context!.Theme.InfoColor;
                    }
                }
            }
        }

        protected override void OnUnmount() => _title.Unmount();

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
        }

        public IEnumerable<IView> GetChildren()
        {
            yield return _title;
        }
    }

    private class ThemedSubheadingView : ViewComponent, IViewContainer
    {
        private readonly IView _subheading;

        public ThemedSubheadingView(string text)
        {
            _subheading = Subheading(text, Color.White);
        }

        protected override void OnMount()
        {
            _subheading.Mount(Context!);
            if (_subheading is IViewContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    if (child is Text textView)
                    {
                        textView.Color = Context!.Theme.WarningColor;
                    }
                }
            }
        }

        protected override void OnUnmount() => _subheading.Unmount();

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
        }

        public IEnumerable<IView> GetChildren()
        {
            yield return _subheading;
        }
    }

    private class ThemedPanelView : ViewComponent, IViewContainer
    {
        private readonly Panel _panel;

        public ThemedPanelView(Panel panel)
        {
            _panel = panel;
        }

        protected override void OnMount()
        {
            _panel.Mount(Context!);
            _panel.BorderColor = Context!.Theme.PrimaryColor;
            _panel.BorderThickness = 1f;
        }

        protected override void OnUnmount() => _panel.Unmount();

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
        }

        public IEnumerable<IView> GetChildren()
        {
            yield return _panel;
        }
    }

    private void SetupReactiveLogging()
    {
        // Log when player name changed
        new ReactiveEffect(() => { Console.WriteLine($"[REACTIVE] Player name changed to: {_playerName.Value}"); });

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
        new ReactiveEffect(() => { Console.WriteLine($"[REACTIVE] Volume changed to: {_volume.Value * 100:F0}%"); });
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _uiContext?.Update(gameTime);

        // Simulate some automatic updates for demo purposes
        var totalSeconds = gameTime.TotalGameTime.TotalSeconds;

        // Every 3 seconds, change something automatically
        if (totalSeconds % 3.0 >= 0.016 || totalSeconds <= 1.0) return;
        Console.WriteLine("\n[AUTO] Triggering automatic update...");

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
                    Console.WriteLine("[AUTO] Resurrecting player!");
                }

                break;
        }
    }

    protected override void OnRenderUI(SpriteBatch spriteBatch)
    {
        base.OnRenderUI(spriteBatch);

        // Draw UI - PrimitiveBatch handles its own Begin/End internally
        // SpriteBatch is used for text rendering within the draw calls
        _uiContext?.Draw(spriteBatch);
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