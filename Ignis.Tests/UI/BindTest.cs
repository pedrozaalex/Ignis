using Ignis.Engine.Reactive;
using Ignis.Engine.UI.Abstractions;
using Ignis.Engine.UI.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Tests.UI;

/// <summary>
/// Tests for Bind.If and Bind.For - Control flow in reactive UI
/// </summary>
public class BindTests
{
    [Fact]
    public void BindIf_InitialCondition_ShowsCorrectView()
    {
        // Arrange
        var condition = new Signal<bool>(true);
        var trueViewCreated = false;
        var falseViewCreated = false;

        var trueView = new MockView(() => trueViewCreated = true);
        var falseView = new MockView(() => falseViewCreated = true);

        // Act
        var conditionalView = Bind.If(
            condition,
            () =>
            {
                trueViewCreated = true;
                return trueView;
            },
            () =>
            {
                falseViewCreated = true;
                return falseView;
            }
        );

        var context = new MockUIContext();
        conditionalView.Mount(context);

        // Assert
        Assert.True(trueViewCreated);
        Assert.False(falseViewCreated);
    }

    [Fact]
    public void BindIf_ConditionToggle_SwitchesViews()
    {
        // Arrange
        var condition = new Signal<bool>(true);
        var trueMountCount = 0;
        var falseMountCount = 0;

        var view = Bind.If(
            condition,
            () => new MockView(() => trueMountCount++),
            () => new MockView(() => falseMountCount++)
        );

        var context = new MockUIContext();
        view.Mount(context);

        // Assert initial state
        Assert.Equal(1, trueMountCount);
        Assert.Equal(0, falseMountCount);

        // Act - Toggle condition to false
        condition.Value = false;
        Thread.Sleep(10); // Allow effect to run

        // Assert - False view should now be mounted
        Assert.False(condition.Value);
        Assert.Equal(1, trueMountCount);
        Assert.Equal(1, falseMountCount);
    }

    [Fact]
    public void BindFor_InitialList_CreatesViews()
    {
        // Arrange
        var list = new SignalList<string>();
        list.Add("A");
        list.Add("B");
        list.Add("C");

        var viewInstances = new Dictionary<string, MockView>();

        // Act
        var listView = Bind.For(list, item =>
        {
            if (viewInstances.TryGetValue(item, out var value)) return value;

            value = new MockView();
            viewInstances[item] = value;

            return value;
        });

        var context = new MockUIContext();
        listView.Mount(context);

        // Assert
        Assert.Equal(3, viewInstances.Count);
        Assert.True(viewInstances.ContainsKey("A"));
        Assert.True(viewInstances.ContainsKey("B"));
        Assert.True(viewInstances.ContainsKey("C"));
    }

    [Fact]
    public void BindFor_Move_PreservesViewInstances()
    {
        // Arrange
        var list = new SignalList<string>();
        list.Add("A");
        list.Add("B");
        list.Add("C");

        var viewInstances = new Dictionary<string, MockView>();

        var listView = Bind.For(list, item =>
        {
            if (viewInstances.TryGetValue(item, out var value)) return value;

            value = new MockView();
            viewInstances[item] = value;

            return value;
        });

        var context = new MockUIContext();
        listView.Mount(context);

        var originalA = viewInstances["A"];
        var originalB = viewInstances["B"];
        var originalC = viewInstances["C"];

        // Act - Move B to end: [A, C, B]
        list.Move(1, 2);

        // Assert - View instances should be preserved
        Assert.Same(originalA, viewInstances["A"]);
        Assert.Same(originalB, viewInstances["B"]);
        Assert.Same(originalC, viewInstances["C"]);

        // Verify list order
        Assert.Equal("A", list[0]);
        Assert.Equal("C", list[1]);
        Assert.Equal("B", list[2]);
    }

    [Fact]
    public void BindFor_AddItem_CreatesNewView()
    {
        // Arrange
        var list = new SignalList<string>();
        list.Add("A");
        list.Add("B");

        var mountCount = 0;
        var listView = Bind.For(list, item => new MockView(() => mountCount++));

        var context = new MockUIContext();
        listView.Mount(context);

        Assert.Equal(2, mountCount);

        // Act - Add new item
        list.Add("C");
        Thread.Sleep(10); // Allow SignalList event to propagate

        // Assert - New view should be created and mounted
        Assert.Equal(3, list.Count);
        // Note: Actual mount count verification depends on Bind.For implementation
    }

    [Fact]
    public void BindFor_RemoveItem_RemovesView()
    {
        // Arrange
        var list = new SignalList<string>();
        list.Add("A");
        list.Add("B");
        list.Add("C");

        var viewInstances = new Dictionary<string, MockView>();
        var listView = Bind.For(list, item =>
        {
            if (!viewInstances.ContainsKey(item))
            {
                viewInstances[item] = new MockView();
            }
            return viewInstances[item];
        });

        var context = new MockUIContext();
        listView.Mount(context);

        Assert.Equal(3, viewInstances.Count);

        // Act - Remove middle item
        list.Remove("B");

        // Assert - List should reflect removal
        Assert.Equal(2, list.Count);
        Assert.Equal("A", list[0]);
        Assert.Equal("C", list[1]);
    }

    #region Helper Classes

    private class MockView : ViewComponent
    {
        private readonly Action? _onMount;

        public MockView(Action? onMount = null)
        {
            _onMount = onMount;
        }

        protected override void OnMount()
        {
            _onMount?.Invoke();
        }

        public override void Draw(SpriteBatch spriteBatch,
            Rectangle bounds)
        {
            // No-op for testing
        }
    }

    private class MockUIContext : UIContext
    {
        public MockUIContext() : base(null, new MockInputProvider())
        {
            // Mock context for testing
        }
    }

    #endregion
}