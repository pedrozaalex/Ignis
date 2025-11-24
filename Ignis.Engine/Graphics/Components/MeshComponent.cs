using Friflo.Engine.ECS;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Engine.Graphics.Components;

/// <summary>
///     Links an entity to a 3D visual mesh
/// </summary>
public struct MeshComponent : IComponent
{
    /// <summary>
    ///     Reference to the loaded MonoGame Model
    /// </summary>
    public Model? ModelRef;

    /// <summary>
    ///     Whether this mesh casts shadows (reserved for future use)
    /// </summary>
    public bool CastShadows;

    public MeshComponent(Model? modelRef, bool castShadows = true)
    {
        ModelRef = modelRef;
        CastShadows = castShadows;
    }
}