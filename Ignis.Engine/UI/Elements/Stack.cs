using Ignis.Engine.UI.Core;

namespace Ignis.Engine.UI.Elements;

/// <summary>
///     Type-safe layout builders that automatically configure children for proper layout.
///     Prevents common pitfalls like interactive elements collapsing to zero size.
/// </summary>
public static class Stack
{
    /// <summary>
    ///     Creates a vertical stack (Column) that automatically stretches children to fill width.
    ///     Children default to Width = Stretch(1) to prevent collapse.
    /// </summary>
    public static IView Vertical(params IView[] children)
    {
        var container = new Container
        {
            Layout =
            {
                LayoutType = LayoutType.Column
            }
        };

        foreach (var child in children)
        {
            // In a vertical stack, children usually want to fill width
            // Only apply if the child hasn't explicitly set a width
            if (child.Layout.Width.IsAuto)
            {
                child.Layout.Width = Units.Stretch(1);
            }
            container.AddChild(child);
        }

        return container;
    }

    /// <summary>
    ///     Creates a horizontal stack (Row) that automatically stretches children to fill height.
    ///     Children default to Height = Stretch(1) to prevent collapse.
    /// </summary>
    public static IView Horizontal(params IView[] children)
    {
        var container = new Container
        {
            Layout =
            {
                LayoutType = LayoutType.Row
            }
        };

        foreach (var child in children)
        {
            // In a horizontal stack, children usually want to fill height
            // Only apply if the child hasn't explicitly set a height
            if (child.Layout.Height.IsAuto)
            {
                child.Layout.Height = Units.Stretch(1);
            }
            container.AddChild(child);
        }

        return container;
    }
}

