using Ignis.Engine.Reactive;
using Ignis.Engine.UI.Abstractions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReactiveEffect = Ignis.Engine.Reactive.Effect;

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

        public Color Color { get; set; } = Color.White;

        public Text(SpriteFont? font = null)
        {
            _font = font;
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (string.IsNullOrEmpty(_text))
                return;

            // Priority: custom font > context default font
            var fontToUse = _font ?? Context?.DefaultFont;
            
            if (fontToUse != null)
            {
                spriteBatch.DrawString(fontToUse, _text, new Vector2(bounds.X, bounds.Y), Color);
            }
        }

        public override (float width, float height)? Measure(float? availableWidth, float? availableHeight)
        {
            if (string.IsNullOrEmpty(_text))
                return (0, 0);

            // Priority: custom font > context default font > approximate
            var fontToUse = _font ?? Context?.DefaultFont;
            
            if (fontToUse != null)
            {
                var size = fontToUse.MeasureString(_text);
                return (size.X, size.Y);
            }
            
            // Fallback measurement when no font available (approximate)
            return (_text.Length * 8f, 14f);
        }
    }

    /// <summary>
    /// Container that lays out children in a column or row.
    /// </summary>
    public class Container : ViewComponent, Core.IViewContainer
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

