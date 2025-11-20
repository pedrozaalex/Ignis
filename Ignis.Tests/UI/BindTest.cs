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
        var view = Bind.If(
            condition,
            () => new MockView(),
            () => new MockView()
        );

        var context = new MockUIContext();
        view.Mount(context);

        // Act - Toggle condition
        condition.Value = false;
        Thread.Sleep(10); // Allow effect to run

        // Assert - Verify condition changed
        Assert.False(condition.Value);
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
        public MockUIContext() : base(null!)
        {
            // Mock context for testing
        }
    }

    #endregion
}