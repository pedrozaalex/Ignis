using System.Numerics;
using Friflo.Engine.ECS;
using Ignis.Graphics;
using Ignis.Physics;

namespace Samples.TowerDefense.ECS;

/// <summary>
/// Factory for creating Tower Defense entities using Friflo archetypes for efficient bulk creation.
/// </summary>
public sealed class EntityFactory(EntityStore store)
{
    // Cached archetypes for efficient entity creation
    private readonly Archetype _turretArchetype = store.GetArchetype(
        ComponentTypes.Get<Transform2D, Turret, SpriteColor>(),
        Tags.Get<TurretTag>()
    );
    private readonly Archetype _freezerTurretArchetype = store.GetArchetype(
        ComponentTypes.Get<Transform2D, Turret, SpriteColor, FreezeAura>(),
        Tags.Get<TurretTag>()
    );
    private readonly Archetype _enemyArchetype = store.GetArchetype(
        ComponentTypes.Get<Transform2D, Enemy, CircleCollider, CollisionLayer, SpriteColor>(),
        Tags.Get<EnemyTag>()
    );
    private readonly Archetype _projectileArchetype = store.GetArchetype(
        ComponentTypes.Get<Transform2D, Velocity2D, Projectile, CircleCollider, CollisionLayer, SpriteColor>(),
        Tags.Get<ProjectileTag>()
    );
    private readonly Archetype _particleArchetype = store.GetArchetype(
        ComponentTypes.Get<Transform2D, Velocity2D, Particle>(),
        Tags.Get<ParticleTag>()
    );
    private readonly Archetype _laserBeamArchetype = store.GetArchetype(
        ComponentTypes.Get<Transform2D, LaserBeam>(),
        Tags.Get<LaserBeamTag>()
    );
    private readonly Archetype _freezePulseArchetype = store.GetArchetype(
        ComponentTypes.Get<Transform2D, FreezePulseRing>(),
        Tags.Get<FreezePulseTag>()
    );

    // Pre-create archetypes for each entity type

    public Entity CreateTurret(TurretType type, int gridX, int gridY, Vector2 worldPos)
    {
        Entity e;
        
        if (type == TurretType.Freezer)
        {
            e = _freezerTurretArchetype.CreateEntity();
            e.GetComponent<FreezeAura>() = new FreezeAura
            {
                Radius = 100f,
                PulseCooldown = 0f,
                PulseInterval = 1.5f,
                SlowAmount = 0.5f,
                SlowDuration = 1.2f
            };
        }
        else
        {
            e = _turretArchetype.CreateEntity();
        }
        
        e.GetComponent<Transform2D>() = new Transform2D(worldPos);
        e.GetComponent<Turret>() = new Turret
        {
            Type = type,
            GridX = gridX,
            GridY = gridY,
            FireCooldown = 0
        };
        e.GetComponent<SpriteColor>() = new SpriteColor(GetTurretColor(type));
        
        return e;
    }

    public Entity CreateEnemy(EnemyType type, Vector2 startPos)
    {
        var maxHealth = type switch
        {
            EnemyType.Grunt => 50,
            EnemyType.Scout => 25,
            EnemyType.Tank => 200,
            EnemyType.Shielded => 75,
            EnemyType.Boss => 500,
            _ => 50
        };

        var maxShield = type == EnemyType.Shielded ? 50 : 0;
        var colliderRadius = type == EnemyType.Boss ? 12f : 8f;

        var e = _enemyArchetype.CreateEntity();
        e.GetComponent<Transform2D>() = new Transform2D(startPos);
        e.GetComponent<Enemy>() = new Enemy
        {
            Type = type,
            Health = maxHealth,
            MaxHealth = maxHealth,
            Shield = maxShield,
            MaxShield = maxShield,
            ShieldRegenCooldown = 0,
            SlowAmount = 0,
            SlowTimer = 0,
            PathIndex = 0,
            PathProgress = 0,
            ReachedEnd = false
        };
        e.GetComponent<CircleCollider>() = new CircleCollider(colliderRadius);
        e.GetComponent<CollisionLayer>() = new CollisionLayer(CollisionLayers.Enemy, CollisionLayers.Projectile);
        e.GetComponent<SpriteColor>() = new SpriteColor(GetEnemyColor(type));
        
        return e;
    }

    public Entity CreateProjectile(Vector2 position, Vector2 direction, Turret turret)
    {
        var velocity = Vector2.Normalize(direction) * turret.ProjectileSpeed;
        var colliderRadius = turret.Type == TurretType.Cannon ? 6f : 4f;
        
        var e = _projectileArchetype.CreateEntity();
        e.GetComponent<Transform2D>() = new Transform2D(position);
        e.GetComponent<Velocity2D>() = new Velocity2D(velocity);
        e.GetComponent<Projectile>() = new Projectile
        {
            SourceType = turret.Type,
            Damage = turret.Damage,
            Speed = turret.ProjectileSpeed,
            SplashRadius = turret.SplashRadius,
            SlowAmount = turret.SlowAmount,
            SlowDuration = turret.SlowDuration
        };
        e.GetComponent<CircleCollider>() = new CircleCollider(colliderRadius);
        e.GetComponent<CollisionLayer>() = new CollisionLayer(CollisionLayers.Projectile, CollisionLayers.Enemy);
        e.GetComponent<SpriteColor>() = new SpriteColor(GetProjectileColor(turret.Type));
        
        return e;
    }

    public Entity CreateParticle(Vector2 position, Vector2 velocity, Color4 color, float size, float life)
    {
        var e = _particleArchetype.CreateEntity();
        e.GetComponent<Transform2D>() = new Transform2D(position);
        e.GetComponent<Velocity2D>() = new Velocity2D(velocity);
        e.GetComponent<Particle>() = new Particle { Life = life, MaxLife = life, Size = size, Color = color };
        
        return e;
    }

    public Entity CreateLaserBeam(Vector2 start, Vector2 end, Color4 color, float duration = 0.15f)
    {
        var e = _laserBeamArchetype.CreateEntity();
        e.GetComponent<Transform2D>() = new Transform2D(start);
        e.GetComponent<LaserBeam>() = new LaserBeam
        {
            Start = start,
            End = end,
            Life = duration,
            MaxLife = duration,
            Color = color
        };
        
        return e;
    }

    public Entity CreateFreezePulseRing(Vector2 origin, float maxRadius, float duration = 0.8f)
    {
        var e = _freezePulseArchetype.CreateEntity();
        e.GetComponent<Transform2D>() = new Transform2D(origin);
        e.GetComponent<FreezePulseRing>() = new FreezePulseRing
        {
            Origin = origin,
            MaxRadius = maxRadius,
            CurrentRadius = 0f,
            Life = duration,
            MaxLife = duration
        };
        
        return e;
    }
    
    // --- Static helper methods ---

    public static Color4 GetTurretColor(TurretType type) => type switch
    {
        TurretType.Blaster => new Color4(0.3f, 0.6f, 0.9f, 1f),
        TurretType.Cannon => new Color4(0.9f, 0.5f, 0.2f, 1f),
        TurretType.Freezer => new Color4(0.4f, 0.8f, 0.9f, 1f),
        _ => Color4.White
    };

    public static Color4 GetEnemyColor(EnemyType type) => type switch
    {
        EnemyType.Grunt => new Color4(0.7f, 0.3f, 0.3f, 1f),
        EnemyType.Scout => new Color4(0.9f, 0.7f, 0.2f, 1f),
        EnemyType.Tank => new Color4(0.4f, 0.4f, 0.5f, 1f),
        EnemyType.Shielded => new Color4(0.3f, 0.5f, 0.8f, 1f),
        EnemyType.Boss => new Color4(0.6f, 0.2f, 0.6f, 1f),
        _ => Color4.White
    };

    public static Color4 GetProjectileColor(TurretType type) => type switch
    {
        TurretType.Blaster => new Color4(0.5f, 0.8f, 1f, 1f),
        TurretType.Cannon => new Color4(1f, 0.6f, 0.2f, 1f),
        TurretType.Freezer => new Color4(0.6f, 0.9f, 1f, 1f),
        _ => Color4.White
    };

    public static int GetTurretCost(TurretType type) => type switch
    {
        TurretType.Blaster => 50,
        TurretType.Cannon => 100,
        TurretType.Freezer => 75,
        _ => 50
    };

    public static string GetTurretName(TurretType type) => type switch
    {
        TurretType.Blaster => "Blaster",
        TurretType.Cannon => "Cannon",
        TurretType.Freezer => "Freezer",
        _ => "Unknown"
    };
}

