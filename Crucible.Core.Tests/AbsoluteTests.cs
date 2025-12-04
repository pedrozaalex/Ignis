using Crucible.Core.Tests.Ecs;
using Crucible.Core.Types;

namespace Crucible.Core.Tests;

/// <summary>
/// Tests for absolute positioned elements.
/// Ported from morphorm tests/absolute.rs
/// </summary>
public class AbsoluteTests
{
    [Fact]
    public void AbsolutePixelsWidthPixelsHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(100.0f))
            .SetPositionType(PositionType.Absolute);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 100.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 100.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void AbsolutePixelsWidthPercentageHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Percentage(25.0f))
            .SetPositionType(PositionType.Absolute);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void AbsolutePixelsWidthStretchHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Stretch(1.0f))
            .SetPositionType(PositionType.Absolute);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 600.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 600.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void AbsolutePixelsWidthAutoHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Auto)
            .SetPositionType(PositionType.Absolute);

        var child = world.Add(node);
        child.SetWidth(Units.Pixels(50.0f))
             .SetHeight(Units.Pixels(50.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 50.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 50.0f), world.Cache.Bounds(child));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 50.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 50.0f), world.Cache.Bounds(child));
    }

    [Fact]
    public void AbsolutePercentageWidthPixelsHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Percentage(50.0f))
            .SetHeight(Units.Pixels(100.0f))
            .SetPositionType(PositionType.Absolute);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 100.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 100.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void AbsoluteStretchWidthPixelsHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Pixels(100.0f))
            .SetPositionType(PositionType.Absolute);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 600.0f, 100.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 600.0f, 100.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void AbsoluteAutoWidthPixelsHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Auto)
            .SetHeight(Units.Pixels(100.0f))
            .SetPositionType(PositionType.Absolute);

        var child = world.Add(node);
        child.SetWidth(Units.Pixels(50.0f))
             .SetHeight(Units.Pixels(50.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 100.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 50.0f), world.Cache.Bounds(child));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 100.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 50.0f), world.Cache.Bounds(child));
    }

    [Fact]
    public void AbsoluteAutoWidthAutoHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Auto)
            .SetHeight(Units.Auto)
            .SetPositionType(PositionType.Absolute);

        var child = world.Add(node);
        child.SetWidth(Units.Pixels(50.0f))
             .SetHeight(Units.Pixels(50.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 50.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 50.0f), world.Cache.Bounds(child));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 50.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 50.0f), world.Cache.Bounds(child));
    }

    [Fact]
    public void AbsoluteStretchWidthStretchHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Stretch(1.0f))
            .SetPositionType(PositionType.Absolute);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 600.0f, 600.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 600.0f, 600.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void AbsolutePercentageWidthPercentageHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Percentage(50.0f))
            .SetHeight(Units.Percentage(25.0f))
            .SetPositionType(PositionType.Absolute);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 150.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 150.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void AutoParentPixelsChildStretchAbsoluteChild()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Auto)
            .SetHeight(Units.Auto);

        var child1 = world.Add(node);
        child1.SetWidth(Units.Pixels(50.0f))
              .SetHeight(Units.Pixels(50.0f));

        var child2 = world.Add(node);
        child2.SetWidth(Units.Stretch(1.0f))
              .SetHeight(Units.Stretch(1.0f))
              .SetPositionType(PositionType.Absolute);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 50.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 50.0f), world.Cache.Bounds(child1));
        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 50.0f), world.Cache.Bounds(child2));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 50.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 50.0f), world.Cache.Bounds(child1));
        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 50.0f), world.Cache.Bounds(child2));
    }

    [Fact]
    public void AutoParentPixelsChildPercentageAbsoluteChild()
    {
        using var world = new TestWorld();

        var root = world.Add()
            .SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f));

        var node = world.Add(root)
            .SetWidth(Units.Auto)
            .SetHeight(Units.Auto);

        var child1 = world.Add(node)
            .SetWidth(Units.Pixels(50.0f))
            .SetHeight(Units.Pixels(50.0f));

        var child2 = world.Add(node)
            .SetWidth(Units.Percentage(50.0f))
            .SetHeight(Units.Percentage(25.0f))
            .SetPositionType(PositionType.Absolute);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 50.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 50.0f), world.Cache.Bounds(child1));
        Assert.Equal(new Rect(0.0f, 0.0f, 25.0f, 12.5f), world.Cache.Bounds(child2));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 50.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 50.0f), world.Cache.Bounds(child1));
        Assert.Equal(new Rect(0.0f, 0.0f, 25.0f, 12.5f), world.Cache.Bounds(child2));
    }
}
