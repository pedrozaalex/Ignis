using Xunit;
using Ignis.Engine.UI;
using Ignis.Engine.UI.Abstractions;
using Ignis.Engine.UI.Input;
using Ignis.Engine.UI.Elements;
using Ignis.Engine.Reactive;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using static Ignis.Engine.UI.Elements.Elements;

namespace Ignis.Tests.UI
{
    /// <summary>
    /// Comprehensive tests for UI interactivity system.
    /// Tests event bubbling, focus management, shortcuts, and drag-drop.
    /// </summary>
    public class InteractivityTest
    {
        [Fact]
        public void PointerDown_ShouldTriggerHandler()
        {
            // Arrange
            var clicked = false;
            var box = new Box(Color.Red);
            box.OnPointerDown(_ => clicked = true);

            var evt = new PointerEvent(Vector2.Zero, 0, PointerType.Mouse, PointerEventType.Down);

            // Act
            box.EventHandlers.InvokePointerDown(evt);

            // Assert
            Assert.True(clicked);
        }

        [Fact]
        public void PointerEvent_StopPropagation_ShouldPreventBubbling()
        {
            // Arrange
            var childHandled = false;
            var parentHandled = false;

            var child = new Box(Color.Red);
            child.OnPointerDown(evt =>
            {
                childHandled = true;
                evt.StopPropagation();
            });

            var parent = new Container(child);
            parent.OnPointerDown(_ => parentHandled = true);

            var evt = new PointerEvent(Vector2.Zero, 0, PointerType.Mouse, PointerEventType.Down);

            // Act
            child.EventHandlers.InvokePointerDown(evt);
            
            // Bubbling would happen in InputManager, but we can test the flag
            if (!evt.Handled)
            {
                parent.EventHandlers.InvokePointerDown(evt);
            }

            // Assert
            Assert.True(childHandled);
            Assert.False(parentHandled); // Should not execute due to stop propagation
            Assert.True(evt.Handled);
        }

        [Fact]
        public void Click_ShouldTriggerOnPointerUp()
        {
            // Arrange
            var clickCount = 0;
            var box = new Box(Color.Blue);
            box.OnClick(() => clickCount++);

            var evt = new PointerEvent(Vector2.Zero, 0, PointerType.Mouse, PointerEventType.Up);

            // Act
            box.EventHandlers.InvokePointerUp(evt);

            // Assert
            Assert.Equal(1, clickCount);
        }

        [Fact]
        public void Focusable_ShouldSetLayoutProperty()
        {
            // Arrange
            var box = new Box(Color.White);

            // Act
            box.Focusable();

            // Assert
            Assert.True(box.Layout.Focusable);
        }

        [Fact]
        public void ElementId_ShouldBeUnique()
        {
            // Arrange & Act
            var box1 = new Box(Color.Red);
            var box2 = new Box(Color.Blue);

            // Assert
            Assert.NotEqual(box1.Layout.ElementId, box2.Layout.ElementId);
        }

        [Fact]
        public void KeyboardEvent_ShouldTriggerHandler()
        {
            // Arrange
            var keyPressed = false;
            var box = new Box(Color.Green);
            box.OnKeyDown(evt =>
            {
                if (evt.Key == Keys.A)
                    keyPressed = true;
            });

            var evt = new KeyboardEvent(Keys.A, ModifierKeys.None, KeyboardEventType.Down);

            // Act
            box.EventHandlers.InvokeKeyDown(evt);

            // Assert
            Assert.True(keyPressed);
        }

        [Fact]
        public void Shortcuts_ShouldParseSimpleKey()
        {
            // Arrange
            var executed = false;
            var box = new Box(Color.Yellow);
            box.Shortcuts(s => s.Bind("A", () => executed = true));

            // Act
            var handled = box.Shortcuts.TryHandle(Keys.A, ModifierKeys.None);

            // Assert
            Assert.True(handled);
            Assert.True(executed);
        }

        [Fact]
        public void Shortcuts_ShouldParseCtrlCombo()
        {
            // Arrange
            var executed = false;
            var box = new Box(Color.Purple);
            box.Shortcuts(s => s.Bind("Ctrl+Z", () => executed = true));

            // Act
            var handled = box.Shortcuts.TryHandle(Keys.Z, ModifierKeys.Control);

            // Assert
            Assert.True(handled);
            Assert.True(executed);
        }

        [Fact]
        public void Shortcuts_ShouldParseMultipleModifiers()
        {
            // Arrange
            var executed = false;
            var box = new Box(Color.Orange);
            box.Shortcuts(s => s.Bind("Ctrl+Shift+S", () => executed = true));

            // Act
            var handled = box.Shortcuts.TryHandle(Keys.S, ModifierKeys.Control | ModifierKeys.Shift);

            // Assert
            Assert.True(handled);
            Assert.True(executed);
        }

        [Fact]
        public void Shortcuts_ShouldNotTriggerWithWrongModifier()
        {
            // Arrange
            var executed = false;
            var box = new Box(Color.Pink);
            box.Shortcuts(s => s.Bind("Ctrl+Z", () => executed = true));

            // Act
            var handled = box.Shortcuts.TryHandle(Keys.Z, ModifierKeys.None);

            // Assert
            Assert.False(handled);
            Assert.False(executed);
        }

        [Fact]
        public void Shortcuts_ShouldSupportMultipleBindings()
        {
            // Arrange
            var undoCount = 0;
            var redoCount = 0;
            
            var box = new Box(Color.Gray);
            box.Shortcuts(s => s
                .Bind("Ctrl+Z", () => undoCount++)
                .Bind("Ctrl+Shift+Z", () => redoCount++)
            );

            // Act
            box.Shortcuts.TryHandle(Keys.Z, ModifierKeys.Control);
            box.Shortcuts.TryHandle(Keys.Z, ModifierKeys.Control | ModifierKeys.Shift);

            // Assert
            Assert.Equal(1, undoCount);
            Assert.Equal(1, redoCount);
        }

        [Fact]
        public void DragEvent_Accept_ShouldSetFlag()
        {
            // Arrange
            var dragEvt = new DragEvent(Vector2.Zero, "payload", DragEventType.Over);

            // Act
            dragEvt.Accept();

            // Assert
            Assert.True(dragEvt.IsAccepted);
        }

        [Fact]
        public void OnDragOver_ShouldReceivePayload()
        {
            // Arrange
            object? receivedPayload = null;
            var box = new Box(Color.Cyan);
            box.OnDragOver(evt =>
            {
                receivedPayload = evt.Payload;
                evt.Accept();
            });

            var payload = new { Name = "TestAsset" };
            var dragEvt = new DragEvent(Vector2.Zero, payload, DragEventType.Over);

            // Act
            box.EventHandlers.InvokeDragOver(dragEvt);

            // Assert
            Assert.Equal(payload, receivedPayload);
            Assert.True(dragEvt.IsAccepted);
        }

        [Fact]
        public void OnDrop_ShouldTriggerWithPayload()
        {
            // Arrange
            var dropped = false;
            object? droppedPayload = null;
            
            var box = new Box(Color.Magenta);
            box.OnDrop(evt =>
            {
                dropped = true;
                droppedPayload = evt.Payload;
            });

            var payload = "TestFile.txt";
            var dropEvt = new DragEvent(Vector2.Zero, payload, DragEventType.Drop);

            // Act
            box.EventHandlers.InvokeDrop(dropEvt);

            // Assert
            Assert.True(dropped);
            Assert.Equal(payload, droppedPayload);
        }

        [Fact]
        public void Draggable_ShouldAttachPointerDownHandler()
        {
            // Arrange
            var box = new Box(Color.Lime);
            var payload = "DragPayload";
            
            box.Draggable(payload);

            // Assert
            Assert.NotNull(box.EventHandlers.OnPointerDown);
        }

        [Fact]
        public void PointerEnter_AndLeave_ShouldTrackHover()
        {
            // Arrange
            var entered = false;
            var left = false;

            var box = new Box(Color.Navy);
            box.OnPointerEnter(_ => entered = true);
            box.OnPointerLeave(_ => left = true);

            // Act
            box.EventHandlers.InvokePointerEnter(new PointerEvent(Vector2.Zero, 0, PointerType.Mouse, PointerEventType.Enter));
            box.EventHandlers.InvokePointerLeave(new PointerEvent(Vector2.Zero, 0, PointerType.Mouse, PointerEventType.Leave));

            // Assert
            Assert.True(entered);
            Assert.True(left);
        }

        [Fact]
        public void MultipleEventHandlers_ShouldAllExecute()
        {
            // Arrange
            var handler1Executed = false;
            var handler2Executed = false;

            var box = new Box(Color.Teal);
            box.OnPointerDown(_ => handler1Executed = true);
            box.OnPointerDown(_ => handler2Executed = true);

            var evt = new PointerEvent(Vector2.Zero, 0, PointerType.Mouse, PointerEventType.Down);

            // Act
            box.EventHandlers.InvokePointerDown(evt);

            // Assert
            Assert.True(handler1Executed);
            Assert.True(handler2Executed);
        }

        [Fact]
        public void ReactiveSignal_InEventHandler_ShouldUpdate()
        {
            // Arrange
            var counter = new Signal<int>(0);
            var box = new Box(Color.Olive);
            
            box.OnClick(() => counter.Value++);

            // Act
            box.EventHandlers.InvokePointerUp(new PointerEvent(Vector2.Zero, 0, PointerType.Mouse, PointerEventType.Up));
            box.EventHandlers.InvokePointerUp(new PointerEvent(Vector2.Zero, 0, PointerType.Mouse, PointerEventType.Up));

            // Assert
            Assert.Equal(2, counter.Value);
        }

        [Fact]
        public void KeyboardModifiers_ShouldCombineCorrectly()
        {
            // Arrange
            var modifiers = ModifierKeys.Control | ModifierKeys.Shift;

            // Assert
            Assert.True(modifiers.HasFlag(ModifierKeys.Control));
            Assert.True(modifiers.HasFlag(ModifierKeys.Shift));
            Assert.False(modifiers.HasFlag(ModifierKeys.Alt));
        }

        [Fact]
        public void InputManager_FocusSignal_ShouldBeReadable()
        {
            // Arrange
            var bounds = new Dictionary<long, Rectangle>();
            var inputManager = new InputManager(bounds);

            // Act
            var focusedId = inputManager.FocusedElementId.Value;

            // Assert
            Assert.Null(focusedId); // Initially no focus
        }

        [Fact]
        public void InputManager_HoverSignal_ShouldBeReadable()
        {
            // Arrange
            var bounds = new Dictionary<long, Rectangle>();
            var inputManager = new InputManager(bounds);

            // Act
            var hoveredId = inputManager.HoveredElementId.Value;

            // Assert
            Assert.Null(hoveredId); // Initially no hover
        }
    }
}

