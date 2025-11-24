using Ignis.Engine.Reactive;
using Ignis.Engine.UI.Abstractions;
using Ignis.Engine.UI.Elements;
using Ignis.Engine.UI.Input;
using Ignis.Engine.UI.Widgets;
using Microsoft.Xna.Framework;

namespace Ignis.Tests.UI;

/// <summary>
/// Tests for Task 3: Advanced Widget Interactivity & Visual States
/// </summary>
public class InteractiveWidgetsTest
{
    [Fact]
    public void WidgetState_ShouldTrackHover_WhenElementIsHovered()
    {
        // Arrange
        var mockInput = new MockInputProvider();
        var mockContext = new MockUIContext(mockInput);
        var button = Elements.Button("Test", () => { }) as ViewComponent;
        
        Assert.NotNull(button);
        button.Mount(mockContext);
        
        // Initially not hovered
        Assert.False(button.CurrentState.HasFlag(WidgetState.Hovered));
        
        // Act - Simulate hover by setting the hovered element ID
        mockContext.Input.HoveredElementId.Value = button.Layout.ElementId;
        
        // Assert - Button should show hovered state
        Assert.True(button.CurrentState.HasFlag(WidgetState.Hovered));
    }

    [Fact]
    public void WidgetState_ShouldTrackActive_WhenMousePressed()
    {
        // Arrange
        var mockInput = new MockInputProvider();
        var mockContext = new MockUIContext(mockInput);
        var button = Elements.Button("Test", () => { }) as ViewComponent;
        
        Assert.NotNull(button);
        button.Mount(mockContext);
        
        // Initially not active
        Assert.False(button.CurrentState.HasFlag(WidgetState.Active));
        
        // Act - Simulate mouse press
        mockContext.Input.ActiveElementId.Value = button.Layout.ElementId;
        
        // Assert - Button should show active state
        Assert.True(button.CurrentState.HasFlag(WidgetState.Active));
    }

    [Fact]
    public void WidgetState_ShouldTrackFocus_WhenElementIsFocused()
    {
        // Arrange
        var text = new Signal<string?>("");
        var mockInput = new MockInputProvider();
        var mockContext = new MockUIContext(mockInput);
        var textField = new TextField(text);
        
        textField.Mount(mockContext);
        
        // Initially not focused
        Assert.False(textField.CurrentState.HasFlag(WidgetState.Focused));
        
        // Act - Simulate focus
        mockContext.Input.FocusedElementId.Value = textField.Layout.ElementId;
        
        // Assert - TextField should show focused state
        Assert.True(textField.CurrentState.HasFlag(WidgetState.Focused));
    }

    [Fact]
    public void Slider_ShouldHavePointerEventHandlers_AfterMount()
    {
        // Arrange
        var value = new Signal<float>(0.5f);
        var slider = new Slider(value, 0f, 1f);
        var mockInput = new MockInputProvider();
        var mockContext = new MockUIContext(mockInput);
        
        // Act
        slider.Mount(mockContext);
        
        // Assert - Slider should have pointer down and move handlers for dragging
        Assert.NotNull(slider.EventHandlers.OnPointerDown);
        Assert.NotNull(slider.EventHandlers.OnPointerMove);
    }

    [Fact]
    public void TextField_ShouldBeFocusable()
    {
        // Arrange
        var text = new Signal<string?>("test");
        var textField = new TextField(text);
        
        // Assert - TextField is focusable
        Assert.True(textField.Layout.Focusable);
    }

    [Fact]
    public void NumberField_ShouldBeFocusable()
    {
        // Arrange
        var value = new Signal<int>(50);
        var numberField = new NumberField<int>(
            value,
            x => x + 10,
            x => x - 10
        );
        
        // Assert - NumberField is focusable
        Assert.True(numberField.Layout.Focusable);
    }
    
    [Fact]
    public void TextField_ShouldHaveTextInputHandler_AfterMount()
    {
        // Arrange
        var text = new Signal<string?>("");
        var textField = new TextField(text);
        var mockInput = new MockInputProvider();
        var mockContext = new MockUIContext(mockInput);
        
        // Act
        textField.Mount(mockContext);
        
        // Assert - EventHandlers should have OnTextInput registered
        Assert.NotNull(textField.EventHandlers.OnTextInput);
    }
}

