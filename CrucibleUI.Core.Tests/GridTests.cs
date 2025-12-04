using CrucibleUI.Core.Tests.Ecs;
using CrucibleUI.Core.Types;

namespace CrucibleUI.Core.Tests;

/// <summary>
/// Tests for grid layout functionality.
/// Ported from morphorm tests/grid.rs
/// </summary>
public class GridTests
{
    [Fact]
    public void GridPixels()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetLayoutType(LayoutType.Grid)
            .SetGridColumns(Units.Pixels(100.0f), Units.Pixels(200.0f))
            .SetGridRows(Units.Pixels(50.0f), Units.Pixels(150.0f));

        var node1 = world.Add(root);
        node1.SetColumnStart(0).SetRowStart(0);

        var node2 = world.Add(root);
        node2.SetColumnStart(1).SetRowStart(0);

        var node3 = world.Add(root);
        node3.SetColumnStart(0).SetRowStart(1);

        var node4 = world.Add(root);
        node4.SetColumnStart(1).SetRowStart(1);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 50.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(100.0f, 0.0f, 200.0f, 50.0f), world.Cache.Bounds(node2));
        Assert.Equal(new Rect(0.0f, 50.0f, 100.0f, 150.0f), world.Cache.Bounds(node3));
        Assert.Equal(new Rect(100.0f, 50.0f, 200.0f, 150.0f), world.Cache.Bounds(node4));
    }

    [Fact]
    public void GridStretch()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetLayoutType(LayoutType.Grid)
            .SetGridColumns(Units.Stretch(1.0f), Units.Stretch(2.0f))
            .SetGridRows(Units.Stretch(2.0f), Units.Stretch(1.0f));

        var node1 = world.Add(root);
        node1.SetColumnStart(0).SetRowStart(0);

        var node2 = world.Add(root);
        node2.SetColumnStart(1).SetRowStart(0);

        var node3 = world.Add(root);
        node3.SetColumnStart(0).SetRowStart(1);

        var node4 = world.Add(root);
        node4.SetColumnStart(1).SetRowStart(1);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 200.0f, 400.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(200.0f, 0.0f, 400.0f, 400.0f), world.Cache.Bounds(node2));
        Assert.Equal(new Rect(0.0f, 400.0f, 200.0f, 200.0f), world.Cache.Bounds(node3));
        Assert.Equal(new Rect(200.0f, 400.0f, 400.0f, 200.0f), world.Cache.Bounds(node4));
    }

    [Fact]
    public void GridPixelsStretchMixed()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetLayoutType(LayoutType.Grid)
            // 100px fixed, remainder (500px) split 1:4 -> 100px, 400px.
            .SetGridColumns(Units.Pixels(100.0f), Units.Stretch(1.0f), Units.Stretch(4.0f))
            .SetGridRows(Units.Pixels(100.0f), Units.Stretch(1.0f));

        var node1 = world.Add(root); // Fixed col
        node1.SetColumnStart(0).SetRowStart(0);

        var node2 = world.Add(root); // Stretch 1.0 col
        node2.SetColumnStart(1).SetRowStart(0);

        var node3 = world.Add(root); // Stretch 4.0 col
        node3.SetColumnStart(2).SetRowStart(0);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 100.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(100.0f, 0.0f, 100.0f, 100.0f), world.Cache.Bounds(node2));
        Assert.Equal(new Rect(200.0f, 0.0f, 400.0f, 100.0f), world.Cache.Bounds(node3));
    }

    [Fact]
    public void GridPercent()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetLayoutType(LayoutType.Grid)
            .SetGridColumns(Units.Percentage(50.0f), Units.Percentage(50.0f))
            .SetGridRows(Units.Percentage(25.0f), Units.Percentage(75.0f));

        var node1 = world.Add(root);
        node1.SetColumnStart(0).SetRowStart(0);

        var node2 = world.Add(root);
        node2.SetColumnStart(1).SetRowStart(1);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 150.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(300.0f, 150.0f, 300.0f, 450.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void GridGaps()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetLayoutType(LayoutType.Grid)
            .SetGridColumns(Units.Pixels(100.0f), Units.Pixels(100.0f))
            .SetGridRows(Units.Pixels(100.0f), Units.Pixels(100.0f))
            .SetHorizontalGap(Units.Pixels(20.0f))
            .SetVerticalGap(Units.Pixels(10.0f));

        var node1 = world.Add(root);
        node1.SetColumnStart(0).SetRowStart(0);

        var node2 = world.Add(root);
        node2.SetColumnStart(1).SetRowStart(1);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 100.0f, 100.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(120.0f, 110.0f, 100.0f, 100.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void GridGapsStretch()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetLayoutType(LayoutType.Grid)
            .SetGridColumns(Units.Stretch(1.0f), Units.Stretch(1.0f))
            .SetGridRows(Units.Stretch(1.0f))
            .SetHorizontalGap(Units.Pixels(20.0f));

        // Available for stretch: 600 - 20 (gap) = 580.
        // Each col: 290.

        var node1 = world.Add(root);
        node1.SetColumnStart(0).SetRowStart(0);

        var node2 = world.Add(root);
        node2.SetColumnStart(1).SetRowStart(0);

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 290.0f, 600.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(310.0f, 0.0f, 290.0f, 600.0f), world.Cache.Bounds(node2));
    }

    [Fact]
    public void GridSpan()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetLayoutType(LayoutType.Grid)
            .SetGridColumns(Units.Pixels(100.0f), Units.Pixels(100.0f), Units.Pixels(100.0f))
            .SetGridRows(Units.Pixels(100.0f))
            .SetHorizontalGap(Units.Pixels(10.0f));

        var node1 = world.Add(root);
        node1.SetColumnStart(0)
             .SetColumnSpan(2)
             .SetRowStart(0);
        
        // Span should cover Col 0 (100) + Gap (10) + Col 1 (100) = 210.

        var node2 = world.Add(root);
        node2.SetColumnStart(1)
             .SetColumnSpan(2)
             .SetRowStart(0);
        
        // Start at Col 1 (110). Cover Col 1 (100) + Gap (10) + Col 2 (100) = 210.

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 210.0f, 100.0f), world.Cache.Bounds(node1));
        Assert.Equal(new Rect(110.0f, 0.0f, 210.0f, 100.0f), world.Cache.Bounds(node2));
    }
}