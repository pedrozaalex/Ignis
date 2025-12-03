using Crucible.Core.Ecs;
using Crucible.Core.Types;

namespace Crucible.Core.Tests;

/// <summary>
/// Tests for stretch edge cases:
/// - Stretch children in tight containers (negative available space)
/// - Stretch dimensions with min/max constraints during content sizing
/// </summary>
public class StretchEdgeCaseTests
{
    /// <summary>
    /// Issue #2: When container has large padding that consumes all space,
    /// stretch children should still get their minimum size, not collapse to zero.
    /// </summary>
    [Fact]
    public void StretchChildInTightContainer_RespectsMinSize()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(200.0f))
            .SetHeight(Units.Pixels(200.0f))
            .SetLayoutType(LayoutType.Row)
            // Padding consumes 300px total, leaving negative available space
            .SetPaddingLeft(Units.Pixels(150.0f))
            .SetPaddingRight(Units.Pixels(150.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Pixels(100.0f))
            .SetMinWidth(Units.Pixels(50.0f));  // Should get at least 50px

        world.Layout(root);

        var bounds = world.Cache.Bounds(node);
        // Child should have MinWidth of 50, not 0
        Assert.Equal(50.0f, bounds?.Width);
    }

    /// <summary>
    /// Issue #2: Multiple stretch children in tight container should all
    /// get their minimum sizes when there's no positive space to distribute.
    /// </summary>
    [Fact]
    public void MultipleStretchChildrenInTightContainer_AllGetMinSize()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(200.0f))
            .SetLayoutType(LayoutType.Row);

        var node1 = world.Add(root);
        node1.SetWidth(Units.Pixels(80.0f))  // Fixed child takes 80px
            .SetHeight(Units.Pixels(100.0f));

        var node2 = world.Add(root);
        node2.SetWidth(Units.Stretch(1.0f))  // Only 20px left, but min is 30
            .SetHeight(Units.Pixels(100.0f))
            .SetMinWidth(Units.Pixels(30.0f));

        var node3 = world.Add(root);
        node3.SetWidth(Units.Stretch(1.0f))  // No space left, but min is 25
            .SetHeight(Units.Pixels(100.0f))
            .SetMinWidth(Units.Pixels(25.0f));

        world.Layout(root);

        // Both stretch children should get their minimum widths
        Assert.Equal(30.0f, world.Cache.Bounds(node2)?.Width);
        Assert.Equal(25.0f, world.Cache.Bounds(node3)?.Width);
    }

    /// <summary>
    /// Issue #3: Stretch width with MaxWidth constraint should be respected
    /// when computing content size.
    /// </summary>
    [Fact]
    public void StretchWithMaxWidth_ContentSizingRespectsMax()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetLayoutType(LayoutType.Column);

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Auto)
            .SetMaxWidth(Units.Pixels(200.0f))  // Should cap width at 200
            // Content sizing returns aspect ratio 1:1 based on width
            .SetContentSize(world.SubLayout, (width, _) => (width ?? 0, width ?? 0));

        world.Layout(root);

        var bounds = world.Cache.Bounds(node);
        // Width should be clamped to MaxWidth of 200, not full parent width of 600
        Assert.Equal(200.0f, bounds?.Width);
        // Height from content sizing should also be 200 (aspect ratio 1:1)
        Assert.Equal(200.0f, bounds?.Height);
    }

    /// <summary>
    /// Issue #3: Stretch height with MaxHeight constraint should be respected
    /// when computing content size.
    /// </summary>
    [Fact]
    public void StretchWithMaxHeight_ContentSizingRespectsMax()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Auto)
            .SetHeight(Units.Stretch(1.0f))
            .SetMaxHeight(Units.Pixels(150.0f))  // Should cap height at 150
            // Content sizing returns aspect ratio 1:1 based on height
            .SetContentSize(world.SubLayout, (_, height) => (height ?? 0, height ?? 0));

        world.Layout(root);

        var bounds = world.Cache.Bounds(node);
        // Height should be clamped to MaxHeight of 150, not full parent height of 600
        Assert.Equal(150.0f, bounds?.Height);
        // Width from content sizing should also be 150 (aspect ratio 1:1)
        Assert.Equal(150.0f, bounds?.Width);
    }

    /// <summary>
    /// Issue #3: Stretch with MinWidth should be respected in content sizing.
    /// </summary>
    [Fact]
    public void StretchWithMinWidth_ContentSizingRespectsMin()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(100.0f))  // Small parent
            .SetHeight(Units.Pixels(600.0f))
            .SetLayoutType(LayoutType.Column);

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Auto)
            .SetMinWidth(Units.Pixels(200.0f))  // Min is larger than parent
            // Content sizing returns aspect ratio 1:1 based on width
            .SetContentSize(world.SubLayout, (width, _) => (width ?? 0, width ?? 0));

        world.Layout(root);

        var bounds = world.Cache.Bounds(node);
        // Width should be at least MinWidth of 200, even though parent is only 100
        Assert.Equal(200.0f, bounds?.Width);
        // Height from content sizing should also be 200 (aspect ratio 1:1)
        Assert.Equal(200.0f, bounds?.Height);
    }
}
