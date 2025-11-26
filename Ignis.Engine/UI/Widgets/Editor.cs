using FontStashSharp;
using Ignis.Engine.Reactive;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Engine.UI.Widgets;

/// <summary>
///     PropertyGrid - Inspector for editing object properties.
///     Key widget for game engine editors.
/// </summary>
public class PropertyGrid : ViewComponent, IViewContainer
{
    private readonly Panel _container;
    private readonly List<IView> _properties = [];

    public PropertyGrid()
    {
        _container = new Panel
        {
            Layout =
            {
                LayoutType = LayoutType.Column,
                PaddingLeft = Units.Pixels(4), // Reduce padding for density
                PaddingRight = Units.Pixels(4),
                PaddingTop = Units.Pixels(4),
                Width = Units.Stretch(1) // Overflow Fix: Ensure grid respects parent width
            }
        };
    }

    public IEnumerable<IView> GetChildren()
    {
        yield return _container;
    }

    public void AddProperty(string label, IView editor)
    {
        var row = new Panel
        {
            BackgroundColor = Color.Transparent,
            Layout =
            {
                LayoutType = LayoutType.Row,
                Height = Units.Auto, // Auto height to fit content
                PaddingBottom = Units.Pixels(2),
                Width = Units.Stretch(1) // Overflow Fix: Row stretches to grid width
            }
        };

        // Only add label if it's not empty
        if (!string.IsNullOrEmpty(label))
        {
            var labelView = new Text
            {
                Content = label, 
                Color = Color.FromNonPremultiplied(180, 180, 180, 255),
                Layout =
                {
                    Width = Units.Pixels(90), // Fixed label width
                    PaddingTop = Units.Pixels(2)
                }
            };
            row.AddChild(labelView);
        }

        // Editor takes remaining space
        editor.Layout.Width = Units.Stretch(1);
        // Slight vertical alignment fix
        editor.Layout.PaddingTop = Units.Pixels(0); 

        row.AddChild(editor);

        _properties.Add(row);
        _container.AddChild(row);
    }

    public void Clear()
    {
        foreach (var prop in _properties)
        {
            _container.RemoveChild(prop);
        }
        _properties.Clear();
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
///     Hierarchy - Scene hierarchy tree view.
/// </summary>
public class Hierarchy<T> : ViewComponent, IViewContainer where T : notnull
{
    private readonly Panel _container;
    private readonly Signal<T?> _selectedItem;
    private readonly TreeView<T> _treeView;

    public Hierarchy(SignalList<TreeNode<T>> rootNodes, Func<T, string> displayFunc, Signal<T?>? selectedItem = null)
    {
        _selectedItem = selectedItem ?? new Signal<T?>(default);
        _treeView = new TreeView<T>(rootNodes, displayFunc, _selectedItem);

        _container = new Panel(_treeView)
        {
            BorderThickness = 1f,
            Layout =
            {
                PaddingTop = Units.Pixels(4),
                Width = Units.Stretch(1), // Ensure full width for hit testing
                Height = Units.Stretch(1)
            }
        };
    }

    public IEnumerable<IView> GetChildren()
    {
        yield return _container;
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
///     ColorPicker - RGB/HSV color selection widget.
/// </summary>
public class ColorPicker : ViewComponent, IViewContainer
{
    private readonly Signal<float> _alpha = new(1f);
    private readonly Signal<Color> _color;
    private readonly Panel _container;
    private readonly Signal<float> _hue = new(0f);
    private readonly Signal<float> _saturation = new(1f);
    private readonly Signal<float> _value = new(1f);

    public ColorPicker(Signal<Color> color)
    {
        _color = color;

        // Build UI: Preview box + RGB sliders or HSV picker
        var preview = new Panel
        {
            BackgroundColor = color.Value,
            BorderColor = Color.White,
            BorderThickness = 1f,
            Layout =
            {
                Width = Units.Pixels(60),
                Height = Units.Pixels(60)
            }
        };

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
            BackgroundColor = Color.Transparent,
            Layout =
            {
                LayoutType = LayoutType.Column
            }
        };

        _container = new Panel(preview, slidersPanel)
        {
            BorderThickness = 1f,
            Layout =
            {
                LayoutType = LayoutType.Row,
                PaddingLeft = Units.Pixels(8),
                PaddingRight = Units.Pixels(8),
                PaddingTop = Units.Pixels(8),
                PaddingBottom = Units.Pixels(8)
            }
        };
    }

    public IEnumerable<IView> GetChildren()
    {
        yield return _container;
    }

    private IView CreateColorSlider(string label, Signal<float> value, Action<float> onChange)
    {
        var row = new Panel
        {
            BackgroundColor = Color.Transparent,
            Layout =
            {
                LayoutType = LayoutType.Row,
                Height = Units.Pixels(28)
            }
        };

        var labelView = new Text
        {
            Content = label, Color = Color.White,
            Layout =
            {
                Width = Units.Pixels(20),
                PaddingTop = Units.Pixels(4)
            }
        };

        var slider = new Slider(value)
        {
            Layout =
            {
                Width = Units.Stretch(1)
            }
        };

        var valueDisplay = new ReactiveText(
            Computed<string>.From(() => ((int)(value.Value * 255)).ToString()),
            null
        )
        {
            Layout =
            {
                Width = Units.Pixels(35),
                PaddingTop = Units.Pixels(4)
            }
        };

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
}

/// <summary>
///     Vector3Field - Editor for 3D vectors (Position, Rotation, Scale).
/// </summary>
public class Vector3Field : ViewComponent, IViewContainer
{
    private readonly Panel _container;
    private readonly Signal<Vector3> _vector;

    public Vector3Field(string label, Signal<Vector3> vector, SpriteFontBase? font = null)
    {
        _vector = vector;

        var labelView = new Text(font)
        {
            Content = label, Color = Color.White,
            Layout =
            {
                Width = Units.Pixels(80),
                PaddingTop = Units.Pixels(6)
            }
        };

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
            BackgroundColor = Color.Transparent,
            Layout =
            {
                LayoutType = LayoutType.Row,
                Height = Units.Pixels(32)
            }
        };
    }

    public IEnumerable<IView> GetChildren()
    {
        yield return _container;
    }

    private IView CreateAxisField(string axis, Computed<float> value, Action<float> onChange)
    {
        var container = new Panel
        {
            BackgroundColor = Color.Transparent,
            Layout =
            {
                LayoutType = LayoutType.Row,
                Width = Units.Stretch(1),
                PaddingLeft = Units.Pixels(4)
            }
        };

        var axisLabel = new Text
        {
            Content = axis, Color = GetAxisColor(axis),
            Layout =
            {
                Width = Units.Pixels(12),
                PaddingTop = Units.Pixels(6)
            }
        };

        var textField = new TextField(new Signal<string?>(value.Value.ToString("F2")))
        {
            Layout =
            {
                Width = Units.Stretch(1),
                Height = Units.Pixels(24)
            }
        };

        container.AddChild(axisLabel);
        container.AddChild(textField);

        return container;
    }

    private Color GetAxisColor(string axis)
    {
        if (Context == null) return Color.White;

        return axis switch
        {
            "X" => Context.Theme.Error, // Red for X axis
            "Y" => Context.Theme.Success, // Green for Y axis
            "Z" => Context.Theme.Info, // Blue for Z axis
            _ => Color.White
        };
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
///     AssetBrowser - Grid/list view of project assets.
/// </summary>
public class AssetBrowser<T> : ViewComponent, IViewContainer where T : notnull
{
    private readonly SignalList<T> _assets;
    private readonly Func<T, Texture2D?> _iconFunc;
    private readonly Signal<bool> _isGridView = new(true);
    private readonly Func<T, string> _nameFunc;
    private readonly ScrollView _scrollView;
    private readonly Signal<T?> _selectedAsset;

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

    public IEnumerable<IView> GetChildren()
    {
        yield return _scrollView;
    }

    private IView AssetTile(T asset)
    {
        var icon = new Icon(_iconFunc(asset), 64);
        var label = new Text
        {
            Content = _nameFunc(asset),
            Color = Color.White
        };

        var tile = new Panel(icon, label)
        {
            BorderThickness = 1f,
            Layout =
            {
                Width = Units.Pixels(80),
                Height = Units.Pixels(100),
                LayoutType = LayoutType.Column,
                Alignment = Alignment.TopCenter,
                PaddingTop = Units.Pixels(8)
            }
        };

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
}

/// <summary>
///     Console - Output log for messages, warnings, and errors.
/// </summary>
public class Console : ViewComponent, IViewContainer
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

    public IEnumerable<IView> GetChildren()
    {
        yield return _scrollView;
    }

    private IView CreateLogEntry(LogEntry entry)
    {
        var icon = new Text
        {
            Content = GetLogIcon(entry.Level),
            Color = GetLogColor(entry.Level),
            Layout =
            {
                Width = Units.Pixels(20)
            }
        };

        var message = new Text
        {
            Content = entry.Message,
            Color = GetLogColor(entry.Level),
            Layout =
            {
                Width = Units.Stretch(1)
            }
        };

        var row = new Panel(icon, message)
        {
            BackgroundColor = Color.Transparent,
            BorderThickness = 0f,
            Layout =
            {
                LayoutType = LayoutType.Row,
                Height = Units.Pixels(24),
                PaddingLeft = Units.Pixels(8),
                PaddingTop = Units.Pixels(4)
            }
        };

        return row;
    }

    private string GetLogIcon(LogLevel level)
    {
        return level switch
        {
            LogLevel.Error => "[X]",
            LogLevel.Warning => "[!]",
            LogLevel.Info => "[i]",
            _ => "[-]"
        };
    }

    private Color GetLogColor(LogLevel level)
    {
        if (Context == null) return Color.LightGray;

        return level switch
        {
            LogLevel.Error => Context.Theme.Error,
            LogLevel.Warning => Context.Theme.Warning,
            LogLevel.Info => Context.Theme.Info,
            _ => Color.LightGray
        };
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
}

public enum LogLevel
{
    Info,
    Warning,
    Error
}

public class LogEntry
{
    public LogEntry(LogLevel level, string message)
    {
        Level = level;
        Message = message;
        Timestamp = DateTime.Now;
    }

    public LogLevel Level { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
}
