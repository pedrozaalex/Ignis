namespace Ignis.Engine.Assets;

/// <summary>
/// Simple text asset for configuration files, JSON, etc.
/// </summary>
public class TextAsset : Asset
{
    /// <summary>
    /// The loaded text content
    /// </summary>
    public string Content { get; private set; } = string.Empty;
    
    /// <summary>
    /// Load text from file
    /// </summary>
    public void Load(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Text file not found: {filePath}");
            
        Content = File.ReadAllText(filePath);
    }
    
    public override void Dispose()
    {
        Content = string.Empty;
    }
}

/// <summary>
/// Binary asset for any raw file data
/// </summary>
public class BinaryAsset : Asset
{
    /// <summary>
    /// The loaded binary data
    /// </summary>
    public byte[] Data { get; private set; } = [];
    
    /// <summary>
    /// Load binary data from file
    /// </summary>
    public void Load(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Binary file not found: {filePath}");
            
        Data = File.ReadAllBytes(filePath);
    }
    
    public override void Dispose()
    {
        Data = [];
    }
}

/// <summary>
/// JSON asset with deserialization support
/// </summary>
public class JsonAsset<T> : Asset where T : class
{
    /// <summary>
    /// The deserialized data
    /// </summary>
    public T? Data { get; private set; }
    
    /// <summary>
    /// Load and deserialize JSON from file
    /// </summary>
    public void Load(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"JSON file not found: {filePath}");
            
        var json = File.ReadAllText(filePath);
        Data = System.Text.Json.JsonSerializer.Deserialize<T>(json);
        
        if (Data == null)
            throw new InvalidOperationException($"Failed to deserialize JSON from {filePath}");
    }
    
    public override void Dispose()
    {
        Data = null;
    }
}

