using Friflo.Engine.ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Engine.Graphics.Components;

/// <summary>
///     Overrides default model material properties
/// </summary>
public struct MaterialComponent : IComponent
{
    /// <summary>
    ///     Tint color applied to the mesh
    /// </summary>
    public Color Color;

    /// <summary>
    ///     Optional texture override (null to use model's default)
    /// </summary>
    public Texture2D? Texture;

    /// <summary>
    ///     Specular power for lighting (higher = shinier)
    /// </summary>
    public float SpecularPower;

    /// <summary>
    ///     Whether lighting calculations are enabled
    /// </summary>
    public bool EnableLighting;

    public MaterialComponent(Color? color = null, Texture2D? texture = null, float specularPower = 16f,
        bool enableLighting = true)
    {
        Color = color ?? Color.White;
        Texture = texture;
        SpecularPower = specularPower;
        EnableLighting = enableLighting;
    }
}