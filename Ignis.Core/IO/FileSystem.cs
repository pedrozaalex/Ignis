using System.Text;

namespace Ignis.Core.IO;

/// <summary>
/// Abstracts file system operations for testability and platform independence.
/// </summary>
public interface IFileSystem
{
    Stream OpenRead(string path);
    void Write(string path, byte[] data);
    bool Exists(string path);
    void Delete(string path);
    byte[] ReadAllBytes(string path);
    string ReadAllText(string path);
    void WriteAllText(string path, string content);
}

/// <summary>
/// In-memory file system for testing. Never touches the hard drive.
/// </summary>
public sealed class MemoryFileSystem : IFileSystem
{
    private readonly Dictionary<string, byte[]> _files = new();
    
    public Stream OpenRead(string path)
    {
        if (!_files.TryGetValue(NormalizePath(path), out var data))
            throw new FileNotFoundException($"File not found: {path}", path);
        
        return new MemoryStream(data, writable: false);
    }
    
    public void Write(string path, byte[] data)
    {
        _files[NormalizePath(path)] = data.ToArray(); // Copy to prevent external modification
    }
    
    public bool Exists(string path)
    {
        return _files.ContainsKey(NormalizePath(path));
    }
    
    public void Delete(string path)
    {
        _files.Remove(NormalizePath(path));
    }
    
    public byte[] ReadAllBytes(string path)
    {
        if (!_files.TryGetValue(NormalizePath(path), out var data))
            throw new FileNotFoundException($"File not found: {path}", path);
        
        return data.ToArray();
    }
    
    public string ReadAllText(string path)
    {
        return Encoding.UTF8.GetString(ReadAllBytes(path));
    }
    
    public void WriteAllText(string path, string content)
    {
        Write(path, Encoding.UTF8.GetBytes(content));
    }
    
    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/').ToLowerInvariant();
    }
}

/// <summary>
/// Real file system implementation using System.IO.
/// </summary>
public sealed class PhysicalFileSystem : IFileSystem
{
    private readonly string _basePath;
    
    public PhysicalFileSystem(string basePath = "")
    {
        _basePath = basePath;
    }
    
    public Stream OpenRead(string path)
    {
        return File.OpenRead(ResolvePath(path));
    }
    
    public void Write(string path, byte[] data)
    {
        var fullPath = ResolvePath(path);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        
        File.WriteAllBytes(fullPath, data);
    }
    
    public bool Exists(string path)
    {
        return File.Exists(ResolvePath(path));
    }
    
    public void Delete(string path)
    {
        File.Delete(ResolvePath(path));
    }
    
    public byte[] ReadAllBytes(string path)
    {
        return File.ReadAllBytes(ResolvePath(path));
    }
    
    public string ReadAllText(string path)
    {
        return File.ReadAllText(ResolvePath(path));
    }
    
    public void WriteAllText(string path, string content)
    {
        var fullPath = ResolvePath(path);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        
        File.WriteAllText(fullPath, content);
    }
    
    private string ResolvePath(string path)
    {
        return string.IsNullOrEmpty(_basePath) 
            ? path 
            : Path.Combine(_basePath, path);
    }
}

