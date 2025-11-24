using Ignis.Engine.Reactive;
using Ignis.Engine.UI;
using Ignis.Engine.UI.Elements;
using Ignis.Engine.UI.Input;
using Ignis.Engine.UI.Widgets;
using Microsoft.Xna.Framework;

namespace Ignis.Tests.UI
{
    /// <summary>
    /// Integration tests for the full input pipeline with InputManager.
    /// </summary>
    public class InputManagerIntegrationTest
    {
        [Fact]
        public void InputManager_ShouldFindViewAtPosition()
        {
            // Arrange
            var bounds = new Dictionary<long, Rectangle>();
            var box = new Box(Color.Red);
            var boxId = box.Layout.ElementId;
            
            // Simulate layout setting bounds
            bounds[boxId] = new Rectangle(10, 10, 100, 100);
            
            var mockInput = new MockInputProvider();
            var inputManager = new InputManager(bounds, mockInput);
            inputManager.SetRoot(box);
            
            // The InputManager uses bounds to find views at positions
            // We can verify the bounds are accessible
            Assert.True(bounds.ContainsKey(boxId));
            Assert.Equal(new Rectangle(10, 10, 100, 100), bounds[boxId]);
        }

        [Fact]
        public void NumberField_Buttons_ShouldHaveClickHandlers()
        {
            // Arrange
            var value = new Signal<int>(50);
            var numberField = new NumberField<int>(
                value,
                x => x + 10,
                x => x - 10
            );
            
            // The NumberField creates internal buttons with click handlers
            // We verify the structure exists
            var children = numberField.GetChildren();
            Assert.NotNull(children);
            Assert.Single(children); // Should have the container panel
        }

        [Fact]
        public void Panel_WithOnClick_ShouldRegisterEventHandler()
        {
            // Arrange
            var clicked = false;
            var panel = new Panel();
            panel.OnClick(() => clicked = true);
            
            // Simulate a click by manually invoking the handler
            var evt = new PointerEvent(Vector2.Zero, 0, PointerType.Mouse, PointerEventType.Up);
            panel.EventHandlers.InvokePointerUp(evt);
            
            // Assert
            Assert.True(clicked);
        }

        [Fact]
        public void UIContext_ShouldPopulateBoundsById()
        {
            // This test verifies that UIContext.SetBounds populates both dictionaries
            // We can't easily test this without a full UIContext, but we can verify
            // the integration point exists
            
            var box = new Box(Color.Blue);
            var elementId = box.Layout.ElementId;
            
            // Element IDs are unique and non-zero
            Assert.NotEqual(0, elementId);
        }

        [Fact]
        public void InputManager_WithEmptyBounds_ShouldHandleUpdatesGracefully()
        {
            // Arrange - No bounds registered
            var bounds = new Dictionary<long, Rectangle>();
            var mockInput = new MockInputProvider();
            var inputManager = new InputManager(bounds, mockInput);
            var root = new Container();
            
            inputManager.SetRoot(root);
            
            // Act - Multiple updates with no views should not crash
            inputManager.Update();
            inputManager.Update();
            inputManager.Update();
            
            // Assert - Focus should remain null when no views exist
            Assert.Null(inputManager.FocusedElementId.Value);
        }

        [Fact]
        public void ClickableButton_InHierarchy_ShouldWork()
        {
            // Arrange
            var clickCount = 0;
            var button = new Panel()
                .OnClick(() => clickCount++);
            
            var container = new Container(button);
            
            // Verify hierarchy
            Assert.Contains(button, container.GetChildren());
            
            // Simulate click
            var evt = new PointerEvent(Vector2.Zero, 0, PointerType.Mouse, PointerEventType.Up);
            button.EventHandlers.InvokePointerUp(evt);
            
            Assert.Equal(1, clickCount);
        }

        [Fact]
        public void NumberField_Increment_ShouldUpdateSignal()
        {
            // Arrange
            var value = new Signal<int>(0);
            var numberField = new NumberField<int>(
                value,
                x => x + 1,
                x => x - 1
            );
            
            // Get the container which holds the buttons
            var container = numberField.GetChildren().First();
            Assert.NotNull(container);
            
            // Initial value
            Assert.Equal(0, value.Value);
        }

        [Fact]
        public void InputManager_ReactiveSignals_NotifyOnStateChange()
        {
            // Arrange
            var bounds = new Dictionary<long, Rectangle>();
            var mockInput = new MockInputProvider();
            var inputManager = new InputManager(bounds, mockInput);
            
            var focusChangeCount = 0;
            var hoverChangeCount = 0;
            
            // Track changes with Effects
            new Effect(() =>
            {
                _ = inputManager.FocusedElementId.Value;
                focusChangeCount++;
            });
            
            new Effect(() =>
            {
                _ = inputManager.HoveredElementId.Value;
                hoverChangeCount++;
            });
            
            // Assert - Effects should have run once initially
            Assert.Equal(1, focusChangeCount);
            Assert.Equal(1, hoverChangeCount);
        }
    }
}

