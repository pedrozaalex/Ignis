using CrucibleUI.Types;
using CrucibleUI.Widgets;

namespace CrucibleUI.Tests.Widgets;

/// <summary>
/// Tests for input state handling.
/// </summary>
public class InputStateTests
{
    [Fact]
    public void Widget_InitialState_NotHoveredOrPressed()
    {
        var panel = new Panel();

        Assert.False(panel.IsHovered);
        Assert.False(panel.IsPressed);
        Assert.False(panel.IsFocused);
    }

    [Fact]
    public void Widget_SetHovered_UpdatesState()
    {
        var panel = new Panel();

        panel.SetHovered(true);

        Assert.True(panel.IsHovered);
    }

    [Fact]
    public void Widget_SetPressed_UpdatesState()
    {
        var button = new Button("Click");

        button.SetPressed(true);

        Assert.True(button.IsPressed);
    }

    [Fact]
    public void Widget_SetFocus_UpdatesState()
    {
        var slider = new Slider(0, 100, 50);

        slider.SetFocused(true);

        Assert.True(slider.IsFocused);
    }

    [Fact]
    public void Button_Click_FiresOnlyWhenReleasedInside()
    {
        var clickCount = 0;
        var button = new Button("Click")
            .Width(Units.Pixels(100))
            .Height(Units.Pixels(40))
            .OnClick(() => clickCount++);

        button.ComputeBounds(0, 0, 100, 40);

        // Press inside
        button.HandleMouseDown(50, 20);
        Assert.True(button.IsPressed);
        Assert.Equal(0, clickCount); // Not clicked yet

        // Release inside - should fire
        button.HandleMouseUp(50, 20);
        Assert.False(button.IsPressed);
        Assert.Equal(1, clickCount);
    }

    [Fact]
    public void Button_PressAndReleaseOutside_DoesNotClick()
    {
        var clickCount = 0;
        var button = new Button("Click")
            .Width(Units.Pixels(100))
            .Height(Units.Pixels(40))
            .OnClick(() => clickCount++);

        button.ComputeBounds(0, 0, 100, 40);

        // Press inside
        button.HandleMouseDown(50, 20);
        Assert.True(button.IsPressed);

        // Release outside - should NOT fire
        button.HandleMouseUp(200, 20);
        Assert.False(button.IsPressed);
        Assert.Equal(0, clickCount);
    }

    [Fact]
    public void Button_Disabled_IgnoresInput()
    {
        var clickCount = 0;
        var button = new Button("Click")
            .Width(Units.Pixels(100))
            .Height(Units.Pixels(40))
            .OnClick(() => clickCount++)
            .Disabled(true);

        button.ComputeBounds(0, 0, 100, 40);

        button.HandleMouseDown(50, 20);
        Assert.False(button.IsPressed);

        button.HandleMouseUp(50, 20);
        Assert.Equal(0, clickCount);
    }

    [Fact]
    public void Slider_Drag_UpdatesValue()
    {
        float lastValue = 50;
        var slider = new Slider(0, 100, 50)
            .Width(Units.Pixels(200))
            .Height(Units.Pixels(20))
            .OnValueChanged(v => lastValue = v);

        slider.ComputeBounds(0, 0, 200, 20);

        // Start drag at value position
        slider.HandleMouseDown(100, 10); // Middle = 50%
        Assert.True(slider.IsDragging);

        // Drag to 75% position
        slider.HandleMouseMove(150, 10);
        Assert.Equal(75f, lastValue);

        // Release
        slider.HandleMouseUp(150, 10);
        Assert.False(slider.IsDragging);
        Assert.Equal(75f, slider.Value);
    }
}
