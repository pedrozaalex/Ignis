using Ignis.Engine.Core;
using Ignis.Engine.Reactive;
using Ignis.Engine.UI;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Widgets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Console = System.Console;
using ReactiveEffect = Ignis.Engine.Reactive.Effect;
using static Ignis.Engine.UI.Elements.Elements;


namespace Ignis.Samples;

/// <summary>
/// HierarchyWidgetSample - Demonstrates TreeView, Hierarchy, and dynamic list updates.
/// Shows how to build and manipulate hierarchical data structures reactively.
/// </summary>
public class HierarchyWidgetSample() : IgnisGame(new EngineSettings
    { WindowTitle = "Ignis UI - Hierarchy & Lists Sample", WindowWidth = 1200, WindowHeight = 700 })
{
    private UIContext? _uiContext;

    // Reactive state
    private readonly SignalList<TreeNode<string>> _sceneNodes = new();
    private readonly Signal<string?> _selectedNode = new(null);
    private readonly SignalList<LogEntry> _logEntries = new();
    private int _entityCounter;

    protected override void Initialize()
    {
        base.Initialize();

        _uiContext = new UIContext(GraphicsDevice, App.Input);

        // Use the automatically loaded default font
        if (DefaultFont != null)
        {
            _uiContext.SetDefaultFont(DefaultFont);
        }

        // Setup initial scene
        InitializeScene();

        // Build UI
        var ui = BuildUI();
        _uiContext.SetRoot(ui);

        // Setup reactive logging
        SetupReactiveEffects();

        LogInfo("Hierarchy Widget Sample initialized");
    }

    private void InitializeScene()
    {
        var root = new TreeNode<string>("Scene Root")
        {
            IsExpanded = { Value = true }
        };

        root.AddChild(new TreeNode<string>("Main Camera", 1));
        root.AddChild(new TreeNode<string>("Directional Light", 1));

        var player = new TreeNode<string>("Player", 1) { IsExpanded = { Value = true } };
        player.AddChild(new TreeNode<string>("Mesh Renderer", 2));
        player.AddChild(new TreeNode<string>("Collider", 2));

        root.AddChild(player);

        _sceneNodes.Add(root);
        _entityCounter = 3;
    }

    private IView BuildUI()
    {
        // 1. Hierarchy Panel
        var hierarchyPanel = CreateHierarchyPanel();

        // 2. Right Panel (Controls + Console)
        var rightPanel = CreateRightPanel();

        // 3. Main Split
        return new Splitter(hierarchyPanel, rightPanel, isVertical: false)
        {
            SplitRatio = 0.4f,
            Layout = { Width = Units.Stretch(1), Height = Units.Stretch(1) }
        };
    }

    private Panel CreateHierarchyPanel()
    {
        var title = CreateTitle("Scene Hierarchy");

        var hierarchy = new Hierarchy<string>(
            _sceneNodes,
            name => name,
            _selectedNode
        )
        {
            Layout = { Height = Units.Stretch(1) }
        };

        return new Panel(title, hierarchy)
        {
            BorderThickness = 1f,
            Layout = { LayoutType = LayoutType.Column }
        };
    }

    private Splitter CreateRightPanel()
    {
        var controlsPanel = CreateControlsPanel();
        var consolePanel = CreateConsolePanel();

        return new Splitter(controlsPanel, consolePanel, isVertical: true)
        {
            SplitRatio = 0.4f,
            Layout = { Width = Units.Stretch(1), Height = Units.Stretch(1) }
        };
    }

    private Panel CreateControlsPanel()
    {
        var title = CreateTitle("Controls");

        // Selected Item Info
        var infoPanel = new Panel(
            new Label("Selected Entity:", null, Color.Gray),
            new Label(Computed<string>.From(() => _selectedNode.Value ?? "None"), null, Color.White)
        )
        {
            Layout = { PaddingBottom = Units.Pixels(20) }
        };

        // Buttons
        var buttons = new Panel(
            Button("Add New Entity", AddRootEntity),
            Button("Add Child to Selected", AddChildEntity),
            Button("Remove Selected", RemoveSelectedEntity),
            Button("Clear Console", () => _logEntries.Clear())
        )
        {
            Layout = { LayoutType = LayoutType.Column, RowGap = Units.Pixels(5) }
        };

        return new Panel(title, infoPanel, buttons)
        {
            Layout = { PaddingLeft = Units.Pixels(15), PaddingRight = Units.Pixels(15) }
        };
    }

    private IView CreateConsolePanel()
    {
        var title = CreateTitle("Console Log");

        var console = new Engine.UI.Widgets.Console(_logEntries)
        {
            Layout = { Height = Units.Stretch(1) }
        };

        return new Panel(title, console)
        {
            BorderThickness = 1f,
            Layout = { LayoutType = LayoutType.Column }
        };
    }

    private static Panel CreateTitle(string text)
    {
        return new Panel(new Label(text, null, Color.White))
        {
            Layout = { Height = Units.Pixels(30), PaddingLeft = Units.Pixels(10), PaddingTop = Units.Pixels(5) },
            BackgroundColor = Color.FromNonPremultiplied(40, 40, 40, 255)
        };
    }

    // --- Interaction Logic ---

    private void AddRootEntity()
    {
        if (_sceneNodes.Count == 0) return; // Guard

        _entityCounter++;
        var newEntity = new TreeNode<string>($"Entity {_entityCounter}", 1);
        _sceneNodes[0].AddChild(newEntity); // Add to root
        LogInfo($"Added {newEntity.Data} to root");
    }

    private void AddChildEntity()
    {
        var selectedName = _selectedNode.Value;
        if (selectedName == null)
        {
            LogWarning("No entity selected!");
            return;
        }

        // Find selected node
        var node = FindNode(_sceneNodes[0], selectedName);
        if (node != null)
        {
            _entityCounter++;
            var child = new TreeNode<string>($"Child {_entityCounter}");
            node.AddChild(child);
            node.IsExpanded.Value = true; // Auto-expand
            LogInfo($"Added {child.Data} to {node.Data}");
        }
    }

    private void RemoveSelectedEntity()
    {
        var selectedName = _selectedNode.Value;
        if (selectedName == null) return;

        if (selectedName == "Scene Root")
        {
            LogError("Cannot remove Scene Root!");
            return;
        }

        if (RemoveNode(_sceneNodes[0], selectedName))
        {
            LogInfo($"Removed {selectedName}");
            _selectedNode.Value = null;
        }
    }

    // Helper: Recursive find
    private TreeNode<string>? FindNode(TreeNode<string> root, string name)
    {
        if (root.Data == name) return root;
        foreach (var child in root.Children.Items)
        {
            var found = FindNode(child, name);
            if (found != null) return found;
        }

        return null;
    }

    // Helper: Recursive remove
    private bool RemoveNode(TreeNode<string> root, string name)
    {
        // Check children
        var toRemove = root.Children.Items.FirstOrDefault(c => c.Data == name);
        if (toRemove != null)
        {
            root.Children.Remove(toRemove);
            return true;
        }

        foreach (var child in root.Children.Items)
        {
            if (RemoveNode(child, name)) return true;
        }

        return false;
    }

    // --- Helpers ---

    private void LogInfo(string msg) => _logEntries.Add(new LogEntry(LogLevel.Info, msg));
    private void LogWarning(string msg) => _logEntries.Add(new LogEntry(LogLevel.Warning, msg));
    private void LogError(string msg) => _logEntries.Add(new LogEntry(LogLevel.Error, msg));

    private void SetupReactiveEffects()
    {
        _ = new ReactiveEffect(() =>
        {
            if (_selectedNode.Value != null)
                Console.WriteLine($"[Sample] Selection changed: {_selectedNode.Value}");
        });
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _uiContext?.Update(gameTime);
    }

    protected override void OnRenderUI(SpriteBatch spriteBatch)
    {
        base.OnRenderUI(spriteBatch);
        _uiContext?.Draw(spriteBatch);
    }
}