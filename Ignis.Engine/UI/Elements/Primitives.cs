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
        public Color Color { get; set; } = Color.White;

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
            if (string.IsNullOrEmpty(_text))
                return;

            if (_font != null)
            {
                spriteBatch.DrawString(_font, _text, new Vector2(bounds.X, bounds.Y), Color);
            }
            else
            {
                // Fallback: Draw subtle placeholder to indicate text location
                // Draw a thin border in a dark gray color
                Context?.PrimitiveBatch?.DrawBorder(bounds, 1f, new Color(80, 80, 80, 128));
            }
        }

        public override (float width, float height)? Measure(float? availableWidth, float? availableHeight)
        {
            if (string.IsNullOrEmpty(_text))
                return (0, 0);

            if (_font != null)
            {
                var size = _font.MeasureString(_text);
                return (size.X, size.Y);
            }
            
            // Fallback measurement when no font available
            // Approximate: 8px per char width, 14px height
            return (_text.Length * 8f, 14f);
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

