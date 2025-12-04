using Silk.NET.OpenGL;

namespace Ignis.Gfx.Backends.OpenGL;

/// <summary>
/// Wraps an OpenGL buffer (VBO or EBO).
/// </summary>
internal sealed class GLBuffer<T> : IDisposable where T : unmanaged
{
    private readonly GL _gl;
    private readonly uint _handle;
    private readonly BufferTargetARB _target;
    private bool _disposed;
    
    public uint Handle => _handle;
    public BufferTargetARB Target => _target;
    
    public unsafe GLBuffer(GL gl, ReadOnlySpan<T> data, BufferTargetARB target)
    {
        _gl = gl;
        _target = target;
        _handle = gl.GenBuffer();
        
        gl.BindBuffer(target, _handle);
        fixed (void* ptr = data)
        {
            gl.BufferData(target, (nuint)(data.Length * sizeof(T)), ptr, BufferUsageARB.StaticDraw);
        }
    }
    
    public unsafe void Update(ReadOnlySpan<T> data)
    {
        _gl.BindBuffer(_target, _handle);
        fixed (void* ptr = data)
        {
            _gl.BufferData(_target, (nuint)(data.Length * sizeof(T)), ptr, BufferUsageARB.DynamicDraw);
        }
    }
    
    public void Bind() => _gl.BindBuffer(_target, _handle);
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { _gl.DeleteBuffer(_handle); } catch { }
    }
}

