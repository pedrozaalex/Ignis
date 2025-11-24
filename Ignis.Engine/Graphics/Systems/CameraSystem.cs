using Friflo.Engine.ECS.Systems;
using Ignis.Engine.Core;
using Ignis.Engine.ECS.Components;
using Ignis.Engine.Graphics.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector3 = System.Numerics.Vector3;

namespace Ignis.Engine.Graphics.Systems;

/// <summary>
///     Calculates View and Projection matrices for all cameras based on their transforms
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
            var forward = Vector3.Transform(Vector3.UnitZ, rot);
            var target = pos + forward;

            // Calculate the up vector
            var up = Vector3.Transform(Vector3.UnitY, rot);

            // Convert to XNA vectors
            var xnaPosition = new Microsoft.Xna.Framework.Vector3(pos.X, pos.Y, pos.Z);
            var xnaTarget = new Microsoft.Xna.Framework.Vector3(target.X, target.Y, target.Z);
            var xnaUp = new Microsoft.Xna.Framework.Vector3(up.X, up.Y, up.Z);

            // Create view matrix
            camera.ViewMatrix = Matrix.CreateLookAt(xnaPosition, xnaTarget, xnaUp);

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