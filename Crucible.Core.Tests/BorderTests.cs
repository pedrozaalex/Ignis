using Crucible.Core;
using Crucible.Core.Ecs;
using Crucible.Core.Types;
using Friflo.Engine.ECS;
using Xunit;

namespace Crucible.Core.Tests;

/// <summary>
/// Tests for border properties.
/// Ported from morphorm tests/border.rs
/// </summary>
public class BorderTests
{
    [Fact]
    public void BorderPixelsStretchChild()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetBorder(Units.Pixels(50.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Stretch(1.0f));

        world.Layout(root);

        Assert.Equal(new Rect(50.0f, 50.0f, 500.0f, 500.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void BorderPixelsStretchChild2()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetBorder(Units.Pixels(50.0f));

        var node1 = world.Add(root);
        node1.SetWidth(Units.Stretch(1.0f))
             .SetHeight(Units.Stretch(1.0f));

        var node2 = world.Add(root);
        node2.SetWidth(Units.Stretch(1.0f))
             .SetHeight(Units.Stretch(1.0f));

        world.Layout(root);

        Assert.Equal(new Rect(50.0f, 50.0f, 500.0f, 250.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(50.0f, 300.0f, 500.0f, 250.0f), world.Cache.Bounds(node2));

        root.SetLayoutType(LayoutType.Row);

        world.Layout(root);

        Assert.Equal(new Rect(50.0f, 50.0f, 250.0f, 500.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(300.0f, 50.0f, 250.0f, 500.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void BorderPercentageStretchChild()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetBorder(Units.Percentage(10.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Stretch(1.0f));

        world.Layout(root);

        Assert.Equal(new Rect(60.0f, 60.0f, 480.0f, 480.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void BorderParentAuto()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft);

        var node = world.Add(root);
        node.SetWidth(Units.Auto)
            .SetHeight(Units.Auto)
            .SetBorder(Units.Pixels(10.0f));

        var child = world.Add(node);
        child.SetWidth(Units.Pixels(10.0f))
             .SetHeight(Units.Pixels(10.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 30.0f, 30.0f), world.Cache.Bounds(node));
    }
}
