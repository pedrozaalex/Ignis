using Ignis.Engine.Input;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Input;
using Microsoft.Xna.Framework;

namespace Ignis.Tests.UI;

public class MockUIContext : UIContext
{
    private readonly Dictionary<long, Rectangle> _testBounds = new();
    
    public MockUIContext(IInputProvider inputProvider) 
        : base(null, inputProvider, null, Theme.Dark)
    {
    }
    
    public void SetTestBounds(long elementId, Rectangle bounds)
    {
        _testBounds[elementId] = bounds;
    }
    
    public new Rectangle GetBounds(object node)
    {
        if (node is IView view && _testBounds.TryGetValue(view.Layout.ElementId, out var bounds))
        {
            return bounds;
        }
        return base.GetBounds(node);
    }
}

