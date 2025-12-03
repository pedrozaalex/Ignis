using Crucible.Core;
using Crucible.Core.Ecs;
using Crucible.Core.Extensions;
using Crucible.Core.Types;
using Friflo.Engine.ECS;
using Xunit;
using Xunit.Abstractions;

namespace Crucible.Core.Tests;

public class DebugTests
{
    private readonly ITestOutputHelper _output;
    public DebugTests(ITestOutputHelper output) { _output = output; }

    [Fact]
    public void DebugPercentage()
    {
        using var world = new TestWorld();
        var root = world.Add();
        root.SetWidth(Units.Pixels(600.0f))
            .SetHeight(Units.Pixels(600.0f))
            .SetLayoutType(LayoutType.Row);

        var node = world.Add(root);
        node.SetWidth(Units.Percentage(50.0f))
            .SetHeight(Units.Pixels(150.0f));

        var childNode = new Crucible.Core.Ecs.EntityNode(node);
        _output.WriteLine($"child.Width = {childNode.Width}");
        _output.WriteLine($"child.Main(Row) = {childNode.Main(LayoutType.Row)}");
        
        var rootNode = new Crucible.Core.Ecs.EntityNode(root);
        _output.WriteLine($"root.Width = {rootNode.Width}");

        world.Layout(root);

        var bounds = world.Cache.Bounds(node);
        _output.WriteLine($"bounds = {bounds}");
        
        Assert.Equal(new Rect(0.0f, 0.0f, 300.0f, 150.0f), bounds);
    }
}

