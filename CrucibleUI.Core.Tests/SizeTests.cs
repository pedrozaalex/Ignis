using CrucibleUI.Core.Tests.Ecs;
using CrucibleUI.Core.Types;

namespace CrucibleUI.Core.Tests;

/// <summary>
/// Tests for basic size properties (width/height with various unit types).
/// Ported from morphorm tests/size.rs
/// </summary>
public class SizeTests
{
    [Fact]
    public void PixelsWidthPixelsHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(150.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PercentageWidthPixelsHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Percentage(50.0f))
            .SetHeight(Units.Pixels(150.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 150.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 150.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void StretchWidthPixelsHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Pixels(150.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 600.0f, 150.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 600.0f, 150.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PercentageWidthPercentageHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Percentage(50.0f))
            .SetHeight(Units.Percentage(25.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 150.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 150.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void StretchWidthPercentageHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Percentage(25.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 600.0f, 150.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 600.0f, 150.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void StretchWidthStretchHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Stretch(1.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 600.0f, 600.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 600.0f, 600.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void AutoWidthPixelsChild()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node1 = world.Add(root);
        node1.SetWidth(Units.Auto)
             .SetHeight(Units.Pixels(150.0f));

        var node2 = world.Add(node1);
        node2.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Pixels(100.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node1));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node1));
    }

    [Fact]
    public void AutoWidthPixelsChildAbsolute()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node1 = world.Add(root);
        node1.SetWidth(Units.Auto)
             .SetHeight(Units.Pixels(150.0f));

        var node2 = world.Add(node1);
        node2.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Pixels(100.0f))
             .SetPositionType(PositionType.Absolute);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 0.0f, 150.0f), world.Cache.Bounds(node1));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 0.0f, 150.0f), world.Cache.Bounds(node1));
    }

    [Fact]
    public void AutoWidthPixelsChildrenAbsolute()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node1 = world.Add(root);
        node1.SetWidth(Units.Auto)
             .SetHeight(Units.Pixels(150.0f));

        var node2 = world.Add(node1);
        node2.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Pixels(100.0f))
             .SetPositionType(PositionType.Absolute);

        var node3 = world.Add(node1);
        node3.SetWidth(Units.Pixels(200.0f))
             .SetHeight(Units.Pixels(100.0f))
             .SetPositionType(PositionType.Absolute);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 0.0f, 150.0f), world.Cache.Bounds(node1));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 0.0f, 150.0f), world.Cache.Bounds(node1));
    }

    [Fact]
    public void AutoWidthPixelsChildrenAbsoluteWithPixelsLeft()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node1 = world.Add(root);
        node1.SetWidth(Units.Auto)
             .SetHeight(Units.Pixels(150.0f));

        var node2 = world.Add(node1);
        node2.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Pixels(100.0f))
             .SetPositionType(PositionType.Absolute);

        var node3 = world.Add(node1);
        node3.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Pixels(100.0f))
             .SetLeft(Units.Pixels(100.0f))
             .SetPositionType(PositionType.Absolute);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 0.0f, 150.0f), world.Cache.Bounds(node1));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 0.0f, 150.0f), world.Cache.Bounds(node1));
    }

    [Fact]
    public void AutoWidthMultipleChildren()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node1 = world.Add(root);
        node1.SetWidth(Units.Auto)
             .SetHeight(Units.Pixels(150.0f))
             .SetLayoutType(LayoutType.Row);

        var node2 = world.Add(node1);
        node2.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Pixels(100.0f));

        var node3 = world.Add(node1);
        node3.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Pixels(100.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 200.0f, 150.0f), world.Cache.Bounds(node1));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 200.0f, 150.0f), world.Cache.Bounds(node1));

        node1.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node1));
    }
}
