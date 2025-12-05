using CrucibleUI.Types;
using CrucibleUI.Widgets;

namespace CrucibleUI.Tests.Widgets;

/// <summary>
/// Tests for core widget functionality.
/// </summary>
public class WidgetTests
{
    [Fact]
    public void Panel_CreateWithSize_HasCorrectDimensions()
    {
        var panel = new Panel()
            .Width(Units.Pixels(200))
            .Height(Units.Pixels(100));

        Assert.Equal(Units.Pixels(200), panel.WidthValue);
        Assert.Equal(Units.Pixels(100), panel.HeightValue);
    }

    [Fact]
    public void Panel_WithPadding_SetsAllSides()
    {
        var panel = new Panel().Padding(Units.Pixels(10));

        Assert.Equal(Units.Pixels(10), panel.PaddingLeftValue);
        Assert.Equal(Units.Pixels(10), panel.PaddingRightValue);
        Assert.Equal(Units.Pixels(10), panel.PaddingTopValue);
        Assert.Equal(Units.Pixels(10), panel.PaddingBottomValue);
    }

    [Fact]
    public void Panel_WithChildren_ContainsAllChildren()
    {
        var label1 = new Label("First");
        var label2 = new Label("Second");

        var panel = new Panel().Children(label1, label2);

        Assert.Equal(2, panel.ChildWidgets.Count);
        Assert.Same(label1, panel.ChildWidgets[0]);
        Assert.Same(label2, panel.ChildWidgets[1]);
    }

    [Fact]
    public void Label_CreateWithText_StoresText()
    {
        var label = new Label("Hello World");

        Assert.Equal("Hello World", label.Text);
    }

    [Fact]
    public void Label_WithFontSize_SetsFontSize()
    {
        var label = new Label("Hello").FontSize(24);

        Assert.Equal(24f, label.FontSizeValue);
    }

    [Fact]
    public void Button_CreateWithText_StoresText()
    {
        var button = new Button("Click Me");

        Assert.Equal("Click Me", button.Text);
    }

    [Fact]
    public void Button_WithClickHandler_StoresHandler()
    {
        var clicked = false;
        var button = new Button("Click").OnClick(() => clicked = true);

        // Simulate click
        button.ClickHandler?.Invoke();

        Assert.True(clicked);
    }

    [Fact]
    public void Slider_CreateWithRange_StoresRange()
    {
        var slider = new Slider(0, 100, 50);

        Assert.Equal(0f, slider.Min);
        Assert.Equal(100f, slider.Max);
        Assert.Equal(50f, slider.Value);
    }

    [Fact]
    public void Slider_ValueChanged_InvokesHandler()
    {
        float newValue = 0;
        var slider = new Slider(0, 100, 50)
            .OnValueChanged(v => newValue = v);

        slider.SetValue(75);

        Assert.Equal(75f, newValue);
    }

    [Fact]
    public void Slider_ValueClamped_WithinRange()
    {
        var slider = new Slider(0, 100, 50);

        slider.SetValue(150);
        Assert.Equal(100f, slider.Value);

        slider.SetValue(-50);
        Assert.Equal(0f, slider.Value);
    }

    [Fact]
    public void Widget_Row_SetsLayoutType()
    {
        var panel = new Panel().Row();

        Assert.Equal(LayoutType.Row, panel.LayoutTypeValue);
    }

    [Fact]
    public void Widget_Column_SetsLayoutType()
    {
        var panel = new Panel().Column();

        Assert.Equal(LayoutType.Column, panel.LayoutTypeValue);
    }

    [Fact]
    public void Widget_Stretch_SetsStretchUnits()
    {
        var panel = new Panel().Stretch();

        Assert.Equal(Units.Stretch(1), panel.WidthValue);
        Assert.Equal(Units.Stretch(1), panel.HeightValue);
    }
}
