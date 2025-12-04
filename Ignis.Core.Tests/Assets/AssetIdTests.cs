namespace Ignis.Core.Tests.Assets;

using Ignis.Core.Assets;

public class AssetIdTests
{
    [Fact]
    public void FromPath_CreatesDeterministicId()
    {
        var id1 = AssetId.FromPath("textures/player.png");
        var id2 = AssetId.FromPath("textures/player.png");
        
        Assert.Equal(id1, id2);
    }
    
    [Fact]
    public void FromPath_DifferentPaths_CreateDifferentIds()
    {
        var id1 = AssetId.FromPath("textures/player.png");
        var id2 = AssetId.FromPath("textures/enemy.png");
        
        Assert.NotEqual(id1, id2);
    }
    
    [Fact]
    public void Empty_IsDefault()
    {
        var empty = AssetId.Empty;
        var defaultId = default(AssetId);
        
        Assert.Equal(empty, defaultId);
    }
    
    [Fact]
    public void IsEmpty_ReturnsTrueForDefault()
    {
        Assert.True(AssetId.Empty.IsEmpty);
        Assert.True(default(AssetId).IsEmpty);
    }
    
    [Fact]
    public void IsEmpty_ReturnsFalseForValidId()
    {
        var id = AssetId.FromPath("some/path.txt");
        
        Assert.False(id.IsEmpty);
    }
    
    [Fact]
    public void ToString_ReturnsReadableString()
    {
        var id = AssetId.FromPath("textures/player.png");
        
        var str = id.ToString();
        
        Assert.NotNull(str);
        Assert.NotEmpty(str);
    }
    
    [Fact]
    public void GetHashCode_ConsistentForEqualIds()
    {
        var id1 = AssetId.FromPath("test.png");
        var id2 = AssetId.FromPath("test.png");
        
        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }
}

