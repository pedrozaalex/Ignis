using Ignis.Engine.Reactive;
using Ignis.Engine.UI.Abstractions;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Elements;
using Ignis.Engine.UI.Widgets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Console = Ignis.Engine.UI.Widgets.Console;

// using Console = Ignis.Engine.UI.Widgets.Console;

namespace Ignis.Engine.UI.Examples
{
    /// <summary>
    /// EditorLayout - Example of a complete game editor UI built with the Widget library.
    /// Demonstrates typical editor structure: MenuBar, Hierarchy, Scene View, Inspector, Console.
    /// </summary>
    public class EditorLayout : ViewComponent, IViewContainer
    {
        private readonly IView _root;
        
        // State
        private readonly Signal<string?> _selectedEntityName = new Signal<string?>("None");
        private readonly SignalList<TreeNode<string>> _hierarchyNodes = new SignalList<TreeNode<string>>();
        private readonly SignalList<LogEntry> _consoleEntries = new SignalList<LogEntry>();
        private readonly Signal<Vector3> _selectedPosition = new Signal<Vector3>(Vector3.Zero);

        public EditorLayout()
        {
            _root = BuildLayout();
            InitializeSampleData();
        }

        private IView BuildLayout()
        {
            // Top Menu Bar
            var menuBar = CreateMenuBar();

            // Main Content Area (Hierarchy | Scene View + Inspector | Console)
            var mainContent = CreateMainContent();

            // Root container
            var container = new Panel(menuBar, mainContent)
            {
                BackgroundColor = new Color(45, 45, 48)
            };
            container.Layout.LayoutType = LayoutType.Column;
            container.Layout.Width = Units.Stretch(1);
            container.Layout.Height = Units.Stretch(1);

            return container;
        }

        private IView CreateMenuBar()
        {
            var menuBar = new MenuBar
            {
                BackgroundColor = new Color(45, 45, 48)
            };

            var fileMenu = new Menu("File");
            fileMenu.AddItem(new MenuItem("New Scene", () => { }));
            fileMenu.AddItem(new MenuItem("Open Scene", () => { }, "Ctrl+O"));
            fileMenu.AddItem(new MenuItem("Save Scene", () => { }, "Ctrl+S"));
            fileMenu.AddItem(MenuItem.Separator());
            fileMenu.AddItem(new MenuItem("Exit", () => { }));

            var editMenu = new Menu("Edit");
            editMenu.AddItem(new MenuItem("Undo", () => { }, "Ctrl+Z"));
            editMenu.AddItem(new MenuItem("Redo", () => { }, "Ctrl+Y"));

            var gameObjectMenu = new Menu("GameObject");
            gameObjectMenu.AddItem(new MenuItem("Create Empty", () => CreateEntity("Empty GameObject")));
            gameObjectMenu.AddItem(new MenuItem("Create Cube", () => CreateEntity("Cube")));
            gameObjectMenu.AddItem(new MenuItem("Create Sphere", () => CreateEntity("Sphere")));

            menuBar.AddMenu(fileMenu);
            menuBar.AddMenu(editMenu);
            menuBar.AddMenu(gameObjectMenu);

            return menuBar;
        }

        private IView CreateMainContent()
        {
            // Left: Hierarchy Panel
            var hierarchy = CreateHierarchyPanel();

            // Center-Right: Scene View + Inspector + Console (stacked and split)
            var centerRight = CreateCenterRightPanel();

            // Horizontal splitter
            var splitter = new Splitter(hierarchy, centerRight, isVertical: false)
            {
                SplitRatio = 0.20f // 20% for hierarchy
            };
            splitter.Layout.Height = Units.Stretch(1);

            return splitter;
        }

        private IView CreateHierarchyPanel()
        {
            var titleBar = CreatePanelTitle("Hierarchy");
            
            var hierarchyView = new Hierarchy<string>(
                _hierarchyNodes,
                name => name,
                _selectedEntityName
            );
            hierarchyView.Layout.Height = Units.Stretch(1);

            var panel = new Panel(titleBar, hierarchyView)
            {
                BackgroundColor = new Color(37, 37, 38),
                BorderColor = new Color(63, 63, 70),
                BorderThickness = 1f
            };
            panel.Layout.LayoutType = LayoutType.Column;

            return panel;
        }

        private IView CreateCenterRightPanel()
        {
            // Top: Scene View (larger)
            var sceneView = CreateSceneViewPanel();

            // Bottom: Inspector + Console side-by-side
            var inspector = CreateInspectorPanel();
            var console = CreateConsolePanel();

            var bottomSplit = new Splitter(inspector, console, isVertical: false)
            {
                SplitRatio = 0.4f // 40% inspector, 60% console
            };
            bottomSplit.Layout.Height = Units.Stretch(1);

            // Vertical splitter
            var verticalSplit = new Splitter(sceneView, bottomSplit, isVertical: true)
            {
                SplitRatio = 0.6f // 60% scene view, 40% bottom panel
            };

            return verticalSplit;
        }

        private IView CreateSceneViewPanel()
        {
            var titleBar = CreatePanelTitle("Scene");

            // Scene viewport (placeholder - actual 3D rendering happens here)
            var viewport = new Panel
            {
                BackgroundColor = new Color(30, 30, 35)
            };
            viewport.Layout.Height = Units.Stretch(1);

            // Scene controls (Grid, Gizmos toggles)
            var controls = CreateSceneControls();
            controls.Layout.Height = Units.Pixels(35);

            var sceneContainer = new Panel(controls, viewport)
            {
                BackgroundColor = new Color(37, 37, 38)
            };
            sceneContainer.Layout.LayoutType = LayoutType.Column;
            sceneContainer.Layout.Height = Units.Stretch(1);

            var panel = new Panel(titleBar, sceneContainer)
            {
                BackgroundColor = new Color(37, 37, 38),
                BorderColor = new Color(63, 63, 70),
                BorderThickness = 1f
            };
            panel.Layout.LayoutType = LayoutType.Column;

            return panel;
        }

        private IView CreateSceneControls()
        {
            var gridToggle = new Checkbox("Grid", new Signal<bool>(true));
            gridToggle.Layout.PaddingLeft = Units.Pixels(8);

            var gizmosToggle = new Checkbox("Gizmos", new Signal<bool>(true));
            gizmosToggle.Layout.PaddingLeft = Units.Pixels(8);

            var panel = new Panel(gridToggle, gizmosToggle)
            {
                BackgroundColor = new Color(45, 45, 48)
            };
            panel.Layout.LayoutType = LayoutType.Row;
            panel.Layout.PaddingTop = Units.Pixels(6);

            return panel;
        }

        private IView CreateInspectorPanel()
        {
            var titleBar = CreatePanelTitle("Inspector");

            var propertyGrid = new PropertyGrid();

            // Add sample properties (bound to reactive state)
            propertyGrid.AddProperty("Name", new TextField(_selectedEntityName));
            propertyGrid.AddProperty("Position", new Vector3Field("", _selectedPosition));
            propertyGrid.AddProperty("Rotation", new Vector3Field("", new Signal<Vector3>(Vector3.Zero)));
            propertyGrid.AddProperty("Scale", new Vector3Field("", new Signal<Vector3>(Vector3.One)));

            // Add a color property
            var colorSignal = new Signal<Color>(Color.White);
            propertyGrid.AddProperty("Color", new ColorPicker(colorSignal));

            var scrollView = new ScrollView(propertyGrid);
            scrollView.Layout.Height = Units.Stretch(1);

            var panel = new Panel(titleBar, scrollView)
            {
                BackgroundColor = new Color(37, 37, 38),
                BorderColor = new Color(63, 63, 70),
                BorderThickness = 1f
            };
            panel.Layout.LayoutType = LayoutType.Column;

            return panel;
        }

        private IView CreateConsolePanel()
        {
            var titleBar = CreatePanelTitle("Console");

            var consoleView = new Console(_consoleEntries);
            consoleView.Layout.Height = Units.Stretch(1);

            // Console controls (Clear button, filters)
            var controls = CreateConsoleControls();
            controls.Layout.Height = Units.Pixels(30);

            var panel = new Panel(titleBar, consoleView, controls)
            {
                BackgroundColor = new Color(37, 37, 38),
                BorderColor = new Color(63, 63, 70),
                BorderThickness = 1f
            };
            panel.Layout.LayoutType = LayoutType.Column;

            return panel;
        }

        private IView CreateConsoleControls()
        {
            var clearButton = new Text() { Content = "Clear", Color = Color.White };
            clearButton.Layout.PaddingLeft = Units.Pixels(8);
            clearButton.Layout.PaddingTop = Units.Pixels(6);
            // TODO: Wire up click to clear console

            var errorFilter = new Checkbox("Errors", new Signal<bool>(true));
            errorFilter.Layout.PaddingLeft = Units.Pixels(16);

            var warningFilter = new Checkbox("Warnings", new Signal<bool>(true));
            warningFilter.Layout.PaddingLeft = Units.Pixels(8);

            var infoFilter = new Checkbox("Info", new Signal<bool>(true));
            infoFilter.Layout.PaddingLeft = Units.Pixels(8);

            var panel = new Panel(clearButton, errorFilter, warningFilter, infoFilter)
            {
                BackgroundColor = new Color(45, 45, 48)
            };
            panel.Layout.LayoutType = LayoutType.Row;

            return panel;
        }

        private IView CreatePanelTitle(string title)
        {
            var titleLabel = new Text() { Content = title, Color = Color.White };

            var panel = new Panel(titleLabel)
            {
                BackgroundColor = new Color(45, 45, 48),
                BorderColor = new Color(63, 63, 70),
                BorderThickness = 0f
            };
            panel.Layout.Height = Units.Pixels(28);
            panel.Layout.PaddingLeft = Units.Pixels(8);
            panel.Layout.PaddingTop = Units.Pixels(6);

            return panel;
        }

        private void InitializeSampleData()
        {
            // Sample hierarchy
            var root = new TreeNode<string>("Scene Root");
            root.IsExpanded.Value = true;

            var camera = new TreeNode<string>("Main Camera", 1);
            var light = new TreeNode<string>("Directional Light", 1);
            
            var cube = new TreeNode<string>("Cube", 1);
            cube.IsExpanded.Value = true;
            var cubeMesh = new TreeNode<string>("Mesh", 2);
            var cubeMaterial = new TreeNode<string>("Material", 2);
            cube.AddChild(cubeMesh);
            cube.AddChild(cubeMaterial);

            root.AddChild(camera);
            root.AddChild(light);
            root.AddChild(cube);

            _hierarchyNodes.Add(root);

            // Sample console entries
            _consoleEntries.Add(new LogEntry(LogLevel.Info, "Editor initialized"));
            _consoleEntries.Add(new LogEntry(LogLevel.Info, "Scene loaded successfully"));
            _consoleEntries.Add(new LogEntry(LogLevel.Warning, "Asset 'texture.png' not found, using default"));
            _consoleEntries.Add(new LogEntry(LogLevel.Error, "Failed to compile shader: unexpected token"));
            _consoleEntries.Add(new LogEntry(LogLevel.Info, "Build completed in 2.3s"));
        }

        private void CreateEntity(string name)
        {
            var newNode = new TreeNode<string>(name, 1);
            if (_hierarchyNodes.Count > 0)
            {
                _hierarchyNodes[0].AddChild(newNode);
            }
            
            _consoleEntries.Add(new LogEntry(LogLevel.Info, $"Created new entity: {name}"));
        }

        protected override void OnMount()
        {
            _root.Mount(Context!);

            // React to selection changes
            CreateEffect(() =>
            {
                var selectedName = _selectedEntityName.Value;
                _consoleEntries.Add(new LogEntry(LogLevel.Info, $"Selected: {selectedName}"));
            });
        }

        protected override void OnUnmount()
        {
            _root.Unmount();
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
        }

        public IEnumerable<IView> GetChildren()
        {
            yield return _root;
        }
    }
}

