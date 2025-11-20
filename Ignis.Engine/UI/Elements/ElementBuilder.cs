using Ignis.Engine.Reactive;
using Ignis.Engine.UI.Abstractions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Engine.UI.Elements
{
    /// <summary>
    /// Static builder API for creating UI elements declaratively.
    /// </summary>
    public static class Elements
    {
        /// <summary>
        /// Creates a container that lays out children vertically.
        /// </summary>
        public static IView Column(params IView[] children)
        {
            var container = new Container(children);
            container.Layout.LayoutType = LayoutType.Column;
            return container;
        }

        /// <summary>
        /// Creates a container that lays out children horizontally.
        /// </summary>
        public static IView Row(params IView[] children)
        {
            var container = new Container(children);
            container.Layout.LayoutType = LayoutType.Row;
            return container;
        }

        /// <summary>
        /// Creates a text label with static text.
        /// </summary>
        public static IView Label(string text, SpriteFont? font = null)
        {
            return new Text(font) { Content = text };
        }

        /// <summary>
        /// Creates a text label bound to a signal.
        /// </summary>
        public static IView Label(Signal<string> textSignal, SpriteFont? font = null)
        {
            return new ReactiveText(textSignal, font);
        }

        /// <summary>
        /// Creates a colored box.
        /// </summary>
        public static IView ColorBox(Color color, float width, float height)
        {
            var box = new Box(color);
            box.Layout.Width = Units.Pixels(width);
            box.Layout.Height = Units.Pixels(height);
            return box;
        }

        /// <summary>
        /// Creates a button (simplified - just a box with text for now).
        /// </summary>
        public static IView Button(string label, Action onClick, SpriteFont? font = null)
        {
            return new ButtonView(label, onClick, font);
        }

        /// <summary>
        /// Creates a float input field.
        /// </summary>
        public static IView FloatField(string label, Signal<float> value, SpriteFont? font = null)
        {
            return new FloatFieldView(label, value, font);
        }

        /// <summary>
        /// Wraps a view with padding.
        /// </summary>
        public static IView Padding(IView child, float padding)
        {
            var container = new Container(child);
            container.Layout.PaddingLeft = Units.Pixels(padding);
            container.Layout.PaddingRight = Units.Pixels(padding);
            container.Layout.PaddingTop = Units.Pixels(padding);
            container.Layout.PaddingBottom = Units.Pixels(padding);
            return container;
        }

        /// <summary>
        /// Creates a spacer with fixed size.
        /// </summary>
        public static IView Spacer(float size)
        {
            var box = new Box(Color.Transparent);
            box.Layout.Width = Units.Pixels(size);
            box.Layout.Height = Units.Pixels(size);
            return box;
        }
    }

    /// <summary>
    /// Text that automatically updates when a signal changes.
    /// </summary>
    internal class ReactiveText : Text
    {
        private readonly Func<string> _textGetter;

        public ReactiveText(Signal<string> textSignal, SpriteFont? font) : base(font)
        {
            _textGetter = () => textSignal.Value;
        }

        public ReactiveText(Computed<string> textComputed, SpriteFont? font) : base(font)
        {
            _textGetter = () => textComputed.Value;
        }

        protected override void OnMount()
        {
            CreateEffect(() =>
            {
                Content = _textGetter();
            });
        }
    }

    /// <summary>
    /// Simple button view.
    /// </summary>
    internal class ButtonView : ViewComponent, Core.IViewContainer
    {
        private readonly IView _content;

        public ButtonView(string label, Action onClick, SpriteFont? font)
        {
            
            // Build button as a colored box with text
            var background = new Box(new Color(100, 100, 200));
            background.Layout.Width = Units.Pixels(100);
            background.Layout.Height = Units.Pixels(30);
            
            var text = new Text(font) { Content = label, Color = Color.White };
            
            _content = new Container(background, text);
            _content.Layout.LayoutType = LayoutType.Column;
            _content.Layout.Alignment = Alignment.Center;
        }

        protected override void OnMount()
        {
            _content.Mount(Context!);
        }

        protected override void OnUnmount()
        {
            _content.Unmount();
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // TODO: Handle click detection
        }

        public IEnumerable<IView> GetChildren()
        {
            yield return _content;
        }
    }

    /// <summary>
    /// Float input field (simplified - displays value and increment/decrement buttons).
    /// </summary>
    internal class FloatFieldView : ViewComponent, Core.IViewContainer
    {
        private readonly IView _content;

        public FloatFieldView(string label, Signal<float> value, SpriteFont? font)
        {
            // Build as: [Label] [Value] [+] [-]
            var labelText = new Text(font) { Content = label };
            var valueText = new ReactiveText(
                Computed<string>.From(() => value.Value.ToString("F2")),
                font
            );

            _content = new Container(labelText, valueText);
            _content.Layout.LayoutType = LayoutType.Row;
        }

        protected override void OnMount()
        {
            _content.Mount(Context!);
        }

        protected override void OnUnmount()
        {
            _content.Unmount();
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
        }

        public IEnumerable<IView> GetChildren()
        {
            yield return _content;
        }
    }
}

