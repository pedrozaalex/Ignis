using Friflo.Engine.ECS;
using Microsoft.Xna.Framework;

namespace Ignis.Engine.Graphics.Components;

/// <summary>
/// Defines a camera lens with projection settings
/// </summary>
public struct CameraComponent(
    float fieldOfView = MathHelper.PiOver4,
    float nearPlane = 0.1f,
    float farPlane = 1000f,
    float aspectRatio = 16f / 9f,
    bool isActive = true)
    : IComponent
{
    /// <summary>
    /// Field of view in radians (default: 60 degrees = ~1.047 radians)
    /// </summary>
    public float FieldOfView = fieldOfView;
    
    /// <summary>
    /// Near clipping plane distance
    /// </summary>
    public float NearPlane = nearPlane;
    
    /// <summary>
    /// Far clipping plane distance
    /// </summary>
    public float FarPlane = farPlane;
    
    /// <summary>
    /// Aspect ratio (width/height) - usually set automatically
    /// </summary>
    public float AspectRatio = aspectRatio;
    
    /// <summary>
    /// Whether this camera is currently active
    /// </summary>
    public bool IsActive = isActive;
    
    /// <summary>
    /// View matrix (calculated by CameraSystem)
    /// </summary>
    public Matrix ViewMatrix = Matrix.Identity;
    
    /// <summary>
    /// Projection matrix (calculated by CameraSystem)
    /// </summary>
    public Matrix ProjectionMatrix = Matrix.Identity;
}

