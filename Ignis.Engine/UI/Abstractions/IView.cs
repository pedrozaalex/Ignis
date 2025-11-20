using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Engine.UI.Abstractions
{
    /// <summary>
    /// IView - Represents a node in the UI tree.
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// Draws the view within the given bounds.
        /// </summary>
        void Draw(SpriteBatch spriteBatch, Rectangle bounds);

        /// <summary>
        /// Called when the view is added to the live tree. Use to set up Signal subscriptions.
        /// </summary>
        void Mount(Core.UIContext context);

        /// <summary>
        /// Called when the view is removed from the tree. Clean up subscriptions here.
        /// </summary>
        void Unmount();

        /// <summary>
        /// Measures the desired size of the view given available space constraints.
        /// Returns (width, height) or null if the view has no intrinsic size.
        /// </summary>
        (float width, float height)? Measure(float? availableWidth, float? availableHeight);

        /// <summary>
        /// Gets the layout properties for this view node.
        /// </summary>
        IViewLayout Layout { get; }
    }

    /// <summary>
    /// Layout properties for a view.
    /// </summary>
    public interface IViewLayout
    {
        Units Width { get; set; }
        Units Height { get; set; }
        Units MinWidth { get; set; }
        Units MinHeight { get; set; }
        Units MaxWidth { get; set; }
        Units MaxHeight { get; set; }
        
        Units Left { get; set; }
        Units Right { get; set; }
        Units Top { get; set; }
        Units Bottom { get; set; }
        
        Units PaddingLeft { get; set; }
        Units PaddingRight { get; set; }
        Units PaddingTop { get; set; }
        Units PaddingBottom { get; set; }
        
        Units RowGap { get; set; }
        Units ColumnGap { get; set; }
        
        LayoutType LayoutType { get; set; }
        PositionType PositionType { get; set; }
        Alignment Alignment { get; set; }
        
        bool Visible { get; set; }
    }

    /// <summary>
    /// Base implementation of IViewLayout.
    /// </summary>
    public class ViewLayout : IViewLayout
    {
        public Units Width { get; set; } = Units.Auto;
        public Units Height { get; set; } = Units.Auto;
        public Units MinWidth { get; set; } = Units.Auto;
        public Units MinHeight { get; set; } = Units.Auto;
        public Units MaxWidth { get; set; } = Units.Auto;
        public Units MaxHeight { get; set; } = Units.Auto;
        
        public Units Left { get; set; } = Units.Auto;
        public Units Right { get; set; } = Units.Auto;
        public Units Top { get; set; } = Units.Auto;
        public Units Bottom { get; set; } = Units.Auto;
        
        public Units PaddingLeft { get; set; } = Units.Pixels(0);
        public Units PaddingRight { get; set; } = Units.Pixels(0);
        public Units PaddingTop { get; set; } = Units.Pixels(0);
        public Units PaddingBottom { get; set; } = Units.Pixels(0);
        
        public Units RowGap { get; set; } = Units.Pixels(8);
        public Units ColumnGap { get; set; } = Units.Pixels(8);
        
        public LayoutType LayoutType { get; set; } = LayoutType.Column;
        public PositionType PositionType { get; set; } = PositionType.Relative;
        public Alignment Alignment { get; set; } = Alignment.TopLeft;
        
        public bool Visible { get; set; } = true;
    }
}

