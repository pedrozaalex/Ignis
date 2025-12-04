using System;

namespace Ignis.Graphics;

/// <summary>
/// Null rendering server for testing and headless operation.
/// All operations are no-ops but track resource creation.
/// </summary>
public class NullRenderingServer : IRenderingServer
{
    private int _nextMeshId = 1;
    private int _nextTextureId = 1;
    private int _nextShaderId = 1;
    private int _nextFontId = 1;
    private int _nextRenderTargetId = 1;
    
    public int Width { get; private set; }
    public int Height { get; private set; }
    
    public ShaderHandle DefaultShader3D { get; private set; }
    public ShaderHandle DefaultShader2D { get; private set; }
    public ShaderHandle DefaultShaderText { get; private set; }
    
    public RenderCapabilities Capabilities { get; } = new()
    {
        BackendName = "Null",
        MaxTextureSize = 16384,
        MaxTextureSlots = 16,
        SupportsCompute = false,
        SupportsGeometryShaders = false,
        SupportsTessellation = false,
        SupportsInstancing = true,
        SupportsMRT = true,
        MaxMSAASamples = 8,
        SupportsAnisotropicFiltering = true,
        MaxAnisotropy = 16
    };
    
    public void Initialize(IntPtr windowHandle, int width, int height)
    {
        Width = width;
        Height = height;
        
        // Create default shaders
        DefaultShader3D = new ShaderHandle(_nextShaderId++);
        DefaultShader2D = new ShaderHandle(_nextShaderId++);
        DefaultShaderText = new ShaderHandle(_nextShaderId++);
    }
    
    public void Resize(int width, int height)
    {
        Width = width;
        Height = height;
    }
    
    // --- Meshes ---
    
    public MeshHandle CreateMesh(MeshData data) => new(_nextMeshId++);
    public void UpdateMesh(MeshHandle handle, MeshData data) { }
    public void DestroyMesh(MeshHandle handle) { }
    
    // --- Textures ---
    
    public TextureHandle CreateTexture(ReadOnlySpan<byte> pixelData, TextureDesc desc) => new(_nextTextureId++);
    public TextureHandle CreateTextureFromFile(string path) => new(_nextTextureId++);
    public void UpdateTexture(TextureHandle handle, ReadOnlySpan<byte> pixelData, int x, int y, int width, int height) { }
    public void DestroyTexture(TextureHandle handle) { }
    
    // --- Shaders ---
    
    public ShaderHandle CreateShader(string vertexSource, string fragmentSource) => new(_nextShaderId++);
    public ShaderHandle CreateShaderFromBytecode(ReadOnlySpan<byte> vertexBytecode, ReadOnlySpan<byte> fragmentBytecode) => new(_nextShaderId++);
    public void DestroyShader(ShaderHandle handle) { }
    
    // --- Fonts ---
    
    public FontHandle CreateFont(string name, ReadOnlySpan<byte> ttfData) => new(_nextFontId++);
    public FontHandle CreateFontFromFile(string path) => new(_nextFontId++);
    public void DestroyFont(FontHandle handle) { }
    public (float width, float height) MeasureText(FontHandle font, string text, float fontSize) => (text.Length * fontSize * 0.6f, fontSize);
    
    // --- Render Targets ---
    
    public RenderTargetHandle CreateRenderTarget(RenderTargetDesc desc) => new(_nextRenderTargetId++);
    public TextureHandle GetRenderTargetTexture(RenderTargetHandle handle) => new(handle.Id);
    public void DestroyRenderTarget(RenderTargetHandle handle) { }
    
    // --- Command Lists ---
    
    public IRenderCommandList CreateCommandList() => new RenderCommandList();
    public void Submit(IRenderCommandList commands) { }
    
    // --- Frame Control ---
    
    public void BeginPass(RenderPass pass) { }
    public void EndPass() { }
    public void SwapBuffers() { }
    
    public void Dispose() { }
}

