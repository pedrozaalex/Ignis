using System.Numerics;

namespace Ignis.Graphics;

/// <summary>
/// A single 2D particle with position, velocity, and lifetime.
/// </summary>
public struct Particle2D
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Life;
    public float MaxLife;
    public float Size;
    public Color4 Color;
    public float Rotation;
    public float RotationSpeed;

    /// <summary>Normalized lifetime (0 = just spawned, 1 = dead).</summary>
    public readonly float NormalizedLife => MaxLife > 0 ? 1f - (Life / MaxLife) : 1f;

    /// <summary>True if the particle is still alive.</summary>
    public readonly bool IsAlive => Life > 0;
}

/// <summary>
/// Configuration for spawning particles.
/// </summary>
public struct ParticleSpawnConfig
{
    public Vector2 Position;
    public Vector2 VelocityMin;
    public Vector2 VelocityMax;
    public float LifeMin;
    public float LifeMax;
    public float SizeMin;
    public float SizeMax;
    public Color4 ColorStart;
    public Color4 ColorEnd;
    public float RotationMin;
    public float RotationMax;
    public float RotationSpeedMin;
    public float RotationSpeedMax;

    public static ParticleSpawnConfig Default => new()
    {
        VelocityMin = new(-50, -50),
        VelocityMax = new(50, 50),
        LifeMin = 0.5f,
        LifeMax = 1.5f,
        SizeMin = 2f,
        SizeMax = 8f,
        ColorStart = Color4.White,
        ColorEnd = Color4.Transparent
    };
}

/// <summary>
/// Simple 2D particle system for effects like explosions, sparks, etc.
/// </summary>
public class ParticleSystem2D
{
    private readonly List<Particle2D> _particles = [];
    private readonly Random _random = new();

    /// <summary>Maximum number of particles allowed.</summary>
    public int MaxParticles { get; set; } = 1000;

    /// <summary>Global drag applied to all particles (0-1, 0 = no drag).</summary>
    public float Drag { get; set; } = 0.02f;

    /// <summary>Gravity applied to particles.</summary>
    public Vector2 Gravity { get; set; } = Vector2.Zero;

    /// <summary>Current active particle count.</summary>
    public int ParticleCount => _particles.Count;

    /// <summary>Read-only access to particles for rendering.</summary>
    public IReadOnlyList<Particle2D> Particles => _particles;

    /// <summary>
    /// Spawns particles at a position with the given config.
    /// </summary>
    public void Emit(int count, ParticleSpawnConfig config)
    {
        for (int i = 0; i < count && _particles.Count < MaxParticles; i++)
        {
            _particles.Add(CreateParticle(config));
        }
    }

    /// <summary>
    /// Spawns an explosion burst of particles.
    /// </summary>
    public void EmitBurst(Vector2 position, int count, Color4 color, float speed = 100f, float life = 0.5f)
    {
        var config = new ParticleSpawnConfig
        {
            Position = position,
            VelocityMin = new(-speed, -speed),
            VelocityMax = new(speed, speed),
            LifeMin = life * 0.8f,
            LifeMax = life * 1.2f,
            SizeMin = 2f,
            SizeMax = 6f,
            ColorStart = color,
            ColorEnd = color.WithAlpha(0)
        };
        Emit(count, config);
    }

    /// <summary>
    /// Updates all particles.
    /// </summary>
    public void Update(float deltaTime)
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var p = _particles[i];

            p.Life -= deltaTime;
            if (p.Life <= 0)
            {
                _particles.RemoveAt(i);
                continue;
            }

            p.Velocity += Gravity * deltaTime;
            p.Velocity *= 1f - Drag;
            p.Position += p.Velocity * deltaTime;
            p.Rotation += p.RotationSpeed * deltaTime;

            _particles[i] = p;
        }
    }

    /// <summary>
    /// Clears all particles.
    /// </summary>
    public void Clear() => _particles.Clear();

    private Particle2D CreateParticle(ParticleSpawnConfig config)
    {
        // Compute life once so MaxLife matches the actual assigned Life value.
        var life = Lerp(config.LifeMin, config.LifeMax, (float)_random.NextDouble());
        return new Particle2D
        {
            Position = config.Position,
            Velocity = new(
                Lerp(config.VelocityMin.X, config.VelocityMax.X, (float)_random.NextDouble()),
                Lerp(config.VelocityMin.Y, config.VelocityMax.Y, (float)_random.NextDouble())
            ),
            Life = life,
            MaxLife = life,
            Size = Lerp(config.SizeMin, config.SizeMax, (float)_random.NextDouble()),
            Color = config.ColorStart,
            Rotation = Lerp(config.RotationMin, config.RotationMax, (float)_random.NextDouble()),
            RotationSpeed = Lerp(config.RotationSpeedMin, config.RotationSpeedMax, (float)_random.NextDouble())
        };
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}

/// <summary>
/// A positioned particle effect with its own lifetime.
/// </summary>
public class ParticleEffect2D
{
    public Vector2 Position { get; set; }
    public Color4 Color { get; set; } = Color4.White;
    public float Lifetime { get; private set; }
    public float MaxLifetime { get; }
    public ParticleSystem2D Particles { get; } = new();

    public bool IsAlive => Lifetime > 0 || Particles.ParticleCount > 0;
    public float NormalizedLife => MaxLifetime > 0 ? Lifetime / MaxLifetime : 0;

    public ParticleEffect2D(Vector2 position, float lifetime = 1f)
    {
        Position = position;
        Lifetime = lifetime;
        MaxLifetime = lifetime;
    }

    public void Update(float deltaTime)
    {
        if (Lifetime > 0)
            Lifetime -= deltaTime;
        Particles.Update(deltaTime);
    }
}
