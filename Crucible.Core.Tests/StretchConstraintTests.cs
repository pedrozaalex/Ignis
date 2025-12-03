using Crucible.Core.Ecs;
using Crucible.Core.Types;

namespace Crucible.Core.Tests;

/// <summary>
/// Tests for stretch distribution with min/max constraints.
/// These tests verify the freeze-and-redistribute algorithm works correctly
/// when stretch items hit their min/max constraints.
/// </summary>
public class StretchConstraintTests
{
    /// <summary>
    /// When a stretch child is constrained by MaxWidth, the excess space
    /// should be redistributed to the remaining stretch children.
    /// 
    /// Layout: 400px wide parent, two Stretch(1.0) children
    /// Child 1: MaxWidth = 100px
    /// Child 2: No constraints
    /// 
    /// Without freeze-redistribute: Each gets 200px, child1 clamped to 100px = 100px wasted
    /// With freeze-redistribute: Child1 gets 100px, Child2 gets 300px
    /// </summary>
    [Fact]
    public void StretchWithMaxConstraint_RedistributesSpace()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(400.0f))
            .SetHeight(Units.Pixels(100.0f))
            .SetLayoutType(LayoutType.Row);

        var child1 = world.Add(root);
        child1.SetWidth(Units.Stretch(1.0f))
              .SetMaxWidth(Units.Pixels(100.0f));

        var child2 = world.Add(root);
        child2.SetWidth(Units.Stretch(1.0f));

        world.Layout(root);

        var bounds1 = world.Cache.Bounds(child1);
        var bounds2 = world.Cache.Bounds(child2);

        Assert.NotNull(bounds1);
        Assert.NotNull(bounds2);

        // Child1 should be clamped to its max of 100px
        Assert.Equal(100.0f, bounds1.Value.Width);
        
        // Child2 should get the remaining 300px
        Assert.Equal(300.0f, bounds2.Value.Width);
    }

    /// <summary>
    /// When a stretch child is constrained by MinWidth, the other children
    /// should receive less space to accommodate.
    /// 
    /// Layout: 400px wide parent, two Stretch(1.0) children  
    /// Child 1: MinWidth = 300px
    /// Child 2: No constraints
    /// 
    /// Without freeze-redistribute: Each gets 200px, child1 clamped to 300px = overlap
    /// With freeze-redistribute: Child1 gets 300px, Child2 gets 100px
    /// </summary>
    [Fact]
    public void StretchWithMinConstraint_RedistributesSpace()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(400.0f))
            .SetHeight(Units.Pixels(100.0f))
            .SetLayoutType(LayoutType.Row);

        var child1 = world.Add(root);
        child1.SetWidth(Units.Stretch(1.0f))
              .SetMinWidth(Units.Pixels(300.0f));

        var child2 = world.Add(root);
        child2.SetWidth(Units.Stretch(1.0f));

        world.Layout(root);

        var bounds1 = world.Cache.Bounds(child1);
        var bounds2 = world.Cache.Bounds(child2);

        Assert.NotNull(bounds1);
        Assert.NotNull(bounds2);

        // Child1 should get its min of 300px
        Assert.Equal(300.0f, bounds1.Value.Width);
        
        // Child2 should get the remaining 100px
        Assert.Equal(100.0f, bounds2.Value.Width);
    }

    /// <summary>
    /// Multiple constrained stretch children with different constraints.
    /// 
    /// Layout: 600px wide parent, three Stretch(1.0) children
    /// Child 1: MaxWidth = 100px
    /// Child 2: MaxWidth = 150px  
    /// Child 3: No constraints
    /// 
    /// Initial: Each gets 200px
    /// After freeze: Child1=100px, Child2=150px, Child3=350px
    /// </summary>
    [Fact]
    public void MultipleStretchWithMaxConstraints_RedistributesSpace()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(100.0f))
            .SetLayoutType(LayoutType.Row);

        var child1 = world.Add(root);
        child1.SetWidth(Units.Stretch(1.0f))
              .SetMaxWidth(Units.Pixels(100.0f));

        var child2 = world.Add(root);
        child2.SetWidth(Units.Stretch(1.0f))
              .SetMaxWidth(Units.Pixels(150.0f));

        var child3 = world.Add(root);
        child3.SetWidth(Units.Stretch(1.0f));

        world.Layout(root);

        var bounds1 = world.Cache.Bounds(child1);
        var bounds2 = world.Cache.Bounds(child2);
        var bounds3 = world.Cache.Bounds(child3);

        Assert.NotNull(bounds1);
        Assert.NotNull(bounds2);
        Assert.NotNull(bounds3);

        // Child1 clamped to 100px
        Assert.Equal(100.0f, bounds1.Value.Width);
        
        // Child2 clamped to 150px
        Assert.Equal(150.0f, bounds2.Value.Width);
        
        // Child3 gets remaining 350px
        Assert.Equal(350.0f, bounds3.Value.Width);
    }

    /// <summary>
    /// Stretch with different factors and constraints.
    /// 
    /// Layout: 400px wide parent
    /// Child 1: Stretch(2.0), MaxWidth = 100px
    /// Child 2: Stretch(1.0), no constraints
    /// 
    /// Initial ratio 2:1 would give Child1=266.67px, Child2=133.33px
    /// After clamping: Child1=100px, Child2 gets remaining 300px
    /// </summary>
    [Fact]
    public void StretchWithDifferentFactorsAndConstraints()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(400.0f))
            .SetHeight(Units.Pixels(100.0f))
            .SetLayoutType(LayoutType.Row);

        var child1 = world.Add(root);
        child1.SetWidth(Units.Stretch(2.0f))
              .SetMaxWidth(Units.Pixels(100.0f));

        var child2 = world.Add(root);
        child2.SetWidth(Units.Stretch(1.0f));

        world.Layout(root);

        var bounds1 = world.Cache.Bounds(child1);
        var bounds2 = world.Cache.Bounds(child2);

        Assert.NotNull(bounds1);
        Assert.NotNull(bounds2);

        // Child1 clamped to max 100px
        Assert.Equal(100.0f, bounds1.Value.Width);
        
        // Child2 gets remaining 300px
        Assert.Equal(300.0f, bounds2.Value.Width);
    }

    /// <summary>
    /// When using stretch gap with max constraint, the gap should be clamped
    /// and remaining space redistributed to stretch children.
    /// 
    /// This test fails because the C# port doesn't implement the freeze-redistribute
    /// logic for gaps (ItemType::After in the Rust code).
    /// 
    /// Layout: 500px wide parent, two Stretch(1.0) children, Stretch(1.0) gap with MaxGap=50px
    /// 
    /// Without constraint: Each child gets ~166.67px, gap gets ~166.67px
    /// With constraint: Gap clamped to 50px, each child gets (500-50)/2 = 225px
    /// </summary>
    [Fact]
    public void StretchGapWithMaxConstraint_RedistributesSpace()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(500.0f))
            .SetHeight(Units.Pixels(100.0f))
            .SetLayoutType(LayoutType.Row)
            .SetHorizontalGap(Units.Stretch(1.0f))
            .SetMaxHorizontalGap(Units.Pixels(50.0f));

        var child1 = world.Add(root);
        child1.SetWidth(Units.Stretch(1.0f));

        var child2 = world.Add(root);
        child2.SetWidth(Units.Stretch(1.0f));

        world.Layout(root);

        var bounds1 = world.Cache.Bounds(child1);
        var bounds2 = world.Cache.Bounds(child2);

        Assert.NotNull(bounds1);
        Assert.NotNull(bounds2);

        // Gap should be clamped to max 50px
        // Remaining space = 500 - 50 = 450px
        // Each child gets 225px
        Assert.Equal(225.0f, bounds1.Value.Width);
        Assert.Equal(225.0f, bounds2.Value.Width);
        
        // Child2 should start at child1.width + gap = 225 + 50 = 275
        Assert.Equal(275.0f, bounds2.Value.PosX);
    }
}
