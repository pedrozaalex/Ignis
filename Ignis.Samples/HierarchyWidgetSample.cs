using Ignis.Engine.Core;
using Ignis.Engine.Reactive;
using Ignis.Engine.UI;
using Ignis.Engine.UI.Abstractions;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Widgets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Console = System.Console;
using ReactiveEffect = Ignis.Engine.Reactive.Effect;

namespace Ignis.Samples;

/// <summary>
/// HierarchyWidgetSample - Demonstrates TreeView, Hierarchy, and dynamic list updates.
/// Shows how to build and manipulate hierarchical data structures reactively.
/// </summary>
public class HierarchyWidgetSample : IgnisGame
{
    private UIContext? _uiContext;
    private SpriteBatch? _spriteBatch;
    
    // Reactive state
    private readonly SignalList<TreeNode<string>> _sceneNodes = new SignalList<TreeNode<string>>();
    private readonly Signal<string?> _selectedNode = new Signal<string?>(null);
    private readonly SignalList<LogEntry> _logEntries = new SignalList<LogEntry>();
    private int _entityCounter = 0;

    public HierarchyWidgetSample() : base(new IgnisApp(new EngineSettings
    {
        WindowTitle = "Ignis UI - Hierarchy & Lists Sample",
        WindowWidth = 1200,
        WindowHeight = 700
    }))
    {
    }

    protected override void Initialize()
    {
        base.Initialize();
        
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _uiContext = new UIContext(GraphicsDevice);
        
        // Setup initial scene
        InitializeScene();
        
        // Build UI
        var ui = BuildUI();
        _uiContext.SetRoot(ui);
        
        // Setup reactive logging
        SetupReactiveEffects();
        
        LogInfo("Hierarchy Widget Sample initialized");
        LogInfo("Watch the hierarchy and console update reactively!");
    }

    private void InitializeScene()
    {
        var root = new TreeNode<string>("Scene Root", 0);
        root.IsExpanded.Value = true;

        var camera = new TreeNode<string>("Main Camera", 1);
        var light = new TreeNode<string>("Directional Light", 1);
        
        var player = new TreeNode<string>("Player", 1);
        player.IsExpanded.Value = true;
        player.AddChild(new TreeNode<string>("Mesh Renderer", 2));
        player.AddChild(new TreeNode<string>("Collider", 2));
        player.AddChild(new TreeNode<string>("Script", 2));

        root.AddChild(camera);
        root.AddChild(light);
        root.AddChild(player);

        _sceneNodes.Add(root);
        _entityCounter = 3; // Camera, Light, Player
    }

    private IView BuildUI()
    {
        // Left side - Hierarchy
        var hierarchyPanel = CreateHierarchyPanel();
        
        // Right side - Console and Controls
        var rightPanel = CreateRightPanel();

        // Main split view
        var splitter = new Splitter(hierarchyPanel, rightPanel, isVertical: false)
        {
            SplitRatio = 0.5f
        };
        splitter.Layout.Width = Units.Stretch(1);
        splitter.Layout.Height = Units.Stretch(1);

        return splitter;
    }

    private IView CreateHierarchyPanel()
    {
        var title = CreateTitle("Scene Hierarchy");

        var hierarchy = new Hierarchy<string>(
            _sceneNodes,
            name => name,
            _selectedNode
        );
        hierarchy.Layout.Height = Units.Stretch(1);

        var panel = new Panel(title, hierarchy)
        {
            BackgroundColor = new Color(37, 37, 38),
            BorderColor = new Color(63, 63, 70),
            BorderThickness = 1f
        };
        panel.Layout.LayoutType = LayoutType.Column;

        return panel;
    }

    private IView CreateRightPanel()
    {
        var controlsPanel = CreateControlsPanel();
        var consolePanel = CreateConsolePanel();

        var splitter = new Splitter(controlsPanel, consolePanel, isVertical: true)
        {
            SplitRatio = 0.35f
        };

        return splitter;
    }

    private IView CreateControlsPanel()
    {
        var title = CreateTitle("Controls");

        // Selection info
        var selectionLabel = new Label("Selected:", null, Color.LightGray);
        selectionLabel.Layout.PaddingBottom = Units.Pixels(5);

        var selectedName = new Label(
            Computed<string>.From(() => _selectedNode.Value ?? "(none)")
        );
        selectedName.Layout.PaddingBottom = Units.Pixels(20);

        // Buttons (Note: Clicks not wired yet, but showing structure)
        var buttonPanel = CreateButtonPanel();

        var container = new Panel(
            title,
            selectionLabel,
            selectedName,
            buttonPanel
        )
        {
            BackgroundColor = new Color(37, 37, 38),
            BorderColor = new Color(63, 63, 70),
            BorderThickness = 1f
        };
        container.Layout.LayoutType = LayoutType.Column;
        container.Layout.PaddingLeft = Units.Pixels(15);
        container.Layout.PaddingRight = Units.Pixels(15);

        return container;
    }

    private IView CreateButtonPanel()
    {
        var addButton = CreateButton("Add Entity");
        var addChildButton = CreateButton("Add Child");
        var removeButton = CreateButton("Remove");
        var clearLogsButton = CreateButton("Clear Logs");

        var panel = new Panel(
            addButton,
            addChildButton,
            removeButton,
            clearLogsButton
        )
        {
            BackgroundColor = Color.Transparent
        };
        panel.Layout.LayoutType = LayoutType.Column;

        return panel;
    }

    private IView CreateButton(string text)
    {
        var label = new Label(text, null, Color.White);
        
        var button = new Panel(label)
        {
            BackgroundColor = new Color(0, 122, 204),
            BorderColor = new Color(0, 100, 180),
            BorderThickness = 1f
        };
        button.Layout.Width = Units.Pixels(150);
        button.Layout.Height = Units.Pixels(32);
        button.Layout.PaddingLeft = Units.Pixels(15);
        button.Layout.PaddingTop = Units.Pixels(8);
        button.Layout.PaddingBottom = Units.Pixels(10);

        return button;
    }

    private IView CreateConsolePanel()
    {
        var title = CreateTitle("Console");

        var consoleWidget = new Ignis.Engine.UI.Widgets.Console(_logEntries);
        consoleWidget.Layout.Height = Units.Stretch(1);

        // Stats
        var statsLabel = new Label(
            Computed<string>.From(() => $"Entries: {_logEntries.Count}")
        );
        statsLabel.Layout.Height = Units.Pixels(25);
        statsLabel.Layout.PaddingLeft = Units.Pixels(10);
        statsLabel.Layout.PaddingTop = Units.Pixels(5);

        var panel = new Panel(title, consoleWidget, statsLabel)
        {
            BackgroundColor = new Color(37, 37, 38),
            BorderColor = new Color(63, 63, 70),
            BorderThickness = 1f
        };
        panel.Layout.LayoutType = LayoutType.Column;

        return panel;
    }

    private IView CreateTitle(string text)
    {
        var label = new Label(text, null, Color.White);

        var panel = new Panel(label)
        {
            BackgroundColor = new Color(45, 45, 48)
        };
        panel.Layout.Height = Units.Pixels(30);
        panel.Layout.PaddingLeft = Units.Pixels(10);
        panel.Layout.PaddingTop = Units.Pixels(7);

        return panel;
    }

    private void SetupReactiveEffects()
    {
        // Log selection changes
        new ReactiveEffect(() =>
        {
            var selected = _selectedNode.Value;
            if (selected != null)
            {
                LogInfo($"Selected: {selected}");
            }
        });

        // Log hierarchy changes
        _sceneNodes.ItemAdded += (item, index) =>
        {
            LogInfo($"Node added to hierarchy: {item.Data}");
        };

        _sceneNodes.ItemRemoved += (item, index) =>
        {
            LogWarning($"Node removed from hierarchy: {item.Data}");
        };
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _uiContext?.Update(gameTime);
        
        // Auto-demo: Add entities every 5 seconds
        var totalSeconds = gameTime.TotalGameTime.TotalSeconds;
        if (totalSeconds % 5.0 < 0.016 && totalSeconds > 2.0)
        {
            AddRandomEntity();
        }
    }

    private void AddRandomEntity()
    {
        if (_sceneNodes.Count == 0) return;

        _entityCounter++;
        var entityTypes = new[] { "Cube", "Sphere", "Plane", "Light", "Camera", "Empty" };
        var randomType = entityTypes[new Random().Next(entityTypes.Length)];
        var entityName = $"{randomType} {_entityCounter}";

        var newNode = new TreeNode<string>(entityName, 1);
        
        // Add to root
        var root = _sceneNodes[0];
        root.AddChild(newNode);

        LogInfo($"Auto-created: {entityName}");

        // Auto-expand root if collapsed
        if (!root.IsExpanded.Value)
        {
            root.IsExpanded.Value = true;
        }
    }

    private void LogInfo(string message)
    {
        _logEntries.Add(new LogEntry(LogLevel.Info, message));
        Console.WriteLine($"[INFO] {message}");
    }

    private void LogWarning(string message)
    {
        _logEntries.Add(new LogEntry(LogLevel.Warning, message));
        Console.WriteLine($"[WARNING] {message}");
    }

    private void LogError(string message)
    {
        _logEntries.Add(new LogEntry(LogLevel.Error, message));
        Console.WriteLine($"[ERROR] {message}");
    }

    protected override void OnRenderUI(SpriteBatch spriteBatch)
    {
        base.OnRenderUI(spriteBatch);
        
        if (_uiContext != null)
        {
            spriteBatch.Begin();
            _uiContext.Draw(spriteBatch);
            spriteBatch.End();
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

