using System.Numerics;
using Friflo.Engine.ECS;

namespace Ignis.Engine.ECS.Components;

/// <summary>
/// World (absolute) transform component - The calculated Matrix4x4 used for rendering
/// This is read-only and calculated by TransformSystem
/// Note: Uses Friflo's built-in Position, Rotation, and Scale3 components for local transforms
/// </summary>
public struct WorldTransform(Matrix4x4 value) : IComponent
{
    /// <summary>
    /// The world transformation matrix
    /// </summary>
    public Matrix4x4 Value = value;
}

/// <summary>
/// Tag component indicating that a transform needs to be recalculated
/// Added automatically via event handlers when Position, Rotation, or Scale3 change
/// </summary>
public struct TransformDirty : ITag
{
    // Empty tag component
}

