using System.Numerics;
using Friflo.Engine.ECS;
using Ignis.Engine.Core;
using Ignis.Engine.ECS;
using Ignis.Engine.ECS.Components;
using Ignis.Engine.ECS.Systems;
using Ignis.Engine.Graphics.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector3 = System.Numerics.Vector3;
using Quaternion = System.Numerics.Quaternion;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace Ignis.Samples;

/// <summary>
/// Phase 3 Sample: Spinning Cube with Orbiting Camera
/// Demonstrates rendering, camera system, materials, and lighting
/// </summary>
public class SpinningCubeSample() : IgnisGame(new IgnisApp(new EngineSettings
{
    WindowTitle = "Ignis Engine - Spinning Cube Sample",
    WindowWidth = 1280,
    WindowHeight = 720,
    VSync = true
}))
{
    private Entity _cubeEntity;
    private Entity _cameraPivot;
    private Entity _cameraEntity;
    private Model? _cubeModel;

    protected override void Initialize()
    {
        base.Initialize();

        // Add TransformSystem to the simulation root
        App.SimulationRoot.Add(new TransformSystem());
        Window.AllowUserResizing = true;

        Console.WriteLine("=== Phase 3 Sample: Spinning Cube ===");
        Console.WriteLine("Controls:");
        Console.WriteLine("  - Watch the cube spin on its own axis");
        Console.WriteLine("  - Watch the camera orbit around the cube");
        Console.WriteLine("  - Observe lighting and material effects");
        Console.WriteLine("=====================================\n");
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        // Generate cube.obj file if it doesn't exist
        var contentPath = Path.Combine(Directory.GetCurrentDirectory(), Content.RootDirectory);
        var cubeObjPath = Path.Combine(contentPath, "Cube.obj");
        var cubeXnbPath = Path.Combine(contentPath, "Cube.xnb");

        // Generate the .obj file
        if (!File.Exists(cubeObjPath))
        {
            Console.WriteLine("Generating Cube.obj...");
            CubeGenerator.GenerateCubeObj(contentPath);
        }

        // Build the content if .xnb doesn't exist
        if (!File.Exists(cubeXnbPath))
        {
            Console.WriteLine("Building Cube.xnb with MGCB...");
            var success = ContentBuilder.BuildModel(contentPath, "Cube.obj");

            if (!success)
            {
                Console.WriteLine("Warning: Could not build .xnb file automatically.");
                Console.WriteLine("Please run: dotnet mgcb -@:Content/Content.mgcb");
                Console.WriteLine("Or use the MGCB Editor to build the content.");
                Console.WriteLine("\nExiting...");
                Environment.Exit(1);
            }
        }

        // Try to load the compiled model
        if (_cubeModel == null)
        {
            try
            {
                _cubeModel = Content.Load<Model>("Cube");
                Console.WriteLine("Successfully loaded Cube model!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load Cube model: {ex.Message}");
                Console.WriteLine("\nExiting...");
                Environment.Exit(1);
            }
        }

        string shaderName = "ColorCubeShader.fx";
        string shaderPath = Path.Combine(contentPath, shaderName);
        if (!File.Exists(shaderPath))
        {
            // Write the HLSL string defined in Step 1 of this answer
            File.WriteAllText(shaderPath, GetShaderSourceCode());

            string GetShaderSourceCode()
            {
                return @"
#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;

struct VertexShaderInput
{
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
    
    // The Gradient Trick
	output.Color = float4((input.Position.xyz + 1.0) * 0.5, 1.0);
	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	return input.Color;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};";
            }

            // Update Build Pipeline
            // Note: We run this AFTER generating the OBJ, so the MGCB file contains both
            ContentBuilder.BuildEffect(contentPath, shaderName);
        }

        // 3. Load Assets
        _cubeModel = Content.Load<Model>("Cube");
        Effect colorShader = Content.Load<Effect>("ColorCubeShader");

        // 4. Apply Shader to Model
        // This replaces the default BasicEffect with our custom shader
        foreach (var mesh in _cubeModel.Meshes)
        {
            foreach (var part in mesh.MeshParts)
            {
                part.Effect = colorShader;
            }
        }

        // Setup the scene
        SetupScene();
    }

    private void SetupScene()
    {
        // Create the spinning cube entity
        _cubeEntity = App.World.CreateGameObject();
        _cubeEntity.Position.value = Vector3.Zero;
        _cubeEntity.Rotation.value = Quaternion.Identity;
        _cubeEntity.Scale3.value = new Vector3(1.5f, 1.5f, 1.5f);

        // Add mesh and material components
        _cubeEntity.Add(new MeshComponent(_cubeModel));
        var rgbTexture = new Texture2D(GraphicsDevice, 2, 2);
        rgbTexture.SetData([XnaColor.Red, XnaColor.Green, XnaColor.Blue, XnaColor.White]);
        _cubeEntity.Add(new MaterialComponent(texture: rgbTexture));

        // Create a pivot entity for the camera orbit (at origin)
        _cameraPivot = App.World.CreateGameObject();
        _cameraPivot.Position.value = Vector3.Zero;
        _cameraPivot.Rotation.value = Quaternion.Identity;
        _cameraPivot.Scale3.value = Vector3.One;

        // Create the camera as a child of the pivot
        _cameraEntity = App.World.CreateGameObject();
        _cameraEntity.Position.value = new Vector3(0, 0, 8); // Offset from pivot
        _cameraEntity.Rotation.value = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI); // Look at origin

        // Parent the camera to the pivot
        _cameraPivot.AddChild(_cameraEntity);

        // Add camera component
        _cameraEntity.Add(new CameraComponent(
            aspectRatio: (float)GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height,
            isActive: true
        ));
    }

    protected override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Rotate the cube around X and Z axes
        var cubeRotationSpeed = 1.0f; // radians per second
        var currentCubeRotation = _cubeEntity.Rotation.value;
        var rotationDeltaX = Quaternion.CreateFromAxisAngle(Vector3.UnitX, cubeRotationSpeed * deltaTime);
        var rotationDeltaZ = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, cubeRotationSpeed * deltaTime);
        _cubeEntity.Rotation.value = rotationDeltaZ * rotationDeltaX * currentCubeRotation;

        // Rotate the camera pivot to orbit the camera around the cube
        var cameraOrbitSpeed = 0.5f; // radians per second
        var currentPivotRotation = _cameraPivot.Rotation.value;
        var pivotRotationDelta = Quaternion.CreateFromAxisAngle(Vector3.UnitY, cameraOrbitSpeed * deltaTime);
        _cameraPivot.Rotation.value = pivotRotationDelta * currentPivotRotation;

        base.Update(gameTime);

        // Log status every 2 seconds
        if (!(gameTime.TotalGameTime.TotalSeconds % 2.0 < 0.016)) return;

        var cubeEuler = _cubeEntity.Rotation.value.ToEulerAngles();
        var cubeRotationAngle = cubeEuler.Y;
        Console.WriteLine(
            $"[{App.TotalTime:F2}s] Cube Angle: {cubeRotationAngle:F2} rad, Camera Position: {_cameraEntity.GetComponent<WorldTransform>().Value.Translation:F2}");
    }
}