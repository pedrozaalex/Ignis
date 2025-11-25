using Ignis.Engine.Reactive;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Engine.UI.Widgets;

/// <summary>
///     ScrollView - Scrollable container for content larger than viewport.
/// </summary>
public class ScrollView : ViewComponent, IViewContainer
{
    private readonly IView _content;
    private readonly Signal<float> _scrollX = new(0f);
    private readonly Signal<float> _scrollY = new(0f);

    public ScrollView(IView content)
    {
        _content = content;
        Layout.LayoutType = LayoutType.Column;
    }

    public bool HorizontalScrollEnabled { get; set; } = false;
    public bool VerticalScrollEnabled { get; set; } = true;

    public Color? ScrollbarColor { get; set; }
    public Color? TrackColor { get; set; }

    public IEnumerable<IView> GetChildren()
    {
        yield return _content;
    }

    protected override void OnMount()
    {
        _content.Mount(Context!);

        CreateEffect(() =>
        {
            var scrollY = _scrollY.Value;
            var scrollX = _scrollX.Value;
            // TODO: Apply scroll offset to content rendering
        });
    }

    protected override void OnUnmount()
    {
        _content.Unmount();
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
    {
        // TODO: Clip content to bounds and render scrollbars
    }
}

/// <summary>
///     TabView - Tabbed container for multiple views.
/// </summary>
public class TabView : ViewComponent, IViewContainer
{
    private readonly Panel _contentArea;
    private readonly Signal<int> _selectedIndex;
    private readonly Panel _tabBar;
    private readonly List<(string title, IView content)> _tabs = [];

    public TabView(Signal<int>? selectedIndex = null)
    {
        _selectedIndex = selectedIndex ?? new Signal<int>(0);

        _tabBar = new Panel
        {
            Layout =
            {
                LayoutType = LayoutType.Row,
                Height = Units.Pixels(35)
            }
        };

        _contentArea = new Panel
        {
            Layout =
            {
                Height = Units.Stretch(1)
            }
        };

        Layout.LayoutType = LayoutType.Column;
    }

    public Color? TabBackgroundColor { get; set; }
    public Color? ActiveTabColor { get; set; }
    public Color? InactiveTabColor { get; set; }

    public IEnumerable<IView> GetChildren()
    {
        yield return _tabBar;
        yield return _contentArea;
    }

    public void AddTab(string title, IView content)
    {
        var index = _tabs.Count;
        _tabs.Add((title, content));

        // Create tab button
        var tabButton = new Panel(new Text { Content = title, Color = Color.White })
        {
            BackgroundColor = InactiveTabColor,
            Layout =
            {
                Width = Units.Pixels(120),
                Height = Units.Pixels(35),
                PaddingLeft = Units.Pixels(12),
                PaddingTop = Units.Pixels(8)
            }
        };
        // TODO: Wire up click to set _selectedIndex.Value = index

        _tabBar.AddChild(tabButton);
    }

    protected override void OnMount()
    {
        _tabBar.Mount(Context!);
        _contentArea.Mount(Context!);

        CreateEffect(() =>
        {
            var selectedIdx = _selectedIndex.Value;
            if (selectedIdx >= 0 && selectedIdx < _tabs.Count)
            {
                // Update active tab visual
                // Mount selected content
                var (title, content) = _tabs[selectedIdx];
                _contentArea.RemoveChild(_contentArea.GetChildren().FirstOrDefault()!);
                _contentArea.AddChild(content);
            }
        });
    }

    protected override void OnUnmount()
    {
        _tabBar.Unmount();
        _contentArea.Unmount();

        foreach (var (_, content) in _tabs) content.Unmount();
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
    {
    }
}


/// <summary>
///     TreeView - Hierarchical tree structure display.
/// </summary>
public class TreeView<T> : ViewComponent, IViewContainer where T : notnull
{
    private readonly Func<T, string> _displayFunc;
    private readonly Panel _rootContainer;
    private readonly SignalList<TreeNode<T>> _rootNodes;
    private readonly Signal<T?> _selectedItem;

    public TreeView(SignalList<TreeNode<T>> rootNodes, Func<T, string> displayFunc, Signal<T?>? selectedItem = null)
    {
        _rootNodes = rootNodes;
        _displayFunc = displayFunc;
        _selectedItem = selectedItem ?? new Signal<T?>(default);

        // Container that will host all node views in a vertical stack
        _rootContainer = new Panel
        {
            BackgroundColor = Color.Transparent,
            Layout =
            {
                LayoutType = LayoutType.Column,
                Width = Units.Stretch(1),
                Height = Units.Auto
            }
        };

        // Build tree using Bind.For into the root container
        var listView = Bind.For(_rootNodes, CreateNodeView);
        // Ensure list view stretches to fill container, otherwise it collapses in Auto columns
        listView.Layout.Width = Units.Stretch(1);
        _rootContainer.AddChild(listView);

        // TreeView itself is just a wrapper around the root container
        Layout.LayoutType = LayoutType.Column;
        Layout.Width = Units.Stretch(1);
        Layout.Height = Units.Stretch(1);
    }

    public IEnumerable<IView> GetChildren()
    {
        // Treat TreeView as a single container in the layout tree.
        yield return _rootContainer;
    }

    private IView CreateNodeView(TreeNode<T> node)
    {
        var nodePanel = new Panel
        {
            BackgroundColor = Color.Transparent,
            Layout =
            {
                LayoutType = LayoutType.Column,
                Width = Units.Stretch(1) // Ensure node panel stretches to fill ListView
            }
        };

        // Node header (with expand/collapse arrow)
        var arrowText = new ReactiveText(
            Computed<string>.From(() => node.IsExpanded.Value ? "▼ " : "► "),
            null
        );

        var labelText = new Text
        {
            Content = _displayFunc(node.Data)
        };

        var headerContent = new Panel(arrowText, labelText)
        {
            BackgroundColor = Color.Transparent,
            Layout = { LayoutType = LayoutType.Row }
        };

        var isSelected = Computed<bool>.From(() =>
        {
            var selected = _selectedItem.Value;
            return selected != null && EqualityComparer<T>.Default.Equals(selected, node.Data);
        });

        var header = new Panel(headerContent)
        {
            BackgroundColor = Color.Transparent,
            Layout =
            {
                Width = Units.Stretch(1),
                Height = Units.Pixels(24),
                PaddingLeft = Units.Pixels(node.Depth * 16), // Indent based on depth
                PaddingTop = Units.Pixels(4)
            }
        };

        // Make header clickable
        if (header is ViewComponent headerComponent)
        {
            headerComponent.OnClick(() =>
            {
                // Toggle expansion if has children
                if (node.Children.Count > 0)
                {
                    node.IsExpanded.Value = !node.IsExpanded.Value;
                }

                // Update selection
                _selectedItem.Value = node.Data;
            });

            // Set up reactive background color for selection highlighting
            headerComponent.CreateEffect(() =>
            {
                header.BackgroundColor = isSelected.Value ? Context!.Theme.SurfaceActive : Color.Transparent;
            });
        }

        nodePanel.AddChild(header);

        // Children (if expanded)
        if (node.Children.Count <= 0) return nodePanel;

        var childrenContainer = Bind.If(
            node.IsExpanded,
            () => Bind.For(node.Children, CreateNodeView).Width(Units.Stretch(1)) // Ensure nested list also stretches
        ).Width(Units.Stretch(1)); // Ensure the conditional container stretches

        nodePanel.AddChild(childrenContainer);

        return nodePanel;
    }

    protected override void OnMount()
    {
        _rootContainer.Mount(Context!);
    }

    protected override void OnUnmount()
    {
        _rootContainer.Unmount();
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
    {
        // Intentionally left empty – children are drawn via UIContext/Panel drawing.
    }
}

/// <summary>
///     TreeNode - Node data structure for <see cref="TreeView{T}" />.
/// </summary>
public class TreeNode<T> where T : notnull
{
    public TreeNode(T data, int depth = 0)
    {
        Data = data;
        Depth = depth;
    }

    public T Data { get; set; }
    public SignalList<TreeNode<T>> Children { get; } = new();
    public Signal<bool> IsExpanded { get; } = new(false);
    public int Depth { get; set; }

    public void AddChild(TreeNode<T> child)
    {
        child.Depth = Depth + 1;
        Children.Add(child);
    }
}

/// <summary>
///     MenuBar - Top-level menu with dropdown menus.
/// </summary>
public class MenuBar : ViewComponent, IViewContainer
{
    private readonly Panel _container;
    private readonly List<Menu> _menus = [];

    public MenuBar()
    {
        _container = new Panel
        {
            Layout =
            {
                LayoutType = LayoutType.Row,
                Height = Units.Pixels(30)
            }
        };

        Layout.Height = Units.Pixels(30);
    }

    public Color? BackgroundColor
    {
        get => _container.BackgroundColor;
        set => _container.BackgroundColor = value;
    }

    public IEnumerable<IView> GetChildren()
    {
        yield return _container;
    }

    public void AddMenu(Menu menu)
    {
        _menus.Add(menu);
        _container.AddChild(menu);
    }

    protected override void OnMount()
    {
        _container.Mount(Context!);
    }

    protected override void OnUnmount()
    {
        _container.Unmount();
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
    {
    }
}

/// <summary>
///     Menu - Individual menu in a MenuBar.
/// </summary>
public class Menu : ViewComponent, IViewContainer
{
    private readonly Panel _container;
    private readonly Signal<bool> _isOpen = new(false);
    private readonly List<MenuItem> _items = [];
    private readonly string _title;

    public Menu(string title)
    {
        _title = title;

        var titleLabel = new Text { Content = title, Color = Color.White };
        _container = new Panel(titleLabel)
        {
            BackgroundColor = Color.Transparent,
            Layout =
            {
                Width = Units.Pixels(80),
                Height = Units.Pixels(30),
                PaddingLeft = Units.Pixels(12),
                PaddingTop = Units.Pixels(6)
            }
        };
    }

    public IEnumerable<IView> GetChildren()
    {
        yield return _container;
    }

    public void AddItem(MenuItem item)
    {
        _items.Add(item);
    }

    protected override void OnMount()
    {
        _container.Mount(Context!);

        CreateEffect(() =>
        {
            var isOpen = _isOpen.Value;
            // TODO: Show/hide popup menu
        });
    }

    protected override void OnUnmount()
    {
        _container.Unmount();
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
    {
    }
}

/// <summary>
///     MenuItem - Item in a Menu dropdown.
/// </summary>
public class MenuItem
{
    public MenuItem(string label, Action? onClick = null, string? shortcut = null)
    {
        Label = label;
        OnClick = onClick;
        Shortcut = shortcut;
    }

    public string Label { get; set; }
    public Action? OnClick { get; set; }
    public string? Shortcut { get; set; }
    public bool IsSeparator { get; set; }

    public static MenuItem Separator()
    {
        return new MenuItem("") { IsSeparator = true };
    }
}