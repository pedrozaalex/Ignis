using Ignis.Engine.Reactive;
using Ignis.Engine.UI.Abstractions;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Engine.UI.Widgets
{
    /// <summary>
    /// PropertyGrid - Inspector for editing object properties.
    /// Key widget for game engine editors.
    /// </summary>
    public class PropertyGrid : ViewComponent, Core.IViewContainer
    {
        private readonly Panel _container;
        private readonly List<IView> _properties = new();

        public PropertyGrid()
        {
            _container = new Panel()
            {
                BackgroundColor = new Color(37, 37, 38)
            };
            _container.Layout.LayoutType = LayoutType.Column;
            _container.Layout.PaddingLeft = Units.Pixels(8);
            _container.Layout.PaddingRight = Units.Pixels(8);
            _container.Layout.PaddingTop = Units.Pixels(8);
        }

        public void AddProperty(string label, IView editor)
        {
            var row = new Panel()
            {
                BackgroundColor = Color.Transparent
            };
            row.Layout.LayoutType = LayoutType.Row;
            row.Layout.Height = Units.Pixels(32);
            row.Layout.PaddingBottom = Units.Pixels(4);

            var labelView = new Elements.Text(null) { Content = label, Color = Color.LightGray };
            labelView.Layout.Width = Units.Pixels(100);
            labelView.Layout.PaddingTop = Units.Pixels(6);

            editor.Layout.Width = Units.Stretch(1);

            row.AddChild(labelView);
            row.AddChild(editor);

            _properties.Add(row);
            _container.AddChild(row);
        }

        public void Clear()
        {
            foreach (var prop in _properties)
            {
                prop.Unmount();
            }
            _properties.Clear();
            // Clear container children
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
    /// Hierarchy - Scene hierarchy tree view.
    /// Shows parent-child relationships of game objects.
    /// </summary>
    public class Hierarchy<T> : ViewComponent, Core.IViewContainer where T : notnull
    {
        private readonly TreeView<T> _treeView;
        private readonly Signal<T?> _selectedItem;
        private readonly Panel _container;

        public Hierarchy(SignalList<TreeNode<T>> rootNodes, Func<T, string> displayFunc, Signal<T?>? selectedItem = null)
        {
            _selectedItem = selectedItem ?? new Signal<T?>(default);
            _treeView = new TreeView<T>(rootNodes, displayFunc, _selectedItem);

            _container = new Panel(_treeView)
            {
                BackgroundColor = new Color(37, 37, 38),
                BorderColor = new Color(63, 63, 70),
                BorderThickness = 1f
            };
            _container.Layout.PaddingTop = Units.Pixels(4);
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
    /// ColorPicker - RGB/HSV color selection widget.
    /// </summary>
    public class ColorPicker : ViewComponent, Core.IViewContainer
    {
        private readonly Signal<Color> _color;
        private readonly Panel _container;
        private readonly Signal<float> _hue = new Signal<float>(0f);
        private readonly Signal<float> _saturation = new Signal<float>(1f);
        private readonly Signal<float> _value = new Signal<float>(1f);
        private readonly Signal<float> _alpha = new Signal<float>(1f);

        public ColorPicker(Signal<Color> color)
        {
            _color = color;

            // Build UI: Preview box + RGB sliders or HSV picker
            var preview = new Panel()
            {
                BackgroundColor = color.Value,
                BorderColor = Color.White,
                BorderThickness = 1f
            };
            preview.Layout.Width = Units.Pixels(60);
            preview.Layout.Height = Units.Pixels(60);

            var rSlider = CreateColorSlider("R", 
                new Signal<float>(color.Value.R / 255f), 
                v => UpdateFromRGB());
            var gSlider = CreateColorSlider("G", 
                new Signal<float>(color.Value.G / 255f), 
                v => UpdateFromRGB());
            var bSlider = CreateColorSlider("B", 
                new Signal<float>(color.Value.B / 255f), 
                v => UpdateFromRGB());
            var aSlider = CreateColorSlider("A", 
                _alpha, 
                v => UpdateFromRGB());

            var slidersPanel = new Panel(rSlider, gSlider, bSlider, aSlider)
            {
                BackgroundColor = Color.Transparent
            };
            slidersPanel.Layout.LayoutType = LayoutType.Column;

            _container = new Panel(preview, slidersPanel)
            {
                BackgroundColor = new Color(45, 45, 48),
                BorderColor = new Color(63, 63, 70),
                BorderThickness = 1f
            };
            _container.Layout.LayoutType = LayoutType.Row;
            _container.Layout.PaddingLeft = Units.Pixels(8);
            _container.Layout.PaddingRight = Units.Pixels(8);
            _container.Layout.PaddingTop = Units.Pixels(8);
            _container.Layout.PaddingBottom = Units.Pixels(8);
        }

        private IView CreateColorSlider(string label, Signal<float> value, Action<float> onChange)
        {
            var row = new Panel()
            {
                BackgroundColor = Color.Transparent
            };
            row.Layout.LayoutType = LayoutType.Row;
            row.Layout.Height = Units.Pixels(28);

            var labelView = new Elements.Text(null) { Content = label, Color = Color.White };
            labelView.Layout.Width = Units.Pixels(20);
            labelView.Layout.PaddingTop = Units.Pixels(4);

            var slider = new Slider(value, 0f, 1f);
            slider.Layout.Width = Units.Stretch(1);

            var valueDisplay = new ReactiveText(
                Computed<string>.From(() => ((int)(value.Value * 255)).ToString()),
                null
            );
            valueDisplay.Layout.Width = Units.Pixels(35);
            valueDisplay.Layout.PaddingTop = Units.Pixels(4);

            row.AddChild(labelView);
            row.AddChild(slider);
            row.AddChild(valueDisplay);

            return row;
        }

        private void UpdateFromRGB()
        {
            // TODO: Update _color.Value from individual RGB signals
        }

        protected override void OnMount()
        {
            _container.Mount(Context!);

            CreateEffect(() =>
            {
                var color = _color.Value;
                // Update preview and sliders
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
    /// Vector3Field - Editor for 3D vectors (Position, Rotation, Scale).
    /// </summary>
    public class Vector3Field : ViewComponent, Core.IViewContainer
    {
        private readonly Signal<Vector3> _vector;
        private readonly Panel _container;

        public Vector3Field(string label, Signal<Vector3> vector, SpriteFont? font = null)
        {
            _vector = vector;

            var labelView = new Elements.Text(font) { Content = label, Color = Color.White };
            labelView.Layout.Width = Units.Pixels(80);
            labelView.Layout.PaddingTop = Units.Pixels(6);

            var xField = CreateAxisField("X", 
                Computed<float>.From(() => vector.Value.X),
                v => vector.Value = new Vector3(v, vector.Value.Y, vector.Value.Z));

            var yField = CreateAxisField("Y", 
                Computed<float>.From(() => vector.Value.Y),
                v => vector.Value = new Vector3(vector.Value.X, v, vector.Value.Z));

            var zField = CreateAxisField("Z", 
                Computed<float>.From(() => vector.Value.Z),
                v => vector.Value = new Vector3(vector.Value.X, vector.Value.Y, v));

            _container = new Panel(labelView, xField, yField, zField)
            {
                BackgroundColor = Color.Transparent
            };
            _container.Layout.LayoutType = LayoutType.Row;
            _container.Layout.Height = Units.Pixels(32);
        }

        private IView CreateAxisField(string axis, Computed<float> value, Action<float> onChange)
        {
            var container = new Panel()
            {
                BackgroundColor = Color.Transparent
            };
            container.Layout.LayoutType = LayoutType.Row;
            container.Layout.Width = Units.Stretch(1);
            container.Layout.PaddingLeft = Units.Pixels(4);

            var axisLabel = new Elements.Text(null) { Content = axis, Color = GetAxisColor(axis) };
            axisLabel.Layout.Width = Units.Pixels(12);
            axisLabel.Layout.PaddingTop = Units.Pixels(6);

            var textField = new TextField(new Signal<string?>(value.Value.ToString("F2")))
            {
                BackgroundColor = new Color(51, 51, 55)
            };
            textField.Layout.Width = Units.Stretch(1);
            textField.Layout.Height = Units.Pixels(24);

            container.AddChild(axisLabel);
            container.AddChild(textField);

            return container;
        }

        private Color GetAxisColor(string axis) => axis switch
        {
            "X" => new Color(255, 100, 100), // Red
            "Y" => new Color(100, 255, 100), // Green
            "Z" => new Color(100, 150, 255), // Blue
            _ => Color.White
        };

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
    /// AssetBrowser - Grid/list view of project assets.
    /// </summary>
    public class AssetBrowser<T> : ViewComponent, Core.IViewContainer where T : notnull
    {
        private readonly SignalList<T> _assets;
        private readonly Func<T, string> _nameFunc;
        private readonly Func<T, Texture2D?> _iconFunc;
        private readonly Signal<T?> _selectedAsset;
        private readonly Signal<bool> _isGridView = new Signal<bool>(true);
        private readonly ScrollView _scrollView;

        public AssetBrowser(
            SignalList<T> assets, 
            Func<T, string> nameFunc, 
            Func<T, Texture2D?> iconFunc,
            Signal<T?>? selectedAsset = null)
        {
            _assets = assets;
            _nameFunc = nameFunc;
            _iconFunc = iconFunc;
            _selectedAsset = selectedAsset ?? new Signal<T?>(default);

            var content = Bind.For(_assets, AssetTile);
            _scrollView = new ScrollView(content);
        }

        private IView AssetTile(T asset)
        {
            var icon = new Icon(_iconFunc(asset), 64);
            var label = new Elements.Text(null) 
            { 
                Content = _nameFunc(asset), 
                Color = Color.White 
            };

            var tile = new Panel(icon, label)
            {
                BackgroundColor = new Color(51, 51, 55),
                BorderColor = new Color(63, 63, 70),
                BorderThickness = 1f
            };
            tile.Layout.Width = Units.Pixels(80);
            tile.Layout.Height = Units.Pixels(100);
            tile.Layout.LayoutType = LayoutType.Column;
            tile.Layout.Alignment = Alignment.TopCenter;
            tile.Layout.PaddingTop = Units.Pixels(8);

            // TODO: Wire up click to set _selectedAsset.Value = asset

            return tile;
        }

        protected override void OnMount()
        {
            _scrollView.Mount(Context!);
        }

        protected override void OnUnmount()
        {
            _scrollView.Unmount();
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
        }

        public IEnumerable<IView> GetChildren()
        {
            yield return _scrollView;
        }
    }

    /// <summary>
    /// Console - Output log for messages, warnings, and errors.
    /// </summary>
    public class Console : ViewComponent, Core.IViewContainer
    {
        private readonly SignalList<LogEntry> _entries;
        private readonly ScrollView _scrollView;

        public Console(SignalList<LogEntry> entries)
        {
            _entries = entries;

            var content = Bind.For(_entries, entry => CreateLogEntry(entry));
            _scrollView = new ScrollView(content)
            {
                VerticalScrollEnabled = true
            };
        }

        private IView CreateLogEntry(LogEntry entry)
        {
            var icon = new Elements.Text(null) 
            { 
                Content = GetLogIcon(entry.Level), 
                Color = GetLogColor(entry.Level) 
            };
            icon.Layout.Width = Units.Pixels(20);

            var message = new Elements.Text(null) 
            { 
                Content = entry.Message, 
                Color = GetLogColor(entry.Level) 
            };
            message.Layout.Width = Units.Stretch(1);

            var row = new Panel(icon, message)
            {
                BackgroundColor = Color.Transparent,
                BorderColor = new Color(63, 63, 70),
                BorderThickness = 0f
            };
            row.Layout.LayoutType = LayoutType.Row;
            row.Layout.Height = Units.Pixels(24);
            row.Layout.PaddingLeft = Units.Pixels(8);
            row.Layout.PaddingTop = Units.Pixels(4);

            return row;
        }

        private string GetLogIcon(LogLevel level) => level switch
        {
            LogLevel.Error => "✖",
            LogLevel.Warning => "⚠",
            LogLevel.Info => "ℹ",
            _ => "·"
        };

        private Color GetLogColor(LogLevel level) => level switch
        {
            LogLevel.Error => new Color(255, 100, 100),
            LogLevel.Warning => new Color(255, 200, 100),
            LogLevel.Info => new Color(100, 200, 255),
            _ => Color.LightGray
        };

        protected override void OnMount()
        {
            _scrollView.Mount(Context!);
        }

        protected override void OnUnmount()
        {
            _scrollView.Unmount();
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
        }

        public IEnumerable<IView> GetChildren()
        {
            yield return _scrollView;
        }
    }

    public enum LogLevel { Info, Warning, Error }

    public class LogEntry
    {
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }

        public LogEntry(LogLevel level, string message)
        {
            Level = level;
            Message = message;
            Timestamp = DateTime.Now;
        }
    }
}

