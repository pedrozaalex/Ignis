using CrucibleUI.Interfaces;
using CrucibleUI.Types;

namespace CrucibleUI.Widgets;

/// <summary>
/// ICache implementation that stores computed bounds directly on Widget instances.
/// </summary>
public class WidgetCache : ICache<Widget>
{
    public void SetBounds(Widget node, float posX, float posY, float width, float height)
    {
        node.ComputeBounds(posX, posY, width, height);
    }

    public Rect? Bounds(Widget node)
    {
        return new Rect(node.ComputedX, node.ComputedY, node.ComputedWidth, node.ComputedHeight);
    }
}
