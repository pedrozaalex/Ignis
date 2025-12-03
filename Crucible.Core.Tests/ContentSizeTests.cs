using Crucible.Core;
using Crucible.Core.Ecs;
using Crucible.Core.Types;
using Friflo.Engine.ECS;
using Xunit;

namespace Crucible.Core.Tests;

/// <summary>
/// Tests for content size functionality.
/// Ported from morphorm tests/content_size.rs
/// </summary>
public class ContentSizeTests
{
    [Fact]
    public void ContentSizeHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(400.0f))
            .SetHeight(Units.Auto)
            .SetContentSize(world.SubLayout, (width, _) => (width ?? 0, 100.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 400.0f, 100.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 400.0f, 100.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void ContentSizeWidth()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Auto)
            .SetHeight(Units.Pixels(400.0f))
            .SetContentSize(world.SubLayout, (_, height) => (100.0f, height ?? 0));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 400.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 400.0f), world.Cache.Bounds(node));
    }

    [Fact]
    public void ContentSizeHeight2()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node1 = world.Add(root);
        node1.SetWidth(Units.Stretch(1.0f))
             .SetHeight(Units.Auto)
             .SetContentSize(world.SubLayout, (width, _) => (width ?? 0, (width ?? 0) / 2.0f));

        var node2 = world.Add(root);
        node2.SetWidth(Units.Stretch(1.0f))
             .SetHeight(Units.Auto)
             .SetContentSize(world.SubLayout, (width, _) => (width ?? 0, (width ?? 0) / 2.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 150.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(300.0f, 0.0f, 300.0f, 150.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void ContentSizeWidth2()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Column);

        var node1 = world.Add(root);
        node1.SetWidth(Units.Auto)
             .SetHeight(Units.Stretch(1.0f))
             .SetContentSize(world.SubLayout, (_, height) => ((height ?? 0) / 2.0f, height ?? 0));

        var node2 = world.Add(root);
        node2.SetWidth(Units.Auto)
             .SetHeight(Units.Stretch(1.0f))
             .SetContentSize(world.SubLayout, (_, height) => ((height ?? 0) / 2.0f, height ?? 0));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 150.0f, 300.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(0.0f, 300.0f, 150.0f, 300.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void ContentSizeWidthParentAutoWidth()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Column);

        var node1 = world.Add(root);
        node1.SetWidth(Units.Auto)
             .SetHeight(Units.Auto);

        var node2 = world.Add(node1);
        node2.SetWidth(Units.Auto)
             .SetHeight(Units.Pixels(100.0f))
             .SetContentSize(world.SubLayout, (_, _) => (100.0f, 100.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 100.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 100.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void ContentSizeHeightParentAutoHeight()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node1 = world.Add(root);
        node1.SetWidth(Units.Auto)
             .SetHeight(Units.Auto);

        var node2 = world.Add(node1);
        node2.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Auto)
             .SetContentSize(world.SubLayout, (_, _) => (100.0f, 100.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 100.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 100.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void EqualAspectRatio()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetAlignment(Alignment.TopLeft)
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Stretch(1.0f))
            .SetHeight(Units.Auto)
            .SetContentSize(world.SubLayout, (width, _) => (width ?? 0, width ?? 0));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 600.0f, 600.0f), world.Cache.Bounds(node));

        root.SetLayoutType(LayoutType.Column);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 600.0f, 600.0f), world.Cache.Bounds(node));
    }
}
