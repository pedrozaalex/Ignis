using Silk.NET.OpenGL;

namespace Ignis.Graphics.Backends.OpenGL;

/// <summary>
/// Wraps an OpenGL Vertex Array Object (VAO).
/// Stores the layout configuration for vertex data.
/// </summary>
internal sealed class GLVertexArray : IDisposable
{
    private readonly GL _gl;
    private readonly uint _handle;
    private bool _disposed;
    
    public uint Handle => _handle;
    
    public GLVertexArray(GL gl)
    {
        _gl = gl;
        _handle = gl.GenVertexArray();
    }
    
    public void Bind() => _gl.BindVertexArray(_handle);
    
    public void Unbind() => _gl.BindVertexArray(0);
    
    /// <summary>
    /// Configures a vertex attribute pointer.
    /// </summary>
    /// <param name="index">Shader layout location</param>
    /// <param name="count">Number of components (1-4)</param>
    /// <param name="type">Data type</param>
    /// <param name="stride">Total vertex size in bytes</param>
    /// <param name="offset">Offset in bytes to this attribute</param>
    public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint stride, int offset)
    {
        _gl.EnableVertexAttribArray(index);
        _gl.VertexAttribPointer(index, count, type, false, stride, (void*)offset);
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { _gl.DeleteVertexArray(_handle); } catch { }
    }
}

