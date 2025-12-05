using CrucibleUI.Types;
using CrucibleUI.Widgets;

namespace CrucibleUI.Tests.Widgets;

/// <summary>
/// Tests for hit testing functionality.
/// </summary>
public class HitTestTests
{
    [Fact]
    public void HitTest_PointInsidePanel_ReturnsPanel()
    {
        var panel = new Panel()
            .Width(Units.Pixels(200))
            .Height(Units.Pixels(100));

        // Simulate layout computation at (0, 0)
        panel.ComputeBounds(0, 0, 200, 100);

        var result = panel.HitTest(50, 50);

        Assert.Same(panel, result);
    }

    [Fact]
    public void HitTest_PointOutsidePanel_ReturnsNull()
    {
        var panel = new Panel()
            .Width(Units.Pixels(200))
            .Height(Units.Pixels(100));

        panel.ComputeBounds(0, 0, 200, 100);

        var result = panel.HitTest(250, 50);

        Assert.Null(result);
    }

    [Fact]
    public void HitTest_NestedChildren_ReturnsDeepestChild()
    {
        var inner = new Panel()
            .Width(Units.Pixels(50))
            .Height(Units.Pixels(50));

        var outer = new Panel()
            .Width(Units.Pixels(200))
            .Height(Units.Pixels(100))
            .Children(inner);

        // Outer at (0,0), inner at (10, 10)
        outer.ComputeBounds(0, 0, 200, 100);
        inner.ComputeBounds(10, 10, 50, 50);

        var result = outer.HitTest(30, 30);

        Assert.Same(inner, result);
    }

    [Fact]
    public void HitTest_OutsideChild_ReturnsParent()
    {
        var inner = new Panel()
            .Width(Units.Pixels(50))
            .Height(Units.Pixels(50));

        var outer = new Panel()
            .Width(Units.Pixels(200))
            .Height(Units.Pixels(100))
            .Children(inner);

        outer.ComputeBounds(0, 0, 200, 100);
        inner.ComputeBounds(10, 10, 50, 50);

        // Point at (150, 50) is in outer but not in inner
        var result = outer.HitTest(150, 50);

        Assert.Same(outer, result);
    }

    [Fact]
    public void HitTest_InvisibleWidget_ReturnsNull()
    {
        var panel = new Panel()
            .Width(Units.Pixels(200))
            .Height(Units.Pixels(100))
            .Visible(false);

        panel.ComputeBounds(0, 0, 200, 100);

        var result = panel.HitTest(50, 50);

        Assert.Null(result);
    }

    [Fact]
    public void HitTest_DisabledWidget_StillReturnsWidget()
    {
        var button = new Button("Click")
            .Width(Units.Pixels(100))
            .Height(Units.Pixels(40))
            .Disabled(true);

        button.ComputeBounds(0, 0, 100, 40);

        // Disabled widgets should still be hit-testable for tooltips etc.
        var result = button.HitTest(50, 20);

        Assert.Same(button, result);
    }
}
