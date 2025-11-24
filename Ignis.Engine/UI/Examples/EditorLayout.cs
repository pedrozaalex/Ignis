using Ignis.Engine.Reactive;
using Ignis.Engine.UI.Abstractions;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Elements;
using Ignis.Engine.UI.Widgets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Console = Ignis.Engine.UI.Widgets.Console;
using static Ignis.Engine.UI.Elements.Elements;

// using Console = Ignis.Engine.UI.Widgets.Console;

namespace Ignis.Engine.UI.Examples;

/// <summary>
/// EditorLayout - Example of a complete game editor UI built with the Widget library.
/// Refactored to use robust explicit sizing to prevent layout collapse.
/// </summary>
public class EditorLayout : ViewComponent, IViewContainer
{
    private readonly IView _root;

    // State
    private readonly Signal<string?> _selectedEntityName = new("None");
    private readonly SignalList<TreeNode<string>> _hierarchyNodes = new();
    private readonly SignalList<LogEntry> _consoleEntries = new();
    private readonly Signal<Vector3> _selectedPosition = new(Vector3.Zero);
    private readonly Signal<Vector3> _selectedRotation = new(Vector3.Zero);
    private readonly Signal<Vector3> _selectedScale = new(Vector3.One);
    private readonly Signal<Color> _selectedColor = new(Color.White);

    public EditorLayout()
    {
        InitializeSampleData();
        _root = BuildLayout();
    }

    private IView BuildLayout()
    {
        // 1. Create the leaf panels
        var hierarchyPanel = CreateHierarchyPanel();
        var scenePanel = CreateSceneViewPanel();
        var inspectorPanel = CreateInspectorPanel();
        var consolePanel = CreateConsolePanel();

        // 2. Compose them using Splitters
        // IMPORTANT: We must explicitly Stretch splitters so they define a size 
        // for their percentage-based children.

        // Bottom-Right: Inspector (40%) | Console (60%)
        var bottomRightSplit = new Splitter(inspectorPanel, consolePanel, isVertical: false)
            {
                SplitRatio = 0.4f
            }
            .Width(Units.Stretch(1))
            .Height(Units.Stretch(1));

        // Right Pane: Scene (60%) / Bottom-Right (40%)
        var rightPaneSplit = new Splitter(scenePanel, bottomRightSplit, isVertical: true)
            {
                SplitRatio = 0.6f
            }
            .Width(Units.Stretch(1))
            .Height(Units.Stretch(1));

        // Main Split: Hierarchy (20%) | Right Pane (80%)
        var mainSplit = new Splitter(hierarchyPanel, rightPaneSplit, isVertical: false)
            {
                SplitRatio = 0.2f
            }
            .Width(Units.Stretch(1))
            .Height(Units.Stretch(1));

        // 3. Root Layout: MenuBar + MainSplit
        return Column(
                CreateMenuBar(),
                mainSplit
            )
            // .Background(new Color(45, 45, 48))
            .Width(Units.Stretch(1))
            .Height(Units.Stretch(1));
    }

    private IView CreateMenuBar()
    {
        var menuBar = new MenuBar();

        var fileMenu = new Menu("File");
        fileMenu.AddItem(new MenuItem("New Scene", () => Log("New Scene")));
        fileMenu.AddItem(new MenuItem("Open Scene", () => Log("Open Scene"), "Ctrl+O"));
        fileMenu.AddItem(new MenuItem("Save Scene", () => Log("Save Scene"), "Ctrl+S"));
        fileMenu.AddItem(MenuItem.Separator());
        fileMenu.AddItem(new MenuItem("Exit", () => Log("Exit")));

        var editMenu = new Menu("Edit");
        editMenu.AddItem(new MenuItem("Undo", () => Log("Undo"), "Ctrl+Z"));
        editMenu.AddItem(new MenuItem("Redo", () => Log("Redo"), "Ctrl+Y"));

        var gameObjectMenu = new Menu("GameObject");
        gameObjectMenu.AddItem(new MenuItem("Create Empty", () => CreateEntity("Empty GameObject")));
        gameObjectMenu.AddItem(new MenuItem("Create Cube", () => CreateEntity("Cube")));
        gameObjectMenu.AddItem(new MenuItem("Create Sphere", () => CreateEntity("Sphere")));

        menuBar.AddMenu(fileMenu);
        menuBar.AddMenu(editMenu);
        menuBar.AddMenu(gameObjectMenu);

        return menuBar;
    }

    private IView CreateHierarchyPanel()
    {
        var hierarchyView = new Hierarchy<string>(
                _hierarchyNodes,
                name => name,
                _selectedEntityName
            )
            .Height(Units.Stretch(1));

        return CreatePanelWithTitle("Hierarchy", hierarchyView);
    }

    private IView CreateSceneViewPanel()
    {
        // Placeholder for 3D viewport
        var viewport = Panel()
            .Height(Units.Stretch(1));

        var controls = Row(
                new Checkbox("Grid", new Signal<bool>(true)).PaddingLeft(8),
                new Checkbox("Gizmos", new Signal<bool>(true)).PaddingLeft(8)
            )
            .PaddingTop(6)
            .Height(35);

        var content = Column(controls, viewport)
            // .Background(new Color(37, 37, 38))
            .Height(Units.Stretch(1));

        return CreatePanelWithTitle("Scene", content);
    }

    private IView CreateInspectorPanel()
    {
        var propertyGrid = new PropertyGrid();

        propertyGrid.AddProperty("Name", new TextField(_selectedEntityName));
        propertyGrid.AddProperty("Position", new Vector3Field("", _selectedPosition));
        propertyGrid.AddProperty("Rotation", new Vector3Field("", _selectedRotation));
        propertyGrid.AddProperty("Scale", new Vector3Field("", _selectedScale));
        propertyGrid.AddProperty("Color", new ColorPicker(_selectedColor));

        var scrollView = new ScrollView(propertyGrid)
            .Height(Units.Stretch(1));

        return CreatePanelWithTitle("Inspector", scrollView);
    }

    private IView CreateConsolePanel()
    {
        var consoleView = new Ignis.Engine.UI.Widgets.Console(_consoleEntries)
            .Height(Units.Stretch(1));

        var controls = Row(
                Button("Clear", () => _consoleEntries.Clear())
                    .Height(24)
                    .Width(60)
                    .PaddingLeft(8)
                    .PaddingTop(4),
                new Checkbox("Errors", new Signal<bool>(true)).PaddingLeft(16),
                new Checkbox("Warnings", new Signal<bool>(true)).PaddingLeft(8),
                new Checkbox("Info", new Signal<bool>(true)).PaddingLeft(8)
            )
            // .Background(new Color(45, 45, 48))
            .Height(30);

        var content = Column(consoleView, controls)
            .Height(Units.Stretch(1));

        return CreatePanelWithTitle("Console", content);
    }

    private IView CreatePanelWithTitle(string title, IView content)
    {
        var titleBar = Panel(
                new Text { Content = title, Color = Color.White }
            )
            .Height(28)
            .PaddingLeft(8)
            .PaddingTop(6);

        return Column(titleBar, content)
            .Width(Units.Stretch(1))
            .Height(Units.Stretch(1));
    }

    private void Log(string message)
    {
        _consoleEntries.Add(new LogEntry(LogLevel.Info, message));
    }

    private void InitializeSampleData()
    {
        var root = new TreeNode<string>("Scene Root");
        root.IsExpanded.Value = true;

        var camera = new TreeNode<string>("Main Camera", 1);
        var light = new TreeNode<string>("Directional Light", 1);

        var cube = new TreeNode<string>("Cube", 1);
        cube.IsExpanded.Value = true;
        cube.AddChild(new TreeNode<string>("Mesh", 2));
        cube.AddChild(new TreeNode<string>("Material", 2));

        root.AddChild(camera);
        root.AddChild(light);
        root.AddChild(cube);

        _hierarchyNodes.Add(root);

        _consoleEntries.Add(new LogEntry(LogLevel.Info, "Editor initialized"));
        _consoleEntries.Add(new LogEntry(LogLevel.Warning, "Texture asset missing meta file"));
    }

    private void CreateEntity(string name)
    {
        var newNode = new TreeNode<string>(name, 1);
        if (_hierarchyNodes.Count > 0)
            _hierarchyNodes[0].AddChild(newNode);

        Log($"Created {name}");
    }

    protected override void OnMount()
    {
        _root.Mount(Context!);

        // Selection effect
        CreateEffect(() =>
        {
            if (_selectedEntityName.Value != "None")
                Log($"Selected: {_selectedEntityName.Value}");
        });
    }

    protected override void OnUnmount() => _root.Unmount();

    public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
    {
    }

    public IEnumerable<IView> GetChildren()
    {
        yield return _root;
    }
}