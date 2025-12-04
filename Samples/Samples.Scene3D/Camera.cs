using System.Numerics;

namespace Samples.Scene3D;

/// <summary>
/// First-person camera controller with WASD movement and mouse look.
/// </summary>
public class Camera
{
    public Vector3 Position { get; set; } = new(0, 2, 5);
    public float Yaw { get; set; } = -90f; // Looking toward -Z
    public float Pitch { get; set; } = 0f;
    
    public float MoveSpeed { get; set; } = 5f;
    public float MouseSensitivity { get; set; } = 0.1f;
    public float Fov { get; set; } = 60f;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 100f;
    
    public Vector3 Front { get; private set; } = -Vector3.UnitZ;
    public Vector3 Right { get; private set; } = Vector3.UnitX;
    public Vector3 Up { get; private set; } = Vector3.UnitY;
    
    public Camera()
    {
        UpdateVectors();
    }
    
    public void ProcessMouseMovement(float deltaX, float deltaY)
    {
        Yaw += deltaX * MouseSensitivity;
        Pitch -= deltaY * MouseSensitivity;
        
        // Clamp pitch to avoid gimbal lock
        Pitch = Math.Clamp(Pitch, -89f, 89f);
        
        UpdateVectors();
    }
    
    public void ProcessKeyboard(bool forward, bool backward, bool left, bool right, bool up, bool down, float deltaTime)
    {
        float velocity = MoveSpeed * deltaTime;
        
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
    
    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(Position, Position + Front, Up);
    }
    
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

