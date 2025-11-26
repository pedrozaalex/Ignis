using Friflo.Engine.ECS;
using Ignis.Editor.Systems;
using Ignis.Editor.UI;
using Ignis.Engine.Core;
using Ignis.Engine.ECS;
using Ignis.Engine.ECS.Bridge;
using Ignis.Engine.Reactive;
using Ignis.Engine.UI;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Widgets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Ignis.Engine.UI.Elements.Elements;
using Console = System.Console;
using ReactiveEffect = Ignis.Engine.Reactive.Effect;

namespace Ignis.Editor;

public class Editor : IgnisGame
{
    private UIContext? _uiContext;
    private readonly Engine.Reactive.Signal<string> _sceneTitle = new("Untitled Scene");
    private readonly SelectionSystem _selectionSystem = new();
    private ComponentInspectorV2? _inspector;
    private ReactiveQuery? _entityQuery;
    private readonly SignalList<TreeNode<Entity>> _hierarchyNodes = new();

    public Editor() : base(new EngineSettings
    {
        WindowTitle = "Ignis Editor",
        WindowWidth = 1920,
        WindowHeight = 1080
    })
    {
    }

    protected override void Initialize()
    {
        base.Initialize();

        InitializeScene();

        _uiContext = new UIContext(GraphicsDevice, App.Input);

        if (DefaultFont != null)
        {
            _uiContext!.SetDefaultFont(DefaultFont);
        }

        // Create inspector after UIContext so we have access to theme
        _inspector = new ComponentInspectorV2(_uiContext.Theme);

        // Setup inspector reactive effect after it's created
        _ = new ReactiveEffect(() =>
        {
            var selected = _selectionSystem.SelectedEntity.Value;
            _inspector.Inspect(selected);
        });

        _uiContext!.SetRoot(Root());
    }

    private void InitializeScene()
    {
        var sceneRoot = App.World.CreateGameObject();
        sceneRoot.Add(new EntityName("Scene Root"));

        var camera = App.World.CreateGameObject();
        camera.Add(new EntityName("Main Camera"));
        sceneRoot.AddChild(camera);

        var light = App.World.CreateGameObject();
        light.Add(new EntityName("Directional Light"));
        sceneRoot.AddChild(light);

        var player = App.World.CreateGameObject();
        player.Add(new EntityName("Player"));
        sceneRoot.AddChild(player);

        var playerController = App.World.CreateEntity(new EntityName("PlayerController"));
        player.AddChild(playerController);

        var mesh = App.World.CreateGameObject();
        mesh.Add(new EntityName("Mesh Renderer"));
        player.AddChild(mesh);

        _entityQuery = new ReactiveQuery(App.World.Query());
        RebuildHierarchy();


        _ = new ReactiveEffect(() => { Window.Title = $"Ignis Editor - {_sceneTitle.Value}"; });
    }

    private void RebuildHierarchy()
    {
        _hierarchyNodes.Clear();

        var rootEntities = App.World.Query().Entities
            .Where(e => e.Parent.IsNull)
            .ToList();

        foreach (var entity in rootEntities)
        {
            _hierarchyNodes.Add(TreeNode(entity, 0));
        }
    }

    private TreeNode<Entity> TreeNode(Entity entity, int depth)
    {
        _ = entity.TryGetComponent<EntityName>(out var entityName)
            ? entityName.value
            : $"Entity {entity.Id}";

        var node = new TreeNode<Entity>(entity, depth) { IsExpanded = { Value = false } };

        foreach (var child in entity.ChildEntities)
        {
            node.AddChild(TreeNode(child, depth + 1));
        }

        return node;
    }

    private IView Root()
    {
        var root =
                Column(MenuBar(), MainContent())
                    .Width(Window.ClientBounds.Width)
                    .Height(Window.ClientBounds.Height)
            ;

        Window.ClientSizeChanged += (_, _) =>
        {
            root.Layout.Width = Units.Pixels(Window.ClientBounds.Width);
            root.Layout.Height = Units.Pixels(Window.ClientBounds.Height);
        };

        return root;
    }

    private IView MenuBar()
    {
        var fileMenu = MenuButton("File", () => LogMessage("File menu clicked"));
        var editMenu = MenuButton("Edit", () => LogMessage("Edit menu clicked"));
        var viewMenu = MenuButton("View", () => LogMessage("View menu clicked"));
        var helpMenu = MenuButton("Help", () => LogMessage("Help menu clicked"));

        var panel = Panel(
                    fileMenu,
                    editMenu,
                    viewMenu,
                    helpMenu
                )
                // .Height(24)
                .Background(_uiContext!.Theme.SurfaceActive)
            ;

        panel.Layout.LayoutType = LayoutType.Row;

        return panel;
    }

    private IView MenuButton(string text, Action onClick)
    {
        var button = Panel(Label(text))
                .Background(Color.Transparent)
                .AlignCenter()
                .Padding(8, 4)
                .OnClick(onClick)
            ;

        return button;
    }

    private void LogMessage(string message)
    {
        Console.WriteLine($"[Editor] {message}");
    }

    private IView MainContent()
    {
        var leftPanel = HierarchyPanel();
        var centerAndRight = CenterAndRightPanels();

        return new Splitter(leftPanel, centerAndRight, isVertical: false)
        {
            SplitRatio = 0.2f,
            Layout = { Width = Units.Stretch(1), Height = Units.Stretch(1) },
        };
    }

    private IView CenterAndRightPanels()
    {
        var center = ViewportPanel();
        var right = InspectorPanel();

        return new Splitter(center, right, isVertical: false)
        {
            SplitRatio = 0.7f,
            Layout = { Width = Units.Stretch(1), Height = Units.Stretch(1) }
        };
    }

    private Panel HierarchyPanel()
    {
        var header = PanelHeader("Hierarchy");

        var hierarchyWidget = new Hierarchy<Entity>(
            _hierarchyNodes,
            entity => entity.TryGetComponent<EntityName>(out var name) ? name.value : $"Entity {entity.Id}",
            _selectionSystem.SelectedEntity
        )
        {
            Layout =
            {
                Height = Units.Stretch(1),
                Width = Units.Stretch(1)
            }
        };

        return new Panel(header, hierarchyWidget)
        {
            Layout = { LayoutType = LayoutType.Column },
            BorderThickness = 1f,
            BorderColor = _uiContext!.Theme.Border,
            BackgroundColor = _uiContext!.Theme.SurfaceActive
        };
    }

    private IView ViewportPanel()
    {
        var header = PanelHeader("Viewport");

        var content = Panel(Label("(3D scene will render here)"))
                .Height(Units.Stretch(1))
                .Border(_uiContext!.Theme.Border)
                .Padding(8)
            ;

        return Column(header, content);
    }

    private IView InspectorPanel()
    {
        var header = PanelHeader("Inspector");

        var scrollView = new ScrollView(_inspector!.View)
        {
            Layout =
            {
                Height = Units.Stretch(1),
                Width = Units.Stretch(1)
            },
            VerticalScrollEnabled = true
        };

        // return new Panel(header, scrollView)
        // {
        //     Layout = { LayoutType = LayoutType.Column },
        //     BorderThickness = 1f,
        //     BorderColor = _uiContext!.Theme.Border,
        //     BackgroundColor = _uiContext!.Theme.Background
        // };
        return Panel(header, scrollView)
                .Height(Units.Stretch(1))
                .Background(_uiContext!.Theme.Surface)
            ;
    }

    private Panel PanelHeader(string title)
    {
        return Panel()
            .Height(30)
            .Padding(8, 4)
            .Background(_uiContext!.Theme.SurfaceOverlay)
            .AlignCenter()
            .Children(
                Label(title)
            );
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _entityQuery?.Update();
        _inspector?.Update();
        _uiContext?.Update(gameTime);
    }

    protected override void OnRenderUI(SpriteBatch spriteBatch)
    {
        base.OnRenderUI(spriteBatch);
        _uiContext?.Draw(spriteBatch);
    }
}