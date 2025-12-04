using System.Numerics;
using Ignis.Graphics;
using Samples.Common;

namespace Samples.Triangle;

/// <summary>
/// Simple colored triangle sample using the rendering abstractions.
/// </summary>
public class TriangleSample : GraphicsSample
{
    public override string Name => "Triangle";
    
    private MeshHandle _triangleMesh;
    private float _rotation;
    
    protected override void Load()
    {
        var vertices = new Vertex3D[]
        {
            new(new Vector3(0.0f, 0.5f, 0.0f), Vector3.UnitZ, new Vector2(0.5f, 0.0f), Color4.Red),
            new(new Vector3(-0.5f, -0.5f, 0.0f), Vector3.UnitZ, new Vector2(0.0f, 1.0f), Color4.Green),
            new(new Vector3(0.5f, -0.5f, 0.0f), Vector3.UnitZ, new Vector2(1.0f, 1.0f), Color4.Blue),
        };
        
        var indices = new uint[] { 0, 1, 2 };
        _triangleMesh = RenderingServer.CreateMesh(new MeshData(vertices, indices));
    }
    
    protected override void OnUpdate(float deltaTime)
    {
        _rotation += deltaTime * 45f;
    }
    
    public override void Render(float alpha)
    {
        var pass = new RenderPass
        {
            Target = RenderTargetHandle.Screen,
            ClearColor = new Color4(0.2f, 0.3f, 0.3f),
            ClearDepth = true,
            Viewport = new Rect(0, 0, Width, Height)
        };
        
        RenderingServer.BeginPass(pass);
        
        var aspect = (float)Width / Height;
        var projection = Matrix4x4.CreateOrthographicOffCenter(-aspect, aspect, -1f, 1f, -10f, 10f);
        var view = Matrix4x4.Identity;
        var model = Matrix4x4.CreateRotationZ(_rotation * MathF.PI / 180f);
        
        var commands = RenderingServer.CreateCommandList();
        commands.SetPipeline(RenderingServer.DefaultShader3D);
        commands.SetProjectionMatrix(projection);
        commands.SetViewMatrix(view);
        commands.DrawMesh(_triangleMesh, model);
        RenderingServer.Submit(commands);
        
        RenderingServer.EndPass();
    }
}

