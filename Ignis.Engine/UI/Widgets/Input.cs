using FontStashSharp;
using Ignis.Engine.Reactive;
using Ignis.Engine.UI.Abstractions;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Ignis.Engine.UI.Elements.Elements;

namespace Ignis.Engine.UI.Widgets
{
    /// <summary>
    /// TextField - Single-line text input.
    /// </summary>
    public class TextField : ViewComponent, IViewContainer
    {
        private readonly Signal<string?> _text;
        private readonly Text _textView;
        private readonly Panel _root;
        private string? _placeholder;

        public string Placeholder
        {
            get => _placeholder ?? "";
            set => _placeholder = value;
        }

        public Color? BackgroundColor
        {
            set => _root.BackgroundColor = value;
        }

        public TextField(Signal<string?> text, SpriteFontBase? font = null)
        {
            _text = text;
            _textView = new ReactiveText(Computed<string>.From(() =>
            {
                if (string.IsNullOrEmpty(_text.Value) && !string.IsNullOrEmpty(Placeholder))
                    return Placeholder;

                return _text.Value ?? "";
            }), font);

            // Internal panel for visuals (Background, Border, Padding)
            _root = new Panel(_textView)
            {
                BorderThickness = 1f,
                Layout =
                {
                    // Fill the wrapper component
                    Width = Units.Stretch(1),
                    Height = Units.Auto,

                    // Padding for the text inside
                    PaddingLeft = Units.Pixels(8),
                    PaddingRight = Units.Pixels(8),
                    PaddingTop = Units.Pixels(6),
                    PaddingBottom = Units.Pixels(6)
                }
            };

            // Default Height and make focusable
            Layout.Height = Units.Auto;
            Layout.Focusable = true;
        }

        protected override void OnMount()
        {
            _root.Mount(Context!);
            
            // Set default background to InputBackground from theme if not explicitly set
            if (_root.BackgroundColor == null)
            {
                _root.BackgroundColor = Context!.Theme.InputBackground;
            }

            // Update text view when signal changes
            CreateEffect(() => { _textView.Content = _text.Value ?? ""; });
            
            // Handle text input
            this.OnTextInput(character =>
            {
                if (char.IsControl(character))
                    return;
                    
                _text.Value = (_text.Value ?? "") + character;
            });
            
            // Handle backspace
            this.OnKeyDown(evt =>
            {
                if (evt.Key == Microsoft.Xna.Framework.Input.Keys.Back && !string.IsNullOrEmpty(_text.Value))
                {
                    _text.Value = _text.Value[..^1];
                }
            });
            
            // Update border color when focused
            CreateEffect(() =>
            {
                if (CurrentState.HasFlag(WidgetState.Focused))
                {
                    _root.BorderColor = Context!.Theme.InputFocusBorderColor;
                }
                else
                {
                    _root.BorderColor = Context!.Theme.BorderColor;
                }
            });
        }

        protected override void OnUnmount()
        {
            _root.Unmount();
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Drawing delegated to _background via UIContext
        }

        public IEnumerable<IView> GetChildren()
        {
            yield return _root;
        }
    }

    /// <summary>
    /// NumberField - Numeric input with increment/decrement buttons.
    /// </summary>
    public class NumberField<T> : ViewComponent, IViewContainer where T : struct
    {
        private readonly Signal<T> _value;
        private readonly IView _container;

        public NumberField(Signal<T> value, Func<T, T> increment, Func<T, T> decrement, SpriteFontBase? font = null)
        {
            _value = value;

            var valueText = new ReactiveText(
                Computed<string>.From(() => _value.Value.ToString() ?? "0"),
                font
            )
            {
                Layout =
                {
                    Width = Units.Pixels(60)
                }
            };

            _container = new Panel(
                Button("-", () => _value.Value = decrement(_value.Value)),
                valueText,
                // Row(
                Button("+", () => _value.Value = increment(_value.Value))
                // ).Gap(2)
            )
            {
                Layout =
                {
                    LayoutType = LayoutType.Row,
                    Alignment = Alignment.Center,
                    Height = Units.Auto,
                }
            };
        }

        private static IView Button(string label, Action onClick)
        {
            return new Panel(new Text { Content = label, Color = Color.White })
                .Width(Units.Pixels(22))
                .Height(Units.Pixels(22))
                .AlignCenter()
                .PaddingTop(4)
                .PaddingLeft(8)
                .OnClick(onClick);
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
    public class Checkbox : ViewComponent, IViewContainer
    {
        private readonly Signal<bool> _isChecked;
        private readonly IView _container;

        public Checkbox(string label, Signal<bool> isChecked, SpriteFontBase? font = null)
        {
            _isChecked = isChecked;

            // Visual box component
            var checkBoxVisual = new CheckboxBox(isChecked)
                .Width(18)
                .Height(18);

            var labelView = new Text(font)
            {
                Content = label, Color = Color.White,
                Layout =
                {
                    PaddingLeft = Units.Pixels(8)
                }
            };

            // Using Row helper
            _container = Row(checkBoxVisual, labelView);

            Layout.Height = Units.Pixels(24);
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
    /// Slider - Horizontal value slider.
    /// </summary>
    public class Slider : ViewComponent
    {
        private readonly Signal<float> _value;
        private readonly float _min;
        private readonly float _max;

        public Color TrackColor { get; set; } = new(63, 63, 70);
        public Color ThumbColor { get; set; } = new(0, 122, 204);
        public Color FillColor { get; set; } = new(0, 122, 204);

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
            
            // Handle mouse down (start drag)
            this.OnPointerDown(evt =>
            {
                evt.StopPropagation();
                UpdateValueFromMouse(evt.Position);
            });
            
            // Handle dragging
            this.OnPointerMove(evt =>
            {
                // Only update if this element is active (being pressed)
                if (Context?.Input.ActiveElementId.Value == Layout.ElementId)
                {
                    UpdateValueFromMouse(evt.Position);
                }
            });
        }
        
        private void UpdateValueFromMouse(Vector2 mousePos)
        {
            if (Context == null) return;
            
            var bounds = Context.GetBounds(this);
            // Calculate normalized position (0.0 to 1.0)
            float t = (mousePos.X - bounds.X) / bounds.Width;
            
            // Map to Min/Max range
            float newValue = _min + Math.Clamp(t, 0f, 1f) * (_max - _min);
            _value.Value = newValue;
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Context == null) return;

            var batch = Context.PrimitiveBatch;
            if (batch == null) return;

            // Calculate track position (centered vertically)
            const int trackHeight = 4;
            const int thumbSize = 16;

            var trackY = bounds.Y + (bounds.Height - trackHeight) / 2;
            var trackBounds = new Rectangle(bounds.X, trackY, bounds.Width, trackHeight);

            // Resolve colors from theme
            var trackColor = TrackColor != default ? TrackColor : Context.Theme.BorderColor;
            var fillColor = FillColor != default ? FillColor : Context.Theme.PrimaryColor;
            
            // Draw track background
            batch.DrawFilledRectangle(trackBounds, trackColor);

            // Draw fill (from start to thumb)
            var normalizedValue = (_value.Value - _min) / (_max - _min);
            var thumbX = bounds.X + (int)(bounds.Width * normalizedValue);
            var fillWidth = thumbX - bounds.X;
            if (fillWidth > 0)
            {
                var fillBounds = new Rectangle(bounds.X, trackY, fillWidth, trackHeight);
                batch.DrawFilledRectangle(fillBounds, fillColor);
            }

            // Resolve thumb color based on state
            Color thumbColor;
            if (CurrentState.HasFlag(WidgetState.Active))
                thumbColor = Context.Theme.PrimaryColor;
            else if (CurrentState.HasFlag(WidgetState.Hovered))
                thumbColor = Context.Theme.SliderThumbHoverColor;
            else
                thumbColor = ThumbColor != default ? ThumbColor : Context.Theme.SliderThumbColor;
            
            // Draw thumb
            var thumbBounds = new Rectangle(
                thumbX - thumbSize / 2,
                bounds.Y + (bounds.Height - thumbSize) / 2,
                thumbSize,
                thumbSize
            );
            batch.DrawCircle(thumbBounds.Center.ToVector2(), (float)thumbSize / 2, thumbColor);
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
        private readonly Signal<bool> _isOpen = new(false);

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

        public Color BackgroundColor { get; set; } = new(51, 51, 55);
        public Color BorderColor { get; set; } = new(63, 63, 70);
        public Color CheckColor { get; set; } = new(0, 122, 204);

        public CheckboxBox(Signal<bool> isChecked)
        {
            _isChecked = isChecked;
        }

        protected override void OnMount()
        {
            CreateEffect(() =>
            {
                var v = _isChecked.Value;
            });
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Context == null) return;

            var batch = Context.PrimitiveBatch;
            if (batch == null) return;

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