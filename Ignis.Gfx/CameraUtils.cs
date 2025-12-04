using System.Numerics;

namespace Ignis.Gfx;

/// <summary>
/// Utility class for common camera matrix calculations.
/// </summary>
public static class CameraUtils
{
    /// <summary>Create an orthographic projection for 2D UI (0,0 at top-left).</summary>
    public static Matrix4x4 CreateOrthographicUI(float width, float height)
    {
        return Matrix4x4.CreateOrthographicOffCenter(0, width, height, 0, 0, 1);
    }
    
    /// <summary>Create an orthographic projection for 2D (0,0 at center).</summary>
    public static Matrix4x4 CreateOrthographicCentered(float width, float height)
    {
        var hw = width * 0.5f;
        var hh = height * 0.5f;
        return Matrix4x4.CreateOrthographicOffCenter(-hw, hw, -hh, hh, -1, 1);
    }
    
    /// <summary>Create a perspective projection.</summary>
    public static Matrix4x4 CreatePerspective(float fovDegrees, float aspectRatio, float nearPlane = 0.1f, float farPlane = 1000f)
    {
        var fovRadians = fovDegrees * MathF.PI / 180f;
        return Matrix4x4.CreatePerspectiveFieldOfView(fovRadians, aspectRatio, nearPlane, farPlane);
    }
    
    /// <summary>Create a look-at view matrix.</summary>
    public static Matrix4x4 CreateLookAt(Vector3 position, Vector3 target, Vector3 up)
    {
        return Matrix4x4.CreateLookAt(position, target, up);
    }
    
    /// <summary>Create a simple orbit camera view matrix.</summary>
    public static Matrix4x4 CreateOrbitView(Vector3 target, float distance, float yawDegrees, float pitchDegrees)
    {
        var yaw = yawDegrees * MathF.PI / 180f;
        var pitch = pitchDegrees * MathF.PI / 180f;
        
        // Clamp pitch to avoid gimbal lock
        pitch = Math.Clamp(pitch, -89f * MathF.PI / 180f, 89f * MathF.PI / 180f);
        
        var cosPitch = MathF.Cos(pitch);
        var sinPitch = MathF.Sin(pitch);
        var cosYaw = MathF.Cos(yaw);
        var sinYaw = MathF.Sin(yaw);
        
        var position = target + new Vector3(
            distance * cosPitch * sinYaw,
            distance * sinPitch,
            distance * cosPitch * cosYaw
        );
        
        return Matrix4x4.CreateLookAt(position, target, Vector3.UnitY);
    }
}

/// <summary>
/// Utility class for common transform operations.
/// </summary>
public static class TransformUtils
{
    /// <summary>Create a 2D transform matrix from position, rotation, and scale.</summary>
    public static Matrix4x4 Create2D(Vector2 position, float rotationRadians = 0, Vector2? scale = null)
    {
        var s = scale ?? Vector2.One;
        return Matrix4x4.CreateScale(s.X, s.Y, 1) *
               Matrix4x4.CreateRotationZ(rotationRadians) *
               Matrix4x4.CreateTranslation(position.X, position.Y, 0);
    }
    
    /// <summary>Create a 3D transform matrix from position, rotation, and scale.</summary>
    public static Matrix4x4 Create3D(Vector3 position, Quaternion? rotation = null, Vector3? scale = null)
    {
        var r = rotation ?? Quaternion.Identity;
        var s = scale ?? Vector3.One;
        return Matrix4x4.CreateScale(s) *
               Matrix4x4.CreateFromQuaternion(r) *
               Matrix4x4.CreateTranslation(position);
    }
    
    /// <summary>Create a 3D transform matrix from position and Euler angles (degrees).</summary>
    public static Matrix4x4 Create3DEuler(Vector3 position, Vector3 eulerDegrees, Vector3? scale = null)
    {
        var rotation = Quaternion.CreateFromYawPitchRoll(
            eulerDegrees.Y * MathF.PI / 180f,
            eulerDegrees.X * MathF.PI / 180f,
            eulerDegrees.Z * MathF.PI / 180f
        );
        return Create3D(position, rotation, scale);
    }
    
    /// <summary>Decompose a transform matrix into position, rotation, and scale.</summary>
    public static bool Decompose(Matrix4x4 matrix, out Vector3 position, out Quaternion rotation, out Vector3 scale)
    {
        return Matrix4x4.Decompose(matrix, out scale, out rotation, out position);
    }
}

