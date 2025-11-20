using Ignis.Engine.UI.Abstractions;
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
#pragma warning disable IDE0051 // Remove unused private members - Used for future rendering
        public Color Color { get; set; } = Color.White;
#pragma warning restore IDE0051

        public Box()
        {
        }

        public Box(Color color)
        {
            Color = color;
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Draw a filled rectangle
            // Note: SpriteBatch needs a 1x1 white texture to draw shapes
            // For now, we'll skip actual drawing - this would need a PrimitiveBatch as per architecture
        }
    }

    /// <summary>
    /// Text label view.
    /// </summary>
    public class Text : ViewComponent
    {
        private readonly SpriteFont? _font;
        private string _text = "";
        
        public string Content
        {
            get => _text;
            set => _text = value;
        }

        public Color Color { get; set; } = Color.Black;

        public Text(SpriteFont? font = null)
        {
            _font = font;
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (_font != null && !string.IsNullOrEmpty(_text))
            {
                spriteBatch.DrawString(_font, _text, new Vector2(bounds.X, bounds.Y), Color);
            }
        }

        public override (float width, float height)? Measure(float? availableWidth, float? availableHeight)
        {
            if (_font == null || string.IsNullOrEmpty(_text))
                return null;

            var size = _font.MeasureString(_text);
            return (size.X, size.Y);
        }
    }

    /// <summary>
    /// Container that lays out children in a column or row.
    /// </summary>
    public class Container : ViewComponent, Core.IViewContainer
    {
        private readonly List<IView> _children = new();

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

