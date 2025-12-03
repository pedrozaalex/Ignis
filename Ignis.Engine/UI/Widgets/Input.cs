using FontStashSharp;
using Ignis.Engine.Reactive;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static Ignis.Engine.UI.Elements.Elements;

namespace Ignis.Engine.UI.Widgets;



/// <summary>
/// Generic input component - handles text input with keyboard interaction and validation.
/// Similar to HTML &lt;input/&gt; element - provides visual styling, focus behavior, and input handling.
/// </summary>
public class Input : Panel
{
    private readonly Signal<string?>? _value;
    private readonly Text? _textView;
    private Func<char, bool>? _characterValidator;
    private string? _placeholder;
    private readonly bool _managesInput;

    /// <summary>
    /// Create an Input that handles keyboard input and displays text.
    /// </summary>
    public Input(Signal<string?> value, SpriteFontBase? font = null)
    {
        _value = value;
        _managesInput = true;
        
        // Create text display with placeholder support
        _textView = new ReactiveText(Computed<string>.From(() =>
        {
            if (string.IsNullOrEmpty(_value.Value) && !string.IsNullOrEmpty(Placeholder))
                return Placeholder;
            return _value.Value ?? "";
        }), font);

        // Configure as styled input
        BorderThickness = 1f;
        Layout.Width = Units.Stretch(1);
        Layout.Height = Units.Auto;
        Layout.PaddingLeft = Units.Pixels(8);
        Layout.PaddingRight = Units.Pixels(8);
        Layout.PaddingTop = Units.Pixels(6);
        Layout.PaddingBottom = Units.Pixels(6);
        Layout.Focusable = true;
        
        AddChild(_textView);
    }

    /// <summary>
    /// Create an Input as a visual wrapper only (for complex components that manage their own content).
    /// </summary>
    public Input()
    {
        _managesInput = false;
        
        // Configure as styled input container
        BorderThickness = 1f;
        Layout.Width = Units.Stretch(1);
        Layout.Height = Units.Auto;
        Layout.PaddingLeft = Units.Pixels(8);
        Layout.PaddingRight = Units.Pixels(8);
        Layout.PaddingTop = Units.Pixels(6);
        Layout.PaddingBottom = Units.Pixels(6);
        Layout.Focusable = true;
    }

    public string Placeholder
    {
        get => _placeholder ?? "";
        set => _placeholder = value;
    }

    /// <summary>
    /// Optional character validator - return true to accept the character, false to reject.
    /// Use this to create specialized inputs (e.g., numbers only).
    /// </summary>
    public Func<char, bool>? CharacterValidator
    {
        get => _characterValidator;
        set => _characterValidator = value;
    }

    protected override void OnMount()
    {
        base.OnMount();

        // Set default styling from theme
        if (BackgroundColor == null)
            BackgroundColor = Context!.Theme.InputBackground;

        // Only handle keyboard input if this Input manages its own value
        if (_managesInput && _value != null && _textView != null)
        {
            // Update text view when signal changes
            CreateEffect(() => { _textView.Content = _value.Value ?? ""; });

            // Handle text input with optional character validation
            this.OnTextInput(character =>
            {
                if (char.IsControl(character))
                    return;

                // Apply character validator if provided
                if (_characterValidator != null && !_characterValidator(character))
                    return;

                _value.Value = (_value.Value ?? "") + character;
            });

            // Handle backspace
            this.OnKeyDown(evt =>
            {
                if (evt.Key == Keys.Back && !string.IsNullOrEmpty(_value.Value))
                    _value.Value = _value.Value[..^1];
            });
        }

        // Update visual state on focus/hover
        CreateEffect(() =>
        {
            var state = CurrentState;

            if (state.HasFlag(WidgetState.Focused))
            {
                BorderColor = Context!.Theme.BorderFocus;
                BackgroundColor = Context!.Theme.InputBackground;
            }
            else if (state.HasFlag(WidgetState.Hovered))
            {
                BorderColor = Color.Lerp(Context!.Theme.Border, Context!.Theme.Primary, 0.5f);
                BackgroundColor = Color.Lerp(Context!.Theme.InputBackground, Color.White, 0.05f);
            }
            else
            {
                BorderColor = Context!.Theme.Border;
                BackgroundColor = Context!.Theme.InputBackground;
            }
        });
    }
}

/// <summary>
/// Specialized input validators for common input types.
/// </summary>
public static class InputValidators
{
    /// <summary>
    /// Allows only numeric characters (digits, minus sign, decimal point).
    /// </summary>
    public static bool Number(char c) => char.IsDigit(c) || c == '-' || c == '.';

    /// <summary>
    /// Allows only integer characters (digits and minus sign).
    /// </summary>
    public static bool Integer(char c) => char.IsDigit(c) || c == '-';

    /// <summary>
    /// Allows only alphanumeric characters.
    /// </summary>
    public static bool Alphanumeric(char c) => char.IsLetterOrDigit(c);

    /// <summary>
    /// Allows all characters (no filtering).
    /// </summary>
    public static bool All(char c) => true;
}



/// <summary>
///     TextField - Single-line text input.
///     Simple alias for Input component.
/// </summary>
public class TextField : Input
{
    public TextField(Signal<string?> text, SpriteFontBase? font = null) : base(text, font)
    {
    }
}

/// <summary>
///     NumberInput - Numeric text input with validation (no buttons).
///     Accepts only numeric characters (digits, minus sign, decimal point).
/// </summary>
public class NumberInput : Input
{
    public NumberInput(Signal<string?> value, SpriteFontBase? font = null) : base(value, font)
    {
        CharacterValidator = InputValidators.Number;
    }
}

/// <summary>
///     NumberField - Numeric input with increment/decrement buttons.
/// </summary>
public class NumberField<T> : ViewComponent, IViewContainer where T : struct
{
    private readonly Input _input;
    private readonly Func<T, T> _decrement;
    private readonly Signal<string> _editText;
    private readonly Func<T, T> _increment;
    private readonly Signal<T> _value;
    private readonly ReactiveText _valueText;

    public NumberField(Signal<T> value, Func<T, T> increment, Func<T, T> decrement, SpriteFontBase? font = null)
    {
        _value = value;
        _increment = increment;
        _decrement = decrement;
        _editText = new Signal<string>(value.Value.ToString() ?? "0");

        _valueText = new ReactiveText(
            Computed<string>.From(() => _editText.Value),
            font
        )
        {
            Layout =
            {
                Width = Units.Pixels(60)
            }
        };

        // Use generic Input component
        _input = new Input();
        
        var contentRow = new Container(
            Button("-", () => _value.Value = decrement(_value.Value)),
            _valueText,
            Button("+", () => _value.Value = increment(_value.Value))
        )
        {
            Layout =
            {
                LayoutType = LayoutType.Row,
                Alignment = Alignment.Center
            }
        };
        
        _input.AddChild(contentRow);

        Layout.Focusable = true;
    }

    public IEnumerable<IView> GetChildren()
    {
        yield return _input;
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
        _input.Mount(Context!);

        // Sync edit text with value
        CreateEffect(() => { _editText.Value = _value.Value.ToString() ?? "0"; });

        // Handle text input for numbers
        this.OnTextInput(character =>
        {
            if (char.IsDigit(character) || character == '-' || character == '.') _editText.Value += character;
        });

        // Handle backspace
        this.OnKeyDown(evt =>
        {
            if (evt.Key == Keys.Back && _editText.Value.Length > 0)
                _editText.Value = _editText.Value[..^1];
            else if (evt.Key == Keys.Enter)
                TryUpdateValue();
            else if (evt.Key == Keys.Up)
                _value.Value = _increment(_value.Value);
            else if (evt.Key == Keys.Down) _value.Value = _decrement(_value.Value);
        });
    }

    private void TryUpdateValue()
    {
        if (typeof(T) == typeof(int))
        {
            if (int.TryParse(_editText.Value, out var intValue)) _value.Value = (T)(object)intValue;
        }
        else if (typeof(T) == typeof(float))
        {
            if (float.TryParse(_editText.Value, out var floatValue)) _value.Value = (T)(object)floatValue;
        }
        else if (typeof(T) == typeof(double))
        {
            if (double.TryParse(_editText.Value, out var doubleValue)) _value.Value = (T)(object)doubleValue;
        }
    }

    protected override void OnUnmount()
    {
        _input.Unmount();
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
    {
        // Drawing delegated to Input component
    }
}

/// <summary>
///     Checkbox - Boolean toggle.
/// </summary>
public class Checkbox : ViewComponent, IViewContainer
{
    private readonly IView _container;
    private readonly Signal<bool> _isChecked;

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

        // Using Row helper - store as Panel so we can call OnClick
        var row = Row(checkBoxVisual, labelView);

        if (row is ViewComponent component)
            component.OnClick(() => _isChecked.Value = !_isChecked.Value); // Allow clicking text too
        else
            throw new InvalidOperationException("Row did not return a ViewComponent");

        _container = row;

        Layout.Height = Units.Pixels(24);
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
///     Slider - Horizontal value slider.
/// </summary>
public class Slider : ViewComponent
{
    private readonly float _max;
    private readonly float _min;
    private readonly Signal<float> _value;

    public Slider(Signal<float> value, float min = 0f, float max = 1f)
    {
        _value = value;
        _min = min;
        _max = max;

        Layout.Width = Units.Pixels(200);
        Layout.Height = Units.Pixels(20);
    }

    public Color TrackColor { get; set; } = new(63, 63, 70);
    public Color ThumbColor { get; set; } = new(0, 122, 204);
    public Color FillColor { get; set; } = new(0, 122, 204);

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
            if (Context?.Input.ActiveElementId.Value == Layout.ElementId) UpdateValueFromMouse(evt.Position);
        });
    }

    private void UpdateValueFromMouse(Vector2 mousePos)
    {
        if (Context == null) return;

        var bounds = Context.GetBounds(this);
        // Calculate normalized position (0.0 to 1.0)
        var t = (mousePos.X - bounds.X) / bounds.Width;

        // Map to Min/Max range
        var newValue = _min + Math.Clamp(t, 0f, 1f) * (_max - _min);
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
            var trackColor = TrackColor != default ? TrackColor : Context.Theme.Border;
            var fillColor = FillColor != default ? FillColor : Context.Theme.Primary;

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

            // Resolve thumb color based on state using semantic colors
            Color thumbColor;
            if (CurrentState.HasFlag(WidgetState.Active))
                thumbColor = Context.Theme.Primary;
            else if (CurrentState.HasFlag(WidgetState.Hovered))
                thumbColor = Context.Theme.OnSurface; // Brighter on hover
            else
                thumbColor = ThumbColor != default ? ThumbColor : Context.Theme.TextMuted; // Default to muted text color

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
///     Dropdown - Selection from a list of options.
/// </summary>
public class Dropdown<T> : ViewComponent where T : notnull
{
    private readonly Func<T, string> _displayFunc;
    private readonly Signal<bool> _isOpen = new(false);
    private readonly List<T> _options;
    private readonly Signal<T> _selected;

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
///     CheckboxBox - Internal widget for rendering the checkbox box with check mark.
/// </summary>
internal class CheckboxBox : ViewComponent
{
    private readonly Signal<bool> _isChecked;

    public CheckboxBox(Signal<bool> isChecked)
    {
        _isChecked = isChecked;
    }

    public Color BackgroundColor { get; set; } = new(51, 51, 55);
    public Color BorderColor { get; set; } = new(63, 63, 70);
    public Color CheckColor { get; set; } = new(0, 122, 204);

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

        var state = CurrentState;
        var borderColor = state.HasFlag(WidgetState.Hovered)
            ? Color.Lerp(BorderColor, Color.White, 0.3f)
            : BorderColor;

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
        batch.DrawBorder(bounds, 1f, borderColor);
    }
}