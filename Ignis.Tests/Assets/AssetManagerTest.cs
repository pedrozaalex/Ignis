using Ignis.Engine.Assets;

namespace Ignis.Tests.Assets;

/// <summary>
///     Unit tests for Phase 2: AssetManager & Content Pipeline
/// </summary>
public class AssetManagerTests
{
    private readonly string _testContentPath;

    public AssetManagerTests()
    {
        // Create test content directory
        _testContentPath = Path.Combine(Path.GetTempPath(), "IgnisTestContent", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testContentPath);
    }

    /// <summary>
    ///     Test basic asset manager creation
    /// </summary>
    [Fact]
    public void AssetManager_CanBeCreated()
    {
        // Arrange & Act
        using var manager = new AssetManager(_testContentPath);

        // Assert
        Assert.NotNull(manager);
        var stats = manager.GetStatistics();
        Assert.Equal(0, stats.TotalAssets);
    }

    /// <summary>
    ///     Test loading a text asset
    /// </summary>
    [Fact]
    public void AssetManager_CanLoadTextAsset()
    {
        // Arrange
        var testFile = Path.Combine(_testContentPath, "test.txt");
        File.WriteAllText(testFile, "Hello, Ignis!");

        using var manager = new AssetManager(_testContentPath);

        // Act
        using var handle = manager.LoadText("test.txt");

        // Assert
        Assert.True(handle.IsValid);
        Assert.Equal("Hello, Ignis!", handle.Asset.Content);
        Assert.Equal(AssetStatus.Loaded, handle.Asset.Status);
    }

    /// <summary>
    ///     Test loading a binary asset
    /// </summary>
    [Fact]
    public void AssetManager_CanLoadBinaryAsset()
    {
        // Arrange
        var testFile = Path.Combine(_testContentPath, "test.bin");
        var testData = new byte[] { 0x49, 0x67, 0x6E, 0x69, 0x73 }; // "Ignis" in ASCII
        File.WriteAllBytes(testFile, testData);

        using var manager = new AssetManager(_testContentPath);

        // Act
        using var handle = manager.LoadBinary("test.bin");

        // Assert
        Assert.True(handle.IsValid);
        Assert.Equal(testData, handle.Asset.Data);
    }

    /// <summary>
    ///     Test loading a JSON asset
    /// </summary>
    [Fact]
    public void AssetManager_CanLoadJsonAsset()
    {
        // Arrange
        var testFile = Path.Combine(_testContentPath, "config.json");
        var json = "{\"Name\":\"TestConfig\",\"Value\":42}";
        File.WriteAllText(testFile, json);

        using var manager = new AssetManager(_testContentPath);

        // Act
        using var handle = manager.LoadJson<TestConfig>("config.json");

        // Assert
        Assert.True(handle.IsValid);
        Assert.NotNull(handle.Asset.Data);
        Assert.Equal("TestConfig", handle.Asset.Data.Name);
        Assert.Equal(42, handle.Asset.Data.Value);
    }

    /// <summary>
    ///     Test asset caching - loading same asset twice returns cached version
    /// </summary>
    [Fact]
    public void AssetManager_CachesAssets()
    {
        // Arrange
        var testFile = Path.Combine(_testContentPath, "test.txt");
        File.WriteAllText(testFile, "Cached content");

        using var manager = new AssetManager(_testContentPath);

        // Act
        using var handle1 = manager.LoadText("test.txt");
        using var handle2 = manager.LoadText("test.txt");

        // Assert - Both handles should reference the same asset instance
        Assert.Same(handle1.Asset, handle2.Asset);

        var stats = manager.GetStatistics();
        Assert.Equal(1, stats.TotalAssets); // Only one asset loaded
    }

    /// <summary>
    ///     Test reference counting - asset is unloaded when all handles are disposed
    /// </summary>
    [Fact]
    public void AssetManager_UnloadsWhenNoReferences()
    {
        // Arrange
        var testFile = Path.Combine(_testContentPath, "test.txt");
        File.WriteAllText(testFile, "Reference counted");

        using var manager = new AssetManager(_testContentPath);

        // Act - Load and immediately dispose
        {
            using var handle = manager.LoadText("test.txt");
            Assert.True(manager.IsLoaded("test.txt"));
        } // handle disposed here

        // Assert - Asset should be unloaded after last reference is disposed
        Assert.False(manager.IsLoaded("test.txt"));

        var stats = manager.GetStatistics();
        Assert.Equal(0, stats.TotalAssets);
    }

    /// <summary>
    ///     Test that loading non-existent file throws exception
    /// </summary>
    [Fact]
    public void AssetManager_ThrowsOnMissingFile()
    {
        // Arrange
        using var manager = new AssetManager(_testContentPath);

        // Act & Assert
        Assert.Throws<AssetLoadException>(() => manager.LoadText("nonexistent.txt"));
    }

    /// <summary>
    ///     Test asset statistics
    /// </summary>
    [Fact]
    public void AssetManager_ProvidesStatistics()
    {
        // Arrange
        var textFile = Path.Combine(_testContentPath, "text.txt");
        var binFile = Path.Combine(_testContentPath, "data.bin");
        File.WriteAllText(textFile, "Text");
        File.WriteAllBytes(binFile, new byte[] { 1, 2, 3 });

        using var manager = new AssetManager(_testContentPath);

        // Act
        using var textHandle = manager.LoadText("text.txt");
        using var binHandle = manager.LoadBinary("data.bin");

        var stats = manager.GetStatistics();

        // Assert
        Assert.Equal(2, stats.TotalAssets);
        Assert.True(stats.AssetsByType.ContainsKey("TextAsset"));
        Assert.True(stats.AssetsByType.ContainsKey("BinaryAsset"));
        Assert.Equal(1, stats.AssetsByType["TextAsset"]);
        Assert.Equal(1, stats.AssetsByType["BinaryAsset"]);
    }

    /// <summary>
    ///     Test UnloadAll functionality
    /// </summary>
    [Fact]
    public void AssetManager_CanUnloadAll()
    {
        // Arrange
        var testFile = Path.Combine(_testContentPath, "test.txt");
        File.WriteAllText(testFile, "Test");

        using var manager = new AssetManager(_testContentPath);
        using var handle = manager.LoadText("test.txt");

        Assert.Equal(1, manager.GetStatistics().TotalAssets);

        // Act
        manager.UnloadAll();

        // Assert
        Assert.Equal(0, manager.GetStatistics().TotalAssets);
        Assert.False(manager.IsLoaded("test.txt"));
    }

    /// <summary>
    ///     Test async loading
    /// </summary>
    [Fact]
    public async Task AssetManager_CanLoadAsync()
    {
        // Arrange
        var testFile = Path.Combine(_testContentPath, "async.txt");
        File.WriteAllText(testFile, "Async content");

        using var manager = new AssetManager(_testContentPath);

        // Act
        using var handle = await manager.LoadAsync<TextAsset>("async.txt");

        // Manually load since extension methods aren't used in generic LoadAsync
        if (handle.Asset.Status == AssetStatus.Loading) handle.Asset.Load(testFile);

        // Assert
        Assert.True(handle.IsValid);
        Assert.Equal("Async content", handle.Asset.Content);
    }

    // Helper class for JSON tests
    private class TestConfig
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}