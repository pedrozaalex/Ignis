using Silk.NET.OpenGL;

namespace Ignis.Graphics.Backends.OpenGL;

/// <summary>
/// Wraps an OpenGL framebuffer (render target).
/// </summary>
internal sealed class GLRenderTarget : IDisposable
{
    private readonly GL _gl;
    private readonly uint _fbo;
    private readonly uint _colorTexture;
    private readonly uint _depthRbo;
    private bool _disposed;
    
    public int Width { get; }
    public int Height { get; }
    public uint ColorTextureHandle => _colorTexture;
    
    public GLRenderTarget(GL gl, int width, int height, bool hasDepth)
    {
        _gl = gl;
        Width = width;
        Height = height;
        
        // Create framebuffer
        _fbo = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        
        // Create color texture
        _colorTexture = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, _colorTexture);
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ReadOnlySpan<byte>.Empty);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _colorTexture, 0);
        
        // Create depth renderbuffer if requested
        if (hasDepth)
        {
            _depthRbo = gl.GenRenderbuffer();
            gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthRbo);
            gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8, (uint)width, (uint)height);
            gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, _depthRbo);
        }
        
        // Check completeness
        var status = gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != GLEnum.FramebufferComplete)
            throw new Exception($"Framebuffer incomplete: {status}");
        
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }
    
    public void Bind() => _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
    
    public void Unbind() => _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    
    public void BindColorTexture(TextureUnit unit = TextureUnit.Texture0)
    {
        _gl.ActiveTexture(unit);
        _gl.BindTexture(TextureTarget.Texture2D, _colorTexture);
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            _gl.DeleteFramebuffer(_fbo);
            _gl.DeleteTexture(_colorTexture);
            if (_depthRbo != 0)
                _gl.DeleteRenderbuffer(_depthRbo);
        }
        catch { }
    }
}

