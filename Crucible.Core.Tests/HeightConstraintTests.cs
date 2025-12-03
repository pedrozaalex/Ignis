using Crucible.Core;
using Crucible.Core.Ecs;
using Crucible.Core.Types;
using Friflo.Engine.ECS;
using Xunit;

namespace Crucible.Core.Tests;

/// <summary>
/// Tests for height constraints (min/max height).
/// Ported from morphorm tests/height_constraints.rs
/// </summary>
public class HeightConstraintTests
{
    [Fact]
    public void PixelsMinHeightPixelsHeight()
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
            .SetMinHeight(Units.Pixels(200.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 200.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PixelsMaxHeightPixelsHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(400.0f))
            .SetMaxHeight(Units.Pixels(200.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 200.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PercentageMinHeightPixelsHeight()
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
            .SetMinHeight(Units.Percentage(50.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 300.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PercentageMaxHeightPixelsHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(400.0f))
            .SetMaxHeight(Units.Percentage(50.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 300.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PixelsMinHeightStretchHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(400.0f))
            .SetHeight(Units.Stretch(1.0f))
            .SetMinHeight(Units.Pixels(700.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 400.0f, 700.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 400.0f, 700.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void PercentageMinHeightStretchHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(400.0f))
            .SetHeight(Units.Stretch(1.0f))
            .SetMinHeight(Units.Percentage(150.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 400.0f, 900.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 400.0f, 900.0f), world.Cache.Bounds(node));
    }
}
