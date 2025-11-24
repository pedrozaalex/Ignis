using FontStashSharp;
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
        private Color? Color { get; set; }

        public Box()
        {
        }

        public Box(Color? color)
        {
            Color = color;
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var color = Color ?? Context?.Theme.Border ?? Microsoft.Xna.Framework.Color.Gray;
            Context?.PrimitiveBatch?.DrawFilledRectangle(bounds, color);
        }
    }

    /// <summary>
    /// Text label view.
    /// </summary>
    public class Text(SpriteFontBase? font = null) : ViewComponent
    {
        public string Content { get; set; } = "";

        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Optional font size override. If set, will get a font at this size from the game's FontSystem.
        /// Use this to easily create titles (e.g., 32), headings (e.g., 24), or small text (e.g., 12).
        /// </summary>
        public int? FontSize { get; set; }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (string.IsNullOrEmpty(Content))
                return;

            var fontToUse = GetFont();

            if (fontToUse != null)
            {
                spriteBatch.DrawString(fontToUse, Content, new Vector2(bounds.X, bounds.Y), Color);
            }
        }

        public override (float width, float height)? Measure(float? availableWidth, float? availableHeight)
        {
            if (string.IsNullOrEmpty(Content))
                return (0, 0);

            var fontToUse = GetFont();

            if (fontToUse == null) return (Content.Length * 8f, 14f);

            var size = fontToUse.MeasureString(Content);
            return (size.X, size.Y);
        }

        private SpriteFontBase? GetFont()
        {
            // If FontSize is specified, try to get a font at that size from the FontSystem
            if (FontSize.HasValue && Context?.Game?.FontSystem != null)
            {
                return Context.Game.FontSystem.GetFont(FontSize.Value);
            }

            // Otherwise: custom font > context default font
            return font ?? Context?.DefaultFont;
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