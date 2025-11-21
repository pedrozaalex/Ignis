using System.Numerics;
using Friflo.Engine.ECS;
using Ignis.Engine.ECS.Components;
using Ignis.Engine.Graphics.Components;
using Ignis.Engine.Graphics.Lighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace Ignis.Engine.Graphics.Systems;

/// <summary>
/// The bridge to GPU - renders all meshes using MonoGame
/// </summary>
public class RenderSystem(GraphicsDevice graphicsDevice)
{
    private LightSettings _lightSettings = LightSettings.Default;

    /// <summary>
    /// Updates the global lighting settings
    /// </summary>
    public void SetLightSettings(LightSettings settings)
    {
        _lightSettings = settings;
    }

    /// <summary>
    /// Renders all entities with MeshComponent and WorldTransform
    /// </summary>
    public void Draw(EntityStore world)
    {
        // Find the active camera
        var cameraQuery = world.Query<CameraComponent>();
        CameraComponent? activeCamera = null;

        foreach (var (cameras, entities) in cameraQuery.Chunks)
        {
            for (int i = 0; i < entities.Length; i++)
            {
                if (cameras[i].IsActive)
                {
                    activeCamera = cameras[i];
                    break;
                }
            }

            if (activeCamera.HasValue) break;
        }

        // If no active camera found, create a default one
        if (!activeCamera.HasValue)
        {
            var defaultCamera = new CameraComponent
            {
                FieldOfView = MathHelper.Pi / 3.0f,
                AspectRatio = (float)graphicsDevice.Viewport.Width / graphicsDevice.Viewport.Height,
                NearPlane = 0.1f,
                FarPlane = 1000f,
                IsActive = true,
                ViewMatrix = Matrix.CreateLookAt(new Vector3(0, 5, 10), Vector3.Zero, Vector3.Up),
                ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.Pi / 3.0f,
                    (float)graphicsDevice.Viewport.Width / graphicsDevice.Viewport.Height,
                    0.1f,
                    1000f
                )
            };
            activeCamera = defaultCamera;
        }

        var camera = activeCamera.Value;

        // Set global graphics device settings
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.BlendState = BlendState.Opaque;
        graphicsDevice.RasterizerState = RasterizerState.CullClockwise;
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

        // Query all renderable entities (MeshComponent + WorldTransform)
        var renderQuery = world.Query<MeshComponent, WorldTransform>();

        foreach (var (meshes, transforms, entities) in renderQuery.Chunks)
        {
            for (var i = 0; i < entities.Length; i++)
            {
                var meshComponent = meshes[i];
                var worldTransform = transforms[i];

                // Skip if model is null
                if (meshComponent.ModelRef == null)
                {
                    continue;
                }

                // Get optional material component
                var entity = entities[i];
                MaterialComponent? material = null;
                var hasMaterial = world.GetEntityById(entity).HasComponent<MaterialComponent>();
                if (hasMaterial)
                {
                    material = world.GetEntityById(entity).GetComponent<MaterialComponent>();
                }

                // Convert System.Numerics.Matrix4x4 to XNA Matrix
                var worldMatrix = ConvertToXnaMatrix(worldTransform.Value);

                // Draw the model
                DrawModel(meshComponent.ModelRef, worldMatrix, camera, hasMaterial ? material : null);
            }
        }
    }

    /// <summary>
    /// Draws a single model with the specified transforms
    /// </summary>
    private void DrawModel(Model model, Matrix world, CameraComponent camera, MaterialComponent? material)
    {
        foreach (var mesh in model.Meshes)
        {
            foreach (var effect in mesh.Effects)
            {
                var localWorld = mesh.ParentBone.Transform * world;

                if (effect is BasicEffect basicEffect)
                {
                    basicEffect.World = localWorld;
                    basicEffect.View = camera.ViewMatrix;
                    basicEffect.Projection = camera.ProjectionMatrix;

                    if (material.HasValue)
                    {
                        ApplyMaterialToBasicEffect(basicEffect, material.Value);
                    }
                }
                else
                {
                    effect.Parameters["World"]?.SetValue(localWorld);
                    effect.Parameters["View"]?.SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"]?.SetValue(camera.ProjectionMatrix);
                }
            }

            mesh.Draw();
        }
    }

    /// <summary>
    /// Safely sets a shader parameter if it exists
    /// </summary>
    private void SetEffectParameter(Effect effect, string paramName, Matrix value)
    {
        var param = effect.Parameters[paramName];
        if (param != null) param.SetValue(value);
    }

    // Moved detailed BasicEffect setup to a helper to keep DrawModel clean
    private void ApplyMaterialToBasicEffect(BasicEffect basicEffect, MaterialComponent mat)
    {
        basicEffect.DiffuseColor = mat.Color.ToVector3();
        basicEffect.Alpha = mat.Color.A / 255f;

        if (mat.Texture != null)
        {
            basicEffect.Texture = mat.Texture;
            basicEffect.TextureEnabled = true;
        }

        basicEffect.SpecularPower = mat.SpecularPower;
        basicEffect.SpecularColor = Vector3.One * 0.25f;

        if (mat.EnableLighting) ConfigureLighting(basicEffect);
        else basicEffect.LightingEnabled = false;
    }

    /// <summary>
    /// Configures lighting on a BasicEffect
    /// </summary>
    private void ConfigureLighting(BasicEffect effect)
    {
        effect.LightingEnabled = true;
        effect.PreferPerPixelLighting = true;

        // Ambient light
        effect.AmbientLightColor = _lightSettings.AmbientLightColor;

        // Directional light 0
        effect.DirectionalLight0.Enabled = true;
        effect.DirectionalLight0.Direction = _lightSettings.DirectionalLightDirection;
        effect.DirectionalLight0.DiffuseColor = _lightSettings.DirectionalLightColor;
        effect.DirectionalLight0.SpecularColor = _lightSettings.DirectionalLightColor;

        // Directional light 1
        effect.DirectionalLight1.Enabled = true;
        effect.DirectionalLight1.Direction = _lightSettings.DirectionalLight2Direction;
        effect.DirectionalLight1.DiffuseColor = _lightSettings.DirectionalLight2Color;
        effect.DirectionalLight1.SpecularColor = _lightSettings.DirectionalLight2Color;

        // Directional light 2
        effect.DirectionalLight2.Enabled = true;
        effect.DirectionalLight2.Direction = _lightSettings.DirectionalLight3Direction;
        effect.DirectionalLight2.DiffuseColor = _lightSettings.DirectionalLight3Color;
        effect.DirectionalLight2.SpecularColor = _lightSettings.DirectionalLight3Color;
    }

    /// <summary>
    /// Converts System.Numerics.Matrix4x4 to Microsoft.Xna.Framework.Matrix
    /// </summary>
    private static Matrix ConvertToXnaMatrix(Matrix4x4 matrix)
    {
        return new Matrix(
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44
        );
    }
}