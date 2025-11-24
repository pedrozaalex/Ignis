namespace Ignis.Engine.UI.Core
{
    /// <summary>
    /// Fluent extension methods for styling IView elements declaratively.
    /// Enables method chaining for cleaner UI composition.
    /// </summary>
    public static class ViewExtensions
    {
        // Size
        public static T Width<T>(this T view, Units width) where T : IView
        {
            view.Layout.Width = width;
            return view;
        }

        public static T Width<T>(this T view, float pixels) where T : IView
        {
            view.Layout.Width = Units.Pixels(pixels);
            return view;
        }

        public static T Height<T>(this T view, Units height) where T : IView
        {
            view.Layout.Height = height;
            return view;
        }

        public static T Height<T>(this T view, float pixels) where T : IView
        {
            view.Layout.Height = Units.Pixels(pixels);
            return view;
        }

        public static T Size<T>(this T view, float width, float height) where T : IView
        {
            view.Layout.Width = Units.Pixels(width);
            view.Layout.Height = Units.Pixels(height);
            return view;
        }

        // Min/Max Size
        public static T MinWidth<T>(this T view, Units minWidth) where T : IView
        {
            view.Layout.MinWidth = minWidth;
            return view;
        }

        public static T MinHeight<T>(this T view, Units minHeight) where T : IView
        {
            view.Layout.MinHeight = minHeight;
            return view;
        }

        public static T MaxWidth<T>(this T view, Units maxWidth) where T : IView
        {
            view.Layout.MaxWidth = maxWidth;
            return view;
        }

        public static T MaxHeight<T>(this T view, Units maxHeight) where T : IView
        {
            view.Layout.MaxHeight = maxHeight;
            return view;
        }

        // Padding
        public static T Padding<T>(this T view, float padding) where T : IView
        {
            view.Layout.PaddingLeft = Units.Pixels(padding);
            view.Layout.PaddingRight = Units.Pixels(padding);
            view.Layout.PaddingTop = Units.Pixels(padding);
            view.Layout.PaddingBottom = Units.Pixels(padding);
            return view;
        }

        public static T Padding<T>(this T view, float horizontal, float vertical) where T : IView
        {
            view.Layout.PaddingLeft = Units.Pixels(horizontal);
            view.Layout.PaddingRight = Units.Pixels(horizontal);
            view.Layout.PaddingTop = Units.Pixels(vertical);
            view.Layout.PaddingBottom = Units.Pixels(vertical);
            return view;
        }

        public static T Padding<T>(this T view, float left, float top, float right, float bottom) where T : IView
        {
            view.Layout.PaddingLeft = Units.Pixels(left);
            view.Layout.PaddingTop = Units.Pixels(top);
            view.Layout.PaddingRight = Units.Pixels(right);
            view.Layout.PaddingBottom = Units.Pixels(bottom);
            return view;
        }

        public static T PaddingLeft<T>(this T view, float padding) where T : IView
        {
            view.Layout.PaddingLeft = Units.Pixels(padding);
            return view;
        }

        public static T PaddingRight<T>(this T view, float padding) where T : IView
        {
            view.Layout.PaddingRight = Units.Pixels(padding);
            return view;
        }

        public static T PaddingTop<T>(this T view, float padding) where T : IView
        {
            view.Layout.PaddingTop = Units.Pixels(padding);
            return view;
        }

        public static T PaddingBottom<T>(this T view, float padding) where T : IView
        {
            view.Layout.PaddingBottom = Units.Pixels(padding);
            return view;
        }

        // Alignment
        public static T Align<T>(this T view, Alignment alignment) where T : IView
        {
            view.Layout.Alignment = alignment;
            return view;
        }

        public static T AlignCenter<T>(this T view) where T : IView
        {
            view.Layout.Alignment = Alignment.Center;
            return view;
        }

        public static T AlignLeft<T>(this T view) where T : IView
        {
            view.Layout.Alignment = Alignment.Left;
            return view;
        }

        public static T AlignRight<T>(this T view) where T : IView
        {
            view.Layout.Alignment = Alignment.Right;
            return view;
        }

        // Position
        public static T Left<T>(this T view, float pixels) where T : IView
        {
            view.Layout.Left = Units.Pixels(pixels);
            return view;
        }

        public static T Right<T>(this T view, float pixels) where T : IView
        {
            view.Layout.Right = Units.Pixels(pixels);
            return view;
        }

        public static T Top<T>(this T view, float pixels) where T : IView
        {
            view.Layout.Top = Units.Pixels(pixels);
            return view;
        }

        public static T Bottom<T>(this T view, float pixels) where T : IView
        {
            view.Layout.Bottom = Units.Pixels(pixels);
            return view;
        }

        // Gaps
        public static T Gap<T>(this T view, float gap) where T : IView
        {
            view.Layout.RowGap = Units.Pixels(gap);
            view.Layout.ColumnGap = Units.Pixels(gap);
            return view;
        }

        public static T RowGap<T>(this T view, float gap) where T : IView
        {
            view.Layout.RowGap = Units.Pixels(gap);
            return view;
        }

        public static T ColumnGap<T>(this T view, float gap) where T : IView
        {
            view.Layout.ColumnGap = Units.Pixels(gap);
            return view;
        }
    }
}

