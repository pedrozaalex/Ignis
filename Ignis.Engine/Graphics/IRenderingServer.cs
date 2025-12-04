using System.Numerics;

namespace Ignis.Engine.Graphics;

/// <summary>
/// Interface for recording rendering commands.
/// Commands are recorded onto this list and later submitted to the rendering server.
/// </summary>
public interface IRenderCommandList
{
    // --- State Management ---
    
    /// <summary>
    /// Sets the active shader/pipeline.
    /// </summary>
    void SetPipeline(ShaderHandle shader);
    
    /// <summary>
    /// Binds a texture to a slot (0 = Diffuse/Sprite, 1 = Normal, etc.).
    /// </summary>
    void SetTexture(int slot, TextureHandle texture);
    
    /// <summary>
    /// Sets a scissor rectangle for clipping (e.g., scroll views).
    /// </summary>
    void SetScissorRect(RectF rect);
    
    /// <summary>
    /// Disables scissor testing.
    /// </summary>
    void DisableScissor();
    
    /// <summary>
    /// Sets the blend mode for subsequent draw calls.
    /// </summary>
    void SetBlendMode(BlendMode mode);
    
    /// <summary>
    /// Sets the projection matrix (perspective for 3D, orthographic for UI).
    /// </summary>
    void SetProjectionMatrix(Matrix4x4 matrix);
    
    /// <summary>
    /// Sets the view/camera matrix.
    /// </summary>
    void SetViewMatrix(Matrix4x4 matrix);

    // --- Drawing Commands ---

    /// <summary>
    /// Draws a 3D mesh with the specified world transform.
    /// </summary>
    void DrawMesh(MeshHandle mesh, Matrix4x4 worldMatrix);

    /// <summary>
    /// Draws a sprite (textured quad).
    /// </summary>
    void DrawSprite(TextureHandle texture, Vector2 position, Vector2 size, Color4 tint, float rotationRad = 0);
    
    /// <summary>
    /// Draws a region of a texture (for texture atlases).
    /// </summary>
    void DrawSpriteRegion(TextureHandle texture, RectF srcRegion, Vector2 position, Vector2 size, Color4 tint);

    /// <summary>
    /// Draws text using the specified font.
    /// </summary>
    void DrawText(FontHandle font, string text, Vector2 position, float fontSize, Color4 color);

    /// <summary>
    /// Draws a colored quad (for UI primitives).
    /// </summary>
    void DrawQuad(Vector2 position, Vector2 size, Color4 color);
}

/// <summary>
/// The rendering server interface - manages GPU resources and command submission.
/// This is the main abstraction layer over the actual graphics backend.
/// </summary>
public interface IRenderingServer : IDisposable
{
    /// <summary>
    /// Initialize the rendering server with the window handle and initial size.
    /// </summary>
    void Initialize(nint windowHandle, int width, int height);
    
    /// <summary>
    /// Handle window resize.
    /// </summary>
    void Resize(int width, int height);
    
    /// <summary>
    /// Current viewport width.
    /// </summary>
    int ViewportWidth { get; }
    
    /// <summary>
    /// Current viewport height.
    /// </summary>
    int ViewportHeight { get; }

    // --- Resource Management ---
    
    /// <summary>
    /// Creates a mesh from vertex/index data.
    /// </summary>
    MeshHandle CreateMesh(MeshData data);
    
    /// <summary>
    /// Destroys a mesh resource.
    /// </summary>
    void DestroyMesh(MeshHandle handle);

    /// <summary>
    /// Creates a texture from pixel data.
    /// </summary>
    TextureHandle CreateTexture(ReadOnlySpan<byte> pixelData, int width, int height, TextureFormat format);
    
    /// <summary>
    /// Destroys a texture resource.
    /// </summary>
    void DestroyTexture(TextureHandle handle);
    
    /// <summary>
    /// Creates a font from TTF data.
    /// </summary>
    FontHandle CreateFont(string fontName, byte[] ttfData);
    
    /// <summary>
    /// Destroys a font resource.
    /// </summary>
    void DestroyFont(FontHandle handle);

    // --- Command List Management ---
    
    /// <summary>
    /// Creates a new command list for recording draw commands.
    /// </summary>
    IRenderCommandList CreateCommandList();
    
    /// <summary>
    /// Submits a command list for execution.
    /// </summary>
    void Submit(IRenderCommandList commands);
    
    // --- Frame Control ---
    
    /// <summary>
    /// Begins a render pass (binds target, clears buffers).
    /// </summary>
    void BeginPass(RenderPass pass);
    
    /// <summary>
    /// Ends the current render pass.
    /// </summary>
    void EndPass();
    
    /// <summary>
    /// Presents the frame to the screen.
    /// </summary>
    void SwapBuffers();
}

/// <summary>
/// Null implementation of IRenderingServer for headless mode.
/// </summary>
public class NullRenderingServer : IRenderingServer
{
    public int ViewportWidth { get; private set; }
    public int ViewportHeight { get; private set; }

    public void Initialize(nint windowHandle, int width, int height)
    {
        ViewportWidth = width;
        ViewportHeight = height;
    }

    public void Resize(int width, int height)
    {
        ViewportWidth = width;
        ViewportHeight = height;
    }

    public MeshHandle CreateMesh(MeshData data) => MeshHandle.Invalid;
    public void DestroyMesh(MeshHandle handle) { }

    public TextureHandle CreateTexture(ReadOnlySpan<byte> pixelData, int width, int height, TextureFormat format) 
        => TextureHandle.Invalid;
    public void DestroyTexture(TextureHandle handle) { }

    public FontHandle CreateFont(string fontName, byte[] ttfData) => FontHandle.Invalid;
    public void DestroyFont(FontHandle handle) { }

    public IRenderCommandList CreateCommandList() => new NullCommandList();
    public void Submit(IRenderCommandList commands) { }

    public void BeginPass(RenderPass pass) { }
    public void EndPass() { }
    public void SwapBuffers() { }

    public void Dispose() { }
}

/// <summary>
/// Null command list for headless mode.
/// </summary>
public class NullCommandList : IRenderCommandList
{
    public void SetPipeline(ShaderHandle shader) { }
    public void SetTexture(int slot, TextureHandle texture) { }
    public void SetScissorRect(RectF rect) { }
    public void DisableScissor() { }
    public void SetBlendMode(BlendMode mode) { }
    public void SetProjectionMatrix(Matrix4x4 matrix) { }
    public void SetViewMatrix(Matrix4x4 matrix) { }
    public void DrawMesh(MeshHandle mesh, Matrix4x4 worldMatrix) { }
    public void DrawSprite(TextureHandle texture, Vector2 position, Vector2 size, Color4 tint, float rotationRad = 0) { }
    public void DrawSpriteRegion(TextureHandle texture, RectF srcRegion, Vector2 position, Vector2 size, Color4 tint) { }
    public void DrawText(FontHandle font, string text, Vector2 position, float fontSize, Color4 color) { }
    public void DrawQuad(Vector2 position, Vector2 size, Color4 color) { }
}
