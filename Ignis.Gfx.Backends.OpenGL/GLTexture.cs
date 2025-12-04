using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Ignis.Gfx.Backends.OpenGL;

/// <summary>
/// Wraps an OpenGL texture.
/// </summary>
internal sealed class GLTexture : IDisposable
{
    private readonly GL _gl;
    private readonly uint _handle;
    private bool _disposed;
    
    public uint Handle => _handle;
    public int Width { get; }
    public int Height { get; }
    
    public unsafe GLTexture(GL gl, ReadOnlySpan<byte> pixelData, int width, int height, TextureFormat format, TextureFilter filter, TextureWrap wrap, bool generateMips)
    {
        _gl = gl;
        Width = width;
        Height = height;
        
        _handle = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, _handle);
        
        // Set wrapping
        var wrapMode = wrap switch
        {
            TextureWrap.Repeat => TextureWrapMode.Repeat,
            TextureWrap.Clamp => TextureWrapMode.ClampToEdge,
            TextureWrap.Mirror => TextureWrapMode.MirroredRepeat,
            _ => TextureWrapMode.Repeat
        };
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapMode);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapMode);
        
        // Set filtering
        var (minFilter, magFilter) = filter switch
        {
            TextureFilter.Point => (TextureMinFilter.Nearest, TextureMagFilter.Nearest),
            TextureFilter.Linear => (TextureMinFilter.Linear, TextureMagFilter.Linear),
            TextureFilter.Trilinear => (TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear),
            TextureFilter.Anisotropic => (TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear),
            _ => (TextureMinFilter.Linear, TextureMagFilter.Linear)
        };
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
        
        // Upload data
        var (internalFormat, pixelFormat, pixelType) = format switch
        {
            TextureFormat.RGBA8 => (InternalFormat.Rgba8, PixelFormat.Rgba, PixelType.UnsignedByte),
            TextureFormat.RGB8 => (InternalFormat.Rgb8, PixelFormat.Rgb, PixelType.UnsignedByte),
            TextureFormat.R8 => (InternalFormat.R8, PixelFormat.Red, PixelType.UnsignedByte),
            TextureFormat.RGBA16F => (InternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float),
            TextureFormat.RGBA32F => (InternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float),
            _ => (InternalFormat.Rgba8, PixelFormat.Rgba, PixelType.UnsignedByte)
        };
        
        fixed (byte* ptr = pixelData)
        {
            gl.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, (uint)width, (uint)height, 0, pixelFormat, pixelType, ptr);
        }
        
        if (generateMips)
            gl.GenerateMipmap(TextureTarget.Texture2D);
    }
    
    public static GLTexture FromFile(GL gl, string path, TextureFilter filter = TextureFilter.Linear, TextureWrap wrap = TextureWrap.Repeat)
    {
        using var image = Image.Load<Rgba32>(path);
        var pixels = new byte[image.Width * image.Height * 4];
        image.CopyPixelDataTo(pixels);
        
        return new GLTexture(gl, pixels, image.Width, image.Height, TextureFormat.RGBA8, filter, wrap, true);
    }
    
    public unsafe void Update(ReadOnlySpan<byte> pixelData, int x, int y, int width, int height)
    {
        _gl.BindTexture(TextureTarget.Texture2D, _handle);
        fixed (byte* ptr = pixelData)
        {
            _gl.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, (uint)width, (uint)height, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
        }
    }
    
    public void Bind(TextureUnit unit = TextureUnit.Texture0)
    {
        _gl.ActiveTexture(unit);
        _gl.BindTexture(TextureTarget.Texture2D, _handle);
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { _gl.DeleteTexture(_handle); } catch { }
    }
}

