using System.Collections.Concurrent;

namespace Ignis.Engine.Assets;

/// <summary>
///     Asset loading status
/// </summary>
public enum AssetStatus
{
    NotLoaded,
    Loading,
    Loaded,
    Failed
}

/// <summary>
///     Base class for all asset types
/// </summary>
public abstract class Asset : IDisposable
{
    /// <summary>
    ///     Reference count for memory management
    /// </summary>
    internal int ReferenceCount;

    /// <summary>
    ///     Unique asset path/identifier
    /// </summary>
    public string Path { get; internal set; } = string.Empty;

    /// <summary>
    ///     Current loading status
    /// </summary>
    public AssetStatus Status { get; internal set; } = AssetStatus.NotLoaded;

    /// <summary>
    ///     Error message if loading failed
    /// </summary>
    public string? ErrorMessage { get; internal set; }

    /// <summary>
    ///     Dispose of asset resources
    /// </summary>
    public abstract void Dispose();
}

/// <summary>
///     Handle to an asset that manages reference counting
/// </summary>
public class AssetHandle<T> : IDisposable where T : Asset
{
    private readonly AssetManager _manager;
    private T? _asset;
    private bool _disposed;

    internal AssetHandle(AssetManager manager, T asset)
    {
        _manager = manager;
        _asset = asset;
        Interlocked.Increment(ref asset.ReferenceCount);
    }

    public T Asset
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AssetHandle<T>));
            return _asset ?? throw new InvalidOperationException("Asset not loaded");
        }
    }

    public bool IsValid => _asset != null && _asset.Status == AssetStatus.Loaded;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_asset != null)
        {
            var newCount = Interlocked.Decrement(ref _asset.ReferenceCount);
            if (newCount <= 0) _manager.UnloadAsset(_asset.Path);
        }

        _asset = null;
    }
}

/// <summary>
///     Asset Manager - Centralized asset loading and caching system
///     Handles reference counting and automatic unloading
/// </summary>
public class AssetManager : IDisposable
{
    private readonly string _contentRoot;
    private readonly ConcurrentDictionary<string, Asset> _loadedAssets = new();

    public AssetManager(string contentRoot = "Content")
    {
        _contentRoot = contentRoot;
    }

    public void Dispose()
    {
        UnloadAll();
    }

    /// <summary>
    ///     Event triggered when an asset is loaded
    /// </summary>
    public event Action<string, Asset>? AssetLoaded;

    /// <summary>
    ///     Event triggered when an asset is unloaded
    /// </summary>
    public event Action<string>? AssetUnloaded;

    /// <summary>
    ///     Load an asset synchronously
    /// </summary>
    public AssetHandle<T> Load<T>(string assetPath) where T : Asset, new()
    {
        // Normalize path
        var normalizedPath = NormalizePath(assetPath);

        // Check if already loaded
        if (_loadedAssets.TryGetValue(normalizedPath, out var existingAsset))
        {
            if (existingAsset is T typedAsset) return new AssetHandle<T>(this, typedAsset);

            throw new InvalidOperationException(
                $"Asset '{normalizedPath}' already loaded as different type: {existingAsset.GetType().Name}");
        }

        // Create new asset
        var asset = new T
        {
            Path = normalizedPath,
            Status = AssetStatus.Loading
        };

        try
        {
            // Call the asset's Load method via reflection if it exists
            var loadMethod = typeof(T).GetMethod("Load", [typeof(string)]);
            if (loadMethod != null)
                loadMethod.Invoke(asset, [GetFullPath(normalizedPath)]);
            else
                // Fall back to LoadAssetImpl for custom loading logic
                LoadAssetImpl(asset, GetFullPath(normalizedPath));

            asset.Status = AssetStatus.Loaded;
            _loadedAssets[normalizedPath] = asset;

            AssetLoaded?.Invoke(normalizedPath, asset);

            return new AssetHandle<T>(this, asset);
        }
        catch (Exception ex)
        {
            asset.Status = AssetStatus.Failed;
            asset.ErrorMessage = ex.Message;
            throw new AssetLoadException($"Failed to load asset '{normalizedPath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    ///     Load an asset asynchronously
    /// </summary>
    public async Task<AssetHandle<T>> LoadAsync<T>(string assetPath) where T : Asset, new()
    {
        return await Task.Run(() => Load<T>(assetPath));
    }

    /// <summary>
    ///     Check if an asset is loaded
    /// </summary>
    public bool IsLoaded(string assetPath)
    {
        var normalizedPath = NormalizePath(assetPath);
        return _loadedAssets.TryGetValue(normalizedPath, out var asset)
               && asset.Status == AssetStatus.Loaded;
    }

    /// <summary>
    ///     Unload an asset (called automatically when reference count reaches 0)
    /// </summary>
    internal void UnloadAsset(string assetPath)
    {
        var normalizedPath = NormalizePath(assetPath);

        if (_loadedAssets.TryRemove(normalizedPath, out var asset))
        {
            asset.Dispose();
            AssetUnloaded?.Invoke(normalizedPath);
        }
    }

    /// <summary>
    ///     Force unload all assets
    /// </summary>
    public void UnloadAll()
    {
        foreach (var kvp in _loadedAssets)
        {
            kvp.Value.Dispose();
            AssetUnloaded?.Invoke(kvp.Key);
        }

        _loadedAssets.Clear();
    }

    /// <summary>
    ///     Get statistics about loaded assets
    /// </summary>
    public AssetStatistics GetStatistics()
    {
        return new AssetStatistics
        {
            TotalAssets = _loadedAssets.Count,
            AssetsByType = _loadedAssets.Values
                .GroupBy(a => a.GetType().Name)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    /// <summary>
    ///     Virtual method for loading specific asset types
    ///     Override in derived AssetManager classes or use extension methods
    /// </summary>
    protected virtual void LoadAssetImpl(Asset asset, string fullPath)
    {
        // Default implementation - assets should override their own loading logic
        throw new NotImplementedException(
            $"Asset type {asset.GetType().Name} must implement its own loading logic");
    }

    private string NormalizePath(string path)
    {
        return path.Replace('\\', '/').Trim('/');
    }

    private string GetFullPath(string normalizedPath)
    {
        return Path.Combine(_contentRoot, normalizedPath);
    }
}

/// <summary>
///     Asset loading exception
/// </summary>
public class AssetLoadException : Exception
{
    public AssetLoadException(string message) : base(message)
    {
    }

    public AssetLoadException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
///     Asset statistics
/// </summary>
public class AssetStatistics
{
    public int TotalAssets { get; set; }
    public Dictionary<string, int> AssetsByType { get; set; } = new();
}