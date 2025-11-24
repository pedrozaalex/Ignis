namespace Ignis.Engine.Assets;

/// <summary>
///     Extension methods for AssetManager to handle common asset types
/// </summary>
public static class AssetManagerExtensions
{
    /// <summary>
    ///     Load a text file
    /// </summary>
    public static AssetHandle<TextAsset> LoadText(this AssetManager manager, string assetPath)
    {
        var handle = manager.Load<TextAsset>(assetPath);
        return handle;
    }

    /// <summary>
    ///     Load a binary file
    /// </summary>
    public static AssetHandle<BinaryAsset> LoadBinary(this AssetManager manager, string assetPath)
    {
        var handle = manager.Load<BinaryAsset>(assetPath);
        return handle;
    }

    /// <summary>
    ///     Load a JSON file
    /// </summary>
    public static AssetHandle<JsonAsset<T>> LoadJson<T>(this AssetManager manager, string assetPath)
        where T : class
    {
        var handle = manager.Load<JsonAsset<T>>(assetPath);
        return handle;
    }
}