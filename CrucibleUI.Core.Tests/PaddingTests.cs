using CrucibleUI.Core.Tests.Ecs;
using CrucibleUI.Core.Types;

namespace CrucibleUI.Core.Tests;

/// <summary>
/// Tests for padding properties.
/// Ported from morphorm tests/padding.rs
/// </summary>
public class PaddingTests
{
    [Fact]
    public void PixelsPaddingLeftPixelsSize()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetPaddingLeft(Units.Pixels(20.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(150.0f))
            .SetLeft(Units.Auto);

        world.Layout(root);

        Assert.Equal(new Rect(20.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(20.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PercentagePaddingLeftPixelsSize()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetPaddingLeft(Units.Percentage(50.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(150.0f))
            .SetLeft(Units.Auto);

        world.Layout(root);

        Assert.Equal(new Rect(300.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(300.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PixelsPaddingTopPixelsSize()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetPaddingTop(Units.Pixels(20.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(150.0f))
            .SetTop(Units.Auto);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 20.0f, 100.0f, 150.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 20.0f, 100.0f, 150.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PercentagePaddingTopPixelsSize()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetPaddingTop(Units.Percentage(50.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(150.0f))
            .SetLeft(Units.Auto);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 300.0f, 100.0f, 150.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 300.0f, 100.0f, 150.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PixelsPaddingRightPixelsSize()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetPaddingRight(Units.Pixels(20.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(150.0f))
            .SetLeft(Units.Stretch(1.0f))
            .SetRight(Units.Auto);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PercentagePaddingRightPixelsSize()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetPaddingRight(Units.Percentage(50.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(150.0f))
            .SetLeft(Units.Stretch(1.0f))
            .SetRight(Units.Auto);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PixelsPaddingBottomPixelsSize()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetPaddingBottom(Units.Pixels(20.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(150.0f))
            .SetTop(Units.Stretch(1.0f))
            .SetBottom(Units.Auto);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PercentagePaddingBottomPixelsSize()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetPaddingBottom(Units.Percentage(50.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(150.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 150.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PixelsPaddingPixelsSize()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetPaddingLeft(Units.Pixels(20.0f))
            .SetPaddingTop(Units.Pixels(20.0f))
            .SetPaddingRight(Units.Pixels(20.0f))
            .SetPaddingBottom(Units.Pixels(20.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(150.0f))
            .SetLeft(Units.Auto)
            .SetTop(Units.Auto)
            .SetRight(Units.Auto)
            .SetBottom(Units.Auto);

        world.Layout(root);

        Assert.Equal(new Rect(20.0f, 20.0f, 100.0f, 150.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(20.0f, 20.0f, 100.0f, 150.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PercentagePaddingPixelsSize()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetPaddingLeft(Units.Percentage(50.0f))
            .SetPaddingTop(Units.Percentage(50.0f))
            .SetPaddingRight(Units.Percentage(50.0f))
            .SetPaddingBottom(Units.Percentage(50.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(150.0f))
            .SetLeft(Units.Auto)
            .SetTop(Units.Auto)
            .SetRight(Units.Auto)
            .SetBottom(Units.Auto);

        world.Layout(root);

        Assert.Equal(new Rect(300.0f, 300.0f, 100.0f, 150.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(300.0f, 300.0f, 100.0f, 150.0f), world.Cache.Bounds(node));
    }
}
