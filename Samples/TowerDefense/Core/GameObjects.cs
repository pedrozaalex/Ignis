using System.Numerics;
using Ignis.Graphics;

namespace Samples.TowerDefense.Core;

#region Enums

/// <summary>
/// Types of turrets available to build.
/// </summary>
public enum TurretType
{
    /// <summary>Basic rapid-fire turret. Low damage, fast fire rate, cheap.</summary>
    Blaster,
    /// <summary>Slow powerful cannon. High damage, slow fire rate, splash damage.</summary>
    Cannon,
    /// <summary>Slowing beam turret. Low damage but applies slow effect to enemies.</summary>
    Freezer
}

/// <summary>
/// Types of enemies that spawn in waves.
/// </summary>
public enum EnemyType
{
    /// <summary>Basic grunt. Moderate speed and health.</summary>
    Grunt,
    /// <summary>Fast scout. Low health but very fast.</summary>
    Scout,
    /// <summary>Heavy tank. Slow but very high health.</summary>
    Tank,
    /// <summary>Shielded enemy. Regenerates shield over time.</summary>
    Shielded,
    /// <summary>Boss enemy. Massive health, spawns minions on death.</summary>
    Boss
}

/// <summary>
/// Tile types for the game grid.
/// </summary>
public enum TileType
{
    /// <summary>Empty buildable ground.</summary>
    Ground,
    /// <summary>Path where enemies walk.</summary>
    Path,
    /// <summary>Blocked terrain (mountains, water).</summary>
    Blocked,
    /// <summary>Spawn point for enemies.</summary>
    Spawn,
    /// <summary>Exit point enemies try to reach.</summary>
    Exit
}

/// <summary>
/// Game states for the main gameplay.
/// </summary>
public enum GamePhase
{
    /// <summary>Player is placing turrets before wave starts.</summary>
    Build,
    /// <summary>Wave is in progress.</summary>
    Wave,
    /// <summary>Game is paused.</summary>
    Paused,
    /// <summary>Level completed successfully.</summary>
    Victory,
    /// <summary>Player lost all lives.</summary>
    GameOver
}

#endregion

#region Turrets

/// <summary>
/// Turret stats and behavior definition.
/// </summary>
public class TurretDefinition
{
    public TurretType Type { get; init; }
    public string Name { get; init; } = "";
    public int Cost { get; init; }
    public int SellValue => Cost / 2;
    public float Range { get; init; }
    public float FireRate { get; init; } // Shots per second
    public int Damage { get; init; }
    public float ProjectileSpeed { get; init; }
    public float SplashRadius { get; init; } // 0 = no splash
    public float SlowAmount { get; init; } // 0-1, 0 = no slow
    public float SlowDuration { get; init; }

    public static readonly TurretDefinition Blaster = new()
    {
        Type = TurretType.Blaster,
        Name = "Blaster",
        Cost = 50,
        Range = 120f,
        FireRate = 3f,
        Damage = 10,
        ProjectileSpeed = 400f,
        SplashRadius = 0,
        SlowAmount = 0,
        SlowDuration = 0
    };

    public static readonly TurretDefinition Cannon = new()
    {
        Type = TurretType.Cannon,
        Name = "Cannon",
        Cost = 100,
        Range = 150f,
        FireRate = 0.8f,
        Damage = 50,
        ProjectileSpeed = 300f,
        SplashRadius = 40f,
        SlowAmount = 0,
        SlowDuration = 0
    };

    public static readonly TurretDefinition Freezer = new()
    {
        Type = TurretType.Freezer,
        Name = "Freezer",
        Cost = 75,
        Range = 100f,
        FireRate = 2f,
        Damage = 5,
        ProjectileSpeed = 350f,
        SplashRadius = 0,
        SlowAmount = 0.5f,
        SlowDuration = 2f
    };

    public static TurretDefinition Get(TurretType type) => type switch
    {
        TurretType.Blaster => Blaster,
        TurretType.Cannon => Cannon,
        TurretType.Freezer => Freezer,
        _ => Blaster
    };
}

/// <summary>
/// A placed turret on the map.
/// </summary>
public class Turret
{
    public TurretDefinition Definition { get; }
    public Vector2 Position { get; set; }
    public int GridX { get; set; }
    public int GridY { get; set; }
    public float Rotation { get; set; }
    public float FireCooldown { get; set; }
    public Enemy? Target { get; set; }

    public Turret(TurretDefinition definition, int gridX, int gridY, Vector2 worldPos)
    {
        Definition = definition;
        GridX = gridX;
        GridY = gridY;
        Position = worldPos;
    }

    public bool CanFire => FireCooldown <= 0;

    public void Update(float dt)
    {
        if (FireCooldown > 0)
            FireCooldown -= dt;
    }

    public void Fire()
    {
        FireCooldown = 1f / Definition.FireRate;
    }
}

#endregion

#region Enemies

/// <summary>
/// Enemy stats definition.
/// </summary>
public class EnemyDefinition
{
    public EnemyType Type { get; init; }
    public string Name { get; init; } = "";
    public int MaxHealth { get; init; }
    public float Speed { get; init; }
    public int GoldReward { get; init; }
    public int ScoreValue { get; init; }
    public bool HasShield { get; init; }
    public int ShieldHealth { get; init; }
    public float ShieldRegenRate { get; init; }
    public bool SpawnsOnDeath { get; init; }
    public int SpawnCount { get; init; }

    public static readonly EnemyDefinition Grunt = new()
    {
        Type = EnemyType.Grunt,
        Name = "Grunt",
        MaxHealth = 50,
        Speed = 60f,
        GoldReward = 5,
        ScoreValue = 10
    };

    public static readonly EnemyDefinition Scout = new()
    {
        Type = EnemyType.Scout,
        Name = "Scout",
        MaxHealth = 25,
        Speed = 120f,
        GoldReward = 3,
        ScoreValue = 15
    };

    public static readonly EnemyDefinition Tank = new()
    {
        Type = EnemyType.Tank,
        Name = "Tank",
        MaxHealth = 200,
        Speed = 30f,
        GoldReward = 15,
        ScoreValue = 25
    };

    public static readonly EnemyDefinition Shielded = new()
    {
        Type = EnemyType.Shielded,
        Name = "Shielded",
        MaxHealth = 75,
        Speed = 50f,
        GoldReward = 10,
        ScoreValue = 30,
        HasShield = true,
        ShieldHealth = 50,
        ShieldRegenRate = 10f
    };

    public static readonly EnemyDefinition Boss = new()
    {
        Type = EnemyType.Boss,
        Name = "Boss",
        MaxHealth = 500,
        Speed = 25f,
        GoldReward = 50,
        ScoreValue = 100,
        SpawnsOnDeath = true,
        SpawnCount = 3
    };

    public static EnemyDefinition Get(EnemyType type) => type switch
    {
        EnemyType.Grunt => Grunt,
        EnemyType.Scout => Scout,
        EnemyType.Tank => Tank,
        EnemyType.Shielded => Shielded,
        EnemyType.Boss => Boss,
        _ => Grunt
    };
}

/// <summary>
/// An active enemy on the map.
/// </summary>
public class Enemy
{
    public EnemyDefinition Definition { get; }
    public Vector2 Position { get; set; }
    public int Health { get; set; }
    public int Shield { get; set; }
    public float ShieldRegenCooldown { get; set; }
    public float SlowAmount { get; set; }
    public float SlowTimer { get; set; }
    public int PathIndex { get; set; }
    public float PathProgress { get; set; } // 0-1 between current and next waypoint
    public bool IsAlive { get; set; } = true;
    public bool ReachedEnd { get; set; }

    public Enemy(EnemyDefinition definition, Vector2 startPos)
    {
        Definition = definition;
        Position = startPos;
        Health = definition.MaxHealth;
        Shield = definition.HasShield ? definition.ShieldHealth : 0;
    }

    public float EffectiveSpeed
    {
        get
        {
            var speed = Definition.Speed;
            if (SlowTimer > 0)
                speed *= (1f - SlowAmount);
            return speed;
        }
    }

    public void TakeDamage(int damage, float slowAmount = 0, float slowDuration = 0)
    {
        // Apply slow
        if (slowAmount > 0 && slowDuration > 0)
        {
            SlowAmount = MathF.Max(SlowAmount, slowAmount);
            SlowTimer = MathF.Max(SlowTimer, slowDuration);
        }

        // Damage shield first
        if (Shield > 0)
        {
            var shieldDamage = Math.Min(Shield, damage);
            Shield -= shieldDamage;
            damage -= shieldDamage;
            ShieldRegenCooldown = 3f; // Delay regen after taking damage
        }

        Health -= damage;
        if (Health <= 0)
        {
            Health = 0;
            IsAlive = false;
        }
    }

    public void Update(float dt)
    {
        // Update slow timer
        if (SlowTimer > 0)
        {
            SlowTimer -= dt;
            if (SlowTimer <= 0)
                SlowAmount = 0;
        }

        // Regenerate shield
        if (Definition.HasShield && Shield < Definition.ShieldHealth)
        {
            if (ShieldRegenCooldown > 0)
                ShieldRegenCooldown -= dt;
            else
                Shield = Math.Min(Definition.ShieldHealth, Shield + (int)(Definition.ShieldRegenRate * dt));
        }
    }
}

#endregion

#region Projectiles

/// <summary>
/// A projectile fired by a turret.
/// </summary>
public class Projectile
{
    public Turret Source { get; }
    public Enemy Target { get; }
    public Vector2 Position { get; set; }
    public bool IsAlive { get; set; } = true;

    public Projectile(Turret source, Enemy target)
    {
        Source = source;
        Target = target;
        Position = source.Position;
    }

    public void Update(float dt)
    {
        if (!Target.IsAlive)
        {
            IsAlive = false;
            return;
        }

        var dir = Vector2.Normalize(Target.Position - Position);
        Position += dir * Source.Definition.ProjectileSpeed * dt;

        // Check if hit
        if (Vector2.Distance(Position, Target.Position) < 15f)
        {
            IsAlive = false;
        }
    }
}

#endregion

#region Level & Map

/// <summary>
/// A single tile on the game grid.
/// </summary>
public struct Tile
{
    public TileType Type;
    public int PathOrder; // For path tiles, which order in the path (-1 for non-path)
}

/// <summary>
/// Spawn instruction for a wave.
/// </summary>
public struct SpawnEntry
{
    public EnemyType Type;
    public float Delay; // Seconds after wave start to spawn
    public int Count;
    public float Interval; // Seconds between each spawn in this group
}

/// <summary>
/// A single wave of enemies.
/// </summary>
public class WaveData
{
    public int WaveNumber { get; init; }
    public List<SpawnEntry> Spawns { get; init; } = [];
    public int GoldBonus { get; init; }
}

/// <summary>
/// Complete level definition.
/// </summary>
public class LevelData
{
    public int LevelNumber { get; init; }
    public string Name { get; init; } = "";
    public int GridWidth { get; init; } = 16;
    public int GridHeight { get; init; } = 9;
    public Tile[,]? Grid { get; set; }
    public List<Vector2> Path { get; init; } = []; // Waypoints in world coords
    public List<WaveData> Waves { get; init; } = [];
    public int StartingGold { get; init; } = 100;
    public int StartingLives { get; init; } = 20;
}

#endregion

#region Visual Effects

/// <summary>
/// Simple particle effect for explosions, impacts, etc.
/// </summary>
public class ParticleEffect
{
    public Vector2 Position;
    public Color4 Color;
    public float Lifetime;
    public float MaxLifetime;
    public float Size;
    public List<Particle> Particles = [];

    public bool IsAlive => Lifetime > 0;

    public void Update(float dt)
    {
        Lifetime -= dt;
        foreach (var p in Particles)
        {
            p.Position += p.Velocity * dt;
            p.Velocity *= 0.95f; // Drag
            p.Life -= dt;
        }
        Particles.RemoveAll(p => p.Life <= 0);
    }
}

public class Particle
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Life;
    public float Size;
    public Color4 Color;
}

#endregion
