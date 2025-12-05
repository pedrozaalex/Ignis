using CrucibleUI.Widgets;

namespace CrucibleUI.Tests.Widgets;

/// <summary>
/// Tests for styling capabilities.
/// </summary>
public class StyleTests
{
    [Fact]
    public void Widget_BackgroundColor_SetsColor()
    {
        var panel = new Panel()
            .Background(0.2f, 0.2f, 0.3f, 1.0f);

        Assert.Equal(0.2f, panel.BackgroundColor.R);
        Assert.Equal(0.2f, panel.BackgroundColor.G);
        Assert.Equal(0.3f, panel.BackgroundColor.B);
        Assert.Equal(1.0f, panel.BackgroundColor.A);
    }

    [Fact]
    public void Widget_BorderColor_SetsColor()
    {
        var panel = new Panel()
            .BorderColor(1.0f, 0.5f, 0.0f, 1.0f);

        Assert.Equal(1.0f, panel.BorderColorValue.R);
        Assert.Equal(0.5f, panel.BorderColorValue.G);
        Assert.Equal(0.0f, panel.BorderColorValue.B);
    }

    [Fact]
    public void Label_TextColor_SetsColor()
    {
        var label = new Label("Hello")
            .Color(1.0f, 1.0f, 1.0f, 1.0f);

        Assert.Equal(1.0f, label.TextColor.R);
        Assert.Equal(1.0f, label.TextColor.G);
        Assert.Equal(1.0f, label.TextColor.B);
    }

    [Fact]
    public void Button_HoverStyle_SetsHoverBackground()
    {
        var button = new Button("Click")
            .Background(0.3f, 0.3f, 0.4f, 1.0f)
            .HoverBackground(0.4f, 0.4f, 0.5f, 1.0f);

        Assert.Equal(0.3f, button.BackgroundColor.R);
        Assert.Equal(0.4f, button.HoverBackgroundColor.R);
    }

    [Fact]
    public void Button_PressedStyle_SetsPressedBackground()
    {
        var button = new Button("Click")
            .PressedBackground(0.2f, 0.2f, 0.3f, 1.0f);

        Assert.Equal(0.2f, button.PressedBackgroundColor.R);
    }

    [Fact]
    public void Widget_CornerRadius_SetsRadius()
    {
        var panel = new Panel().CornerRadius(8);

        Assert.Equal(8f, panel.CornerRadiusValue);
    }
}
