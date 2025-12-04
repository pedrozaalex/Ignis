using System.Numerics;
using Silk.NET.OpenGL;

namespace Ignis.Graphics.Backends.OpenGL;

/// <summary>
/// Wraps an OpenGL shader program (vertex + fragment).
/// </summary>
internal sealed class GLShader : IDisposable
{
    private readonly GL _gl;
    private readonly uint _handle;
    private readonly Dictionary<string, int> _uniformLocations = new();
    private bool _disposed;
    
    public uint Handle => _handle;
    
    public GLShader(GL gl, string vertexSource, string fragmentSource)
    {
        _gl = gl;
        
        // Compile vertex shader
        var vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(vertexShader, vertexSource);
        gl.CompileShader(vertexShader);
        CheckShaderCompilation(gl, vertexShader, "Vertex");
        
        // Compile fragment shader
        var fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(fragmentShader, fragmentSource);
        gl.CompileShader(fragmentShader);
        CheckShaderCompilation(gl, fragmentShader, "Fragment");
        
        // Link program
        _handle = gl.CreateProgram();
        gl.AttachShader(_handle, vertexShader);
        gl.AttachShader(_handle, fragmentShader);
        gl.LinkProgram(_handle);
        CheckProgramLinking(gl, _handle);
        
        // Cleanup individual shaders
        gl.DetachShader(_handle, vertexShader);
        gl.DetachShader(_handle, fragmentShader);
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);
    }
    
    private static void CheckShaderCompilation(GL gl, uint shader, string type)
    {
        gl.GetShader(shader, ShaderParameterName.CompileStatus, out var status);
        if (status == 0)
        {
            var log = gl.GetShaderInfoLog(shader);
            throw new Exception($"{type} shader compilation error: {log}");
        }
    }
    
    private static void CheckProgramLinking(GL gl, uint program)
    {
        gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out var status);
        if (status == 0)
        {
            var log = gl.GetProgramInfoLog(program);
            throw new Exception($"Shader program linking error: {log}");
        }
    }
    
    public void Use() => _gl.UseProgram(_handle);
    
    private int GetUniformLocation(string name)
    {
        if (!_uniformLocations.TryGetValue(name, out var location))
        {
            location = _gl.GetUniformLocation(_handle, name);
            _uniformLocations[name] = location;
        }
        return location;
    }
    
    public void SetInt(string name, int value)
    {
        var location = GetUniformLocation(name);
        if (location >= 0) _gl.Uniform1(location, value);
    }
    
    public void SetFloat(string name, float value)
    {
        var location = GetUniformLocation(name);
        if (location >= 0) _gl.Uniform1(location, value);
    }
    
    public void SetVec2(string name, Vector2 value)
    {
        var location = GetUniformLocation(name);
        if (location >= 0) _gl.Uniform2(location, value.X, value.Y);
    }
    
    public void SetVec3(string name, Vector3 value)
    {
        var location = GetUniformLocation(name);
        if (location >= 0) _gl.Uniform3(location, value.X, value.Y, value.Z);
    }
    
    public void SetVec4(string name, Vector4 value)
    {
        var location = GetUniformLocation(name);
        if (location >= 0) _gl.Uniform4(location, value.X, value.Y, value.Z, value.W);
    }
    
    public unsafe void SetMat4(string name, Matrix4x4 value)
    {
        var location = GetUniformLocation(name);
        if (location >= 0)
        {
            _gl.UniformMatrix4(location, 1, false, (float*)&value);
        }
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { _gl.DeleteProgram(_handle); } catch { }
    }
}

