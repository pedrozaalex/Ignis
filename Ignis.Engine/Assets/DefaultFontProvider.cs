using FontStashSharp;

namespace Ignis.Engine.Assets;

/// <summary>
///     Provides automatic default font loading for the UI system using FontStackSharp.
///     Uses embedded Roboto fonts with optimal scaling settings for high-quality rendering.
/// </summary>
public static class DefaultFontProvider
{
    public const int DefaultFontSize = 14;

    /// <summary>
    ///     Creates and configures a FontSystem with optimal scaling parameters.
    ///     This uses embedded Roboto fonts from FontStashSharp for zero-dependency font rendering.
    /// </summary>
    /// <returns>The configured FontSystem, or null if initialization failed</returns>
    public static FontSystem? CreateDefaultFontSystem()
    {
        try
        {
            // Configure optimal scaling for high-quality font rendering
            FontSystemDefaults.FontResolutionFactor = 2.0f;
            FontSystemDefaults.KernelWidth = 2;
            FontSystemDefaults.KernelHeight = 2;

            Console.WriteLine("[DefaultFontProvider] Creating FontSystem with enhanced scaling parameters...");

            var fontSystem = new FontSystem();

            // Try to load a system font (Arial or fallback to any available system font)
            try
            {
                var fonts = new[] { "RobotoFlex.ttf", "Arial.ttf", "arial.ttf", "calibri.ttf", "segoeui.ttf" };
                var fontsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
                foreach (var font in fonts)
                {
                    var fontPath = Path.Combine(fontsFolder, font);

                    if (!File.Exists(fontPath)) continue;

                    fontSystem.AddFont(File.ReadAllBytes(fontPath));
                    Console.WriteLine($"[DefaultFontProvider] Loaded {font} from system fonts");
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DefaultFontProvider] WARNING: Could not load system font: {ex.Message}");
                Console.WriteLine("[DefaultFontProvider] FontSystem created but no fonts loaded");
            }

            Console.WriteLine("[DefaultFontProvider] FontSystem created successfully!");
            return fontSystem;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DefaultFontProvider] ERROR: Could not create FontSystem: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    ///     Gets a SpriteFontBase at the default size from a FontSystem.
    /// </summary>
    /// <param name="fontSystem">The FontSystem to get the font from</param>
    /// <param name="size">Font size in pixels (default: 22)</param>
    /// <returns>The font at the specified size</returns>
    public static SpriteFontBase? GetDefaultFont(FontSystem fontSystem, int size = DefaultFontSize)
    {
        try
        {
            return fontSystem.GetFont(size);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DefaultFontProvider] ERROR: Could not get font at size {size}: {ex.Message}");
            return null;
        }
    }
}