using Ignis.Engine.UI.Elements;

namespace Ignis.Engine.UI
{
    /// <summary>
    /// Fluent extension methods for Text styling.
    /// </summary>
    public static class TextExtensions
    {
        /// <summary>
        /// Sets the font size for a Text element.
        /// The font will be dynamically loaded from the game's FontSystem at the specified size.
        /// </summary>
        public static T FontSize<T>(this T text, int size) where T : Text
        {
            text.FontSize = size;
            return text;
        }

        /// <summary>
        /// Creates a title-sized text (32pt).
        /// </summary>
        public static T Title<T>(this T text) where T : Text
        {
            text.FontSize = 32;
            return text;
        }

        /// <summary>
        /// Creates a heading-sized text (24pt).
        /// </summary>
        public static T Heading<T>(this T text) where T : Text
        {
            text.FontSize = 24;
            return text;
        }

        /// <summary>
        /// Creates a subheading-sized text (18pt).
        /// </summary>
        public static T Subheading<T>(this T text) where T : Text
        {
            text.FontSize = 18;
            return text;
        }

        /// <summary>
        /// Creates small text (12pt).
        /// </summary>
        public static T Small<T>(this T text) where T : Text
        {
            text.FontSize = 12;
            return text;
        }
    }
}

