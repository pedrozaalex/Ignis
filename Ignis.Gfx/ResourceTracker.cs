namespace Ignis.Gfx;

/// <summary>
/// Tracks GPU resource handles and manages their lifetimes.
/// Provides automatic cleanup via Dispose pattern.
/// </summary>
public class ResourceTracker : IDisposable
{
    private readonly IRenderingServer _server;
    private readonly HashSet<MeshHandle> _meshes = new();
    private readonly HashSet<TextureHandle> _textures = new();
    private readonly HashSet<ShaderHandle> _shaders = new();
    private readonly HashSet<FontHandle> _fonts = new();
    private readonly HashSet<RenderTargetHandle> _renderTargets = new();
    private bool _disposed;
    
    public ResourceTracker(IRenderingServer server)
    {
        _server = server;
    }
    
    // --- Mesh tracking ---
    
    public MeshHandle CreateMesh(MeshData data)
    {
        var handle = _server.CreateMesh(data);
        _meshes.Add(handle);
        return handle;
    }
    
    public void ReleaseMesh(MeshHandle handle)
    {
        if (_meshes.Remove(handle))
            _server.DestroyMesh(handle);
    }
    
    // --- Texture tracking ---
    
    public TextureHandle CreateTexture(ReadOnlySpan<byte> pixelData, TextureDesc desc)
    {
        var handle = _server.CreateTexture(pixelData, desc);
        _textures.Add(handle);
        return handle;
    }
    
    public TextureHandle CreateTextureFromFile(string path)
    {
        var handle = _server.CreateTextureFromFile(path);
        _textures.Add(handle);
        return handle;
    }
    
    public void ReleaseTexture(TextureHandle handle)
    {
        if (_textures.Remove(handle))
            _server.DestroyTexture(handle);
    }
    
    // --- Shader tracking ---
    
    public ShaderHandle CreateShader(string vertexSource, string fragmentSource)
    {
        var handle = _server.CreateShader(vertexSource, fragmentSource);
        _shaders.Add(handle);
        return handle;
    }
    
    public void ReleaseShader(ShaderHandle handle)
    {
        if (_shaders.Remove(handle))
            _server.DestroyShader(handle);
    }
    
    // --- Font tracking ---
    
    public FontHandle CreateFont(string name, ReadOnlySpan<byte> ttfData)
    {
        var handle = _server.CreateFont(name, ttfData);
        _fonts.Add(handle);
        return handle;
    }
    
    public FontHandle CreateFontFromFile(string path)
    {
        var handle = _server.CreateFontFromFile(path);
        _fonts.Add(handle);
        return handle;
    }
    
    public void ReleaseFont(FontHandle handle)
    {
        if (_fonts.Remove(handle))
            _server.DestroyFont(handle);
    }
    
    // --- Render target tracking ---
    
    public RenderTargetHandle CreateRenderTarget(RenderTargetDesc desc)
    {
        var handle = _server.CreateRenderTarget(desc);
        _renderTargets.Add(handle);
        return handle;
    }
    
    public void ReleaseRenderTarget(RenderTargetHandle handle)
    {
        if (_renderTargets.Remove(handle))
            _server.DestroyRenderTarget(handle);
    }
    
    // --- Statistics ---
    
    public int MeshCount => _meshes.Count;
    public int TextureCount => _textures.Count;
    public int ShaderCount => _shaders.Count;
    public int FontCount => _fonts.Count;
    public int RenderTargetCount => _renderTargets.Count;
    public int TotalResourceCount => MeshCount + TextureCount + ShaderCount + FontCount + RenderTargetCount;
    
    // --- Disposal ---
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        // Release all tracked resources
        foreach (var handle in _meshes)
            _server.DestroyMesh(handle);
        _meshes.Clear();
        
        foreach (var handle in _textures)
            _server.DestroyTexture(handle);
        _textures.Clear();
        
        foreach (var handle in _shaders)
            _server.DestroyShader(handle);
        _shaders.Clear();
        
        foreach (var handle in _fonts)
            _server.DestroyFont(handle);
        _fonts.Clear();
        
        foreach (var handle in _renderTargets)
            _server.DestroyRenderTarget(handle);
        _renderTargets.Clear();
        
        GC.SuppressFinalize(this);
    }
    
    ~ResourceTracker()
    {
        Dispose();
    }
}

