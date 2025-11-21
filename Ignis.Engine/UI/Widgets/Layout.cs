using Ignis.Engine.Reactive;
using Ignis.Engine.UI.Abstractions;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Engine.UI.Widgets
{
    /// <summary>
    /// ScrollView - Scrollable container for content larger than viewport.
    /// </summary>
    public class ScrollView : ViewComponent, IViewContainer
    {
        private readonly IView _content;
        private readonly Signal<float> _scrollX = new Signal<float>(0f);
        private readonly Signal<float> _scrollY = new Signal<float>(0f);

        public bool HorizontalScrollEnabled { get; set; } = false;
        public bool VerticalScrollEnabled { get; set; } = true;

        public Color ScrollbarColor { get; set; } = new Color(104, 104, 104);
        public Color TrackColor { get; set; } = new Color(62, 62, 66);

        public ScrollView(IView content)
        {
            _content = content;
            Layout.LayoutType = LayoutType.Column;
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

        public IEnumerable<IView> GetChildren()
        {
            yield return _content;
        }
    }

    /// <summary>
    /// TabView - Tabbed container for multiple views.
    /// </summary>
    public class TabView : ViewComponent, IViewContainer
    {
        private readonly List<(string title, IView content)> _tabs = [];
        private readonly Signal<int> _selectedIndex;
        private readonly Panel _tabBar;
        private readonly Panel _contentArea;

        public Color TabBackgroundColor { get; set; } = new Color(45, 45, 48);
        public Color ActiveTabColor { get; set; } = new Color(0, 122, 204);
        public Color InactiveTabColor { get; set; } = new Color(62, 62, 66);

        public TabView(Signal<int>? selectedIndex = null)
        {
            _selectedIndex = selectedIndex ?? new Signal<int>(0);

            _tabBar = new Panel
            {
                BackgroundColor = TabBackgroundColor
            };
            _tabBar.Layout.LayoutType = LayoutType.Row;
            _tabBar.Layout.Height = Units.Pixels(35);

            _contentArea = new Panel
            {
                BackgroundColor = new Color(37, 37, 38)
            };
            _contentArea.Layout.Height = Units.Stretch(1);

            Layout.LayoutType = LayoutType.Column;
        }

        public void AddTab(string title, IView content)
        {
            var index = _tabs.Count;
            _tabs.Add((title, content));

            // Create tab button
            var tabButton = new Panel(new Text() { Content = title, Color = Color.White })
            {
                BackgroundColor = InactiveTabColor
            };
            tabButton.Layout.Width = Units.Pixels(120);
            tabButton.Layout.Height = Units.Pixels(35);
            tabButton.Layout.PaddingLeft = Units.Pixels(12);
            tabButton.Layout.PaddingTop = Units.Pixels(8);
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

            foreach (var (_, content) in _tabs)
            {
                content.Unmount();
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
        }

        public IEnumerable<IView> GetChildren()
        {
            yield return _tabBar;
            yield return _contentArea;
        }
    }

    /// <summary>
    /// TreeView - Hierarchical tree structure display.
    /// </summary>
    public class TreeView<T> : ViewComponent, IViewContainer where T : notnull
    {
        private readonly SignalList<TreeNode<T>> _rootNodes;
        private readonly Func<T, string> _displayFunc;
        private readonly Signal<T?> _selectedItem;
        private readonly Panel _rootContainer;

        public TreeView(SignalList<TreeNode<T>> rootNodes, Func<T, string> displayFunc, Signal<T?>? selectedItem = null)
        {
            _rootNodes = rootNodes;
            _displayFunc = displayFunc;
            _selectedItem = selectedItem ?? new Signal<T?>(default);

            // Container that will host all node views in a vertical stack
            _rootContainer = new Panel
            {
                BackgroundColor = Color.Transparent
            };
            _rootContainer.Layout.LayoutType = LayoutType.Column;
            _rootContainer.Layout.Width = Units.Stretch(1);
            _rootContainer.Layout.Height = Units.Auto;

            // Build tree using Bind.For into the root container
            var listView = Bind.For(_rootNodes, node => CreateNodeView(node));
            _rootContainer.AddChild(listView);

            // TreeView itself is just a wrapper around the root container
            Layout.LayoutType = LayoutType.Column;
            Layout.Width = Units.Stretch(1);
            Layout.Height = Units.Stretch(1);
        }

        private IView CreateNodeView(TreeNode<T> node)
        {
            var nodePanel = new Panel
            {
                BackgroundColor = Color.Transparent
            };
            nodePanel.Layout.LayoutType = LayoutType.Column;

            // Node header (with expand/collapse arrow)
            var header = new Panel(
                new Text()
                {
                    Content = (node.IsExpanded.Value ? "▼ " : "► ") + _displayFunc(node.Data),
                    Color = Color.White
                }
            )
            {
                BackgroundColor = Color.Transparent
            };
            header.Layout.Height = Units.Pixels(24);
            header.Layout.PaddingLeft = Units.Pixels(node.Depth * 16); // Indent based on depth
            header.Layout.PaddingTop = Units.Pixels(4);

            nodePanel.AddChild(header);

            // Children (if expanded)
            if (node.Children.Count <= 0) return nodePanel;
            
            var childrenContainer = Bind.If(
                node.IsExpanded,
                () => Bind.For(node.Children, CreateNodeView)
            );
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

        public IEnumerable<IView> GetChildren()
        {
            // Treat TreeView as a single container in the layout tree.
            yield return _rootContainer;
        }
    }

    /// <summary>
    /// TreeNode - Node data structure for <see cref="TreeView{T}"/>.
    /// </summary>
    public class TreeNode<T> where T : notnull
    {
        public T Data { get; set; }
        public SignalList<TreeNode<T>> Children { get; } = new SignalList<TreeNode<T>>();
        public Signal<bool> IsExpanded { get; } = new Signal<bool>(false);
        public int Depth { get; set; }

        public TreeNode(T data, int depth = 0)
        {
            Data = data;
            Depth = depth;
        }

        public void AddChild(TreeNode<T> child)
        {
            child.Depth = Depth + 1;
            Children.Add(child);
        }
    }

    /// <summary>
    /// MenuBar - Top-level menu with dropdown menus.
    /// </summary>
    public class MenuBar : ViewComponent, IViewContainer
    {
        private readonly Panel _container;
        private readonly List<Menu> _menus = [];

        public Color BackgroundColor
        {
            get => _container.BackgroundColor;
            set => _container.BackgroundColor = value;
        }

        public MenuBar()
        {
            _container = new Panel
            {
                BackgroundColor = new Color(45, 45, 48)
            };
            _container.Layout.LayoutType = LayoutType.Row;
            _container.Layout.Height = Units.Pixels(30);

            Layout.Height = Units.Pixels(30);
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

        public IEnumerable<IView> GetChildren()
        {
            yield return _container;
        }
    }

    /// <summary>
    /// Menu - Individual menu in a MenuBar.
    /// </summary>
    public class Menu : ViewComponent, IViewContainer
    {
        private readonly string _title;
        private readonly List<MenuItem> _items = [];
        private readonly Signal<bool> _isOpen = new Signal<bool>(false);
        private readonly Panel _container;

        public Menu(string title)
        {
            _title = title;

            var titleLabel = new Text() { Content = title, Color = Color.White };
            _container = new Panel(titleLabel)
            {
                BackgroundColor = Color.Transparent
            };
            _container.Layout.Width = Units.Pixels(80);
            _container.Layout.Height = Units.Pixels(30);
            _container.Layout.PaddingLeft = Units.Pixels(12);
            _container.Layout.PaddingTop = Units.Pixels(6);
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

        public IEnumerable<IView> GetChildren()
        {
            yield return _container;
        }
    }

    /// <summary>
    /// MenuItem - Item in a Menu dropdown.
    /// </summary>
    public class MenuItem
    {
        public string Label { get; set; }
        public Action? OnClick { get; set; }
        public string? Shortcut { get; set; }
        public bool IsSeparator { get; set; }

        public MenuItem(string label, Action? onClick = null, string? shortcut = null)
        {
            Label = label;
            OnClick = onClick;
            Shortcut = shortcut;
        }

        public static MenuItem Separator() => new MenuItem("") { IsSeparator = true };
    }
}

