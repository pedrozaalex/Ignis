using System.Numerics;
using System.Runtime.InteropServices;

namespace Ignis.Engine.Graphics;

/// <summary>
/// RGBA color with float components (0-1 range).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Color4 : IEquatable<Color4>
{
    public float R, G, B, A;

    public Color4(float r, float g, float b, float a = 1f)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public Color4(int r, int g, int b, int a = 255)
    {
        R = r / 255f;
        G = g / 255f;
        B = b / 255f;
        A = a / 255f;
    }

    public static Color4 White => new(1, 1, 1, 1);
    public static Color4 Black => new(0, 0, 0, 1);
    public static Color4 Transparent => new(0, 0, 0, 0);
    public static Color4 Red => new(1, 0, 0, 1);
    public static Color4 Green => new(0, 1, 0, 1);
    public static Color4 Blue => new(0, 0, 1, 1);
    public static Color4 Yellow => new(1, 1, 0, 1);
    public static Color4 Cyan => new(0, 1, 1, 1);
    public static Color4 Magenta => new(1, 0, 1, 1);
    public static Color4 Gray => new(0.5f, 0.5f, 0.5f, 1);
    public static Color4 LightGray => new(0.75f, 0.75f, 0.75f, 1);
    public static Color4 DarkGray => new(0.25f, 0.25f, 0.25f, 1);
    
    public Vector3 ToVector3() => new(R, G, B);

    public static Color4 Lerp(Color4 a, Color4 b, float t)
    {
        return new Color4(
            a.R + (b.R - a.R) * t,
            a.G + (b.G - a.G) * t,
            a.B + (b.B - a.B) * t,
            a.A + (b.A - a.A) * t
        );
    }

    public bool Equals(Color4 other) => R == other.R && G == other.G && B == other.B && A == other.A;
    public override bool Equals(object? obj) => obj is Color4 other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(R, G, B, A);
    public static bool operator ==(Color4 left, Color4 right) => left.Equals(right);
    public static bool operator !=(Color4 left, Color4 right) => !left.Equals(right);
}

/// <summary>
/// 2D rectangle with float components.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct RectF : IEquatable<RectF>
{
    public float X, Y, Width, Height;

    public RectF(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public float Left => X;
    public float Right => X + Width;
    public float Top => Y;
    public float Bottom => Y + Height;
    public Vector2 Position => new(X, Y);
    public Vector2 Size => new(Width, Height);
    public Vector2 Center => new(X + Width / 2, Y + Height / 2);

    public static RectF Empty => new(0, 0, 0, 0);

    public bool Contains(Vector2 point) =>
        point.X >= X && point.X < X + Width &&
        point.Y >= Y && point.Y < Y + Height;

    public bool Contains(float px, float py) =>
        px >= X && px < X + Width &&
        py >= Y && py < Y + Height;

    public bool Equals(RectF other) => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
    public override bool Equals(object? obj) => obj is RectF other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
    public static bool operator ==(RectF left, RectF right) => left.Equals(right);
    public static bool operator !=(RectF left, RectF right) => !left.Equals(right);
}

/// <summary>
/// 2D rectangle with integer components (screen coordinates).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct RectI : IEquatable<RectI>
{
    public int X, Y, Width, Height;

    public RectI(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public int Left => X;
    public int Right => X + Width;
    public int Top => Y;
    public int Bottom => Y + Height;

    public static RectI Empty => new(0, 0, 0, 0);

    public bool Contains(int px, int py) =>
        px >= X && px < X + Width &&
        py >= Y && py < Y + Height;

    public bool Contains(Vector2 point) =>
        point.X >= X && point.X < X + Width &&
        point.Y >= Y && point.Y < Y + Height;

    public static implicit operator RectF(RectI r) => new(r.X, r.Y, r.Width, r.Height);

    public bool Equals(RectI other) => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
    public override bool Equals(object? obj) => obj is RectI other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
    public static bool operator ==(RectI left, RectI right) => left.Equals(right);
    public static bool operator !=(RectI left, RectI right) => !left.Equals(right);
}

/// <summary>
/// Texture formats.
/// </summary>
public enum TextureFormat
{
    RGBA8,
    RGB8,
    R8,
    RGBA16F,
    Depth24Stencil8
}

/// <summary>
/// Blend modes for rendering.
/// </summary>
public enum BlendMode
{
    Opaque,
    AlphaBlend,
    Additive,
    Premultiplied
}

/// <summary>
/// Handle to a GPU texture resource.
/// </summary>
public readonly struct TextureHandle : IEquatable<TextureHandle>
{
    public readonly int Id;

    public TextureHandle(int id) => Id = id;

    public static TextureHandle Invalid => new(0);
    public bool IsValid => Id != 0;

    public bool Equals(TextureHandle other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is TextureHandle other && Equals(other);
    public override int GetHashCode() => Id;
    public static bool operator ==(TextureHandle left, TextureHandle right) => left.Equals(right);
    public static bool operator !=(TextureHandle left, TextureHandle right) => !left.Equals(right);
}

/// <summary>
/// Handle to a GPU mesh resource.
/// </summary>
public readonly struct MeshHandle : IEquatable<MeshHandle>
{
    public readonly int Id;

    public MeshHandle(int id) => Id = id;

    public static MeshHandle Invalid => new(0);
    public bool IsValid => Id != 0;

    public bool Equals(MeshHandle other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is MeshHandle other && Equals(other);
    public override int GetHashCode() => Id;
    public static bool operator ==(MeshHandle left, MeshHandle right) => left.Equals(right);
    public static bool operator !=(MeshHandle left, MeshHandle right) => !left.Equals(right);
}

/// <summary>
/// Handle to a GPU shader resource.
/// </summary>
public readonly struct ShaderHandle : IEquatable<ShaderHandle>
{
    public readonly int Id;

    public ShaderHandle(int id) => Id = id;

    public static ShaderHandle Invalid => new(0);
    public bool IsValid => Id != 0;

    public bool Equals(ShaderHandle other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is ShaderHandle other && Equals(other);
    public override int GetHashCode() => Id;
    public static bool operator ==(ShaderHandle left, ShaderHandle right) => left.Equals(right);
    public static bool operator !=(ShaderHandle left, ShaderHandle right) => !left.Equals(right);
}

/// <summary>
/// Handle to a font resource.
/// </summary>
public readonly struct FontHandle : IEquatable<FontHandle>
{
    public readonly int Id;

    public FontHandle(int id) => Id = id;

    public static FontHandle Invalid => new(0);
    public bool IsValid => Id != 0;

    public bool Equals(FontHandle other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is FontHandle other && Equals(other);
    public override int GetHashCode() => Id;
    public static bool operator ==(FontHandle left, FontHandle right) => left.Equals(right);
    public static bool operator !=(FontHandle left, FontHandle right) => !left.Equals(right);
}

/// <summary>
/// Handle to a render target resource.
/// </summary>
public readonly struct RenderTargetHandle : IEquatable<RenderTargetHandle>
{
    public readonly int Id;

    public RenderTargetHandle(int id) => Id = id;

    /// <summary>
    /// Screen/backbuffer target.
    /// </summary>
    public static RenderTargetHandle Screen => new(0);
    public bool IsScreen => Id == 0;

    public bool Equals(RenderTargetHandle other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is RenderTargetHandle other && Equals(other);
    public override int GetHashCode() => Id;
    public static bool operator ==(RenderTargetHandle left, RenderTargetHandle right) => left.Equals(right);
    public static bool operator !=(RenderTargetHandle left, RenderTargetHandle right) => !left.Equals(right);
}

/// <summary>
/// Defines rendering pass configuration.
/// </summary>
public struct RenderPass
{
    public RenderTargetHandle Target;
    public Color4 ClearColor;
    public bool ClearDepth;
    public RectF Viewport;
}

/// <summary>
/// Vertex with position and color for 2D rendering.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct VertexPositionColor
{
    public Vector3 Position;
    public Color4 Color;

    public VertexPositionColor(Vector3 position, Color4 color)
    {
        Position = position;
        Color = color;
    }
}

/// <summary>
/// Vertex with position, color, and texture coordinates.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct VertexPositionColorTexture
{
    public Vector3 Position;
    public Color4 Color;
    public Vector2 TexCoord;

    public VertexPositionColorTexture(Vector3 position, Color4 color, Vector2 texCoord)
    {
        Position = position;
        Color = color;
        TexCoord = texCoord;
    }
}

/// <summary>
/// Mesh data for creating GPU mesh resources.
/// </summary>
public class MeshData
{
    public required float[] Vertices { get; init; }
    public required int[] Indices { get; init; }
    public required VertexLayout Layout { get; init; }
}

/// <summary>
/// Describes the layout of vertex data.
/// </summary>
public class VertexLayout
{
    public required VertexAttribute[] Attributes { get; init; }
    public required int Stride { get; init; }
}

/// <summary>
/// Single vertex attribute.
/// </summary>
public struct VertexAttribute
{
    public string Name;
    public VertexAttributeType Type;
    public int ComponentCount;
    public int Offset;
}

public enum VertexAttributeType
{
    Float,
    Int,
    Byte
}
