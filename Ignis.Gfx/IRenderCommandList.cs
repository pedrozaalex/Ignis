using System.Numerics;

namespace Ignis.Gfx;

/// <summary>
/// Records rendering commands for deferred execution.
/// Commands are recorded in order and executed when submitted to the rendering server.
/// </summary>
public interface IRenderCommandList
{
    // --- Pipeline State ---
    
    /// <summary>Set the active shader/material pipeline.</summary>
    void SetPipeline(ShaderHandle shader);
    
    /// <summary>Bind a texture to a slot (0 = diffuse, 1 = normal, etc.).</summary>
    void SetTexture(int slot, TextureHandle texture);
    
    /// <summary>Set blend mode for subsequent draw calls.</summary>
    void SetBlendMode(BlendMode mode);
    
    /// <summary>Set depth testing mode.</summary>
    void SetDepthTest(bool enabled, DepthFunc func = DepthFunc.Less);
    
    /// <summary>Enable/disable depth writing.</summary>
    void SetDepthWrite(bool enabled);
    
    /// <summary>Set face culling mode.</summary>
    void SetCullMode(CullMode mode);
    
    // --- Scissor/Clipping ---
    
    /// <summary>Enable scissor rectangle for clipping (UI scroll areas, etc.).</summary>
    void SetScissorRect(Rect rect);
    
    /// <summary>Disable scissor testing.</summary>
    void DisableScissor();
    
    // --- Camera/Transform ---
    
    /// <summary>Set projection matrix (perspective or orthographic).</summary>
    void SetProjectionMatrix(Matrix4x4 matrix);
    
    /// <summary>Set view/camera matrix.</summary>
    void SetViewMatrix(Matrix4x4 matrix);
    
    // --- 3D Drawing ---
    
    /// <summary>Draw a mesh with the given world transform.</summary>
    void DrawMesh(MeshHandle mesh, Matrix4x4 worldMatrix);
    
    /// <summary>Draw multiple instances of a mesh (if supported by backend).</summary>
    void DrawMeshInstanced(MeshHandle mesh, ReadOnlySpan<Matrix4x4> worldMatrices);
    
    // --- 2D/Sprite Drawing ---
    
    /// <summary>Draw a textured sprite.</summary>
    void DrawSprite(
        TextureHandle texture,
        Vector2 position,
        Vector2 size,
        Color4 tint,
        float rotationRad = 0f);
    
    /// <summary>Draw a region of a texture (texture atlas support).</summary>
    void DrawSpriteRegion(
        TextureHandle texture,
        Rect srcRegion,
        Vector2 position,
        Vector2 size,
        Color4 tint,
        float rotationRad = 0f);
    
    /// <summary>Draw a colored quad (no texture).</summary>
    void DrawQuad(Vector2 position, Vector2 size, Color4 color);
    
    /// <summary>Draw a colored quad with rounded corners.</summary>
    void DrawRoundedQuad(Vector2 position, Vector2 size, Color4 color, float cornerRadius);
    
    /// <summary>Draw a quad outline/border.</summary>
    void DrawQuadOutline(Vector2 position, Vector2 size, Color4 color, float thickness = 1f);
    
    /// <summary>Draw a line between two points.</summary>
    void DrawLine(Vector2 start, Vector2 end, Color4 color, float thickness = 1f);
    
    // --- Text Drawing ---
    
    /// <summary>Draw text at the given position.</summary>
    void DrawText(FontHandle font, string text, Vector2 position, float fontSize, Color4 color);
    
    /// <summary>Draw text with alignment and bounds.</summary>
    void DrawText(
        FontHandle font,
        string text,
        Rect bounds,
        float fontSize,
        Color4 color,
        HorizontalAlign horizontalAlign = HorizontalAlign.Left,
        VerticalAlign verticalAlign = VerticalAlign.Top);
    
    // --- Debug Markers ---
    
    /// <summary>Push a debug marker for GPU profiling tools.</summary>
    void PushDebugMarker(string name);
    
    /// <summary>Pop the current debug marker.</summary>
    void PopDebugMarker();
}

/// <summary>Horizontal text alignment.</summary>
public enum HorizontalAlign
{
    Left,
    Center,
    Right
}

/// <summary>Vertical text alignment.</summary>
public enum VerticalAlign
{
    Top,
    Center,
    Bottom
}

