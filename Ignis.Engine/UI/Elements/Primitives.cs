using Ignis.Engine.UI.Abstractions;
using Ignis.Engine.UI.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Engine.UI.Elements
{
    /// <summary>
    /// Basic primitive views.
    /// </summary>
    /// <summary>
    /// A simple colored box.
    /// </summary>
    public class Box : ViewComponent
    {
        private Color Color { get; set; } = Color.White;

        public Box()
        {
        }

        public Box(Color color)
        {
            Color = color;
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Draw using PrimitiveBatch
            Context?.PrimitiveBatch?.DrawFilledRectangle(bounds, Color);
        }
    }

    /// <summary>
    /// Text label view.
    /// </summary>
    public class Text(SpriteFont? font = null) : ViewComponent
    {
        public string Content { get; set; } = "";

        public Color Color { get; set; } = Color.White;

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (string.IsNullOrEmpty(Content))
                return;

            // Priority: custom font > context default font
            var fontToUse = font ?? Context?.DefaultFont;

            if (fontToUse != null)
            {
                spriteBatch.DrawString(fontToUse, Content, new Vector2(bounds.X, bounds.Y), Color, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
            }
        }

        public override (float width, float height)? Measure(float? availableWidth, float? availableHeight)
        {
            if (string.IsNullOrEmpty(Content))
                return (0, 0);

            // Priority: custom font > context default font > approximate
            var fontToUse = font ?? Context?.DefaultFont;

            if (fontToUse == null) return (Content.Length * 8f, 14f);

            var size = fontToUse.MeasureString(Content);
            return (size.X, size.Y);
        }
    }

    /// <summary>
    /// Container that lays out children in a column or row.
    /// </summary>
    public class Container : ViewComponent, IViewContainer
    {
        private readonly List<IView> _children = [];

        public Container(params IView[] children)
        {
            _children.AddRange(children);
        }

        public void AddChild(IView child)
        {
            _children.Add(child);
            if (Context != null)
            {
                child.Mount(Context);
            }
        }

        protected override void OnMount()
        {
            foreach (var child in _children)
            {
                child.Mount(Context!);
            }
        }

        protected override void OnUnmount()
        {
            foreach (var child in _children)
            {
                child.Unmount();
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Children are drawn by UIContext
        }

        public IEnumerable<IView> GetChildren() => _children;
    }
}