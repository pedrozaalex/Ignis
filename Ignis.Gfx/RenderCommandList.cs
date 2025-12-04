using System.Numerics;

namespace Ignis.Gfx;

/// <summary>
/// Base command list that records commands into a buffer.
/// Backends can inherit and execute these, or use their own format.
/// </summary>
public class RenderCommandList : IRenderCommandList
{
    private readonly List<RenderCommand> _commands = new();
    
    public IReadOnlyList<RenderCommand> Commands => _commands;
    
    public void Clear() => _commands.Clear();
    
    // --- Pipeline State ---
    
    public void SetPipeline(ShaderHandle shader) =>
        _commands.Add(new RenderCommand(CommandType.SetPipeline) { Shader = shader });
    
    public void SetTexture(int slot, TextureHandle texture) =>
        _commands.Add(new RenderCommand(CommandType.SetTexture) { TextureSlot = slot, Texture = texture });
    
    public void SetBlendMode(BlendMode mode) =>
        _commands.Add(new RenderCommand(CommandType.SetBlendMode) { BlendMode = mode });
    
    public void SetDepthTest(bool enabled, DepthFunc func = DepthFunc.Less) =>
        _commands.Add(new RenderCommand(CommandType.SetDepthTest) { DepthTestEnabled = enabled, DepthFunc = func });
    
    public void SetDepthWrite(bool enabled) =>
        _commands.Add(new RenderCommand(CommandType.SetDepthWrite) { DepthWriteEnabled = enabled });
    
    public void SetCullMode(CullMode mode) =>
        _commands.Add(new RenderCommand(CommandType.SetCullMode) { CullMode = mode });
    
    // --- Scissor ---
    
    public void SetScissorRect(Rect rect) =>
        _commands.Add(new RenderCommand(CommandType.SetScissor) { ScissorRect = rect, ScissorEnabled = true });
    
    public void DisableScissor() =>
        _commands.Add(new RenderCommand(CommandType.SetScissor) { ScissorEnabled = false });
    
    // --- Camera ---
    
    public void SetProjectionMatrix(Matrix4x4 matrix) =>
        _commands.Add(new RenderCommand(CommandType.SetProjection) { Matrix = matrix });
    
    public void SetViewMatrix(Matrix4x4 matrix) =>
        _commands.Add(new RenderCommand(CommandType.SetView) { Matrix = matrix });
    
    // --- Shader Uniforms ---
    
    public void SetUniform(string name, Vector3 value) =>
        _commands.Add(new RenderCommand(CommandType.SetUniformVec3) { UniformName = name, UniformVec3 = value });
    
    public void SetUniform(string name, float value) =>
        _commands.Add(new RenderCommand(CommandType.SetUniformFloat) { UniformName = name, UniformFloat = value });
    
    public void SetUniform(string name, Color4 value) =>
        _commands.Add(new RenderCommand(CommandType.SetUniformColor) { UniformName = name, Color = value });
    
    // --- 3D Drawing ---
    
    public void DrawMesh(MeshHandle mesh, Matrix4x4 worldMatrix) =>
        _commands.Add(new RenderCommand(CommandType.DrawMesh) { Mesh = mesh, Matrix = worldMatrix });
    
    public void DrawMeshInstanced(MeshHandle mesh, ReadOnlySpan<Matrix4x4> worldMatrices)
    {
        // Store as array for the command
        _commands.Add(new RenderCommand(CommandType.DrawMeshInstanced) 
        { 
            Mesh = mesh, 
            InstanceMatrices = worldMatrices.ToArray() 
        });
    }
    
    // --- 2D Drawing ---
    
    public void DrawSprite(TextureHandle texture, Vector2 position, Vector2 size, Color4 tint, float rotationRad = 0f) =>
        _commands.Add(new RenderCommand(CommandType.DrawSprite)
        {
            Texture = texture,
            Position = position,
            Size = size,
            Color = tint,
            Rotation = rotationRad
        });
    
    public void DrawSpriteRegion(TextureHandle texture, Rect srcRegion, Vector2 position, Vector2 size, Color4 tint, float rotationRad = 0f) =>
        _commands.Add(new RenderCommand(CommandType.DrawSpriteRegion)
        {
            Texture = texture,
            SrcRect = srcRegion,
            Position = position,
            Size = size,
            Color = tint,
            Rotation = rotationRad
        });
    
    public void DrawQuad(Vector2 position, Vector2 size, Color4 color) =>
        _commands.Add(new RenderCommand(CommandType.DrawQuad)
        {
            Position = position,
            Size = size,
            Color = color
        });
    
    public void DrawRoundedQuad(Vector2 position, Vector2 size, Color4 color, float cornerRadius) =>
        _commands.Add(new RenderCommand(CommandType.DrawRoundedQuad)
        {
            Position = position,
            Size = size,
            Color = color,
            CornerRadius = cornerRadius
        });
    
    public void DrawQuadOutline(Vector2 position, Vector2 size, Color4 color, float thickness = 1f) =>
        _commands.Add(new RenderCommand(CommandType.DrawQuadOutline)
        {
            Position = position,
            Size = size,
            Color = color,
            Thickness = thickness
        });
    
    public void DrawLine(Vector2 start, Vector2 end, Color4 color, float thickness = 1f) =>
        _commands.Add(new RenderCommand(CommandType.DrawLine)
        {
            Position = start,
            LineEnd = end,
            Color = color,
            Thickness = thickness
        });
    
    // --- Text ---
    
    public void DrawText(FontHandle font, string text, Vector2 position, float fontSize, Color4 color) =>
        _commands.Add(new RenderCommand(CommandType.DrawText)
        {
            Font = font,
            Text = text,
            Position = position,
            FontSize = fontSize,
            Color = color
        });
    
    public void DrawText(FontHandle font, string text, Rect bounds, float fontSize, Color4 color, 
        HorizontalAlign horizontalAlign = HorizontalAlign.Left, VerticalAlign verticalAlign = VerticalAlign.Top) =>
        _commands.Add(new RenderCommand(CommandType.DrawTextBounded)
        {
            Font = font,
            Text = text,
            TextBounds = bounds,
            FontSize = fontSize,
            Color = color,
            HAlign = horizontalAlign,
            VAlign = verticalAlign
        });
    
    // --- Debug ---
    
    public void PushDebugMarker(string name) =>
        _commands.Add(new RenderCommand(CommandType.PushDebugMarker) { DebugMarker = name });
    
    public void PopDebugMarker() =>
        _commands.Add(new RenderCommand(CommandType.PopDebugMarker));
}

public enum CommandType
{
    SetPipeline,
    SetTexture,
    SetBlendMode,
    SetDepthTest,
    SetDepthWrite,
    SetCullMode,
    SetScissor,
    SetProjection,
    SetView,
    SetUniformVec3,
    SetUniformFloat,
    SetUniformColor,
    DrawMesh,
    DrawMeshInstanced,
    DrawSprite,
    DrawSpriteRegion,
    DrawQuad,
    DrawRoundedQuad,
    DrawQuadOutline,
    DrawLine,
    DrawText,
    DrawTextBounded,
    PushDebugMarker,
    PopDebugMarker
}

/// <summary>
/// Union-style struct holding data for any render command.
/// </summary>
public struct RenderCommand(CommandType type)
{
    public CommandType Type = type;
    
    // Pipeline state
    public ShaderHandle Shader;
    public TextureHandle Texture;
    public int TextureSlot;
    public BlendMode BlendMode;
    public bool DepthTestEnabled;
    public DepthFunc DepthFunc;
    public bool DepthWriteEnabled;
    public CullMode CullMode;
    
    // Scissor
    public Rect ScissorRect;
    public bool ScissorEnabled;
    
    // Transforms
    public Matrix4x4 Matrix;
    public Matrix4x4[]? InstanceMatrices;
    
    // Uniforms
    public string? UniformName;
    public Vector3 UniformVec3;
    public float UniformFloat;
    
    // Mesh
    public MeshHandle Mesh;
    
    // 2D drawing
    public Vector2 Position;
    public Vector2 Size;
    public Vector2 LineEnd;
    public Color4 Color;
    public float Rotation;
    public float Thickness;
    public float CornerRadius;
    public Rect SrcRect;
    
    // Text
    public FontHandle Font;
    public string? Text;
    public Rect TextBounds;
    public float FontSize;
    public HorizontalAlign HAlign;
    public VerticalAlign VAlign;
    
    // Debug
    public string? DebugMarker;
}

