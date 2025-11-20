using ObjParser;
using ObjParser.Types;

namespace Ignis.Samples;

/// <summary>
/// Generates a cube .obj file for use in samples
/// </summary>
public static class CubeGenerator
{
    /// <summary>
    /// Creates a cube.obj file in the Content directory with RGB vertex colors
    /// </summary>
    public static void GenerateCubeObj(string contentPath)
    {
        var obj = new ObjModel();
        
        // Add vertices for a unit cube (centered at origin, 2x2x2)
        // Each vertex gets a unique color based on its position in RGB space
        // Coordinates map: -1 -> 0.0, +1 -> 1.0 for color components
        
        // Front face (Z-)
        int v0 = obj.AddVertex(new Vertex { X = -1, Y = -1, Z = -1 }); // Black (0,0,0)
        int v1 = obj.AddVertex(new Vertex { X = 1, Y = -1, Z = -1 });  // Red (1,0,0)
        int v2 = obj.AddVertex(new Vertex { X = 1, Y = 1, Z = -1 });   // Yellow (1,1,0)
        int v3 = obj.AddVertex(new Vertex { X = -1, Y = 1, Z = -1 });  // Green (0,1,0)
        
        // Back face (Z+)
        int v4 = obj.AddVertex(new Vertex { X = -1, Y = -1, Z = 1 });  // Blue (0,0,1)
        int v5 = obj.AddVertex(new Vertex { X = 1, Y = -1, Z = 1 });   // Magenta (1,0,1)
        int v6 = obj.AddVertex(new Vertex { X = 1, Y = 1, Z = 1 });    // White (1,1,1)
        int v7 = obj.AddVertex(new Vertex { X = -1, Y = 1, Z = 1 });   // Cyan (0,1,1)
        
        // Add normals
        int nFront = obj.AddNormal(new VertexNormal { I = 0, J = 0, K = -1 });
        int nBack = obj.AddNormal(new VertexNormal { I = 0, J = 0, K = 1 });
        int nTop = obj.AddNormal(new VertexNormal { I = 0, J = 1, K = 0 });
        int nBottom = obj.AddNormal(new VertexNormal { I = 0, J = -1, K = 0 });
        int nLeft = obj.AddNormal(new VertexNormal { I = -1, J = 0, K = 0 });
        int nRight = obj.AddNormal(new VertexNormal { I = 1, J = 0, K = 0 });
        
        // Add texture coordinates (basic UV mapping)
        int t0 = obj.AddTextureVertex(new TextureVertex { X = 0, Y = 1 });
        int t1 = obj.AddTextureVertex(new TextureVertex { X = 1, Y = 1 });
        int t2 = obj.AddTextureVertex(new TextureVertex { X = 1, Y = 0 });
        int t3 = obj.AddTextureVertex(new TextureVertex { X = 0, Y = 0 });
        
        obj.SetObjectName("Cube");
        obj.SetGroups("Cube");
        
        // Front face
        obj.AddFace(new Face
        {
            VertexIndexList = [v0, v1, v2],
            TextureVertexIndexList = [t0, t1, t2],
            NormalIndexList = [nFront, nFront, nFront]
        });
        obj.AddFace(new Face
        {
            VertexIndexList = [v0, v2, v3],
            TextureVertexIndexList = [t0, t2, t3],
            NormalIndexList = [nFront, nFront, nFront]
        });
        
        // Back face
        obj.AddFace(new Face
        {
            VertexIndexList = [v5, v4, v7],
            TextureVertexIndexList = [t0, t1, t2],
            NormalIndexList = [nBack, nBack, nBack]
        });
        obj.AddFace(new Face
        {
            VertexIndexList = [v5, v7, v6],
            TextureVertexIndexList = [t0, t2, t3],
            NormalIndexList = [nBack, nBack, nBack]
        });
        
        // Top face
        obj.AddFace(new Face
        {
            VertexIndexList = [v3, v2, v6],
            TextureVertexIndexList = [t0, t1, t2],
            NormalIndexList = [nTop, nTop, nTop]
        });
        obj.AddFace(new Face
        {
            VertexIndexList = [v3, v6, v7],
            TextureVertexIndexList = [t0, t2, t3],
            NormalIndexList = [nTop, nTop, nTop]
        });
        
        // Bottom face
        obj.AddFace(new Face
        {
            VertexIndexList = [v4, v5, v1],
            TextureVertexIndexList = [t0, t1, t2],
            NormalIndexList = [nBottom, nBottom, nBottom]
        });
        obj.AddFace(new Face
        {
            VertexIndexList = [v4, v1, v0],
            TextureVertexIndexList = [t0, t2, t3],
            NormalIndexList = [nBottom, nBottom, nBottom]
        });
        
        // Left face
        obj.AddFace(new Face
        {
            VertexIndexList = [v4, v0, v3],
            TextureVertexIndexList = [t0, t1, t2],
            NormalIndexList = [nLeft, nLeft, nLeft]
        });
        obj.AddFace(new Face
        {
            VertexIndexList = [v4, v3, v7],
            TextureVertexIndexList = [t0, t2, t3],
            NormalIndexList = [nLeft, nLeft, nLeft]
        });
        
        // Right face
        obj.AddFace(new Face
        {
            VertexIndexList = [v1, v5, v6],
            TextureVertexIndexList = [t0, t1, t2],
            NormalIndexList = [nRight, nRight, nRight]
        });
        obj.AddFace(new Face
        {
            VertexIndexList = [v1, v6, v2],
            TextureVertexIndexList = [t0, t2, t3],
            NormalIndexList = [nRight, nRight, nRight]
        });
        
        // Ensure Content directory exists
        Directory.CreateDirectory(contentPath);
        
        // Save the .obj file
        string objPath = Path.Combine(contentPath, "Cube.obj");
        obj.Save(objPath, ["Generated by Ignis Engine CubeGenerator"]);
        
        Console.WriteLine($"Generated cube.obj at: {objPath}");
        Console.WriteLine($"Vertices: {obj.Vertices.Count}, Faces: {obj.Faces.Count}");
    }
}

