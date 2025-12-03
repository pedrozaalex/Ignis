using Crucible.Core;
using Crucible.Core.Ecs;
using Crucible.Core.Types;
using Friflo.Engine.ECS;
using Xunit;

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
}
