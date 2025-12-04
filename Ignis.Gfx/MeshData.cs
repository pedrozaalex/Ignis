using System.Numerics;
using System.Runtime.InteropServices;

namespace Ignis.Gfx;

/// <summary>Standard vertex format for 3D meshes.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct Vertex3D
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TexCoord;
    public Color4 Color;
    
    public Vertex3D(Vector3 position, Vector3 normal, Vector2 texCoord, Color4 color)
    {
        Position = position;
        Normal = normal;
        TexCoord = texCoord;
        Color = color;
    }
    
    public Vertex3D(Vector3 position, Vector3 normal, Vector2 texCoord)
        : this(position, normal, texCoord, Color4.White) { }
    
    public Vertex3D(Vector3 position)
        : this(position, Vector3.UnitY, Vector2.Zero, Color4.White) { }
    
    public static int SizeInBytes => Marshal.SizeOf<Vertex3D>();
}

/// <summary>Vertex format for 2D sprites/UI.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct Vertex2D
{
    public Vector2 Position;
    public Vector2 TexCoord;
    public Color4 Color;
    
    public Vertex2D(Vector2 position, Vector2 texCoord, Color4 color)
    {
        Position = position;
        TexCoord = texCoord;
        Color = color;
    }
    
    public static int SizeInBytes => Marshal.SizeOf<Vertex2D>();
}

/// <summary>Mesh data to upload to GPU.</summary>
public class MeshData
{
    public Vertex3D[] Vertices { get; }
    public uint[] Indices { get; }
    
    public MeshData(Vertex3D[] vertices, uint[] indices)
    {
        Vertices = vertices;
        Indices = indices;
    }
    
    public int VertexCount => Vertices.Length;
    public int IndexCount => Indices.Length;
    public int TriangleCount => Indices.Length / 3;
    
    /// <summary>Recalculates normals based on triangle faces.</summary>
    public void RecalculateNormals()
    {
        for (var i = 0; i < Vertices.Length; i++)
            Vertices[i].Normal = Vector3.Zero;
        
        for (var i = 0; i < Indices.Length; i += 3)
        {
            var i0 = (int)Indices[i];
            var i1 = (int)Indices[i + 1];
            var i2 = (int)Indices[i + 2];
            
            var v0 = Vertices[i0].Position;
            var v1 = Vertices[i1].Position;
            var v2 = Vertices[i2].Position;
            
            var faceNormal = Vector3.Cross(v1 - v0, v2 - v0);
            
            Vertices[i0].Normal += faceNormal;
            Vertices[i1].Normal += faceNormal;
            Vertices[i2].Normal += faceNormal;
        }
        
        for (var i = 0; i < Vertices.Length; i++)
        {
            var len = Vertices[i].Normal.Length();
            if (len > 0.0001f)
                Vertices[i].Normal /= len;
        }
    }
}

/// <summary>Helper to build mesh data procedurally.</summary>
public class MeshBuilder
{
    private readonly List<Vertex3D> _vertices = new();
    private readonly List<uint> _indices = new();
    
    public MeshBuilder AddVertex(Vertex3D vertex)
    {
        _vertices.Add(vertex);
        return this;
    }
    
    public MeshBuilder AddVertex(Vector3 position, Vector3 normal, Vector2 texCoord, Color4 color)
    {
        _vertices.Add(new Vertex3D(position, normal, texCoord, color));
        return this;
    }
    
    public MeshBuilder AddTriangle(uint i0, uint i1, uint i2)
    {
        _indices.Add(i0);
        _indices.Add(i1);
        _indices.Add(i2);
        return this;
    }
    
    public MeshBuilder AddQuad(uint i0, uint i1, uint i2, uint i3)
    {
        _indices.Add(i0); _indices.Add(i1); _indices.Add(i2);
        _indices.Add(i0); _indices.Add(i2); _indices.Add(i3);
        return this;
    }
    
    public MeshData Build() => new(_vertices.ToArray(), _indices.ToArray());
    
    public void Clear()
    {
        _vertices.Clear();
        _indices.Clear();
    }
    
    public uint VertexCount => (uint)_vertices.Count;
    
    /// <summary>Creates a unit cube centered at origin.</summary>
    public static MeshData CreateCube()
    {
        var builder = new MeshBuilder();
        
        // Front face (z = 0.5)
        builder.AddVertex(new Vector3(-0.5f, -0.5f, 0.5f), Vector3.UnitZ, new Vector2(0, 1), Color4.White);
        builder.AddVertex(new Vector3(0.5f, -0.5f, 0.5f), Vector3.UnitZ, new Vector2(1, 1), Color4.White);
        builder.AddVertex(new Vector3(0.5f, 0.5f, 0.5f), Vector3.UnitZ, new Vector2(1, 0), Color4.White);
        builder.AddVertex(new Vector3(-0.5f, 0.5f, 0.5f), Vector3.UnitZ, new Vector2(0, 0), Color4.White);
        builder.AddQuad(0, 1, 2, 3);
        
        // Back face (z = -0.5)
        builder.AddVertex(new Vector3(0.5f, -0.5f, -0.5f), -Vector3.UnitZ, new Vector2(0, 1), Color4.White);
        builder.AddVertex(new Vector3(-0.5f, -0.5f, -0.5f), -Vector3.UnitZ, new Vector2(1, 1), Color4.White);
        builder.AddVertex(new Vector3(-0.5f, 0.5f, -0.5f), -Vector3.UnitZ, new Vector2(1, 0), Color4.White);
        builder.AddVertex(new Vector3(0.5f, 0.5f, -0.5f), -Vector3.UnitZ, new Vector2(0, 0), Color4.White);
        builder.AddQuad(4, 5, 6, 7);
        
        // Top face (y = 0.5)
        builder.AddVertex(new Vector3(-0.5f, 0.5f, 0.5f), Vector3.UnitY, new Vector2(0, 1), Color4.White);
        builder.AddVertex(new Vector3(0.5f, 0.5f, 0.5f), Vector3.UnitY, new Vector2(1, 1), Color4.White);
        builder.AddVertex(new Vector3(0.5f, 0.5f, -0.5f), Vector3.UnitY, new Vector2(1, 0), Color4.White);
        builder.AddVertex(new Vector3(-0.5f, 0.5f, -0.5f), Vector3.UnitY, new Vector2(0, 0), Color4.White);
        builder.AddQuad(8, 9, 10, 11);
        
        // Bottom face (y = -0.5)
        builder.AddVertex(new Vector3(-0.5f, -0.5f, -0.5f), -Vector3.UnitY, new Vector2(0, 1), Color4.White);
        builder.AddVertex(new Vector3(0.5f, -0.5f, -0.5f), -Vector3.UnitY, new Vector2(1, 1), Color4.White);
        builder.AddVertex(new Vector3(0.5f, -0.5f, 0.5f), -Vector3.UnitY, new Vector2(1, 0), Color4.White);
        builder.AddVertex(new Vector3(-0.5f, -0.5f, 0.5f), -Vector3.UnitY, new Vector2(0, 0), Color4.White);
        builder.AddQuad(12, 13, 14, 15);
        
        // Right face (x = 0.5)
        builder.AddVertex(new Vector3(0.5f, -0.5f, 0.5f), Vector3.UnitX, new Vector2(0, 1), Color4.White);
        builder.AddVertex(new Vector3(0.5f, -0.5f, -0.5f), Vector3.UnitX, new Vector2(1, 1), Color4.White);
        builder.AddVertex(new Vector3(0.5f, 0.5f, -0.5f), Vector3.UnitX, new Vector2(1, 0), Color4.White);
        builder.AddVertex(new Vector3(0.5f, 0.5f, 0.5f), Vector3.UnitX, new Vector2(0, 0), Color4.White);
        builder.AddQuad(16, 17, 18, 19);
        
        // Left face (x = -0.5)
        builder.AddVertex(new Vector3(-0.5f, -0.5f, -0.5f), -Vector3.UnitX, new Vector2(0, 1), Color4.White);
        builder.AddVertex(new Vector3(-0.5f, -0.5f, 0.5f), -Vector3.UnitX, new Vector2(1, 1), Color4.White);
        builder.AddVertex(new Vector3(-0.5f, 0.5f, 0.5f), -Vector3.UnitX, new Vector2(1, 0), Color4.White);
        builder.AddVertex(new Vector3(-0.5f, 0.5f, -0.5f), -Vector3.UnitX, new Vector2(0, 0), Color4.White);
        builder.AddQuad(20, 21, 22, 23);
        
        return builder.Build();
    }
    
    /// <summary>Creates a quad on the XY plane.</summary>
    public static MeshData CreateQuad(float width = 1f, float height = 1f)
    {
        var hw = width * 0.5f;
        var hh = height * 0.5f;
        
        var builder = new MeshBuilder();
        builder.AddVertex(new Vector3(-hw, -hh, 0), Vector3.UnitZ, new Vector2(0, 1), Color4.White);
        builder.AddVertex(new Vector3(hw, -hh, 0), Vector3.UnitZ, new Vector2(1, 1), Color4.White);
        builder.AddVertex(new Vector3(hw, hh, 0), Vector3.UnitZ, new Vector2(1, 0), Color4.White);
        builder.AddVertex(new Vector3(-hw, hh, 0), Vector3.UnitZ, new Vector2(0, 0), Color4.White);
        builder.AddQuad(0, 1, 2, 3);
        
        return builder.Build();
    }
}

