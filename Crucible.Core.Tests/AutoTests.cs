using Crucible.Core.Tests.Ecs;
using Crucible.Core.Types;

namespace Crucible.Core.Tests;

/// <summary>
/// Tests for auto sizing with min width/height constraints and content sizing.
/// Ported from morphorm tests/auto.rs
/// </summary>
public class AutoTests
{
    [Fact]
    public void AutoMinWidth()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Auto)
            .SetHeight(Units.Auto);

        var child1 = world.Add(node);
        child1.SetWidth(Units.Stretch(1.0f))
              .SetMinWidth(Units.Auto)
              .SetHeight(Units.Pixels(50.0f))
              .SetLayoutType(LayoutType.Row)
              .SetContentSize(world.SubLayout, (_, height) => (50.0f, height ?? 0));

        var child2 = world.Add(node);
        child2.SetWidth(Units.Stretch(1.0f))
              .SetMinWidth(Units.Auto)
              .SetHeight(Units.Pixels(50.0f))
              .SetLayoutType(LayoutType.Row)
              .SetContentSize(world.SubLayout, (_, height) => (80.0f, height ?? 0));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 80.0f, 100.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 80.0f, 50.0f), world.Cache.Bounds(child1));
        Assert.Equal(new Rect(0.0f, 50.0f, 80.0f, 50.0f), world.Cache.Bounds(child2));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 80.0f, 100.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 80.0f, 50.0f), world.Cache.Bounds(child1));
        Assert.Equal(new Rect(0.0f, 50.0f, 80.0f, 50.0f), world.Cache.Bounds(child2));
    }

    [Fact]
    public void AutoMinWidth2()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Auto)
            .SetHeight(Units.Auto);

        var child1 = world.Add(node);
        child1.SetWidth(Units.Stretch(1.0f))
              .SetMinWidth(Units.Auto)
              .SetHeight(Units.Pixels(50.0f));

        var subchild1 = world.Add(child1);
        subchild1.SetWidth(Units.Stretch(1.0f))
                 .SetMinWidth(Units.Auto)
                 .SetHeight(Units.Pixels(50.0f))
                 .SetLayoutType(LayoutType.Row)
                 .SetContentSize(world.SubLayout, (_, height) => (50.0f, height ?? 0));

        var child2 = world.Add(node);
        child2.SetWidth(Units.Stretch(1.0f))
              .SetMinWidth(Units.Auto)
              .SetHeight(Units.Pixels(50.0f));

        var subchild2 = world.Add(child2);
        subchild2.SetWidth(Units.Stretch(1.0f))
                 .SetMinWidth(Units.Auto)
                 .SetHeight(Units.Pixels(50.0f))
                 .SetLayoutType(LayoutType.Row)
                 .SetContentSize(world.SubLayout, (_, height) => (80.0f, height ?? 0));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 80.0f, 100.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 80.0f, 50.0f), world.Cache.Bounds(child1));
        Assert.Equal(new Rect(0.0f, 50.0f, 80.0f, 50.0f), world.Cache.Bounds(child2));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 80.0f, 100.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 80.0f, 50.0f), world.Cache.Bounds(child1));
        Assert.Equal(new Rect(0.0f, 50.0f, 80.0f, 50.0f), world.Cache.Bounds(child2));
    }

    [Fact]
    public void AutoMinWidth3()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Auto)
            .SetHeight(Units.Pixels(100.0f));

        var child1 = world.Add(node);
        child1.SetWidth(Units.Stretch(1.0f))
              .SetMinWidth(Units.Auto)
              .SetHeight(Units.Stretch(1.0f))
              .SetLayoutType(LayoutType.Row)
              .SetContentSize(world.SubLayout, (_, height) => (50.0f, height ?? 0));

        var child2 = world.Add(node);
        child2.SetWidth(Units.Stretch(1.0f))
              .SetMinWidth(Units.Auto)
              .SetHeight(Units.Stretch(1.0f))
              .SetLayoutType(LayoutType.Row)
              .SetContentSize(world.SubLayout, (_, height) => (80.0f, height ?? 0));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 80.0f, 100.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 80.0f, 50.0f), world.Cache.Bounds(child1));
        Assert.Equal(new Rect(0.0f, 50.0f, 80.0f, 50.0f), world.Cache.Bounds(child2));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 80.0f, 100.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 80.0f, 50.0f), world.Cache.Bounds(child1));
        Assert.Equal(new Rect(0.0f, 50.0f, 80.0f, 50.0f), world.Cache.Bounds(child2));
    }

    [Fact]
    public void AutoMinWidth4()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Auto)
            .SetLayoutType(LayoutType.Row);

        var child1 = world.Add(node);
        child1.SetWidth(Units.Stretch(1.0f))
              .SetMinHeight(Units.Auto)
              .SetHeight(Units.Stretch(1.0f))
              .SetLayoutType(LayoutType.Row)
              .SetContentSize(world.SubLayout, (width, _) => (width ?? 0, 50.0f));

        var child2 = world.Add(node);
        child2.SetWidth(Units.Stretch(1.0f))
              .SetMinHeight(Units.Auto)
              .SetHeight(Units.Stretch(1.0f))
              .SetLayoutType(LayoutType.Row)
              .SetContentSize(world.SubLayout, (width, _) => (width ?? 0, 80.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 80.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 80.0f), world.Cache.Bounds(child1));
        Assert.Equal(new Rect(50.0f, 0.0f, 50.0f, 80.0f), world.Cache.Bounds(child2));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 80.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 50.0f, 80.0f), world.Cache.Bounds(child1));
        Assert.Equal(new Rect(50.0f, 0.0f, 50.0f, 80.0f), world.Cache.Bounds(child2));
    }
}
