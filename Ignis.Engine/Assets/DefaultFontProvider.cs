using System.Diagnostics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Engine.Assets;

/// <summary>
/// Provides automatic default font loading for the UI system.
/// Generates and builds a default Arial font if it doesn't exist.
/// </summary>
public static class DefaultFontProvider
{
    private const string DefaultFontName = "DefaultFont";
    private const string FontSpriteFont = "DefaultFont.spritefont";

    /// <summary>
    /// Ensures a default font exists and loads it.
    /// This is called automatically during IgnisGame.LoadContent().
    /// </summary>
    /// <param name="contentManager">MonoGame ContentManager to load from</param>
    /// <param name="contentPath">Path to Content directory</param>
    /// <returns>The loaded SpriteFont, or null if loading failed</returns>
    public static SpriteFont? EnsureAndLoadDefaultFont(ContentManager contentManager, string contentPath)
    {
        var fontSpriteFontPath = Path.Combine(contentPath, FontSpriteFont);
        var fontXnbPath = Path.Combine(contentPath, $"{DefaultFontName}.xnb");

        // Generate .spritefont if missing
        if (!File.Exists(fontSpriteFontPath))
        {
            Console.WriteLine("[DefaultFontProvider] Generating DefaultFont.spritefont...");
            GenerateDefaultFontFile(fontSpriteFontPath);
        }

        // Build .xnb if missing
        if (!File.Exists(fontXnbPath))
        {
            Console.WriteLine("[DefaultFontProvider] Building DefaultFont.xnb with MGCB...");
            var success = BuildFont(contentPath, FontSpriteFont);

            if (!success)
            {
                Console.WriteLine("[DefaultFontProvider] WARNING: Could not build default font automatically.");
                Console.WriteLine("[DefaultFontProvider] Text rendering may not work. Please build Content manually.");
                return null;
            }
        }

        // Load the font
        try
        {
            var font = contentManager.Load<SpriteFont>(DefaultFontName);
            font.DefaultCharacter = '?'; 
            Console.WriteLine("[DefaultFontProvider] Default font loaded successfully!");
            return font;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DefaultFontProvider] ERROR: Could not load default font: {ex.Message}");
            return null;
        }
    }

    private static void GenerateDefaultFontFile(string path)
    {
        var fontContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<XnaContent xmlns:Graphics=""Microsoft.Xna.Framework.Content.Pipeline.Graphics"">
  <Asset Type=""Graphics:FontDescription"">
    <FontName>Arial</FontName>
    <Size>16</Size>
    <Spacing>0</Spacing>
    <UseKerning>true</UseKerning>
    <Style>Regular</Style>
    <CharacterRegions>
      <CharacterRegion>
        <Start>&#32;</Start>
        <End>&#126;</End>
      </CharacterRegion>
      <!-- Add support for Geometric Shapes (Arrows, etc.) -->
      <CharacterRegion>
        <Start>&#9600;</Start>
        <End>&#9727;</End>
      </CharacterRegion>
    </CharacterRegions>
  </Asset>
</XnaContent>";
        
        // Ensure the directory exists before writing
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        File.WriteAllText(path, fontContent);
        Console.WriteLine($"[DefaultFontProvider] Created {path}");
    }

    private static bool BuildFont(string contentPath, string fontFileName)
    {
        var fontFilePath = Path.Combine(contentPath, fontFileName);

        if (!File.Exists(fontFilePath))
        {
            Console.WriteLine($"[DefaultFontProvider] Error: {fontFilePath} not found!");
            return false;
        }

        var mgcbPath = FindMgcb();

        if (string.IsNullOrEmpty(mgcbPath))
        {
            Console.WriteLine("[DefaultFontProvider] Warning: MGCB tool not found.");
            return false;
        }

        var outputDir = Path.Combine(contentPath, "bin", "DesktopGL");
        var intermediateDir = Path.Combine(contentPath, "obj", "DesktopGL");

        Directory.CreateDirectory(outputDir);
        Directory.CreateDirectory(intermediateDir);

        var args = new List<string>
        {
            $"/outputDir:\"{outputDir}\"",
            $"/intermediateDir:\"{intermediateDir}\"",
            "/platform:DesktopGL",
            "/profile:HiDef",
            $"/workingDir:\"{contentPath}\"",
            "/importer:FontDescriptionImporter",
            "/processor:FontDescriptionProcessor",
            "/processorParam:PremultiplyAlpha=True",
            "/processorParam:TextureFormat=Compressed",
            $"/build:\"{fontFileName}\""
        };

        var arguments = string.Join(" ", args);

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = mgcbPath,
                Arguments = arguments,
                WorkingDirectory = contentPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                Console.WriteLine("[DefaultFontProvider] Failed to start MGCB process");
                return false;
            }

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                var xnbFileName = Path.GetFileNameWithoutExtension(fontFileName) + ".xnb";
                var builtXnbPath = Path.Combine(outputDir, xnbFileName);
                var targetXnbPath = Path.Combine(contentPath, xnbFileName);

                if (!File.Exists(builtXnbPath)) return false;
                
                File.Copy(builtXnbPath, targetXnbPath, overwrite: true);

                // Also copy to runtime directory
                var executableDir = AppDomain.CurrentDomain.BaseDirectory;
                var runtimeContentDir = Path.Combine(executableDir, "Content");
                Directory.CreateDirectory(runtimeContentDir);

                var runtimeXnbPath = Path.Combine(runtimeContentDir, xnbFileName);
                File.Copy(builtXnbPath, runtimeXnbPath, overwrite: true);

                Console.WriteLine("[DefaultFontProvider] Font built and copied successfully");
                return true;
            }

            Console.WriteLine("[DefaultFontProvider] MGCB build failed:");
            Console.WriteLine(output);
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine(error);
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DefaultFontProvider] Error running MGCB: {ex.Message}");
            return false;
        }
    }

    private static string FindMgcb()
    {
        string[] possiblePaths =
        [
            "mgcb",
            "dotnet-mgcb",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet", "tools",
                "mgcb.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet", "tools",
                "dotnet-mgcb.exe")
        ];

        foreach (var path in possiblePaths)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = "/?",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);

                if (process == null) continue;

                process.WaitForExit();

                if (process.ExitCode is 0 or 1) return path;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[DefaultFontProvider] MGCB check failed for '{path}': {ex.Message}");
            }
        }

        return string.Empty;
    }
}