using FontStashSharp;
using Ignis.Engine.Reactive;
using Ignis.Engine.UI.Abstractions;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Engine.UI.Widgets
{
    /// <summary>
    /// Label - Styled text display with optional icon.
    /// </summary>
    public class Label : ViewComponent, IViewContainer
    {
        private readonly IView _container;

        public Label(string text, SpriteFontBase? font = null, Color? color = null)
        {
            var textView = new Text(font) 
            { 
                Content = text, 
                Color = color ?? Color.White 
            };

            _container = new Panel(textView)
            {
                BackgroundColor = Color.Transparent
            };
            _container.Layout.PaddingLeft = Units.Pixels(4);
            _container.Layout.PaddingRight = Units.Pixels(4);
            _container.Layout.PaddingTop = Units.Pixels(2);
            _container.Layout.PaddingBottom = Units.Pixels(2);
        }

        public Label(Signal<string> text, SpriteFontBase? font = null, Color? color = null)
        {
            var textView = new ReactiveText(text, font)
            {
                Color = color ?? Color.White
            };

            _container = new Panel(textView)
            {
                BackgroundColor = Color.Transparent
            };
            _container.Layout.PaddingLeft = Units.Pixels(4);
            _container.Layout.PaddingRight = Units.Pixels(4);
        }

        public Label(Computed<string> text, SpriteFontBase? font = null, Color? color = null)
        {
            var textView = new ReactiveText(text, font)
            {
                Color = color ?? Color.White
            };

            _container = new Panel(textView)
            {
                BackgroundColor = Color.Transparent
            };
            _container.Layout.PaddingLeft = Units.Pixels(4);
            _container.Layout.PaddingRight = Units.Pixels(4);
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
    /// ProgressBar - Visual progress indicator.
    /// </summary>
    public class ProgressBar : ViewComponent
    {
        private readonly Signal<float> _progress;

        public Color? BackgroundColor { get; set; }
        public Color? FillColor { get; set; }
        public Color? BorderColor { get; set; }

        public ProgressBar(Signal<float> progress)
        {
            _progress = progress;

            Layout.Width = Units.Pixels(200);
            Layout.Height = Units.Pixels(20);
        }

        protected override void OnMount()
        {
            CreateEffect(() =>
            {
                var val = _progress.Value;
                // Clamp between 0 and 1
                if (val < 0f) _progress.Value = 0f;
                if (val > 1f) _progress.Value = 1f;
            });
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Context?.PrimitiveBatch == null) return;

            var batch = Context.PrimitiveBatch;
            
            var bg = BackgroundColor ?? Context.Theme.BackgroundColor;
            var fill = FillColor ?? Context.Theme.PrimaryColor;
            var border = BorderColor ?? Context.Theme.BorderColor;
            
            // Draw background
            batch.DrawFilledRectangle(bounds, bg);

            // Draw fill (based on progress)
            var progress = Math.Clamp(_progress.Value, 0f, 1f);
            if (progress > 0)
            {
                var fillWidth = (int)(bounds.Width * progress);
                var fillBounds = new Rectangle(bounds.X, bounds.Y, fillWidth, bounds.Height);
                batch.DrawFilledRectangle(fillBounds, fill);
            }

            // Draw border
            batch.DrawBorder(bounds, 1f, border);
        }
    }

    /// <summary>
    /// Separator - Visual divider line.
    /// </summary>
    public class Separator : ViewComponent
    {
        public Color? Color { get; set; }
        public float Thickness { get; set; } = 1f;
        public bool IsVertical { get; set; }

        public Separator(bool isVertical = false)
        {
            IsVertical = isVertical;

            if (isVertical)
            {
                Layout.Width = Units.Pixels(Thickness);
                Layout.Height = Units.Stretch(1);
            }
            else
            {
                Layout.Width = Units.Stretch(1);
                Layout.Height = Units.Pixels(Thickness);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Context?.PrimitiveBatch == null) return;
            var color = Color ?? Context.Theme.BorderColor;
            Context.PrimitiveBatch.DrawFilledRectangle(bounds, color);
        }
    }

    /// <summary>
    /// Icon - Display a textured icon or glyph.
    /// </summary>
    public class Icon : ViewComponent
    {
        private readonly Texture2D? _texture;
        private readonly Rectangle? _sourceRect;

        public Color Tint { get; set; } = Color.White;

        public Icon(Texture2D? texture, int size = 16, Rectangle? sourceRect = null)
        {
            _texture = texture;
            _sourceRect = sourceRect;

            Layout.Width = Units.Pixels(size);
            Layout.Height = Units.Pixels(size);
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (_texture != null)
            {
                spriteBatch.Draw(
                    _texture,
                    bounds,
                    _sourceRect,
                    Tint
                );
            }
        }
    }

    /// <summary>
    /// Tooltip - Popup help text.
    /// </summary>
    public class Tooltip : ViewComponent, IViewContainer
    {
        private readonly IView _content;
        private readonly Signal<bool> _isVisible;
        private readonly Color? _backgroundColor;

        public Tooltip(string text, Signal<bool> isVisible, SpriteFontBase? font = null, Color? backgroundColor = null)
        {
            _isVisible = isVisible;
            _backgroundColor = backgroundColor;

            var textView = new Text(font) { Content = text, Color = Color.White };

            _content = new Panel(textView)
            {
                BorderThickness = 1f
            };
            _content.Layout.PaddingLeft = Units.Pixels(8);
            _content.Layout.PaddingRight = Units.Pixels(8);
            _content.Layout.PaddingTop = Units.Pixels(4);
            _content.Layout.PaddingBottom = Units.Pixels(4);
            _content.Layout.PositionType = PositionType.Absolute;
        }

        protected override void OnMount()
        {
            _content.Mount(Context!);
            
            // Set background color from theme after mounting when Context is available
            if (_content is Panel panel)
            {
                panel.BackgroundColor = _backgroundColor ?? Context!.Theme.TooltipBackground;
            }

            CreateEffect(() =>
            {
                _content.Layout.Visible = _isVisible.Value;
            });
        }

        protected override void OnUnmount()
        {
            _content.Unmount();
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
        }

        public IEnumerable<IView> GetChildren()
        {
            if (_isVisible.Value)
                yield return _content;
        }
    }

    /// <summary>
    /// Badge - Small notification indicator.
    /// </summary>
    public class Badge : ViewComponent, IViewContainer
    {
        private readonly IView _container;

        public Badge(string text, SpriteFontBase? font = null, Color? backgroundColor = null)
        {
            var textView = new Text(font) { Content = text, Color = Color.White };

            _container = new Panel(textView)
            {
                BackgroundColor = backgroundColor,
                CornerRadius = 10f
            };
            _container.Layout.PaddingLeft = Units.Pixels(6);
            _container.Layout.PaddingRight = Units.Pixels(6);
            _container.Layout.PaddingTop = Units.Pixels(2);
            _container.Layout.PaddingBottom = Units.Pixels(2);
            _container.Layout.Height = Units.Pixels(20);

            Layout.Height = Units.Pixels(20);
        }

        public Badge(Signal<int> count, SpriteFontBase? font = null, Color? backgroundColor = null)
        {
            var textView = new ReactiveText(
                Computed<string>.From(() => count.Value.ToString()),
                font
            )
            {
                Color = Color.White
            };

            _container = new Panel(textView)
            {
                BackgroundColor = backgroundColor,
                CornerRadius = 10f
            };
            _container.Layout.PaddingLeft = Units.Pixels(6);
            _container.Layout.PaddingRight = Units.Pixels(6);
            _container.Layout.PaddingTop = Units.Pixels(2);
            _container.Layout.PaddingBottom = Units.Pixels(2);
            _container.Layout.Height = Units.Pixels(20);

            Layout.Height = Units.Pixels(20);
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
}

