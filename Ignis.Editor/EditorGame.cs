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
using ReactiveEffect = Ignis.Engine.Reactive.Effect;

namespace Ignis.Editor;

public class EditorGame : IgnisGame
{
    private UIContext? _uiContext;
    private readonly Engine.Reactive.Signal<string> _sceneTitle = new("Untitled Scene");
    private readonly SelectionSystem _selectionSystem = new();
    private readonly ComponentInspector _inspector = new();
    private ReactiveQuery? _entityQuery;
    private readonly SignalList<TreeNode<Entity>> _hierarchyNodes = new();

    public EditorGame() : base(new EngineSettings
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
            _uiContext.SetDefaultFont(DefaultFont);
        }

        _uiContext.SetRoot(BuildMainLayout());
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

        var mesh = App.World.CreateGameObject();
        mesh.Add(new EntityName("Mesh Renderer"));
        player.AddChild(mesh);

        _entityQuery = new ReactiveQuery(App.World.Query<Position>());
        RebuildHierarchy();

        _ = new ReactiveEffect(() =>
        {
            var selected = _selectionSystem.SelectedEntity.Value;
            _inspector.Inspect(selected);
        });
    }

    private void RebuildHierarchy()
    {
        _hierarchyNodes.Clear();

        var rootEntities = App.World.Query<Position>().Entities
            .Where(e => e.Parent.IsNull)
            .ToList();

        foreach (var entity in rootEntities)
        {
            _hierarchyNodes.Add(BuildTreeNode(entity));
        }
    }

    private TreeNode<Entity> BuildTreeNode(Entity entity)
    {
        var name = entity.TryGetComponent<EntityName>(out var entityName)
            ? entityName.value
            : $"Entity {entity.Id}";

        var node = new TreeNode<Entity>(entity) { IsExpanded = { Value = true } };

        foreach (var child in entity.ChildEntities)
        {
            node.AddChild(BuildTreeNode(child));
        }

        return node;
    }

    private IView BuildMainLayout()
    {
        var menuBar = BuildMenuBar();
        var mainContent = BuildMainContent();

        var root = new Panel(menuBar, mainContent)
        {
            Layout =
            {
                LayoutType = LayoutType.Column, Width = Units.Pixels(Window.ClientBounds.Width),
                Height = Units.Pixels(Window.ClientBounds.Height)
            },
            BackgroundColor = Color.FromNonPremultiplied(30, 30, 30, 255)
        };
        
        Window.ClientSizeChanged += (s, e) =>
        {
            root.Layout.Width = Units.Pixels(Window.ClientBounds.Width);
            root.Layout.Height = Units.Pixels(Window.ClientBounds.Height);
        };
        
        return root;
    }

    private IView BuildMenuBar()
    {
        return new Panel(
            Label("File"),
            Label("Edit"),
            Label("View"),
            Label("Help")
        )
        {
            Layout =
            {
                LayoutType = LayoutType.Row,
                Height = Units.Pixels(30),
                PaddingLeft = Units.Pixels(10),
                ColumnGap = Units.Pixels(20),
                PaddingTop = Units.Pixels(5)
            },
            BackgroundColor = Color.FromNonPremultiplied(40, 40, 40, 255),
            BorderThickness = 0f
        };
    }

    private IView BuildMainContent()
    {
        var leftPanel = BuildHierarchyPanel();
        var centerAndRight = BuildCenterAndRightPanels();

        return new Splitter(leftPanel, centerAndRight, isVertical: false)
        {
            SplitRatio = 0.2f,
            Layout = { Width = Units.Stretch(1), Height = Units.Stretch(1) }
        };
    }

    private IView BuildCenterAndRightPanels()
    {
        var center = BuildViewportPanel();
        var right = BuildInspectorPanel();

        return new Splitter(center, right, isVertical: false)
        {
            SplitRatio = 0.7f,
            Layout = { Width = Units.Stretch(1), Height = Units.Stretch(1) }
        };
    }

    private Panel BuildHierarchyPanel()
    {
        var header = BuildPanelHeader("Hierarchy");

        var hierarchyWidget = new Hierarchy<Entity>(
            _hierarchyNodes,
            entity =>
            {
                if (entity.TryGetComponent<EntityName>(out var name))
                    return name.value;
                return $"Entity {entity.Id}";
            },
            _selectionSystem.SelectedEntity
        )
        {
            Layout = { Height = Units.Stretch(1) }
        };

        return new Panel(header, hierarchyWidget)
        {
            Layout = { LayoutType = LayoutType.Column },
            BorderThickness = 1f,
            BorderColor = Color.FromNonPremultiplied(60, 60, 60, 255),
            BackgroundColor = Color.FromNonPremultiplied(25, 25, 25, 255)
        };
    }

    private Panel BuildViewportPanel()
    {
        var header = BuildPanelHeader("Viewport");

        var content = new Panel(
            Label("(3D scene will render here)")
        )
        {
            Layout =
            {
                Height = Units.Stretch(1),
                PaddingLeft = Units.Pixels(10),
                PaddingTop = Units.Pixels(10)
            },
            BackgroundColor = Color.FromNonPremultiplied(45, 45, 50, 255)
        };

        return new Panel(header, content)
        {
            Layout = { LayoutType = LayoutType.Column },
            BorderThickness = 1f,
            BorderColor = Color.FromNonPremultiplied(60, 60, 60, 255)
        };
    }

    private Panel BuildInspectorPanel()
    {
        var header = BuildPanelHeader("Inspector");

        var scrollView = new ScrollView(_inspector.View)
        {
            Layout = { Height = Units.Stretch(1) },
            VerticalScrollEnabled = true
        };

        return new Panel(header, scrollView)
        {
            Layout = { LayoutType = LayoutType.Column },
            BorderThickness = 1f,
            BorderColor = Color.FromNonPremultiplied(60, 60, 60, 255),
            BackgroundColor = Color.FromNonPremultiplied(25, 25, 25, 255)
        };
    }

    private static Panel BuildPanelHeader(string title)
    {
        return new Panel(Label(title))
        {
            Layout =
            {
                Height = Units.Pixels(30),
                PaddingLeft = Units.Pixels(10),
                PaddingTop = Units.Pixels(5)
            },
            BackgroundColor = Color.FromNonPremultiplied(50, 50, 50, 255)
        };
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _entityQuery?.Update();
        _uiContext?.Update(gameTime);
    }

    protected override void OnRenderUI(SpriteBatch spriteBatch)
    {
        base.OnRenderUI(spriteBatch);
        _uiContext?.Draw(spriteBatch);
    }
}