using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Ignis.Engine.Core;
using Ignis.Engine.ECS.Components;
using Ignis.Engine.Graphics.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Position = Friflo.Engine.ECS.Position;
using Rotation = Friflo.Engine.ECS.Rotation;

namespace Ignis.Engine.Graphics.Systems;

/// <summary>
/// Calculates View and Projection matrices for all cameras based on their transforms
/// </summary>
public class CameraSystem(GraphicsDevice graphicsDevice) : QuerySystem<CameraComponent, WorldTransform>
{
    protected override void OnUpdate()
    {
        // Process all entities with Camera, Position, and Rotation
        Query.ForEachEntity((ref camera, ref wt, _) =>
        {
            var pos = wt.Value.Translation;
            wt.Value.ExtractRotation(out var rot);
            
            // Calculate the target point (forward direction from rotation)
            var forward = System.Numerics.Vector3.Transform(System.Numerics.Vector3.UnitZ, rot);
            var target = pos + forward;
            
            // Calculate the up vector
            var up = System.Numerics.Vector3.Transform(System.Numerics.Vector3.UnitY, rot);
            
            // Convert to XNA vectors
            var xnaPosition = new Vector3(pos.X, pos.Y, pos.Z);
            var xnaTarget = new Vector3(target.X, target.Y, target.Z);
            var xnaUp = new Vector3(up.X, up.Y, up.Z);
            
            // Create view matrix
            camera.ViewMatrix =  Matrix.CreateLookAt(xnaPosition, xnaTarget, xnaUp);
            
            // Create projection matrix
            camera.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                camera.FieldOfView,
                camera.AspectRatio,
                camera.NearPlane,
                camera.FarPlane
            );
            
            if (!camera.IsActive) return;

            camera.AspectRatio = (float)graphicsDevice.Viewport.Width / graphicsDevice.Viewport.Height;
        });
    }
}

