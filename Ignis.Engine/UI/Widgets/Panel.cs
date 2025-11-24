using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Engine.UI.Widgets
{
    /// <summary>
    /// Panel - A styled container with optional background, border, and corner radius.
    /// Foundation for most editor UI elements.
    /// </summary>
    public class Panel : ViewComponent, IViewContainer
    {
        private readonly List<IView> _children = [];
        private Color? _originalBackgroundColor;
        private Color? _currentBackgroundColor;

        public Color? BackgroundColor 
        { 
            get => _currentBackgroundColor ?? _originalBackgroundColor;
            set
            {
                _originalBackgroundColor = value;
                _currentBackgroundColor = value;
            }
        }
        public Color? BorderColor { get; set; }
        public float BorderThickness { get; set; }
        public float CornerRadius { get; set; }

        public Panel(params IView[] children)
        {
            _children.AddRange(children);
            Layout.LayoutType = LayoutType.Column;
        }

        // Fluent styling methods
        public Panel Background(Color color)
        {
            BackgroundColor = color;
            return this;
        }

        public Panel Border(Color color, float thickness = 1f)
        {
            BorderColor = color;
            BorderThickness = thickness;
            return this;
        }

        public Panel Rounded(float radius)
        {
            CornerRadius = radius;
            return this;
        }

        public Panel Children(params IView[] children)
        {
            foreach (var child in children)
            {
                AddChild(child);
            }

            return this;
        }

        public void AddChild(IView child)
        {
            _children.Add(child);
            if (Context != null)
            {
                child.Mount(Context);
            }
        }

        public void RemoveChild(IView child)
        {
            child.Unmount();
            _children.Remove(child);
        }

        protected override void OnMount()
        {
            foreach (var child in _children)
            {
                child.Mount(Context!);
            }
            
            // If this panel has a click handler, apply hover effects
            if (EventHandlers.OnPointerUp != null || EventHandlers.OnPointerDown != null)
            {
                CreateEffect(() =>
                {
                    var state = CurrentState;
                    
                    // Apply hover/active color changes using theme state colors
                    if (state.HasFlag(WidgetState.Active))
                    {
                        // Use theme's active state color
                        var baseColor = _originalBackgroundColor ?? Context!.Theme.Surface;
                        _currentBackgroundColor = baseColor == Context!.Theme.Primary 
                            ? Context.Theme.PrimaryActive 
                            : Context.Theme.SurfaceActive;
                    }
                    else if (state.HasFlag(WidgetState.Hovered))
                    {
                        // Use theme's hover state color
                        var baseColor = _originalBackgroundColor ?? Context!.Theme.Surface;
                        _currentBackgroundColor = baseColor == Context!.Theme.Primary 
                            ? Context.Theme.PrimaryHover 
                            : Context.Theme.SurfaceHover;
                    }
                    else
                    {
                        // Normal state
                        _currentBackgroundColor = _originalBackgroundColor;
                    }
                });
            }
        }

        protected override void OnUnmount()
        {
            foreach (var child in _children)
            {
                child.Unmount();
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Context?.PrimitiveBatch == null) return;

            var primitiveBatch = Context.PrimitiveBatch;

            var bg = BackgroundColor ?? Context.Theme.Surface;
            var border = BorderColor ?? Context.Theme.Border;

            // Draw background
            if (bg.A > 0)
            {
                primitiveBatch.DrawRoundedRectangle(bounds, CornerRadius, bg);
            }

            // Draw border
            if (BorderThickness > 0 && border.A > 0)
            {
                primitiveBatch.DrawBorder(bounds, BorderThickness, border, CornerRadius);
            }
        }

        public IEnumerable<IView> GetChildren() => _children;
    }

    /// <summary>
    /// Window - A draggable, resizable panel with a title bar.
    /// </summary>
    public class Window : Panel
    {
        private readonly IView _titleBar;
        private readonly IView _content;

        public string Title { get; set; }
        public bool IsResizable { get; set; } = true;
        public bool IsDraggable { get; set; } = true;

        public Window(string title, IView content)
        {
            Title = title;
            _content = content;

            // Create title bar
            var titleLabel = new Text { Content = title, Color = Color.White };
            _titleBar = new Panel(titleLabel)
            {
            };
            _titleBar.Layout.Height = Units.Pixels(30);
            _titleBar.Layout.Width = Units.Stretch(1);
            _titleBar.Layout.PaddingLeft = Units.Pixels(10);
            _titleBar.Layout.PaddingTop = Units.Pixels(7);

            // Add to layout
            AddChild(_titleBar);
            AddChild(_content);

            // Style (colors will be resolved from theme)
            BorderThickness = 1f;
            Layout.Width = Units.Pixels(400);
            Layout.Height = Units.Pixels(300);
        }
    }

    /// <summary>
    /// Splitter - Divides space between two views with a draggable divider.
    /// </summary>
    public class Splitter : Panel
    {
        private readonly IView _first;
        private readonly IView _second;
        private readonly bool _isVertical;

        public float SplitRatio { get; set; } = 0.5f;
        public float DividerThickness { get; set; } = 4f;
        public Color? DividerColor { get; set; }

        public Splitter(IView first, IView second, bool isVertical = false)
        {
            _first = first;
            _second = second;
            _isVertical = isVertical;

            Layout.LayoutType = isVertical ? LayoutType.Column : LayoutType.Row;

            // Configure first panel
            _first.Layout.Width = isVertical ? Units.Stretch(1) : Units.Percentage(SplitRatio * 100);
            _first.Layout.Height = isVertical ? Units.Percentage(SplitRatio * 100) : Units.Stretch(1);

            // Configure second panel
            _second.Layout.Width = isVertical ? Units.Stretch(1) : Units.Percentage((1 - SplitRatio) * 100);
            _second.Layout.Height = isVertical ? Units.Percentage((1 - SplitRatio) * 100) : Units.Stretch(1);

            AddChild(_first);
            // TODO: Add divider view
            AddChild(_second);
        }
    }
}