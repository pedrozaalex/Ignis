using CrucibleUI.Types;

namespace CrucibleUI.Widgets;

/// <summary>
/// A text label widget.
/// </summary>
public class Label : Widget
{
    public string Text { get; set; }
    public float FontSizeValue { get; private set; } = 14f;
    public WidgetColor TextColor { get; private set; } = WidgetColor.White;

    /// <summary>
    /// Optional delegate for measuring text dimensions.
    /// Set this to integrate with your rendering system's text measurement.
    /// </summary>
    public static Func<string, float, (float Width, float Height)>? TextMeasurer { get; set; }

    public Label(string text)
    {
        Text = text;
    }

    public Label FontSize(float size)
    {
        FontSizeValue = size;
        return this;
    }

    public Label Color(float r, float g, float b, float a = 1f)
    {
        TextColor = new WidgetColor(r, g, b, a);
        return this;
    }

    // Fluent builders with concrete return type
    public Label Width(Units value) => Width<Label>(value);
    public Label Height(Units value) => Height<Label>(value);
    public Label Padding(Units value) => Padding<Label>(value);
    public Label Stretch() => Stretch<Label>();
    public Label Visible(bool visible) => Visible<Label>(visible);

    public override (float Width, float Height)? GetContentSize(float? parentWidth, float? parentHeight)
    {
        if (TextMeasurer != null && !string.IsNullOrEmpty(Text))
        {
            return TextMeasurer(Text, FontSizeValue);
        }
        // Fallback: estimate based on character count
        return (Text.Length * FontSizeValue * 0.6f, FontSizeValue);
    }
}
