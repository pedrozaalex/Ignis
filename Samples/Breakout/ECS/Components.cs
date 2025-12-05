using System.Numerics;
using Friflo.Engine.ECS;
using Ignis.Graphics;

namespace Samples.Breakout.ECS;

// ─────────────────────────────────────────────────────────────────────────────
// Tags for entity identification
// ─────────────────────────────────────────────────────────────────────────────

public struct PaddleTag : ITag { }
public struct BallTag : ITag { }
public struct BrickTag : ITag { }
public struct PowerUpTag : ITag { }
public struct ParticleTag : ITag { }

// ─────────────────────────────────────────────────────────────────────────────
// Core transform/physics components
// ─────────────────────────────────────────────────────────────────────────────

public struct Transform2D : IComponent
{
    public Vector2 Position;
    public Transform2D(Vector2 v) => Position = v;
    public Transform2D(float x, float y) => Position = new Vector2(x, y);
}

public struct Velocity2D : IComponent
{
    public Vector2 Value;
    public Velocity2D(Vector2 v) => Value = v;
}

public struct Size : IComponent
{
    public Vector2 Value;
    public Size(Vector2 v) => Value = v;
    public Size(float w, float h) => Value = new Vector2(w, h);
}

public struct Radius : IComponent
{
    public float Value;
    public Radius(float r) => Value = r;
}

// ─────────────────────────────────────────────────────────────────────────────
// Paddle components
// ─────────────────────────────────────────────────────────────────────────────

public struct PaddleSpeed : IComponent
{
    public float Value;
    public PaddleSpeed(float s) => Value = s;
}

// ─────────────────────────────────────────────────────────────────────────────
// Ball components
// ─────────────────────────────────────────────────────────────────────────────

public struct BallState : IComponent
{
    public bool IsLaunched;
    public const float MinSpeed = 300f;
    public const float MaxSpeed = 800f;
    public const float SpeedIncrement = 10f;
}

// ─────────────────────────────────────────────────────────────────────────────
// Brick components
// ─────────────────────────────────────────────────────────────────────────────

public enum BrickType : byte
{
    Normal,
    Hard,
    Unbreakable,
    PowerUp
}

public struct Brick : IComponent
{
    public BrickType Type;
    public int HitsRemaining;
    public int ColorIndex;

    public int GetPoints() => Type switch
    {
        BrickType.Normal => 10,
        BrickType.Hard => 25,
        BrickType.PowerUp => 50,
        _ => 0
    };
}

// ─────────────────────────────────────────────────────────────────────────────
// Power-up components
// ─────────────────────────────────────────────────────────────────────────────

public enum PowerUpType : byte
{
    ExtraLife,
    WidePaddle,
    MultiBall,
    SlowBall
}

public struct PowerUp : IComponent
{
    public PowerUpType Type;
    public float FallSpeed;
}

// ─────────────────────────────────────────────────────────────────────────────
// Visual/rendering components
// ─────────────────────────────────────────────────────────────────────────────

public struct SpriteColor : IComponent
{
    public Color4 Value;
    public SpriteColor(Color4 c) => Value = c;
    public SpriteColor(float r, float g, float b, float a = 1f) => Value = new Color4(r, g, b, a);
}

// ─────────────────────────────────────────────────────────────────────────────
// Particle components
// ─────────────────────────────────────────────────────────────────────────────

public struct Particle : IComponent
{
    public float Life;
    public float Drag;
}

// ─────────────────────────────────────────────────────────────────────────────
// Marker components for lifecycle
// ─────────────────────────────────────────────────────────────────────────────

public struct Dead : ITag { }

