using System.Numerics;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Ignis.Engine.ECS.Components;

namespace Ignis.Engine.ECS.Systems;

/// <summary>
/// Transform System - Handles recursive world matrix calculation for scene hierarchy
/// Uses "Dirty Flag" + "Recursive Propagation" strategy for efficiency
/// Uses Friflo's built-in Position, Rotation, and Scale3 components
/// </summary>
public class TransformSystem : QuerySystem<WorldTransform>
{
    /// <summary>
    /// Recursively process a node and its children
    /// </summary>
    /// <param name="entity">The entity to process</param>
    /// <param name="parentMatrix">The parent's world matrix</param>
    private void ProcessNode(Entity entity, Matrix4x4 parentMatrix)
    {
        // Read Friflo's built-in transform components (they have .value property)
        var pos = entity.Position.value;
        var rot = entity.Rotation.value;
        var scale = entity.Scale3.value;
        
        // Calculate local matrix: TRS order (Scale * Rotation * Translation)
        Matrix4x4 localMatrix = 
            Matrix4x4.CreateScale(scale) * 
            Matrix4x4.CreateFromQuaternion(rot) * 
            Matrix4x4.CreateTranslation(pos);
        
        // Calculate world matrix by multiplying with parent
        Matrix4x4 worldMatrix = localMatrix * parentMatrix;
        
        // Update world transform component
        entity.Set(new WorldTransform(worldMatrix));
        
        // Recursively process children
        foreach (var child in entity.ChildEntities)
        {
            ProcessNode(child, worldMatrix);
        }
    }
    
    protected override void OnUpdate()
    {
        Query.ForEachEntity((ref worldTransform, entity) =>
        {
            // Only process root entities (those without a parent)
            if (entity.Parent.IsNull)
            {
                ProcessNode(entity, Matrix4x4.Identity);
            }
        });
    }
}
