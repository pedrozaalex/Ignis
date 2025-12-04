namespace Ignis.Core.Assets;

/// <summary>
/// Loads raw data into usable objects.
/// </summary>
public interface IAssetProvider
{
    /// <summary>Load an asset by ID. Returns cached instance if already loaded.</summary>
    T Load<T>(AssetId id) where T : class;
    
    /// <summary>Unload an asset, freeing resources.</summary>
    void Unload(AssetId id);
    
    /// <summary>Check if an asset is currently loaded.</summary>
    bool IsLoaded(AssetId id);
    
    /// <summary>Unload all assets.</summary>
    void UnloadAll();
}

