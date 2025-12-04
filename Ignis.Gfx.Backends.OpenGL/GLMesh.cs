using Silk.NET.OpenGL;

namespace Ignis.Gfx.Backends.OpenGL;

/// <summary>
/// A GPU mesh resource holding VAO, VBO, and EBO.
/// </summary>
internal sealed class GLMesh : IDisposable
{
    private readonly GL _gl;
    private readonly uint _vao;
    private readonly uint _vbo;
    private readonly uint _ebo;
    private bool _disposed;
    
    public int IndexCount { get; private set; }
    
    public unsafe GLMesh(GL gl, MeshData data)
    {
        _gl = gl;
        IndexCount = data.IndexCount;
        
        // Convert Vertex3D to flat float array
        var vertexData = ConvertToFloatArray(data.Vertices);
        
        // Create and bind VAO first
        _vao = gl.GenVertexArray();
        gl.BindVertexArray(_vao);
        
        // Create and bind VBO while VAO is bound
        _vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (float* v = vertexData)
        {
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexData.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
        }
        
        // Create and bind EBO while VAO is bound
        _ebo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        fixed (uint* i = data.Indices)
        {
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(data.Indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);
        }
        
        // Setup vertex attributes while VAO is still bound
        // Vertex3D layout: Position(3) + Normal(3) + TexCoord(2) + Color(4) = 12 floats
        const uint stride = 12 * sizeof(float);
        
        // Position: location 0
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);
        gl.EnableVertexAttribArray(0);
        
        // Normal: location 1
        gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));
        gl.EnableVertexAttribArray(1);
        
        // TexCoord: location 2
        gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, (void*)(6 * sizeof(float)));
        gl.EnableVertexAttribArray(2);
        
        // Color: location 3
        gl.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, stride, (void*)(8 * sizeof(float)));
        gl.EnableVertexAttribArray(3);
        
        // Unbind VAO (VBO and EBO stay associated with it)
        gl.BindVertexArray(0);
    }
    
    private static float[] ConvertToFloatArray(Vertex3D[] vertices)
    {
        // 12 floats per vertex: pos(3) + normal(3) + uv(2) + color(4)
        var result = new float[vertices.Length * 12];
        for (var i = 0; i < vertices.Length; i++)
        {
            var offset = i * 12;
            var v = vertices[i];
            result[offset + 0] = v.Position.X;
            result[offset + 1] = v.Position.Y;
            result[offset + 2] = v.Position.Z;
            result[offset + 3] = v.Normal.X;
            result[offset + 4] = v.Normal.Y;
            result[offset + 5] = v.Normal.Z;
            result[offset + 6] = v.TexCoord.X;
            result[offset + 7] = v.TexCoord.Y;
            result[offset + 8] = v.Color.R;
            result[offset + 9] = v.Color.G;
            result[offset + 10] = v.Color.B;
            result[offset + 11] = v.Color.A;
        }
        return result;
    }
    
    public unsafe void Update(MeshData data)
    {
        var vertexData = ConvertToFloatArray(data.Vertices);
        
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (float* v = vertexData)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexData.Length * sizeof(float)), v, BufferUsageARB.DynamicDraw);
        }
        
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        fixed (uint* i = data.Indices)
        {
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(data.Indices.Length * sizeof(uint)), i, BufferUsageARB.DynamicDraw);
        }
        
        IndexCount = data.IndexCount;
    }
    
    public void Bind() => _gl.BindVertexArray(_vao);
    
    public unsafe void Draw()
    {
        _gl.BindVertexArray(_vao);
        _gl.DrawElements(PrimitiveType.Triangles, (uint)IndexCount, DrawElementsType.UnsignedInt, null);
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            _gl.DeleteBuffer(_vbo);
            _gl.DeleteBuffer(_ebo);
            _gl.DeleteVertexArray(_vao);
        }
        catch { }
    }
}

