using System.Numerics;
using System.Runtime.InteropServices;

namespace Ignis.Graphics;

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
    
    /// <summary>Creates a ground plane on the XZ plane.</summary>
    public static MeshData CreatePlane(float width = 10f, float depth = 10f)
    {
        var hw = width * 0.5f;
        var hd = depth * 0.5f;
        
        var builder = new MeshBuilder();
        builder.AddVertex(new Vector3(-hw, 0, -hd), Vector3.UnitY, new Vector2(0, 0), Color4.White);
        builder.AddVertex(new Vector3(hw, 0, -hd), Vector3.UnitY, new Vector2(1, 0), Color4.White);
        builder.AddVertex(new Vector3(hw, 0, hd), Vector3.UnitY, new Vector2(1, 1), Color4.White);
        builder.AddVertex(new Vector3(-hw, 0, hd), Vector3.UnitY, new Vector2(0, 1), Color4.White);
        builder.AddQuad(0, 2, 1, 0);
        builder.AddQuad(0, 3, 2, 0);
        
        return builder.Build();
    }
    
    /// <summary>Creates a UV sphere.</summary>
    public static MeshData CreateSphere(float radius = 0.5f, int segments = 16, int rings = 16)
    {
        var builder = new MeshBuilder();
        
        for (int ring = 0; ring <= rings; ring++)
        {
            float phi = MathF.PI * ring / rings;
            float y = MathF.Cos(phi) * radius;
            float ringRadius = MathF.Sin(phi) * radius;
            
            for (int seg = 0; seg <= segments; seg++)
            {
                float theta = 2f * MathF.PI * seg / segments;
                float x = MathF.Cos(theta) * ringRadius;
                float z = MathF.Sin(theta) * ringRadius;
                
                var pos = new Vector3(x, y, z);
                var normal = Vector3.Normalize(pos);
                var uv = new Vector2((float)seg / segments, (float)ring / rings);
                
                builder.AddVertex(pos, normal, uv, Color4.White);
            }
        }
        
        for (int ring = 0; ring < rings; ring++)
        {
            for (int seg = 0; seg < segments; seg++)
            {
                uint current = (uint)(ring * (segments + 1) + seg);
                uint next = current + 1;
                uint below = current + (uint)(segments + 1);
                uint belowNext = below + 1;
                
                builder.AddTriangle(current, below, next);
                builder.AddTriangle(next, below, belowNext);
            }
        }
        
        return builder.Build();
    }
    
    /// <summary>Creates a cylinder along the Y axis.</summary>
    public static MeshData CreateCylinder(float radius = 0.5f, float height = 1f, int segments = 16)
    {
        var builder = new MeshBuilder();
        float halfHeight = height * 0.5f;
        
        // Side vertices
        for (int i = 0; i <= segments; i++)
        {
            float theta = 2f * MathF.PI * i / segments;
            float x = MathF.Cos(theta) * radius;
            float z = MathF.Sin(theta) * radius;
            var normal = Vector3.Normalize(new Vector3(x, 0, z));
            float u = (float)i / segments;
            
            builder.AddVertex(new Vector3(x, -halfHeight, z), normal, new Vector2(u, 1), Color4.White);
            builder.AddVertex(new Vector3(x, halfHeight, z), normal, new Vector2(u, 0), Color4.White);
        }
        
        // Side indices
        for (int i = 0; i < segments; i++)
        {
            uint bl = (uint)(i * 2);
            uint tl = bl + 1;
            uint br = bl + 2;
            uint tr = bl + 3;
            
            builder.AddTriangle(bl, br, tl);
            builder.AddTriangle(tl, br, tr);
        }
        
        // Top cap center
        uint topCenter = builder.VertexCount;
        builder.AddVertex(new Vector3(0, halfHeight, 0), Vector3.UnitY, new Vector2(0.5f, 0.5f), Color4.White);
        
        uint topStart = builder.VertexCount;
        for (int i = 0; i <= segments; i++)
        {
            float theta = 2f * MathF.PI * i / segments;
            float x = MathF.Cos(theta) * radius;
            float z = MathF.Sin(theta) * radius;
            builder.AddVertex(new Vector3(x, halfHeight, z), Vector3.UnitY,
                new Vector2(0.5f + x / radius * 0.5f, 0.5f + z / radius * 0.5f), Color4.White);
        }
        
        for (int i = 0; i < segments; i++)
            builder.AddTriangle(topCenter, topStart + (uint)i, topStart + (uint)i + 1);
        
        // Bottom cap center
        uint bottomCenter = builder.VertexCount;
        builder.AddVertex(new Vector3(0, -halfHeight, 0), -Vector3.UnitY, new Vector2(0.5f, 0.5f), Color4.White);
        
        uint bottomStart = builder.VertexCount;
        for (int i = 0; i <= segments; i++)
        {
            float theta = 2f * MathF.PI * i / segments;
            float x = MathF.Cos(theta) * radius;
            float z = MathF.Sin(theta) * radius;
            builder.AddVertex(new Vector3(x, -halfHeight, z), -Vector3.UnitY,
                new Vector2(0.5f + x / radius * 0.5f, 0.5f + z / radius * 0.5f), Color4.White);
        }
        
        for (int i = 0; i < segments; i++)
            builder.AddTriangle(bottomCenter, bottomStart + (uint)i + 1, bottomStart + (uint)i);
        
        return builder.Build();
    }
}

