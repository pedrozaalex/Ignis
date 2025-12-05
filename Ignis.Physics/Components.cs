using System.Numerics;
using Friflo.Engine.ECS;

namespace Ignis.Physics;

// ─────────────────────────────────────────────────────────────────────────────
// Collider Components
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Circle collider for simple collision detection.
/// </summary>
public struct CircleCollider : IComponent
{
    public float Radius;
    public Vector2 Offset;
    
    public CircleCollider(float radius, Vector2 offset = default)
    {
        Radius = radius;
        Offset = offset;
    }
}

/// <summary>
/// Axis-aligned bounding box collider.
/// </summary>
public struct BoxCollider : IComponent
{
    public Vector2 Size;
    public Vector2 Offset;
    
    public BoxCollider(Vector2 size, Vector2 offset = default)
    {
        Size = size;
        Offset = offset;
    }
    
    public BoxCollider(float width, float height) : this(new Vector2(width, height)) { }
}

// ─────────────────────────────────────────────────────────────────────────────
// Collision Layer Components
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Collision layer mask - determines which layers this entity belongs to and can collide with.
/// </summary>
public struct CollisionLayer : IComponent
{
    /// <summary>Layer this entity belongs to (bitmask).</summary>
    public uint Layer;
    
    /// <summary>Layers this entity can collide with (bitmask).</summary>
    public uint Mask;
    
    public CollisionLayer(uint layer, uint mask)
    {
        Layer = layer;
        Mask = mask;
    }
    
    public readonly bool CanCollideWith(CollisionLayer other) => (Layer & other.Mask) != 0 && (other.Layer & Mask) != 0;
}

/// <summary>
/// Common collision layer definitions.
/// </summary>
public static class CollisionLayers
{
    public const uint None = 0;
    public const uint Default = 1 << 0;
    public const uint Player = 1 << 1;
    public const uint Enemy = 1 << 2;
    public const uint Projectile = 1 << 3;
    public const uint Pickup = 1 << 4;
    public const uint Environment = 1 << 5;
    public const uint All = uint.MaxValue;
}

// ─────────────────────────────────────────────────────────────────────────────
// Velocity Component (shared physics concept)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// 2D velocity for physics movement.
/// </summary>
public struct PhysicsVelocity : IComponent
{
    public Vector2 Value;
    
    public PhysicsVelocity(Vector2 v) => Value = v;
    public PhysicsVelocity(float x, float y) => Value = new Vector2(x, y);
}

// ─────────────────────────────────────────────────────────────────────────────
// Collision Event Components
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Stores collision events for an entity this frame.
/// Processed and cleared each physics update.
/// </summary>
public struct CollisionEvents : IComponent
{
    /// <summary>Number of collisions this frame (max 8).</summary>
    public int Count;
    
    /// <summary>Entity IDs of collided entities.</summary>
    public int Entity0, Entity1, Entity2, Entity3, Entity4, Entity5, Entity6, Entity7;
    
    public void Add(int entityId)
    {
        switch (Count)
        {
            case 0: Entity0 = entityId; break;
            case 1: Entity1 = entityId; break;
            case 2: Entity2 = entityId; break;
            case 3: Entity3 = entityId; break;
            case 4: Entity4 = entityId; break;
            case 5: Entity5 = entityId; break;
            case 6: Entity6 = entityId; break;
            case 7: Entity7 = entityId; break;
            default: return; // Max reached
        }
        Count++;
    }
    
    public readonly int GetEntityId(int index) => index switch
    {
        0 => Entity0,
        1 => Entity1,
        2 => Entity2,
        3 => Entity3,
        4 => Entity4,
        5 => Entity5,
        6 => Entity6,
        7 => Entity7,
        _ => 0
    };
    
    public void Clear() => Count = 0;
}

// ─────────────────────────────────────────────────────────────────────────────
// Tags
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Mark entity as a trigger (no physical response, just detection).</summary>
public struct TriggerTag : ITag { }

/// <summary>Mark entity as static (doesn't move, optimizes broadphase).</summary>
public struct StaticTag : ITag { }

