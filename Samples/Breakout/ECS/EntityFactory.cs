using System.Numerics;
using Friflo.Engine.ECS;
using Ignis.Graphics;

namespace Samples.Breakout.ECS;

/// <summary>
/// Factory methods for creating Breakout entities with proper component batches.
/// </summary>
public static class EntityFactory
{
    public static Entity CreatePaddle(EntityStore store, float screenWidth, float screenHeight)
    {
        var e = store.CreateEntity(
            new Transform2D(screenWidth / 2f, screenHeight - 50f),
            new Size(100f, 15f),
            new Velocity2D(Vector2.Zero),
            new PaddleSpeed(600f),
            new SpriteColor(0.8f, 0.8f, 0.9f),
            Tags.Get<PaddleTag>()
        );
        return e;
    }

    public static Entity CreateBall(EntityStore store, Vector2 position)
    {
        var e = store.CreateEntity(
            new Transform2D(position),
            new Velocity2D(Vector2.Zero),
            new Radius(8f),
            new BallState { IsLaunched = false },
            new SpriteColor(Color4.White),
            Tags.Get<BallTag>()
        );
        return e;
    }

    public static Entity CreateBrick(EntityStore store, Vector2 position, BrickType type, int colorIndex)
    {
        var hitsRemaining = type switch
        {
            BrickType.Hard => 2,
            BrickType.Unbreakable => int.MaxValue,
            _ => 1
        };

        var e = store.CreateEntity(
            new Transform2D(position),
            new Size(50f, 20f),
            new Brick { Type = type, HitsRemaining = hitsRemaining, ColorIndex = colorIndex },
            new SpriteColor(GetBrickColor(type, colorIndex, hitsRemaining)),
            Tags.Get<BrickTag>()
        );
        return e;
    }

    public static Entity CreatePowerUp(EntityStore store, Vector2 position, PowerUpType type)
    {
        var color = type switch
        {
            PowerUpType.ExtraLife => new Color4(0.2f, 0.9f, 0.2f, 1f),
            PowerUpType.WidePaddle => new Color4(0.2f, 0.5f, 0.9f, 1f),
            PowerUpType.SlowBall => new Color4(0.9f, 0.9f, 0.2f, 1f),
            PowerUpType.MultiBall => new Color4(0.9f, 0.2f, 0.9f, 1f),
            _ => Color4.White
        };

        var e = store.CreateEntity(
            new Transform2D(position),
            new Size(20f, 20f),
            new Velocity2D(new Vector2(0, 150f)),
            new PowerUp { Type = type, FallSpeed = 150f },
            new SpriteColor(color),
            Tags.Get<PowerUpTag>()
        );
        return e;
    }

    public static Entity CreateParticle(EntityStore store, Vector2 position, Vector2 velocity, Color4 color, float life)
    {
        var e = store.CreateEntity(
            new Transform2D(position),
            new Velocity2D(velocity),
            new Particle { Life = life, Drag = 0.98f },
            new SpriteColor(color),
            Tags.Get<ParticleTag>()
        );
        return e;
    }

    private static readonly Color4[] BrickColors =
    [
        new(0.9f, 0.2f, 0.2f, 1f),
        new(0.9f, 0.5f, 0.2f, 1f),
        new(0.9f, 0.9f, 0.2f, 1f),
        new(0.2f, 0.9f, 0.2f, 1f),
        new(0.2f, 0.5f, 0.9f, 1f),
        new(0.7f, 0.2f, 0.9f, 1f)
    ];

    public static Color4 GetBrickColor(BrickType type, int colorIndex, int hitsRemaining = 1)
    {
        if (type == BrickType.Unbreakable)
            return new Color4(0.4f, 0.4f, 0.4f, 1f);

        var baseColor = BrickColors[colorIndex % BrickColors.Length];

        // Darken if brick has been hit
        if (type == BrickType.Hard && hitsRemaining == 1)
            return new Color4(baseColor.R * 0.6f, baseColor.G * 0.6f, baseColor.B * 0.6f, 1f);

        return baseColor;
    }

    public static Color4[] GetBrickColorPalette() => BrickColors;
}

