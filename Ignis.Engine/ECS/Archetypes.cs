using System.Numerics;
using Friflo.Engine.ECS;
using Ignis.Engine.ECS.Components;

namespace Ignis.Engine.ECS;

/// <summary>
///     Standard Archetypes for entity creation
///     Ensures entities are created with the necessary components for common use cases
/// </summary>
public static class Archetypes
{
    extension(EntityStore store)
    {
        /// <summary>
        ///     Get the GameObject archetype - A standard 3D entity with transform capabilities
        ///     Includes: Position, Rotation, Scale3, WorldTransform, TransformDirty
        /// </summary>
        public Archetype GetGameObjectArchetype()
        {
            return store.GetArchetype(
                ComponentTypes.Get<Position, Rotation, Scale3, WorldTransform>()
            );
        }

        /// <summary>
        ///     Helper method to create a GameObject entity with default transform values
        /// </summary>
        public Entity CreateGameObject()
        {
            var archetype = store.GetGameObjectArchetype();
            var entity = archetype.CreateEntity();

            // Initialize with default values
            entity.Position.value = Vector3.Zero;
            entity.Rotation.value = Quaternion.Identity;
            entity.Scale3.value = Vector3.One;

            return entity;
        }
    }
}