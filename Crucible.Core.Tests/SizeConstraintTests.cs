using Crucible.Core.Ecs;
using Crucible.Core.Types;

namespace Crucible.Core.Tests;

/// <summary>
/// Tests for combined size constraints (min/max width and height).
/// Ported from morphorm tests/size_constraints.rs
/// </summary>
public class SizeConstraintTests
{
    [Fact]
    public void MinWidthPixelsMinHeightPixels()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(100.0f))
            .SetMinWidth(Units.Pixels(200.0f))
            .SetMinHeight(Units.Pixels(200.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 200.0f, 200.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void MaxWidthPixelsMaxHeightPixels()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(400.0f))
            .SetHeight(Units.Pixels(400.0f))
            .SetMaxWidth(Units.Pixels(200.0f))
            .SetMaxHeight(Units.Pixels(200.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 200.0f, 200.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void MinWidthPixelsMaxHeightPixels()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(400.0f))
            .SetMinWidth(Units.Pixels(200.0f))
            .SetMaxHeight(Units.Pixels(200.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 200.0f, 200.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void MaxWidthPixelsMinHeightPixels()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(400.0f))
            .SetHeight(Units.Pixels(100.0f))
            .SetMaxWidth(Units.Pixels(200.0f))
            .SetMinHeight(Units.Pixels(200.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 200.0f, 200.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void MinWidthPercentageMinHeightPercentage()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(100.0f))
            .SetMinWidth(Units.Percentage(50.0f))
            .SetMinHeight(Units.Percentage(50.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 300.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void MaxWidthPercentageMaxHeightPercentage()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(400.0f))
            .SetHeight(Units.Pixels(400.0f))
            .SetMaxWidth(Units.Percentage(50.0f))
            .SetMaxHeight(Units.Percentage(50.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 300.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void MinWidthPercentageMaxHeightPercentage()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(400.0f))
            .SetMinWidth(Units.Percentage(50.0f))
            .SetMaxHeight(Units.Percentage(50.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 300.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void MaxWidthPercentageMinHeightPercentage()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(400.0f))
            .SetHeight(Units.Pixels(100.0f))
            .SetMaxWidth(Units.Percentage(50.0f))
            .SetMinHeight(Units.Percentage(50.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 300.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void MinWidthPercentageWidthAuto()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Auto)
            .SetHeight(Units.Auto)
            .SetMinWidth(Units.Percentage(100.0f));

        var node2 = world.Add(node);
        node2.SetWidth(Units.Pixels(200.0f))
             .SetHeight(Units.Pixels(100.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 600.0f, 100.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 200.0f, 100.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void MinWidthAuto()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetPadding(Units.Pixels(200.0f))
            .SetAlignment(Alignment.Center);

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Stretch(1.0f))
            .SetMinWidth(Units.Auto);

        var node2 = world.Add(node);
        node2.SetWidth(Units.Pixels(300.0f))
             .SetHeight(Units.Pixels(300.0f));

        world.Layout(root);

        Assert.Equal(new Rect(150.0f, 200.0f, 300.0f, 200.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 300.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void MinHeightAuto()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetPadding(Units.Pixels(200.0f))
            .SetAlignment(Alignment.Center);

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Stretch(1.0f))
            .SetMinHeight(Units.Auto);

        var node2 = world.Add(node);
        node2.SetWidth(Units.Pixels(300.0f))
             .SetHeight(Units.Pixels(300.0f));

        world.Layout(root);

        Assert.Equal(new Rect(200.0f, 150.0f, 200.0f, 300.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 300.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void MinSizeAuto()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetPadding(Units.Pixels(200.0f))
            .SetAlignment(Alignment.Center);

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Stretch(1.0f))
            .SetMinWidth(Units.Auto)
            .SetMinHeight(Units.Auto);

        var node2 = world.Add(node);
        node2.SetWidth(Units.Pixels(300.0f))
             .SetHeight(Units.Pixels(300.0f));

        world.Layout(root);

        Assert.Equal(new Rect(150.0f, 150.0f, 300.0f, 300.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 300.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void MinWidthAutoAbsolute()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetPadding(Units.Pixels(200.0f))
            .SetAlignment(Alignment.Center);

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Stretch(1.0f))
            .SetMinWidth(Units.Auto)
            .SetPositionType(PositionType.Absolute);

        var node2 = world.Add(node);
        node2.SetWidth(Units.Pixels(300.0f))
             .SetHeight(Units.Pixels(300.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 200.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 300.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void MinHeightAutoAbsolute()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetPadding(Units.Pixels(200.0f))
            .SetAlignment(Alignment.Center);

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Stretch(1.0f))
            .SetMinHeight(Units.Auto)
            .SetPositionType(PositionType.Absolute);

        var node2 = world.Add(node);
        node2.SetWidth(Units.Pixels(300.0f))
             .SetHeight(Units.Pixels(300.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 200.0f, 300.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 300.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void MinSizeAutoAbsolute()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetPadding(Units.Pixels(200.0f))
            .SetAlignment(Alignment.Center);

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Stretch(1.0f))
            .SetMinWidth(Units.Auto)
            .SetMinHeight(Units.Auto)
            .SetPositionType(PositionType.Absolute);

        var node2 = world.Add(node);
        node2.SetWidth(Units.Pixels(300.0f))
             .SetHeight(Units.Pixels(300.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 300.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 300.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void MinWidthAutoChildAbsolute()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetPadding(Units.Pixels(200.0f))
            .SetAlignment(Alignment.Center);

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Stretch(1.0f))
            .SetMinWidth(Units.Auto);

        var node2 = world.Add(node);
        node2.SetWidth(Units.Pixels(300.0f))
             .SetHeight(Units.Pixels(300.0f))
             .SetPositionType(PositionType.Absolute);

        world.Layout(root);

        Assert.Equal(new Rect(200.0f, 200.0f, 200.0f, 200.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 300.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void MinHeightAutoChildAbsolute()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetPadding(Units.Pixels(200.0f))
            .SetAlignment(Alignment.Center);

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Stretch(1.0f))
            .SetMinHeight(Units.Auto);

        var node2 = world.Add(node);
        node2.SetWidth(Units.Pixels(300.0f))
             .SetHeight(Units.Pixels(300.0f))
             .SetPositionType(PositionType.Absolute);

        world.Layout(root);

        Assert.Equal(new Rect(200.0f, 200.0f, 200.0f, 200.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 300.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void MinSizeAutoChildAbsolute()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetPadding(Units.Pixels(200.0f))
            .SetAlignment(Alignment.Center);

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Stretch(1.0f))
            .SetMinWidth(Units.Auto)
            .SetMinHeight(Units.Auto);

        var node2 = world.Add(node);
        node2.SetWidth(Units.Pixels(300.0f))
             .SetHeight(Units.Pixels(300.0f))
             .SetPositionType(PositionType.Absolute);

        world.Layout(root);

        Assert.Equal(new Rect(200.0f, 200.0f, 200.0f, 200.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 300.0f), world.Cache.Bounds(node2));
    }
}
