using CrucibleUI.Types;
using CrucibleUI.Widgets;

namespace CrucibleUI.Tests.Widgets;

/// <summary>
/// Tests for layout integration with widgets.
/// </summary>
public class WidgetLayoutTests
{
    [Fact]
    public void WidgetTree_ToLayoutNodes_ConvertsCorrectly()
    {
        var child1 = new Label("First")
            .Width(Units.Pixels(100))
            .Height(Units.Pixels(30));

        var child2 = new Label("Second")
            .Width(Units.Pixels(100))
            .Height(Units.Pixels(30));

        var root = new Panel()
            .Width(Units.Pixels(400))
            .Height(Units.Pixels(200))
            .Column()
            .Gap(Units.Pixels(10))
            .Children(child1, child2);

        var nodes = root.ToLayoutNodes();

        Assert.Equal(3, nodes.Count); // root + 2 children
    }

    [Fact]
    public void Widget_ComputeLayout_PropagatesToChildren()
    {
        var child = new Panel()
            .Width(Units.Stretch(1))
            .Height(Units.Pixels(50));

        var root = new Panel()
            .Width(Units.Pixels(400))
            .Height(Units.Pixels(200))
            .Column()
            .Padding(Units.Pixels(20))
            .Children(child);

        // Compute layout for entire tree
        root.ComputeLayout();

        // Child should have computed bounds: (20, 20, 360, 50)
        Assert.Equal(20f, child.ComputedX);
        Assert.Equal(20f, child.ComputedY);
        Assert.Equal(360f, child.ComputedWidth);
        Assert.Equal(50f, child.ComputedHeight);
    }

    [Fact]
    public void Widget_Row_LayoutsChildrenHorizontally()
    {
        var child1 = new Panel()
            .Width(Units.Pixels(100))
            .Height(Units.Pixels(50));

        var child2 = new Panel()
            .Width(Units.Pixels(100))
            .Height(Units.Pixels(50));

        var root = new Panel()
            .Width(Units.Pixels(400))
            .Height(Units.Pixels(100))
            .Row()
            .Gap(Units.Pixels(10))
            .Children(child1, child2);

        root.ComputeLayout();

        // child1 at (0, 0), child2 at (110, 0)
        Assert.Equal(0f, child1.ComputedX);
        Assert.Equal(110f, child2.ComputedX);
    }

    [Fact]
    public void Widget_Column_LayoutsChildrenVertically()
    {
        var child1 = new Panel()
            .Width(Units.Pixels(100))
            .Height(Units.Pixels(50));

        var child2 = new Panel()
            .Width(Units.Pixels(100))
            .Height(Units.Pixels(50));

        var root = new Panel()
            .Width(Units.Pixels(400))
            .Height(Units.Pixels(200))
            .Column()
            .Gap(Units.Pixels(10))
            .Children(child1, child2);

        root.ComputeLayout();

        // child1 at (0, 0), child2 at (0, 60)
        Assert.Equal(0f, child1.ComputedY);
        Assert.Equal(60f, child2.ComputedY);
    }

    [Fact]
    public void Widget_Stretch_DividesSpaceEvenly()
    {
        var child1 = new Panel()
            .Width(Units.Stretch(1))
            .Height(Units.Pixels(50));

        var child2 = new Panel()
            .Width(Units.Stretch(1))
            .Height(Units.Pixels(50));

        var root = new Panel()
            .Width(Units.Pixels(400))
            .Height(Units.Pixels(100))
            .Row()
            .Gap(Units.Pixels(0))
            .Children(child1, child2);

        root.ComputeLayout();

        // Each child gets 200px width
        Assert.Equal(200f, child1.ComputedWidth);
        Assert.Equal(200f, child2.ComputedWidth);
    }

    [Fact]
    public void Widget_StretchWithFactors_DividesProportionally()
    {
        var child1 = new Panel()
            .Width(Units.Stretch(1))
            .Height(Units.Pixels(50));

        var child2 = new Panel()
            .Width(Units.Stretch(2))
            .Height(Units.Pixels(50));

        var root = new Panel()
            .Width(Units.Pixels(300))
            .Height(Units.Pixels(100))
            .Row()
            .Gap(Units.Pixels(0))
            .Children(child1, child2);

        root.ComputeLayout();

        // child1 gets 1/3 (100px), child2 gets 2/3 (200px)
        Assert.Equal(100f, child1.ComputedWidth);
        Assert.Equal(200f, child2.ComputedWidth);
    }

    [Fact]
    public void Widget_AutoSizePanel_CentersInParent()
    {
        // Mimics the MainMenuScene structure:
        // Root: Stretch(1) x Stretch(1), Column, Alignment.Center
        // MenuPanel: Auto x Auto, Column, Alignment.Center
        // Button: 250px x 45px

        var button = new Button("Test Button")
            .Width<Button>(Units.Pixels(250))
            .Height<Button>(Units.Pixels(45));

        var menuPanel = new Panel()
            .Column()
            .Alignment(CrucibleUI.Types.Alignment.Center)
            .Children(button);
        // Note: menuPanel has no explicit width/height (Auto)

        var root = new Panel()
            .Width(Units.Stretch(1))
            .Height(Units.Stretch(1))
            .Alignment(CrucibleUI.Types.Alignment.Center)
            .Children(menuPanel);

        // Simulate what happens in OnEnter:
        root.ComputeBounds(0, 0, 1280, 720);
        root.ComputeLayout();

        // Root should be full screen
        Assert.Equal(1280f, root.ComputedWidth);
        Assert.Equal(720f, root.ComputedHeight);

        // MenuPanel should size to its content (button) and be centered
        Assert.Equal(250f, menuPanel.ComputedWidth);
        Assert.Equal(45f, menuPanel.ComputedHeight);

        // MenuPanel X should be centered: (1280 - 250) / 2 = 515
        Assert.Equal(515f, menuPanel.ComputedX);
        // MenuPanel Y should be centered: (720 - 45) / 2 = 337.5
        Assert.Equal(337.5f, menuPanel.ComputedY);
    }
}
