using System.Security.Cryptography;
using System.Text;

namespace Ignis.Core.Assets;

/// <summary>
/// Lightweight handle to a resource. Prevents passing around strings.
/// </summary>
public readonly struct AssetId : IEquatable<AssetId>
{
    private readonly Guid _id;
    
    private AssetId(Guid id)
    {
        _id = id;
    }
    
    /// <summary>Empty/invalid asset ID.</summary>
    public static AssetId Empty => default;
    
    /// <summary>True if this is an empty/default ID.</summary>
    public bool IsEmpty => _id == Guid.Empty;
    
    /// <summary>
    /// Creates a deterministic AssetId from a path string.
    /// Same path always produces the same ID.
    /// </summary>
    public static AssetId FromPath(string path)
    {
        var normalized = path.Replace('\\', '/').ToLowerInvariant();
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        // Use first 16 bytes to create a GUID
        return new AssetId(new Guid(bytes.AsSpan(0, 16)));
    }
    
    /// <summary>
    /// Creates an AssetId from a pre-existing GUID.
    /// </summary>
    public static AssetId FromGuid(Guid guid)
    {
        return new AssetId(guid);
    }
    
    public bool Equals(AssetId other) => _id.Equals(other._id);
    public override bool Equals(object? obj) => obj is AssetId other && Equals(other);
    public override int GetHashCode() => _id.GetHashCode();
    public override string ToString() => _id.ToString("N")[..8]; // Short form for readability
    
    public static bool operator ==(AssetId left, AssetId right) => left.Equals(right);
    public static bool operator !=(AssetId left, AssetId right) => !left.Equals(right);
}

