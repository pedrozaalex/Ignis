namespace Ignis.Core.Assets;

/// <summary>
/// Locates system fonts across Windows, Linux, and macOS.
/// </summary>
public static class SystemFontLocator
{
    /// <summary>
    /// Common system font paths by platform.
    /// </summary>
    private static readonly string[] WindowsFontPaths =
    [
        @"C:\Windows\Fonts\segoeui.ttf",
        @"C:\Windows\Fonts\arial.ttf",
        @"C:\Windows\Fonts\tahoma.ttf",
        @"C:\Windows\Fonts\verdana.ttf",
        @"C:\Windows\Fonts\calibri.ttf"
    ];

    private static readonly string[] LinuxFontPaths =
    [
        "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
        "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf",
        "/usr/share/fonts/truetype/freefont/FreeSans.ttf",
        "/usr/share/fonts/TTF/DejaVuSans.ttf"
    ];

    private static readonly string[] MacOSFontPaths =
    [
        "/System/Library/Fonts/Helvetica.ttc",
        "/System/Library/Fonts/SFNSText.ttf",
        "/Library/Fonts/Arial.ttf"
    ];

    /// <summary>
    /// Finds the first available system font path.
    /// </summary>
    /// <returns>Path to a system font, or null if none found.</returns>
    public static string? FindSystemFont()
    {
        var paths = GetPlatformFontPaths();
        foreach (var path in paths)
        {
            if (File.Exists(path))
                return path;
        }
        return null;
    }

    /// <summary>
    /// Finds all available system fonts.
    /// </summary>
    /// <returns>Enumerable of existing font paths.</returns>
    public static IEnumerable<string> FindAllSystemFonts()
    {
        var paths = GetPlatformFontPaths();
        foreach (var path in paths)
        {
            if (File.Exists(path))
                yield return path;
        }
    }

    /// <summary>
    /// Tries to find a font, checking custom paths first, then falling back to system fonts.
    /// </summary>
    /// <param name="customPaths">Optional custom paths to check first.</param>
    /// <returns>Path to a font, or null if none found.</returns>
    public static string? FindFont(params string[] customPaths)
    {
        foreach (var path in customPaths)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                return path;
        }
        return FindSystemFont();
    }

    private static string[] GetPlatformFontPaths()
    {
        return Window.CurrentPlatform switch
        {
            Platform.Windows => WindowsFontPaths,
            Platform.Linux => LinuxFontPaths,
            Platform.MacOS => MacOSFontPaths,
            _ => [.. WindowsFontPaths, .. LinuxFontPaths, .. MacOSFontPaths]
        };
    }
}
