using System.Text.Json;
using Ignis.Engine.Assets;
using Ignis.Engine.Core;

namespace Ignis.Samples;

/// <summary>
///     Sample demonstrating Phase 2: AssetManager functionality
///     Shows loading and managing different asset types
/// </summary>
public static class AssetSample
{
    public static void Run()
    {
        Console.WriteLine("=== Ignis Engine - Phase 2: Asset Manager Sample ===\n");

        // Create an Ignis app
        var app = new IgnisApp(new EngineSettings
        {
            WindowTitle = "Asset Manager Sample"
        });

        app.Initialize();

        // Create test assets in Content directory
        SetupTestAssets();

        try
        {
            // Demonstrate text asset loading
            DemoTextAsset(app);

            // Demonstrate binary asset loading
            DemoBinaryAsset(app);

            // Demonstrate JSON asset loading
            DemoJsonAsset(app);

            // Demonstrate asset caching
            DemoAssetCaching(app);

            // Show statistics
            ShowStatistics(app);
        }
        finally
        {
            // Cleanup
            CleanupTestAssets();
        }

        Console.WriteLine("\n=== Sample Complete ===");
    }

    private static void SetupTestAssets()
    {
        Directory.CreateDirectory("Content");

        // Create a text file
        File.WriteAllText("Content/readme.txt",
            "Welcome to Ignis Engine!\nThis is a test text asset.");

        // Create a binary file
        File.WriteAllBytes("Content/data.bin",
            [0x49, 0x67, 0x6E, 0x42, 0x73, 0x00, 0x01, 0x02]);

        // Create a JSON file
        var gameConfig = new GameConfig
        {
            GameName = "Ignis Adventure",
            Version = "1.0.0",
            Settings = new GameSettings
            {
                Fullscreen = false,
                VSync = true
            }
        };
        var jsonContent = JsonSerializer.Serialize(gameConfig, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText("Content/config.json", jsonContent);
    }

    private static void DemoTextAsset(IgnisApp app)
    {
        Console.WriteLine("--- Text Asset Loading ---");

        using var textHandle = app.AssetManager.LoadText("readme.txt");

        Console.WriteLine($"Loaded: {textHandle.Asset.Path}");
        Console.WriteLine($"Status: {textHandle.Asset.Status}");
        Console.WriteLine($"Content:\n{textHandle.Asset.Content}\n");
    }

    private static void DemoBinaryAsset(IgnisApp app)
    {
        Console.WriteLine("--- Binary Asset Loading ---");

        using var binHandle = app.AssetManager.LoadBinary("data.bin");

        Console.WriteLine($"Loaded: {binHandle.Asset.Path}");
        Console.WriteLine($"Size: {binHandle.Asset.Data.Length} bytes");
        Console.WriteLine($"Data: {BitConverter.ToString(binHandle.Asset.Data)}\n");
    }

    private static void DemoJsonAsset(IgnisApp app)
    {
        Console.WriteLine("--- JSON Asset Loading ---");

        using var jsonHandle = app.AssetManager.LoadJson<GameConfig>("config.json");

        if (jsonHandle.Asset.Data != null)
        {
            var config = jsonHandle.Asset.Data;
            Console.WriteLine($"Game Name: {config.GameName}");
            Console.WriteLine($"Version: {config.Version}");

            if (config.Settings != null)
            {
                Console.WriteLine($"Fullscreen: {config.Settings.Fullscreen}");
                Console.WriteLine($"VSync: {config.Settings.VSync}");
            }
        }

        Console.WriteLine();
    }

    private static void DemoAssetCaching(IgnisApp app)
    {
        Console.WriteLine("--- Asset Caching Demo ---");

        Console.WriteLine("Loading readme.txt for the second time...");
        using var handle1 = app.AssetManager.LoadText("readme.txt");
        using var handle2 = app.AssetManager.LoadText("readme.txt");

        var isSameAsset = ReferenceEquals(handle1.Asset, handle2.Asset);
        Console.WriteLine($"Same asset instance: {isSameAsset}");
        Console.WriteLine("Reference count is managed automatically\n");
    }

    private static void ShowStatistics(IgnisApp app)
    {
        Console.WriteLine("--- Asset Manager Statistics ---");

        var stats = app.AssetManager.GetStatistics();
        Console.WriteLine($"Total Assets Loaded: {stats.TotalAssets}");

        foreach (var kvp in stats.AssetsByType) Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
    }

    private static void CleanupTestAssets()
    {
        try
        {
            if (Directory.Exists("Content")) Directory.Delete("Content", true);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    // Helper classes for JSON deserialization
    private class GameConfig
    {
        public string GameName { get; init; } = string.Empty;
        public string Version { get; init; } = string.Empty;
        public GameSettings? Settings { get; init; }
    }

    private class GameSettings
    {
        public bool Fullscreen { get; init; }
        public bool VSync { get; init; }
    }
}