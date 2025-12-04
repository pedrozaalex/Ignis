using CrucibleUI.Tests.Ecs;
using CrucibleUI.Types;

namespace CrucibleUI.Tests;

/// <summary>
/// Tests for width constraints (min/max width).
/// Ported from morphorm tests/width_constraints.rs
/// </summary>
public class WidthConstraintTests
{
    [Fact]
    public void PixelsMinWidthPixelsWidth()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(100.0f))
            .SetMinWidth(Units.Pixels(200.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 200.0f, 100.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PixelsMaxWidthPixelsWidth()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(400.0f))
            .SetHeight(Units.Pixels(100.0f))
            .SetMaxWidth(Units.Pixels(200.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 200.0f, 100.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PercentageMinWidthPixelsWidth()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(100.0f))
            .SetMinWidth(Units.Percentage(50.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 100.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PercentageMaxWidthPixelsWidth()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(400.0f))
            .SetHeight(Units.Pixels(100.0f))
            .SetMaxWidth(Units.Percentage(50.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 100.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void StretchMinWidthPixelsWidth()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(100.0f))
            .SetMinWidth(Units.Stretch(1.0f));

        world.Layout(root);

        // Stretch min doesn't affect pixels width
        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 100.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void StretchMaxWidthPixelsWidth()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(400.0f))
            .SetHeight(Units.Pixels(100.0f))
            .SetMaxWidth(Units.Stretch(0.5f));

        world.Layout(root);

        // Stretch max doesn't affect pixels width
        Assert.Equal(new Rect(0.0f, 0.0f, 400.0f, 100.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PixelsMinWidthStretchWidth()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Pixels(400.0f))
            .SetMinWidth(Units.Pixels(700.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 700.0f, 400.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 700.0f, 400.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PercentageMinWidthStretchWidth()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Pixels(400.0f))
            .SetMinWidth(Units.Percentage(150.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 900.0f, 400.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 900.0f, 400.0f), world.Cache.Bounds(node));
    }
}
