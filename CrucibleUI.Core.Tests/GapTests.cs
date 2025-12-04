using CrucibleUI.Core.Tests.Ecs;
using CrucibleUI.Core.Types;

namespace CrucibleUI.Core.Tests;

/// <summary>
/// Tests for gap properties (horizontal/vertical gap).
/// Ported from morphorm tests/gap.rs
/// </summary>
public class GapTests
{
    [Fact]
    public void PixelsHorizontalGap()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetHorizontalGap(Units.Pixels(20.0f))
            .SetLayoutType(LayoutType.Row);

        var node1 = world.Add(root);
        node1.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Pixels(150.0f));

        var node2 = world.Add(root);
        node2.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Pixels(150.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(120.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node2));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(0.0f, 150.0f, 100.0f, 150.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void PercentageHorizontalGap()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetHorizontalGap(Units.Percentage(50.0f))
            .SetLayoutType(LayoutType.Row);

        var node1 = world.Add(root);
        node1.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Pixels(150.0f));

        var node2 = world.Add(root);
        node2.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Pixels(150.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(400.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node2));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(0.0f, 150.0f, 100.0f, 150.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void StretchHorizontalGap()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetHorizontalGap(Units.Stretch(1.0f))
            .SetLayoutType(LayoutType.Row);

        var node1 = world.Add(root);
        node1.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Pixels(150.0f));

        var node2 = world.Add(root);
        node2.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Pixels(150.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(500.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node2));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(0.0f, 150.0f, 100.0f, 150.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void PixelsVerticalGap()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetVerticalGap(Units.Pixels(20.0f))
            .SetLayoutType(LayoutType.Column);

        var node1 = world.Add(root);
        node1.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Pixels(150.0f));

        var node2 = world.Add(root);
        node2.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Pixels(150.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(0.0f, 170.0f, 100.0f, 150.0f), world.Cache.Bounds(node2));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(100.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void PercentageVerticalGap()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetVerticalGap(Units.Percentage(50.0f))
            .SetLayoutType(LayoutType.Column);

        var node1 = world.Add(root);
        node1.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Pixels(150.0f));

        var node2 = world.Add(root);
        node2.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Pixels(150.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(0.0f, 450.0f, 100.0f, 150.0f), world.Cache.Bounds(node2));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(100.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void StretchVerticalGap()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetVerticalGap(Units.Stretch(1.0f))
            .SetLayoutType(LayoutType.Column);

        var node1 = world.Add(root);
        node1.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Pixels(150.0f));

        var node2 = world.Add(root);
        node2.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Pixels(150.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(0.0f, 450.0f, 100.0f, 150.0f), world.Cache.Bounds(node2));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(100.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node2));
    }
}
