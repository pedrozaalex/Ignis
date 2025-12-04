namespace Ignis.Core.Tests.IO;

using Ignis.Core.IO;

public class FileSystemTests
{
    [Fact]
    public void MemoryFileSystem_WriteAndRead_RoundTrips()
    {
        var fs = new MemoryFileSystem();
        var data = new byte[] { 1, 2, 3, 4, 5 };
        
        fs.Write("test.bin", data);
        
        using var stream = fs.OpenRead("test.bin");
        var result = new byte[5];
        stream.ReadExactly(result);
        
        Assert.Equal(data, result);
    }
    
    [Fact]
    public void MemoryFileSystem_OpenRead_ThrowsForMissingFile()
    {
        var fs = new MemoryFileSystem();
        
        Assert.Throws<FileNotFoundException>(() => fs.OpenRead("nonexistent.bin"));
    }
    
    [Fact]
    public void MemoryFileSystem_Exists_ReturnsTrueForExistingFile()
    {
        var fs = new MemoryFileSystem();
        fs.Write("exists.bin", new byte[] { 1 });
        
        Assert.True(fs.Exists("exists.bin"));
        Assert.False(fs.Exists("missing.bin"));
    }
    
    [Fact]
    public void MemoryFileSystem_Delete_RemovesFile()
    {
        var fs = new MemoryFileSystem();
        fs.Write("temp.bin", new byte[] { 1 });
        Assert.True(fs.Exists("temp.bin"));
        
        fs.Delete("temp.bin");
        
        Assert.False(fs.Exists("temp.bin"));
    }
    
    [Fact]
    public void MemoryFileSystem_ReadAllBytes_ReturnsFileContents()
    {
        var fs = new MemoryFileSystem();
        var data = new byte[] { 10, 20, 30 };
        fs.Write("data.bin", data);
        
        var result = fs.ReadAllBytes("data.bin");
        
        Assert.Equal(data, result);
    }
    
    [Fact]
    public void MemoryFileSystem_ReadAllText_ReturnsStringContents()
    {
        var fs = new MemoryFileSystem();
        fs.WriteAllText("text.txt", "Hello, World!");
        
        var result = fs.ReadAllText("text.txt");
        
        Assert.Equal("Hello, World!", result);
    }
}

