using FontStashSharp;
using Ignis.Engine.Assets;
using Ignis.Engine.Reactive;
using Ignis.Engine.UI;
using Ignis.Engine.UI.Abstractions;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Elements;
using Ignis.Engine.UI.Widgets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Engine.UI.Elements
{
    /// <summary>
    /// Static builder API for creating UI elements declaratively.
    /// </summary>
    public static partial class Elements
    {
        /// <summary>
        /// Creates a container that lays out children vertically.
        /// </summary>
        public static IView Column(params IView[] children)
        {
            var container = new Container(children)
            {
                Layout =
                {
                    LayoutType = LayoutType.Column
                }
            };
            return container;
        }

        /// <summary>
        /// Creates a container that lays out children horizontally.
        /// </summary>
        public static IView Row(params IView[] children)
        {
            var container = new Container(children)
            {
                Layout =
                {
                    LayoutType = LayoutType.Row
                }
            };
            return container;
        }

        /// <summary>
        /// Creates a text label with static text.
        /// </summary>
        public static IView Label(string text, SpriteFontBase? font = null, Color? color = null)
        {
            var textView = new Text(font) { Content = text };
            if (color.HasValue)
                textView.Color = color.Value;
            return textView;
        }

        /// <summary>
        /// Creates a text label bound to a signal.
        /// </summary>
        public static IView Label(Signal<string> textSignal, SpriteFontBase? font = null, Color? color = null)
        {
            var reactiveText = new ReactiveText(textSignal, font);
            if (color.HasValue)
                reactiveText.Color = color.Value;
            return reactiveText;
        }

        /// <summary>
        /// Creates a text label bound to a computed value.
        /// </summary>
        public static IView Label(Computed<string> textComputed, SpriteFontBase? font = null, Color? color = null)
        {
            var reactiveText = new ReactiveText(textComputed, font);
            if (color.HasValue)
                reactiveText.Color = color.Value;
            return reactiveText;
        }

        /// <summary>
        /// Creates a colored box.
        /// </summary>
        public static IView ColorBox(Color color, float width, float height)
        {
            var box = new Box(color)
            {
                Layout =
                {
                    Width = Units.Pixels(width),
                    Height = Units.Pixels(height)
                }
            };
            return box;
        }

        /// <summary>
        /// Creates a button.
        /// </summary>
        public static IView Button(string label, Action onClick, SpriteFontBase? font = null)
        {
            return new ButtonView(label, onClick, font);
        }

        /// <summary>
        /// Creates a button with enabled state signal.
        /// </summary>
        public static IView Button(string label, Action onClick, Signal<bool>? isEnabled, SpriteFontBase? font = null)
        {
            return new ButtonView(label, onClick, font) { IsEnabled = isEnabled };
        }

        /// <summary>
        /// Creates a float input field.
        /// </summary>
        public static IView FloatField(string label, Signal<float> value, SpriteFontBase? font = null)
        {
            return new FloatFieldView(label, value, font);
        }

        /// <summary>
        /// Creates a horizontal separator line.
        /// </summary>
        public static IView Rule(Color? color = null, float thickness = 1f)
        {
            var box = new Box(color)
            {
                Layout =
                {
                    Height = Units.Pixels(thickness),
                    Width = Units.Stretch(1)
                }
            };
            return box;
        }

        /// <summary>
        /// Creates a scroll view container.
        /// </summary>
        public static IView ScrollView(params IView[] children)
        {
            var container = new Container(children)
            {
                Layout =
                {
                    LayoutType = LayoutType.Column
                }
            };
            // TODO: Add actual scrolling behavior when input system is ready
            return container;
        }

        /// <summary>
        /// Creates a window with title and content.
        /// </summary>
        public static IView Window(string title, params IView[] content)
        {
            return new Window(title, Column(content));
        }

        /// <summary>
        /// Wraps a view with padding.
        /// </summary>
        public static IView Padding(IView child, float padding)
        {
            var container = new Container(child)
            {
                Layout =
                {
                    PaddingLeft = Units.Pixels(padding),
                    PaddingRight = Units.Pixels(padding),
                    PaddingTop = Units.Pixels(padding),
                    PaddingBottom = Units.Pixels(padding)
                }
            };
            return container;
        }

        /// <summary>
        /// Creates a spacer with fixed size.
        /// </summary>
        public static IView Spacer(float size)
        {
            var box = new Box(Color.Transparent)
            {
                Layout =
                {
                    Width = Units.Pixels(size),
                    Height = Units.Pixels(size)
                }
            };
            return box;
        }

        /// <summary>
        /// Creates a panel (styled container) with optional styling.
        /// </summary>
        public static Panel Panel(params IView[] children)
        {
            return new Panel(children);
        }

        /// <summary>
        /// Creates an empty panel for fluent children-last API.
        /// </summary>
        public static Panel Panel()
        {
            return new Panel();
        }

        public static Text Title(string text, Color? color = null)
        {
            var textView = new Text { Content = text, FontSize = (int)(DefaultFontProvider.DefaultFontSize * 1.5f) };
            if (color.HasValue)
                textView.Color = color.Value;
            return textView;
        }

        public static Text Heading(string text, Color? color = null)
        {
            var textView = new Text { Content = text, FontSize = (int)(DefaultFontProvider.DefaultFontSize * 1.2f) };
            if (color.HasValue)
                textView.Color = color.Value;
            return textView;
        }

        public static Text Subheading(string text, Color? color = null)
        {
            var textView = new Text { Content = text, FontSize = (int)(DefaultFontProvider.DefaultFontSize * 1.1f) };
            if (color.HasValue)
                textView.Color = color.Value;
            return textView;
        }

        public static Text Small(string text, Color? color = null)
        {
            var textView = new Text { Content = text, FontSize = (int)(DefaultFontProvider.DefaultFontSize * 0.75f) };
            if (color.HasValue)
                textView.Color = color.Value;
            return textView;
        }
    }
}

/// <summary>
/// Text that automatically updates when a signal changes.
/// </summary>
public class ReactiveText : Text
{
    private readonly Func<string> _textGetter;

    public ReactiveText(Signal<string> textSignal, SpriteFontBase? font) : base(font)
    {
        _textGetter = () => textSignal.Value;
    }

    public ReactiveText(Computed<string> textComputed, SpriteFontBase? font) : base(font)
    {
        _textGetter = () => textComputed.Value;
    }

    protected override void OnMount()
    {
        CreateEffect(() => { Content = _textGetter(); });
    }
}

/// <summary>
/// Simple button view.
/// </summary>
public class ButtonView : ViewComponent, IViewContainer
{
    private readonly Panel _panel;
    private readonly Text _labelText;

    public Signal<bool>? IsEnabled { get; set; }

    public ButtonView(string label, Action onClick, SpriteFontBase? font)
    {
        _labelText = new Text(font) { Content = label, Color = Color.White };
        _panel = new Panel(_labelText)
                .Width(100)
                .Height(30)
                .AlignCenter()
                .Rounded(4)
                .OnClick(onClick);

        _panel.Layout.Focusable = false; // Buttons don't need focus, just hover/active
    }

    protected override void OnMount()
    {
        _panel.Mount(Context!);
        
        System.Console.WriteLine($"[Button {Layout.ElementId}] Mounted, initial state: {CurrentState}");
        
        // Update background color based on widget state
        CreateEffect(() =>
        {
            var state = CurrentState;
            System.Console.WriteLine($"[Button {Layout.ElementId}] State changed to: {state}");
            
            if (state.HasFlag(WidgetState.Active))
            {
                _panel.BackgroundColor = Context!.Theme.ButtonActiveColor;
            }
            else if (state.HasFlag(WidgetState.Hovered))
            {
                _panel.BackgroundColor = Context!.Theme.ButtonHoverColor;
            }
            else
            {
                _panel.BackgroundColor = Context!.Theme.PrimaryColor;
            }
        });
    }

    protected override void OnUnmount()
    {
        _panel.Unmount();
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
    {
        // Drawing is handled by the panel
    }

    public IEnumerable<IView> GetChildren()
    {
        yield return _panel;
    }
}

/// <summary>
/// Float input field (simplified - displays value and increment/decrement buttons).
/// </summary>
public class FloatFieldView : ViewComponent, IViewContainer
{
    private readonly IView _content;

    public FloatFieldView(string label, Signal<float> value, SpriteFontBase? font)
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