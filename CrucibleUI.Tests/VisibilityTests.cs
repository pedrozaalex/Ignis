using CrucibleUI.Tests.Ecs;
using CrucibleUI.Types;

namespace CrucibleUI.Tests;

/// <summary>
/// Tests for visibility properties.
/// Ported from morphorm tests/visibility.rs
/// </summary>
public class VisibilityTests
{
    [Fact]
    public void Visibility()
    {
        using var world = new TestWorld();

        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f));

        var node = world.Add(root);
        node.SetWidth(Units.Pixels(100.0f))
            .SetHeight(Units.Pixels(100.0f))
            .SetVisibility(false);

        var child = world.Add(node);
        child.SetWidth(Units.Pixels(100.0f))
             .SetHeight(Units.Pixels(100.0f));

        world.Layout(root);

        Assert.Equal(new Rect(0.0f, 0.0f, 0.0f, 0.0f), world.Cache.Bounds(node));
        Assert.Equal(new Rect(0.0f, 0.0f, 0.0f, 0.0f), world.Cache.Bounds(child));
    }
}
