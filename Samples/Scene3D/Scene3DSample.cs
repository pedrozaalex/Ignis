using System.Numerics;
using Ignis.Core;
using Ignis.Graphics;
using Ignis.Graphics.Backends.OpenGL;
using Samples.Common;
using Silk.NET.Input;

namespace Samples.Scene3D;

/// <summary>
/// 3D scene sample with camera navigation and lit objects.
/// </summary>
public class Scene3DSample : GraphicsSample
{
    public override string Name => "3D Scene";

    private FirstPersonCamera _camera = null!;
    private MeshHandle _cubeMesh;
    private MeshHandle _sphereMesh;
    private MeshHandle _cylinderMesh;
    private MeshHandle _planeMesh;

    private Vector2 _lastMousePos;
    private bool _mouseCaptured;
    private float _time;

    // Light settings
    private Vector3 _lightDir = Vector3.Normalize(new Vector3(-0.5f, -1f, -0.3f));
    private Vector3 _lightColor = new Vector3(1f, 0.95f, 0.9f);
    private Vector3 _ambientColor = new Vector3(0.15f, 0.15f, 0.2f);

    protected override void Load()
    {
        _camera = new FirstPersonCamera
        {
            Position = new Vector3(0, 3, 8),
            Pitch = -15f
        };

        // Create meshes with different colors for different "materials"
        _planeMesh = RenderingServer.CreateMesh(CreateColoredPlane(20f, 20f, new Color4(0.3f, 0.3f, 0.35f)));
        _cubeMesh = RenderingServer.CreateMesh(MeshBuilder.CreateCube());
        _sphereMesh = RenderingServer.CreateMesh(MeshBuilder.CreateSphere(0.5f, 24, 24));
        _cylinderMesh = RenderingServer.CreateMesh(MeshBuilder.CreateCylinder(0.4f, 1.2f, 24));
    }

    private MeshData CreateColoredPlane(float width, float depth, Color4 color)
    {
        var hw = width * 0.5f;
        var hd = depth * 0.5f;

        var vertices = new Vertex3D[]
        {
            new(new Vector3(-hw, 0, -hd), Vector3.UnitY, new Vector2(0, 0), color),
            new(new Vector3(hw, 0, -hd), Vector3.UnitY, new Vector2(width/2, 0), color),
            new(new Vector3(hw, 0, hd), Vector3.UnitY, new Vector2(width/2, depth/2), color),
            new(new Vector3(-hw, 0, hd), Vector3.UnitY, new Vector2(0, depth/2), color),
        };

        var indices = new uint[] { 0, 2, 1, 0, 3, 2 };
        return new MeshData(vertices, indices);
    }

    protected override void OnUpdate(float deltaTime)
    {
        _time += deltaTime;
        HandleInput(deltaTime);
    }

    private void HandleInput(float deltaTime)
    {
        var input = Context?.GetInput();
        if (input == null) return;

        // Toggle mouse capture with right click
        if (input.IsMousePressed(MouseButton.Right))
        {
            _mouseCaptured = !_mouseCaptured;
        }

        // Camera movement
        bool forward = input.IsKeyDown(Key.W);
        bool backward = input.IsKeyDown(Key.S);
        bool left = input.IsKeyDown(Key.A);
        bool right = input.IsKeyDown(Key.D);
        bool up = input.IsKeyDown(Key.Space);
        bool down = input.IsKeyDown(Key.ShiftLeft);

        // Speed boost with Ctrl
        float speedMultiplier = input.IsKeyDown(Key.ControlLeft) ? 3f : 1f;
        _camera.MoveSpeed = 5f * speedMultiplier;

        _camera.ProcessKeyboard(forward, backward, left, right, up, down, deltaTime);

        // Mouse look when captured
        if (_mouseCaptured)
        {
            var mousePos = input.MousePosition;
            var delta = mousePos - _lastMousePos;
            _camera.ProcessMouseMovement(delta.X, delta.Y);
            _lastMousePos = mousePos;
        }
        else
        {
            _lastMousePos = input.MousePosition;
        }
    }

    public override void Render(float alpha)
    {
        var pass = new RenderPass
        {
            Target = RenderTargetHandle.Screen,
            ClearColor = new Color4(0.1f, 0.1f, 0.15f),
            ClearDepth = true,
            Viewport = new Rect(0, 0, Width, Height)
        };

        RenderingServer.BeginPass(pass);

        var commands = RenderingServer.CreateCommandList();

        float aspect = (float)Width / Height;
        var projection = _camera.GetProjectionMatrix(aspect);
        var view = _camera.GetViewMatrix();

        // Use lit shader
        if (RenderingServer is OpenGLRenderingServer glServer)
        {
            commands.SetPipeline(glServer.DefaultShader3DLit);
        }
        else
        {
            commands.SetPipeline(RenderingServer.DefaultShader3D);
        }

        commands.SetProjectionMatrix(projection);
        commands.SetViewMatrix(view);

        // Set lighting uniforms
        commands.SetUniform("uLightDir", _lightDir);
        commands.SetUniform("uLightColor", _lightColor);
        commands.SetUniform("uAmbientColor", _ambientColor);
        commands.SetUniform("uViewPos", _camera.Position);

        // Draw ground plane
        commands.SetUniform("uMaterialColor", new Color4(0, 0, 0, 0)); // Use vertex color for plane
        commands.DrawMesh(_planeMesh, Matrix4x4.Identity);

        // Draw objects in a grid with different colors
        DrawColoredCube(commands, new Vector3(-3, 0.5f, -3), new Color4(0.8f, 0.2f, 0.2f)); // Red
        DrawColoredCube(commands, new Vector3(0, 0.5f, -3), new Color4(0.2f, 0.8f, 0.2f));  // Green
        DrawColoredCube(commands, new Vector3(3, 0.5f, -3), new Color4(0.2f, 0.2f, 0.8f));  // Blue

        DrawColoredSphere(commands, new Vector3(-3, 0.5f, 0), new Color4(0.9f, 0.7f, 0.1f)); // Gold
        DrawColoredSphere(commands, new Vector3(0, 0.5f, 0), new Color4(0.8f, 0.8f, 0.8f));  // Silver
        DrawColoredSphere(commands, new Vector3(3, 0.5f, 0), new Color4(0.7f, 0.3f, 0.7f));  // Purple

        DrawColoredCylinder(commands, new Vector3(-3, 0.6f, 3), new Color4(0.2f, 0.7f, 0.7f)); // Cyan
        DrawColoredCylinder(commands, new Vector3(0, 0.6f, 3), new Color4(0.9f, 0.5f, 0.2f));  // Orange
        DrawColoredCylinder(commands, new Vector3(3, 0.6f, 3), new Color4(0.5f, 0.8f, 0.3f));  // Lime

        // Rotating cube in the center
        float rotY = _time * 45f * MathF.PI / 180f;
        var rotatingTransform = Matrix4x4.CreateRotationY(rotY) *
                                Matrix4x4.CreateTranslation(0, 3f + MathF.Sin(_time * 2f) * 0.3f, 0);
        DrawColoredMesh(commands, _cubeMesh, rotatingTransform, new Color4(1f, 1f, 1f));

        RenderingServer.Submit(commands);
        RenderingServer.EndPass();
    }

    private void DrawColoredCube(IRenderCommandList commands, Vector3 position, Color4 color)
    {
        // We need to set up the mesh with the color - for now using vertex colors
        var transform = Matrix4x4.CreateTranslation(position);
        DrawColoredMesh(commands, _cubeMesh, transform, color);
    }

    private void DrawColoredSphere(IRenderCommandList commands, Vector3 position, Color4 color)
    {
        var transform = Matrix4x4.CreateTranslation(position);
        DrawColoredMesh(commands, _sphereMesh, transform, color);
    }

    private void DrawColoredCylinder(IRenderCommandList commands, Vector3 position, Color4 color)
    {
        var transform = Matrix4x4.CreateTranslation(position);
        DrawColoredMesh(commands, _cylinderMesh, transform, color);
    }

    private void DrawColoredMesh(IRenderCommandList commands, MeshHandle mesh, Matrix4x4 transform, Color4 color)
    {
        // Set material color uniform before drawing
        commands.SetUniform("uMaterialColor", color);
        commands.DrawMesh(mesh, transform);
    }

    protected override void Unload()
    {
        RenderingServer.DestroyMesh(_planeMesh);
        RenderingServer.DestroyMesh(_cubeMesh);
        RenderingServer.DestroyMesh(_sphereMesh);
        RenderingServer.DestroyMesh(_cylinderMesh);
    }
}

