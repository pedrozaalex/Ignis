using System.Numerics;

namespace Ignis.Gfx.Samples;

/// <summary>
/// Simple colored triangle sample using the rendering abstractions.
/// </summary>
public class TriangleSample : ISample
{
    public string Name => "Triangle";
    
    private MeshHandle _triangleMesh;
    private float _rotation;
    
    public void Load(IRenderingServer server)
    {
        // Create a simple triangle with vertex colors
        var vertices = new Vertex3D[]
        {
            new(new Vector3(0.0f, 0.5f, 0.0f), Vector3.UnitZ, new Vector2(0.5f, 0.0f), Color4.Red),
            new(new Vector3(-0.5f, -0.5f, 0.0f), Vector3.UnitZ, new Vector2(0.0f, 1.0f), Color4.Green),
            new(new Vector3(0.5f, -0.5f, 0.0f), Vector3.UnitZ, new Vector2(1.0f, 1.0f), Color4.Blue),
        };
        
        var indices = new uint[] { 0, 1, 2 };
        
        var meshData = new MeshData(vertices, indices);
        _triangleMesh = server.CreateMesh(meshData);
    }
    
    public void Update(double deltaTime)
    {
        _rotation += (float)deltaTime * 45f;
    }
    
    public void Render(IRenderingServer server, int width, int height)
    {
        // Use the abstraction layer - no direct GL access
        var pass = new RenderPass
        {
            Target = RenderTargetHandle.Screen,
            ClearColor = new Color4(0.2f, 0.3f, 0.3f, 1.0f),
            ClearDepth = true,
            Viewport = new Rect(0, 0, width, height)
        };
        
        server.BeginPass(pass);
        
        // Setup matrices
        float aspect = (float)width / height;
        var projection = Matrix4x4.CreateOrthographicOffCenter(-aspect, aspect, -1f, 1f, -10f, 10f);
        var view = Matrix4x4.Identity;
        var model = Matrix4x4.CreateRotationZ(_rotation * MathF.PI / 180f);
        
        // Use command list
        var commands = server.CreateCommandList();
        commands.SetPipeline(server.DefaultShader3D);
        commands.SetProjectionMatrix(projection);
        commands.SetViewMatrix(view);
        commands.DrawMesh(_triangleMesh, model);
        server.Submit(commands);
        
        server.EndPass();
    }
    
    public void Dispose()
    {
    }
}

