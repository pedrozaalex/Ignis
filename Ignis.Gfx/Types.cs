using System.Numerics;
using System.Runtime.InteropServices;

namespace Ignis.Gfx;

/// <summary>RGBA color with float components.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct Color4
{
    public float R, G, B, A;
    
    public Color4(float r, float g, float b, float a = 1f)
    {
        R = r; G = g; B = b; A = a;
    }
    
    /// <summary>Creates a color from byte values (0-255).</summary>
    public static Color4 FromBytes(byte r, byte g, byte b, byte a = 255)
        => new(r / 255f, g / 255f, b / 255f, a / 255f);
    
    public static Color4 White => new(1f, 1f, 1f, 1f);
    public static Color4 Black => new(0f, 0f, 0f, 1f);
    public static Color4 Transparent => new(0f, 0f, 0f, 0f);
    public static Color4 Red => new(1f, 0f, 0f, 1f);
    public static Color4 Green => new(0f, 1f, 0f, 1f);
    public static Color4 Blue => new(0f, 0f, 1f, 1f);
    public static Color4 Yellow => new(1f, 1f, 0f, 1f);
    public static Color4 Cyan => new(0f, 1f, 1f, 1f);
    public static Color4 Magenta => new(1f, 0f, 1f, 1f);
    public static Color4 Gray => new(0.5f, 0.5f, 0.5f, 1f);
    
    public Vector4 ToVector4() => new(R, G, B, A);
    public static Color4 FromVector4(Vector4 v) => new(v.X, v.Y, v.Z, v.W);
    
    /// <summary>Linear interpolation between two colors.</summary>
    public static Color4 Lerp(Color4 a, Color4 b, float t) => new(
        a.R + (b.R - a.R) * t,
        a.G + (b.G - a.G) * t,
        a.B + (b.B - a.B) * t,
        a.A + (b.A - a.A) * t
    );
    
    /// <summary>Multiply color by a scalar (preserves alpha).</summary>
    public Color4 WithBrightness(float factor) => new(R * factor, G * factor, B * factor, A);
    
    /// <summary>Returns this color with a different alpha.</summary>
    public Color4 WithAlpha(float alpha) => new(R, G, B, alpha);
}

/// <summary>Axis-aligned rectangle (screen coordinates).</summary>
[StructLayout(LayoutKind.Sequential)]
public struct Rect
{
    public float X, Y, Width, Height;
    
    public Rect(float x, float y, float width, float height)
    {
        X = x; Y = y; Width = width; Height = height;
    }
    
    public Rect(Vector2 position, Vector2 size)
    {
        X = position.X; Y = position.Y; Width = size.X; Height = size.Y;
    }
    
    public float Left => X;
    public float Top => Y;
    public float Right => X + Width;
    public float Bottom => Y + Height;
    
    public Vector2 Position => new(X, Y);
    public Vector2 Size => new(Width, Height);
    public Vector2 Center => new(X + Width * 0.5f, Y + Height * 0.5f);
    
    public bool Contains(Vector2 point) =>
        point.X >= X && point.X <= X + Width &&
        point.Y >= Y && point.Y <= Y + Height;
    
    public bool Intersects(Rect other) =>
        X < other.Right && Right > other.X &&
        Y < other.Bottom && Bottom > other.Y;
    
    /// <summary>Returns the intersection of two rectangles, or empty if they don't intersect.</summary>
    public static Rect Intersect(Rect a, Rect b)
    {
        var x = MathF.Max(a.X, b.X);
        var y = MathF.Max(a.Y, b.Y);
        var right = MathF.Min(a.Right, b.Right);
        var bottom = MathF.Min(a.Bottom, b.Bottom);
        
        if (right <= x || bottom <= y)
            return default;
        
        return new Rect(x, y, right - x, bottom - y);
    }
    
    public static Rect Empty => default;
}

/// <summary>Defines where and how to render a pass.</summary>
public struct RenderPass
{
    /// <summary>Target to render to (Screen or offscreen RenderTarget).</summary>
    public RenderTargetHandle Target;
    
    /// <summary>Clear color for the target.</summary>
    public Color4 ClearColor;
    
    /// <summary>Whether to clear the depth buffer at pass start.</summary>
    public bool ClearDepth;
    
    /// <summary>Viewport rectangle within the target.</summary>
    public Rect Viewport;
    
    public static RenderPass CreateDefault(int width, int height) => new()
    {
        Target = RenderTargetHandle.Screen,
        ClearColor = Color4.Black,
        ClearDepth = true,
        Viewport = new Rect(0, 0, width, height)
    };
}

/// <summary>Description for creating a render target.</summary>
public struct RenderTargetDesc
{
    public int Width;
    public int Height;
    public TextureFormat ColorFormat;
    public bool HasDepth;
    public int SampleCount; // 1 = no MSAA
}

/// <summary>Description for creating a texture.</summary>
public struct TextureDesc
{
    public int Width;
    public int Height;
    public TextureFormat Format;
    public TextureFilter Filter;
    public TextureWrap Wrap;
    public bool GenerateMips;
}

