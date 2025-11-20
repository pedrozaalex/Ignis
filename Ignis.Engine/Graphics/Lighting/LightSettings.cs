using Microsoft.Xna.Framework;

namespace Ignis.Engine.Graphics.Lighting;

/// <summary>
/// Global lighting settings for the scene
/// </summary>
public struct LightSettings
{
    /// <summary>
    /// Ambient light color (affects all surfaces equally)
    /// </summary>
    public Vector3 AmbientLightColor;
    
    /// <summary>
    /// Primary directional light direction (normalized)
    /// </summary>
    public Vector3 DirectionalLightDirection;
    
    /// <summary>
    /// Primary directional light color
    /// </summary>
    public Vector3 DirectionalLightColor;
    
    /// <summary>
    /// Secondary directional light direction (normalized)
    /// </summary>
    public Vector3 DirectionalLight2Direction;
    
    /// <summary>
    /// Secondary directional light color
    /// </summary>
    public Vector3 DirectionalLight2Color;
    
    /// <summary>
    /// Tertiary directional light direction (normalized)
    /// </summary>
    public Vector3 DirectionalLight3Direction;
    
    /// <summary>
    /// Tertiary directional light color
    /// </summary>
    public Vector3 DirectionalLight3Color;
    
    /// <summary>
    /// Creates default lighting settings (similar to BasicEffect.EnableDefaultLighting)
    /// </summary>
    public static LightSettings Default => new()
    {
        AmbientLightColor = new Vector3(0.05333332f, 0.09882354f, 0.1819608f),
        DirectionalLightDirection = new Vector3(-0.5265408f, -0.5735765f, -0.6275069f),
        DirectionalLightColor = new Vector3(1f, 0.9607844f, 0.8078432f),
        DirectionalLight2Direction = new Vector3(0.7198464f, 0.3420201f, 0.6040227f),
        DirectionalLight2Color = new Vector3(0.9647059f, 0.7607844f, 0.4078432f),
        DirectionalLight3Direction = new Vector3(0.4545195f, -0.7660444f, 0.4545195f),
        DirectionalLight3Color = new Vector3(0.3231373f, 0.3607844f, 0.3937255f)
    };
}

