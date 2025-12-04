namespace Ignis.Gfx;

/// <summary>
/// Main rendering server interface. Manages GPU resources and command execution.
/// Each backend (DX11, Vulkan, Metal, etc.) implements this interface.
/// </summary>
public interface IRenderingServer : IDisposable
{
    // --- Initialization ---
    
    /// <summary>Initialize the rendering server with the given window.</summary>
    void Initialize(IntPtr windowHandle, int width, int height);
    
    /// <summary>Handle window resize.</summary>
    void Resize(int width, int height);
    
    /// <summary>Current backbuffer width.</summary>
    int Width { get; }
    
    /// <summary>Current backbuffer height.</summary>
    int Height { get; }
    
    // --- Resource Management: Meshes ---
    
    /// <summary>Upload mesh data to GPU.</summary>
    MeshHandle CreateMesh(MeshData data);
    
    /// <summary>Update existing mesh data (for dynamic meshes).</summary>
    void UpdateMesh(MeshHandle handle, MeshData data);
    
    /// <summary>Release mesh resources.</summary>
    void DestroyMesh(MeshHandle handle);
    
    // --- Resource Management: Textures ---
    
    /// <summary>Create texture from raw pixel data.</summary>
    TextureHandle CreateTexture(ReadOnlySpan<byte> pixelData, TextureDesc desc);
    
    /// <summary>Create texture from file path.</summary>
    TextureHandle CreateTextureFromFile(string path);
    
    /// <summary>Update texture data (for dynamic textures).</summary>
    void UpdateTexture(TextureHandle handle, ReadOnlySpan<byte> pixelData, int x, int y, int width, int height);
    
    /// <summary>Release texture resources.</summary>
    void DestroyTexture(TextureHandle handle);
    
    // --- Resource Management: Shaders ---
    
    /// <summary>Create shader from source code.</summary>
    ShaderHandle CreateShader(string vertexSource, string fragmentSource);
    
    /// <summary>Create shader from precompiled bytecode.</summary>
    ShaderHandle CreateShaderFromBytecode(ReadOnlySpan<byte> vertexBytecode, ReadOnlySpan<byte> fragmentBytecode);
    
    /// <summary>Release shader resources.</summary>
    void DestroyShader(ShaderHandle handle);
    
    // --- Resource Management: Fonts ---
    
    /// <summary>Create font from TTF data.</summary>
    FontHandle CreateFont(string name, ReadOnlySpan<byte> ttfData);
    
    /// <summary>Create font from file path.</summary>
    FontHandle CreateFontFromFile(string path);
    
    /// <summary>Release font resources.</summary>
    void DestroyFont(FontHandle handle);
    
    /// <summary>Measure text dimensions for layout.</summary>
    (float width, float height) MeasureText(FontHandle font, string text, float fontSize);
    
    // --- Resource Management: Render Targets ---
    
    /// <summary>Create an offscreen render target.</summary>
    RenderTargetHandle CreateRenderTarget(RenderTargetDesc desc);
    
    /// <summary>Get the texture associated with a render target (for reading it as input).</summary>
    TextureHandle GetRenderTargetTexture(RenderTargetHandle handle);
    
    /// <summary>Release render target resources.</summary>
    void DestroyRenderTarget(RenderTargetHandle handle);
    
    // --- Command List Management ---
    
    /// <summary>Create a new command list for recording.</summary>
    IRenderCommandList CreateCommandList();
    
    /// <summary>Submit a command list for execution.</summary>
    void Submit(IRenderCommandList commands);
    
    // --- Frame Control ---
    
    /// <summary>Begin a render pass (binds target, clears buffers).</summary>
    void BeginPass(RenderPass pass);
    
    /// <summary>End the current render pass.</summary>
    void EndPass();
    
    /// <summary>Present the frame to the screen.</summary>
    void SwapBuffers();
    
    // --- Built-in Shaders ---
    
    /// <summary>Default shader for 3D lit geometry.</summary>
    ShaderHandle DefaultShader3D { get; }
    
    /// <summary>Default shader for unlit 2D sprites/UI.</summary>
    ShaderHandle DefaultShader2D { get; }
    
    /// <summary>Default shader for text rendering.</summary>
    ShaderHandle DefaultShaderText { get; }
    
    // --- Capabilities Query ---
    
    /// <summary>Query backend capabilities.</summary>
    RenderCapabilities Capabilities { get; }
}

/// <summary>Backend rendering capabilities.</summary>
public struct RenderCapabilities
{
    /// <summary>Backend name (e.g., "DirectX11", "Vulkan").</summary>
    public string BackendName;
    
    /// <summary>Maximum texture dimension.</summary>
    public int MaxTextureSize;
    
    /// <summary>Maximum number of texture slots.</summary>
    public int MaxTextureSlots;
    
    /// <summary>Supports compute shaders.</summary>
    public bool SupportsCompute;
    
    /// <summary>Supports geometry shaders.</summary>
    public bool SupportsGeometryShaders;
    
    /// <summary>Supports tessellation.</summary>
    public bool SupportsTessellation;
    
    /// <summary>Supports hardware instancing.</summary>
    public bool SupportsInstancing;
    
    /// <summary>Supports multiple render targets.</summary>
    public bool SupportsMRT;
    
    /// <summary>Maximum MSAA sample count.</summary>
    public int MaxMSAASamples;
    
    /// <summary>Supports anisotropic filtering.</summary>
    public bool SupportsAnisotropicFiltering;
    
    /// <summary>Maximum anisotropy level.</summary>
    public int MaxAnisotropy;
}

