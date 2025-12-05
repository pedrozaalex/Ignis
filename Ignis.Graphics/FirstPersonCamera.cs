using System.Numerics;

namespace Ignis.Graphics;

/// <summary>
/// First-person camera controller with WASD movement and mouse look.
/// </summary>
public class FirstPersonCamera
{
    /// <summary>Camera position in world space.</summary>
    public Vector3 Position { get; set; } = new(0, 2, 5);

    /// <summary>Yaw angle in degrees (horizontal rotation).</summary>
    public float Yaw { get; set; } = -90f; // Looking toward -Z

    /// <summary>Pitch angle in degrees (vertical rotation).</summary>
    public float Pitch { get; set; } = 0f;

    /// <summary>Movement speed in units per second.</summary>
    public float MoveSpeed { get; set; } = 5f;

    /// <summary>Sprint speed multiplier.</summary>
    public float SprintMultiplier { get; set; } = 2f;

    /// <summary>Mouse sensitivity for look rotation.</summary>
    public float MouseSensitivity { get; set; } = 0.1f;

    /// <summary>Vertical field of view in degrees.</summary>
    public float Fov { get; set; } = 60f;

    /// <summary>Near clipping plane distance.</summary>
    public float NearPlane { get; set; } = 0.1f;

    /// <summary>Far clipping plane distance.</summary>
    public float FarPlane { get; set; } = 100f;

    /// <summary>Normalized forward direction vector.</summary>
    public Vector3 Front { get; private set; } = -Vector3.UnitZ;

    /// <summary>Normalized right direction vector.</summary>
    public Vector3 Right { get; private set; } = Vector3.UnitX;

    /// <summary>Normalized up direction vector.</summary>
    public Vector3 Up { get; private set; } = Vector3.UnitY;

    public FirstPersonCamera()
    {
        UpdateVectors();
    }

    /// <summary>
    /// Process mouse movement for look rotation.
    /// </summary>
    /// <param name="deltaX">Mouse X delta.</param>
    /// <param name="deltaY">Mouse Y delta.</param>
    public void ProcessMouseMovement(float deltaX, float deltaY)
    {
        Yaw += deltaX * MouseSensitivity;
        Pitch -= deltaY * MouseSensitivity;

        // Clamp pitch to avoid gimbal lock
        Pitch = Math.Clamp(Pitch, -89f, 89f);

        UpdateVectors();
    }

    /// <summary>
    /// Process keyboard input for movement.
    /// </summary>
    public void ProcessKeyboard(bool forward, bool backward, bool left, bool right, bool up, bool down, float deltaTime, bool sprint = false)
    {
        float velocity = MoveSpeed * deltaTime * (sprint ? SprintMultiplier : 1f);

        if (forward) Position += Front * velocity;
        if (backward) Position -= Front * velocity;
        if (left) Position -= Right * velocity;
        if (right) Position += Right * velocity;
        if (up) Position += Vector3.UnitY * velocity;
        if (down) Position -= Vector3.UnitY * velocity;
    }

    private void UpdateVectors()
    {
        float yawRad = Yaw * MathF.PI / 180f;
        float pitchRad = Pitch * MathF.PI / 180f;

        Front = Vector3.Normalize(new Vector3(
            MathF.Cos(yawRad) * MathF.Cos(pitchRad),
            MathF.Sin(pitchRad),
            MathF.Sin(yawRad) * MathF.Cos(pitchRad)
        ));

        Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, Front));
    }

    /// <summary>
    /// Get the view matrix for this camera.
    /// </summary>
    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(Position, Position + Front, Up);
    }

    /// <summary>
    /// Get the projection matrix for this camera.
    /// </summary>
    /// <param name="aspectRatio">Viewport aspect ratio (width/height).</param>
    public Matrix4x4 GetProjectionMatrix(float aspectRatio)
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(
            Fov * MathF.PI / 180f,
            aspectRatio,
            NearPlane,
            FarPlane
        );
    }
}
