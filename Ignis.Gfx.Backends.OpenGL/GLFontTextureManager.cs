using System.Drawing;
using FontStashSharp.Interfaces;
using Silk.NET.OpenGL;

namespace Ignis.Gfx.Backends.OpenGL;

/// <summary>
/// FontStashSharp texture manager implementation for OpenGL.
/// </summary>
internal sealed class GLFontTextureManager : ITexture2DManager
{
    private readonly GL _gl;

    public GLFontTextureManager(GL gl)
    {
        _gl = gl;
    }

    public object CreateTexture(int width, int height)
    {
        return new GLFontTexture(_gl, width, height);
    }

    public Point GetTextureSize(object texture)
    {
        var t = (GLFontTexture)texture;
        return new Point(t.Width, t.Height);
    }

    public void SetTextureData(object texture, Rectangle bounds, byte[] data)
    {
        var t = (GLFontTexture)texture;
        t.SetData(bounds, data);
    }
}

/// <summary>
/// OpenGL texture wrapper for FontStashSharp.
/// </summary>
internal sealed class GLFontTexture : IDisposable
{
    private readonly GL _gl;
    private readonly uint _handle;
    
    public int Width { get; }
    public int Height { get; }
    public uint Handle => _handle;

    public unsafe GLFontTexture(GL gl, int width, int height)
    {
        _gl = gl;
        Width = width;
        Height = height;
        
        _handle = gl.GenTexture();
        Bind();
        
        // Allocate texture memory
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, 
            (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        
        // Set filtering (linear is best for fonts)
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
    }

    public void Bind()
    {
        _gl.BindTexture(TextureTarget.Texture2D, _handle);
    }

    public unsafe void SetData(Rectangle bounds, byte[] data)
    {
        Bind();
        fixed (byte* ptr = data)
        {
            _gl.TexSubImage2D(TextureTarget.Texture2D, 0, 
                bounds.Left, bounds.Top, (uint)bounds.Width, (uint)bounds.Height, 
                PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
        }
    }

    public void Dispose()
    {
        _gl.DeleteTexture(_handle);
    }
}

