using System.Numerics;
using Silk.NET.OpenGL;

namespace Ignis.Gfx.Backends.OpenGL;

/// <summary>
/// Batches 2D sprite/quad draws for efficient rendering.
/// </summary>
internal sealed class GL2DBatch : IDisposable
{
    private const int MaxQuadsPerBatch = 2048;
    private const int VerticesPerQuad = 4;
    private const int IndicesPerQuad = 6;
    private const int FloatsPerVertex = 8; // pos(2) + uv(2) + color(4)
    
    private readonly GL _gl;
    private readonly uint _vao;
    private readonly uint _vbo;
    private readonly uint _ebo;
    private readonly float[] _vertices;
    private int _quadCount;
    private uint _currentTexture;
    private bool _disposed;
    
    public GL2DBatch(GL gl)
    {
        unsafe
        {
            _gl = gl;
            _vertices = new float[MaxQuadsPerBatch * VerticesPerQuad * FloatsPerVertex];
        
            // Create VAO
            _vao = gl.GenVertexArray();
            gl.BindVertexArray(_vao);
        
            // Create VBO (dynamic)
            _vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(_vertices.Length * sizeof(float)), ReadOnlySpan<float>.Empty, BufferUsageARB.DynamicDraw);
        
            // Create EBO (static indices)
            var indices = new uint[MaxQuadsPerBatch * IndicesPerQuad];
            for (var i = 0; i < MaxQuadsPerBatch; i++)
            {
                var baseVertex = (uint)(i * 4);
                var baseIndex = i * 6;
                indices[baseIndex + 0] = baseVertex + 0;
                indices[baseIndex + 1] = baseVertex + 1;
                indices[baseIndex + 2] = baseVertex + 2;
                indices[baseIndex + 3] = baseVertex + 0;
                indices[baseIndex + 4] = baseVertex + 2;
                indices[baseIndex + 5] = baseVertex + 3;
            }
        
            _ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
            unsafe
            {
                fixed (uint* ptr = indices)
                {
                    gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), ptr, BufferUsageARB.StaticDraw);
                }
            }
        
            // Setup vertex layout: pos(2) + uv(2) + color(4)
            const uint stride = FloatsPerVertex * sizeof(float);
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, (void*)0);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)(2 * sizeof(float)));
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, stride, (void*)(4 * sizeof(float)));
        
            gl.BindVertexArray(0);
        }
    }
    
    public void Begin()
    {
        _quadCount = 0;
        _currentTexture = 0;
    }
    
    public void DrawQuad(Vector2 position, Vector2 size, Color4 color, uint textureHandle = 0, Rect? srcRect = null)
    {
        if (_quadCount >= MaxQuadsPerBatch || (textureHandle != _currentTexture && _quadCount > 0))
            Flush();
        
        _currentTexture = textureHandle;
        
        var src = srcRect ?? new Rect(0, 0, 1, 1);
        float x = position.X, y = position.Y;
        float w = size.X, h = size.Y;
        float u0 = src.X, v0 = src.Y;
        float u1 = src.X + src.Width, v1 = src.Y + src.Height;
        
        var offset = _quadCount * VerticesPerQuad * FloatsPerVertex;
        
        // Top-left
        _vertices[offset + 0] = x;
        _vertices[offset + 1] = y;
        _vertices[offset + 2] = u0;
        _vertices[offset + 3] = v0;
        _vertices[offset + 4] = color.R;
        _vertices[offset + 5] = color.G;
        _vertices[offset + 6] = color.B;
        _vertices[offset + 7] = color.A;
        
        // Top-right
        _vertices[offset + 8] = x + w;
        _vertices[offset + 9] = y;
        _vertices[offset + 10] = u1;
        _vertices[offset + 11] = v0;
        _vertices[offset + 12] = color.R;
        _vertices[offset + 13] = color.G;
        _vertices[offset + 14] = color.B;
        _vertices[offset + 15] = color.A;
        
        // Bottom-right
        _vertices[offset + 16] = x + w;
        _vertices[offset + 17] = y + h;
        _vertices[offset + 18] = u1;
        _vertices[offset + 19] = v1;
        _vertices[offset + 20] = color.R;
        _vertices[offset + 21] = color.G;
        _vertices[offset + 22] = color.B;
        _vertices[offset + 23] = color.A;
        
        // Bottom-left
        _vertices[offset + 24] = x;
        _vertices[offset + 25] = y + h;
        _vertices[offset + 26] = u0;
        _vertices[offset + 27] = v1;
        _vertices[offset + 28] = color.R;
        _vertices[offset + 29] = color.G;
        _vertices[offset + 30] = color.B;
        _vertices[offset + 31] = color.A;
        
        _quadCount++;
    }
    
    public void DrawLine(Vector2 start, Vector2 end, Color4 color, float thickness)
    {
        var direction = Vector2.Normalize(end - start);
        var perpendicular = new Vector2(-direction.Y, direction.X) * thickness * 0.5f;
        
        var p0 = start - perpendicular;
        var p1 = start + perpendicular;
        var p2 = end + perpendicular;
        var p3 = end - perpendicular;
        
        if (_quadCount >= MaxQuadsPerBatch)
            Flush();
        
        var offset = _quadCount * VerticesPerQuad * FloatsPerVertex;
        
        void WriteVertex(int idx, Vector2 pos)
        {
            var o = offset + idx * FloatsPerVertex;
            _vertices[o + 0] = pos.X;
            _vertices[o + 1] = pos.Y;
            _vertices[o + 2] = 0;
            _vertices[o + 3] = 0;
            _vertices[o + 4] = color.R;
            _vertices[o + 5] = color.G;
            _vertices[o + 6] = color.B;
            _vertices[o + 7] = color.A;
        }
        
        WriteVertex(0, p0);
        WriteVertex(1, p1);
        WriteVertex(2, p2);
        WriteVertex(3, p3);
        
        _quadCount++;
    }
    
    public unsafe void Flush()
    {
        if (_quadCount == 0) return;
        
        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        
        var vertexCount = _quadCount * VerticesPerQuad * FloatsPerVertex;
        fixed (float* ptr = _vertices)
        {
            _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(vertexCount * sizeof(float)), ptr);
        }
        
        if (_currentTexture != 0)
        {
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, _currentTexture);
        }
        
        _gl.DrawElements(PrimitiveType.Triangles, (uint)(_quadCount * IndicesPerQuad), DrawElementsType.UnsignedInt, null);
        
        _quadCount = 0;
    }
    
    public void End()
    {
        Flush();
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        try
        {
            _gl.DeleteVertexArray(_vao);
            _gl.DeleteBuffer(_vbo);
            _gl.DeleteBuffer(_ebo);
        }
        catch
        {
            // Context may already be destroyed, ignore cleanup errors
        }
    }
}

