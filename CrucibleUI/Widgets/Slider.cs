using CrucibleUI.Types;

namespace CrucibleUI.Widgets;

/// <summary>
/// A slider widget for numeric value selection.
/// </summary>
public class Slider : Widget
{
    public float Min { get; }
    public float Max { get; }
    public float Value { get; private set; }
    public bool IsDragging { get; private set; }

    public WidgetColor TrackColor { get; private set; } = new(0.2f, 0.2f, 0.3f, 1f);
    public WidgetColor FillColor { get; private set; } = new(0.4f, 0.6f, 1f, 1f);
    public WidgetColor ThumbColor { get; private set; } = WidgetColor.White;

    private Action<float>? _valueChangedHandler;

    public Slider(float min, float max, float initialValue)
    {
        Min = min;
        Max = max;
        Value = Math.Clamp(initialValue, min, max);
        HeightValue = Units.Pixels(20);
        IsFocusable = true;
    }

    public void SetValue(float value)
    {
        var clamped = Math.Clamp(value, Min, Max);
        if (Math.Abs(clamped - Value) > float.Epsilon)
        {
            Value = clamped;
            _valueChangedHandler?.Invoke(Value);
        }
    }

    public Slider OnValueChanged(Action<float> handler)
    {
        _valueChangedHandler = handler;
        return this;
    }

    public Slider Track(float r, float g, float b, float a = 1f)
    {
        TrackColor = new WidgetColor(r, g, b, a);
        return this;
    }

    public Slider Fill(float r, float g, float b, float a = 1f)
    {
        FillColor = new WidgetColor(r, g, b, a);
        return this;
    }

    public Slider Thumb(float r, float g, float b, float a = 1f)
    {
        ThumbColor = new WidgetColor(r, g, b, a);
        return this;
    }

    // Fluent builders with concrete return type
    public Slider Width(Units value) => Width<Slider>(value);
    public Slider Height(Units value) => Height<Slider>(value);
    public Slider Stretch() => Stretch<Slider>();
    public Slider Visible(bool visible) => Visible<Slider>(visible);
    public Slider Disabled(bool disabled) => Disabled<Slider>(disabled);

    public override void HandleMouseDown(float x, float y)
    {
        if (IsDisabled) return;

        IsDragging = true;
        SetFocused(true);
        UpdateValueFromPosition(x);
    }

    public override void HandleMouseUp(float x, float y)
    {
        IsDragging = false;
    }

    public override void HandleMouseMove(float x, float y)
    {
        if (IsDragging && !IsDisabled)
        {
            UpdateValueFromPosition(x);
        }
    }

    private void UpdateValueFromPosition(float x)
    {
        if (ComputedWidth <= 0) return;

        var relativeX = x - ComputedX;
        var fraction = Math.Clamp(relativeX / ComputedWidth, 0f, 1f);
        SetValue(Min + fraction * (Max - Min));
    }

    /// <summary>
    /// Returns the normalized value (0-1) for rendering.
    /// </summary>
    public float NormalizedValue => (Max - Min) > 0 ? (Value - Min) / (Max - Min) : 0;
}
