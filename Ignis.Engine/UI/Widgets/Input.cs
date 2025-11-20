using Ignis.Engine.Reactive;
using Ignis.Engine.UI.Abstractions;
using Ignis.Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Engine.UI.Widgets
{
    /// <summary>
    /// TextField - Single-line text input.
    /// </summary>
    public class TextField : ViewComponent
    {
        private readonly Signal<string?> _text;
        private readonly Text _textView;
        private readonly Panel _background;
        private string? _placeholder;

        public string Placeholder
        {
            get => _placeholder ?? "";
            set => _placeholder = value;
        }

        public Color BackgroundColor
        {
            get => _background.BackgroundColor;
            set => _background.BackgroundColor = value;
        }

        public TextField(Signal<string?> text, SpriteFont? font = null)
        {
            _text = text;
            _textView = new Text(font);
            _background = new Panel(_textView)
            {
                BackgroundColor = new Color(51, 51, 55),
                BorderColor = new Color(63, 63, 70),
                BorderThickness = 1f
            };
            _background.Layout.PaddingLeft = Units.Pixels(8);
            _background.Layout.PaddingRight = Units.Pixels(8);
            _background.Layout.PaddingTop = Units.Pixels(6);
            _background.Layout.PaddingBottom = Units.Pixels(6);
            _background.Layout.Height = Units.Pixels(30);

            Layout.Width = Units.Pixels(200);
            Layout.Height = Units.Pixels(30);
        }

        protected override void OnMount()
        {
            _background.Mount(Context!);

            // Update text view when signal changes
            CreateEffect(() =>
            {
                _textView.Content = _text.Value ?? "";
            });
        }

        protected override void OnUnmount()
        {
            _background.Unmount();
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Background draws itself and children
        }
    }

    /// <summary>
    /// NumberField - Numeric input with increment/decrement buttons.
    /// </summary>
    public class NumberField<T> : ViewComponent, Core.IViewContainer where T : struct
    {
        private readonly Signal<T> _value;
        private readonly IView _container;
        private readonly Func<T, T> _increment;
        private readonly Func<T, T> _decrement;

        public NumberField(string label, Signal<T> value, Func<T, T> increment, Func<T, T> decrement, SpriteFont? font = null)
        {
            _value = value;
            _increment = increment;
            _decrement = decrement;

            // Build layout: [Label] [Value Display] [-] [+]
            var labelView = new Text(font) { Content = label, Color = Color.White };
            labelView.Layout.Width = Units.Pixels(80);

            var valueText = new ReactiveText(
                Computed<string>.From(() => _value.Value.ToString() ?? "0"),
                font
            );
            valueText.Layout.Width = Units.Pixels(60);

            var decrementBtn = CreateButton("-", () => _value.Value = _decrement(_value.Value));
            var incrementBtn = CreateButton("+", () => _value.Value = _increment(_value.Value));

            _container = new Panel(labelView, valueText, decrementBtn, incrementBtn)
            {
                BackgroundColor = Color.Transparent
            };
            _container.Layout.LayoutType = LayoutType.Row;
            _container.Layout.Alignment = Alignment.Left;

            Layout.Height = Units.Pixels(30);
        }

        private IView CreateButton(string label, Action onClick)
        {
            var btn = new Panel(new Text(null) { Content = label, Color = Color.White })
            {
                BackgroundColor = new Color(62, 62, 66),
                BorderColor = new Color(63, 63, 70),
                BorderThickness = 1f
            };
            btn.Layout.Width = Units.Pixels(25);
            btn.Layout.Height = Units.Pixels(25);
            btn.Layout.Alignment = Alignment.Center;
            btn.Layout.PaddingTop = Units.Pixels(4);
            btn.Layout.PaddingLeft = Units.Pixels(8);
            // TODO: Wire up onClick when input system is ready
            return btn;
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
    /// Checkbox - Boolean toggle.
    /// </summary>
    public class Checkbox : ViewComponent, Core.IViewContainer
    {
        private readonly Signal<bool> _isChecked;
        private readonly IView _container;

        public Checkbox(string label, Signal<bool> isChecked, SpriteFont? font = null)
        {
            _isChecked = isChecked;

            var box = new Panel()
            {
                BackgroundColor = new Color(51, 51, 55),
                BorderColor = new Color(63, 63, 70),
                BorderThickness = 1f
            };
            box.Layout.Width = Units.Pixels(18);
            box.Layout.Height = Units.Pixels(18);

            var labelView = new Text(font) { Content = label, Color = Color.White };
            labelView.Layout.PaddingLeft = Units.Pixels(8);

            _container = new Panel(box, labelView)
            {
                BackgroundColor = Color.Transparent
            };
            _container.Layout.LayoutType = LayoutType.Row;
            _container.Layout.Alignment = Alignment.Left;

            Layout.Height = Units.Pixels(24);
        }

        protected override void OnMount()
        {
            _container.Mount(Context!);

            // Update visual state when checked state changes
            CreateEffect(() =>
            {
                var isChecked = _isChecked.Value;
                // TODO: Update check mark visual when true
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
    /// Slider - Horizontal value slider.
    /// </summary>
    public class Slider : ViewComponent
    {
        private readonly Signal<float> _value;
        private readonly float _min;
        private readonly float _max;

        public Color TrackColor { get; set; } = new Color(63, 63, 70);
        public Color ThumbColor { get; set; } = new Color(0, 122, 204);
        public Color FillColor { get; set; } = new Color(0, 122, 204);

        public Slider(Signal<float> value, float min = 0f, float max = 1f)
        {
            _value = value;
            _min = min;
            _max = max;

            Layout.Width = Units.Pixels(200);
            Layout.Height = Units.Pixels(20);
        }

        protected override void OnMount()
        {
            CreateEffect(() =>
            {
                var val = _value.Value;
                // Clamp value
                if (val < _min) _value.Value = _min;
                if (val > _max) _value.Value = _max;
            });
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Context == null) return;

            var batch = Context.PrimitiveBatch;
            
            // Calculate track position (centered vertically)
            const int trackHeight = 4;
            const int thumbSize = 16;
            
            var trackY = bounds.Y + (bounds.Height - trackHeight) / 2;
            var trackBounds = new Rectangle(bounds.X, trackY, bounds.Width, trackHeight);

            // Draw track background
            batch.DrawFilledRectangle(trackBounds, TrackColor);

            // Draw fill (from start to thumb)
            var normalizedValue = (_value.Value - _min) / (_max - _min);
            var thumbX = bounds.X + (int)(bounds.Width * normalizedValue);
            var fillWidth = thumbX - bounds.X;
            if (fillWidth > 0)
            {
                var fillBounds = new Rectangle(bounds.X, trackY, fillWidth, trackHeight);
                batch.DrawFilledRectangle(fillBounds, FillColor);
            }

            // Draw thumb
            var thumbBounds = new Rectangle(
                thumbX - thumbSize / 2,
                bounds.Y + (bounds.Height - thumbSize) / 2,
                thumbSize,
                thumbSize
            );
            batch.DrawFilledRectangle(thumbBounds, ThumbColor);
        }
    }

    /// <summary>
    /// Dropdown - Selection from a list of options.
    /// </summary>
    public class Dropdown<T> : ViewComponent where T : notnull
    {
        private readonly Signal<T> _selected;
        private readonly List<T> _options;
        private readonly Func<T, string> _displayFunc;
        private readonly Signal<bool> _isOpen = new Signal<bool>(false);

        public Dropdown(Signal<T> selected, List<T> options, Func<T, string>? displayFunc = null)
        {
            _selected = selected;
            _options = options;
            _displayFunc = displayFunc ?? (x => x.ToString() ?? "");

            Layout.Width = Units.Pixels(200);
            Layout.Height = Units.Pixels(30);
        }

        protected override void OnMount()
        {
            CreateEffect(() =>
            {
                var current = _selected.Value;
                var isOpen = _isOpen.Value;
                // TODO: Render dropdown UI
            });
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // TODO: Draw dropdown button and popup list
        }
    }

    /// <summary>
    /// CheckboxBox - Internal widget for rendering the checkbox box with check mark.
    /// </summary>
    internal class CheckboxBox : ViewComponent
    {
        private readonly Signal<bool> _isChecked;

        public Color BackgroundColor { get; set; } = new Color(51, 51, 55);
        public Color BorderColor { get; set; } = new Color(63, 63, 70);
        public Color CheckColor { get; set; } = new Color(0, 122, 204);

        public CheckboxBox(Signal<bool> isChecked)
        {
            _isChecked = isChecked;
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Context == null) return;

            var batch = Context.PrimitiveBatch;
            
            // Draw background
            batch.DrawFilledRectangle(bounds, BackgroundColor);

            // Draw check mark (simple filled box)
            if (_isChecked.Value)
            {
                var checkBounds = new Rectangle(
                    bounds.X + 4,
                    bounds.Y + 4,
                    bounds.Width - 8,
                    bounds.Height - 8
                );
                batch.DrawFilledRectangle(checkBounds, CheckColor);
            }

            // Draw border
            batch.DrawBorder(bounds, 1f, BorderColor);
        }
    }
}

