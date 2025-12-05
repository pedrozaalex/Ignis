using System.Numerics;
using Friflo.Engine.ECS;
using Ignis.Graphics;

namespace Samples.TowerDefense.ECS;

// ─────────────────────────────────────────────────────────────────────────────
// Tags for entity identification
// ─────────────────────────────────────────────────────────────────────────────

public struct TurretTag : ITag { }
public struct EnemyTag : ITag { }
public struct ProjectileTag : ITag { }
public struct ParticleTag : ITag { }

// ─────────────────────────────────────────────────────────────────────────────
// Core transform components
// ─────────────────────────────────────────────────────────────────────────────

public struct Transform2D : IComponent
{
    public Vector2 Position;
    public float Rotation;
    public Transform2D(Vector2 pos, float rot = 0) { Position = pos; Rotation = rot; }
    public Transform2D(float x, float y) { Position = new Vector2(x, y); Rotation = 0; }
}

public struct Velocity2D : IComponent
{
    public Vector2 Value;
    public Velocity2D(Vector2 v) => Value = v;
}

// ─────────────────────────────────────────────────────────────────────────────
// Link components for entity references (O(1) lookup with bidirectional access)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Links a turret or projectile to its current target enemy.
/// Using ILinkComponent provides O(1) lookups and automatic cleanup when target is deleted.
/// </summary>
public struct TargetLink : ILinkComponent
{
    public Entity Target;
    public Entity GetIndexedValue() => Target;
    public TargetLink(Entity target) => Target = target;
}

// ─────────────────────────────────────────────────────────────────────────────
// Turret components
// ─────────────────────────────────────────────────────────────────────────────

public enum TurretType : byte
{
    Blaster,
    Cannon,
    Freezer
}

public struct Turret : IComponent
{
    public TurretType Type;
    public int GridX;
    public int GridY;
    public float FireCooldown;

    public readonly float Range => Type switch
    {
        TurretType.Blaster => 120f,
        TurretType.Cannon => 150f,
        TurretType.Freezer => 100f,
        _ => 120f
    };

    public readonly float FireRate => Type switch
    {
        TurretType.Blaster => 3f,
        TurretType.Cannon => 0.8f,
        TurretType.Freezer => 2f,
        _ => 1f
    };

    public readonly int Damage => Type switch
    {
        TurretType.Blaster => 10,
        TurretType.Cannon => 50,
        TurretType.Freezer => 5,
        _ => 10
    };

    public readonly float ProjectileSpeed => Type switch
    {
        TurretType.Blaster => 400f,
        TurretType.Cannon => 300f,
        TurretType.Freezer => 350f,
        _ => 400f
    };

    public readonly float SplashRadius => Type == TurretType.Cannon ? 40f : 0f;
    public readonly float SlowAmount => Type == TurretType.Freezer ? 0.5f : 0f;
    public readonly float SlowDuration => Type == TurretType.Freezer ? 2f : 0f;

    public readonly int Cost => Type switch
    {
        TurretType.Blaster => 50,
        TurretType.Cannon => 100,
        TurretType.Freezer => 75,
        _ => 50
    };

    public readonly int SellValue => Cost / 2;
}

// ─────────────────────────────────────────────────────────────────────────────
// Enemy components
// ─────────────────────────────────────────────────────────────────────────────

public enum EnemyType : byte
{
    Grunt,
    Scout,
    Tank,
    Shielded,
    Boss
}

public struct Enemy : IComponent
{
    public EnemyType Type;
    public int Health;
    public int MaxHealth;
    public int Shield;
    public int MaxShield;
    public float ShieldRegenCooldown;
    public float SlowAmount;
    public float SlowTimer;
    public int PathIndex;
    public float PathProgress;
    public bool ReachedEnd;

    public readonly float BaseSpeed => Type switch
    {
        EnemyType.Grunt => 60f,
        EnemyType.Scout => 120f,
        EnemyType.Tank => 30f,
        EnemyType.Shielded => 50f,
        EnemyType.Boss => 25f,
        _ => 60f
    };

    public readonly float EffectiveSpeed => SlowTimer > 0 ? BaseSpeed * (1f - SlowAmount) : BaseSpeed;

    public readonly int GoldReward => Type switch
    {
        EnemyType.Grunt => 5,
        EnemyType.Scout => 3,
        EnemyType.Tank => 15,
        EnemyType.Shielded => 10,
        EnemyType.Boss => 50,
        _ => 5
    };

    public readonly int ScoreValue => Type switch
    {
        EnemyType.Grunt => 10,
        EnemyType.Scout => 15,
        EnemyType.Tank => 25,
        EnemyType.Shielded => 30,
        EnemyType.Boss => 100,
        _ => 10
    };

    public readonly bool HasShield => Type == EnemyType.Shielded;
    public readonly float ShieldRegenRate => 10f;
    public readonly bool SpawnsOnDeath => Type == EnemyType.Boss;
    public readonly int SpawnCount => Type == EnemyType.Boss ? 3 : 0;
}

// ─────────────────────────────────────────────────────────────────────────────
// Projectile components
// ─────────────────────────────────────────────────────────────────────────────

public struct Projectile : IComponent
{
    public TurretType SourceType;
    public int Damage;
    public float Speed;
    public float SplashRadius;
    public float SlowAmount;
    public float SlowDuration;
}

// ─────────────────────────────────────────────────────────────────────────────
// Particle components
// ─────────────────────────────────────────────────────────────────────────────

public struct Particle : IComponent
{
    public float Life;
    public float MaxLife;
    public float Size;
    public Color4 Color;
}

// ─────────────────────────────────────────────────────────────────────────────
// Visual effect components
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Visual laser beam effect - instant hit with fading beam.
/// </summary>
public struct LaserBeam : IComponent
{
    public Vector2 Start;
    public Vector2 End;
    public float Life;
    public float MaxLife;
    public Color4 Color;
}

public struct LaserBeamTag : ITag { }

/// <summary>
/// Freeze aura effect around a turret - emits pulses that slow enemies.
/// </summary>
public struct FreezeAura : IComponent
{
    public float Radius;
    public float PulseCooldown;     // Time until next pulse
    public float PulseInterval;     // Time between pulses
    public float SlowAmount;
    public float SlowDuration;
}

/// <summary>
/// Visual expanding ring from freeze pulse.
/// </summary>
public struct FreezePulseRing : IComponent
{
    public Vector2 Origin;
    public float MaxRadius;
    public float CurrentRadius;
    public float Life;
    public float MaxLife;
}

public struct FreezePulseTag : ITag { }

// ─────────────────────────────────────────────────────────────────────────────
// Render components
// ─────────────────────────────────────────────────────────────────────────────

public struct SpriteColor : IComponent
{
    public Color4 Value;
    public SpriteColor(Color4 c) => Value = c;
    public SpriteColor(float r, float g, float b, float a = 1f) => Value = new Color4(r, g, b, a);
}

// ─────────────────────────────────────────────────────────────────────────────
// Marker tags
// ─────────────────────────────────────────────────────────────────────────────

public struct Dead : ITag { }

