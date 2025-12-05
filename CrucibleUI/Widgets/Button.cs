using CrucibleUI.Types;

namespace CrucibleUI.Widgets;

/// <summary>
/// A clickable button widget.
/// </summary>
public class Button : Widget
{
    public string Text { get; set; }
    public float FontSizeValue { get; private set; } = 14f;
    public WidgetColor TextColor { get; private set; } = WidgetColor.White;
    public WidgetColor HoverBackgroundColor { get; private set; } = new(0.4f, 0.4f, 0.5f, 1f);
    public WidgetColor PressedBackgroundColor { get; private set; } = new(0.2f, 0.2f, 0.3f, 1f);

    public Action? ClickHandler { get; private set; }

    public Button(string text)
    {
        Text = text;
        BackgroundColor = new WidgetColor(0.3f, 0.3f, 0.4f, 1f);
    }

    public Button FontSize(float size)
    {
        FontSizeValue = size;
        return this;
    }

    public Button Color(float r, float g, float b, float a = 1f)
    {
        TextColor = new WidgetColor(r, g, b, a);
        return this;
    }

    public Button HoverBackground(float r, float g, float b, float a = 1f)
    {
        HoverBackgroundColor = new WidgetColor(r, g, b, a);
        return this;
    }

    public Button PressedBackground(float r, float g, float b, float a = 1f)
    {
        PressedBackgroundColor = new WidgetColor(r, g, b, a);
        return this;
    }

    public Button OnClick(Action handler)
    {
        ClickHandler = handler;
        return this;
    }

    // Fluent builders with concrete return type
    public Button Width(Units value) => Width<Button>(value);
    public Button Height(Units value) => Height<Button>(value);
    public Button Padding(Units value) => Padding<Button>(value);
    public Button Stretch() => Stretch<Button>();
    public Button Background(float r, float g, float b, float a = 1f) => Background<Button>(r, g, b, a);
    public Button BorderColor(float r, float g, float b, float a = 1f) => BorderColor<Button>(r, g, b, a);
    public Button CornerRadius(float radius) => CornerRadius<Button>(radius);
    public Button Visible(bool visible) => Visible<Button>(visible);
    public Button Disabled(bool disabled) => Disabled<Button>(disabled);

    public override void HandleMouseDown(float x, float y)
    {
        if (IsDisabled) return;
        SetPressed(true);
    }

    public override void HandleMouseUp(float x, float y)
    {
        if (IsDisabled) return;

        var wasPressed = IsPressed;
        SetPressed(false);

        // Fire click only if released inside the button
        if (wasPressed && IsPointInside(x, y))
        {
            ClickHandler?.Invoke();
        }
    }

    private bool IsPointInside(float x, float y)
    {
        return x >= ComputedX && x <= ComputedX + ComputedWidth &&
               y >= ComputedY && y <= ComputedY + ComputedHeight;
    }

    /// <summary>
    /// Returns the current background color based on input state.
    /// </summary>
    public WidgetColor GetCurrentBackground()
    {
        if (IsPressed) return PressedBackgroundColor;
        if (IsHovered) return HoverBackgroundColor;
        return BackgroundColor;
    }

    public override (float Width, float Height)? GetContentSize(float? parentWidth, float? parentHeight)
    {
        if (Label.TextMeasurer != null && !string.IsNullOrEmpty(Text))
        {
            var (w, h) = Label.TextMeasurer(Text, FontSizeValue);
            // Add some padding for button
            return (w + 20, h + 10);
        }
        return (Text.Length * FontSizeValue * 0.6f + 20, FontSizeValue + 10);
    }
}
