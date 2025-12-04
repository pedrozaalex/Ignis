using System.Numerics;
using FontStashSharp;
using FontStashSharp.Interfaces;
using Silk.NET.OpenGL;

namespace Ignis.Graphics.Backends.OpenGL;

/// <summary>
/// FontStashSharp renderer implementation for OpenGL.
/// Batches text quads and renders them efficiently.
/// </summary>
internal sealed class GLFontRenderer : IFontStashRenderer2, IDisposable
{
    private const int MaxVertices = 8192;
    private const int VerticesPerQuad = 4;
    private const int IndicesPerQuad = 6;
    private const int FloatsPerVertex = 9; // pos(3) + color(4) + uv(2)
    
    private readonly GL _gl;
    private readonly GLFontTextureManager _textureManager;
    private readonly GLShader _shader;
    private readonly uint _vao;
    private readonly uint _vbo;
    private readonly uint _ebo;
    private readonly float[] _vertices;
    
    private int _vertexCount;
    private GLFontTexture? _currentTexture;
    private Matrix4x4 _transform = Matrix4x4.Identity;
    private bool _disposed;

    public ITexture2DManager TextureManager => _textureManager;

    public GLFontRenderer(GL gl)
    {
        _gl = gl;
        _textureManager = new GLFontTextureManager(gl);
        _vertices = new float[MaxVertices * FloatsPerVertex];
        
        // Create shader
        _shader = new GLShader(gl, TextVertexShader, TextFragmentShader);
        
        // Create VAO
        _vao = gl.GenVertexArray();
        gl.BindVertexArray(_vao);
        
        // Create VBO (dynamic)
        _vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        unsafe
        {
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(_vertices.Length * sizeof(float)), 
                null, BufferUsageARB.DynamicDraw);
        }
        
        // Create EBO with indices for quads
        var indices = new ushort[MaxVertices / VerticesPerQuad * IndicesPerQuad];
        for (int i = 0, v = 0; i < indices.Length; i += 6, v += 4)
        {
            indices[i + 0] = (ushort)(v + 0);
            indices[i + 1] = (ushort)(v + 1);
            indices[i + 2] = (ushort)(v + 2);
            indices[i + 3] = (ushort)(v + 0);
            indices[i + 4] = (ushort)(v + 2);
            indices[i + 5] = (ushort)(v + 3);
        }
        
        _ebo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        unsafe
        {
            fixed (ushort* ptr = indices)
            {
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, 
                    (nuint)(indices.Length * sizeof(ushort)), ptr, BufferUsageARB.StaticDraw);
            }
        }
        
        // Setup vertex layout: pos(3) + color(4) + uv(2)
        const uint stride = FloatsPerVertex * sizeof(float);
        unsafe
        {
            // Position (location 0)
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);
            // Color (location 1)
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));
            // TexCoord (location 2)
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, (void*)(7 * sizeof(float)));
        }
        
        gl.BindVertexArray(0);
    }

    public void Begin(Matrix4x4 transform)
    {
        _transform = transform;
        _vertexCount = 0;
        _currentTexture = null;
    }

    public void DrawQuad(object texture, 
        ref VertexPositionColorTexture topLeft, 
        ref VertexPositionColorTexture topRight, 
        ref VertexPositionColorTexture bottomLeft, 
        ref VertexPositionColorTexture bottomRight)
    {
        var tex = (GLFontTexture)texture;
        
        // Flush if texture changed or buffer full
        if (_currentTexture != tex || _vertexCount + 4 > MaxVertices)
        {
            Flush();
            _currentTexture = tex;
        }
        
        AddVertex(ref topLeft);
        AddVertex(ref topRight);
        AddVertex(ref bottomRight);
        AddVertex(ref bottomLeft);
    }

    private void AddVertex(ref VertexPositionColorTexture v)
    {
        int offset = _vertexCount * FloatsPerVertex;
        
        // Position
        _vertices[offset + 0] = v.Position.X;
        _vertices[offset + 1] = v.Position.Y;
        _vertices[offset + 2] = v.Position.Z;
        
        // Color (premultiplied alpha for better blending)
        float a = v.Color.A / 255f;
        _vertices[offset + 3] = (v.Color.R / 255f) * a;
        _vertices[offset + 4] = (v.Color.G / 255f) * a;
        _vertices[offset + 5] = (v.Color.B / 255f) * a;
        _vertices[offset + 6] = a;
        
        // TexCoord
        _vertices[offset + 7] = v.TextureCoordinate.X;
        _vertices[offset + 8] = v.TextureCoordinate.Y;
        
        _vertexCount++;
    }

    public void End()
    {
        Flush();
    }

    private unsafe void Flush()
    {
        if (_vertexCount == 0 || _currentTexture == null) return;
        
        // Setup state for text rendering
        _gl.Disable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.Blend);
        // Use premultiplied alpha blending for better text quality
        _gl.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
        
        _shader.Use();
        _shader.SetMat4("uTransform", _transform);
        _shader.SetInt("uTexture", 0);
        
        _gl.ActiveTexture(TextureUnit.Texture0);
        _currentTexture.Bind();
        
        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        
        // Upload vertex data
        fixed (float* ptr = _vertices)
        {
            _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, 
                (nuint)(_vertexCount * FloatsPerVertex * sizeof(float)), ptr);
        }
        
        // Draw
        int quadCount = _vertexCount / VerticesPerQuad;
        _gl.DrawElements(PrimitiveType.Triangles, (uint)(quadCount * IndicesPerQuad), 
            DrawElementsType.UnsignedShort, null);
        
        // Restore depth test
        _gl.Enable(EnableCap.DepthTest);
        
        _vertexCount = 0;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        _shader.Dispose();
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteBuffer(_ebo);
    }

    private const string TextVertexShader = @"
#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec4 aColor;
layout (location = 2) in vec2 aTexCoord;

uniform mat4 uTransform;

out vec4 vColor;
out vec2 vTexCoord;

void main()
{
    vColor = aColor;
    vTexCoord = aTexCoord;
    gl_Position = uTransform * vec4(aPosition, 1.0);
}
";

    private const string TextFragmentShader = @"
#version 330 core

in vec4 vColor;
in vec2 vTexCoord;

uniform sampler2D uTexture;

out vec4 FragColor;

void main()
{
    FragColor = vColor * texture(uTexture, vTexCoord);
}
";
}

