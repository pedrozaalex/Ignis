using System.Numerics;

namespace Samples.Breakout.Core;

/// <summary>
/// Represents the player's paddle.
/// </summary>
public class Paddle
{
    public Vector2 Position;
    public Vector2 Size = new(100f, 15f);
    public float Speed = 600f;

    public float Left => Position.X - Size.X / 2;
    public float Right => Position.X + Size.X / 2;
    public float Top => Position.Y - Size.Y / 2;
    public float Bottom => Position.Y + Size.Y / 2;

    public void ClampToScreen(float screenWidth)
    {
        var halfWidth = Size.X / 2;
        Position.X = Math.Clamp(Position.X, halfWidth, screenWidth - halfWidth);
    }
}

/// <summary>
/// Represents the ball with physics.
/// </summary>
public class Ball
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Radius = 8f;
    public bool IsLaunched;

    public float Left => Position.X - Radius;
    public float Right => Position.X + Radius;
    public float Top => Position.Y - Radius;
    public float Bottom => Position.Y + Radius;

    private const float MinSpeed = 300f;
    private const float MaxSpeed = 800f;
    private const float SpeedIncrement = 10f;

    public void Launch(float angle = -MathF.PI / 2)
    {
        if (IsLaunched) return;

        Velocity = new Vector2(
            MathF.Cos(angle) * MinSpeed,
            MathF.Sin(angle) * MinSpeed
        );
        IsLaunched = true;
    }

    /// <summary>
    /// Update ball position and check wall collisions.
    /// Returns true if a wall collision occurred.
    /// </summary>
    public bool Update(float deltaTime, float screenWidth, float screenHeight)
    {
        if (!IsLaunched) return false;

        Position += Velocity * deltaTime;
        var hitWall = false;

        // Wall collisions
        if (Left <= 0)
        {
            Position.X = Radius;
            Velocity.X = MathF.Abs(Velocity.X);
            hitWall = true;
        }
        else if (Right >= screenWidth)
        {
            Position.X = screenWidth - Radius;
            Velocity.X = -MathF.Abs(Velocity.X);
            hitWall = true;
        }

        // Ceiling collision
        if (Top <= 0)
        {
            Position.Y = Radius;
            Velocity.Y = MathF.Abs(Velocity.Y);
            hitWall = true;
        }

        return hitWall;
    }

    public bool CheckPaddleCollision(Paddle paddle)
    {
        if (!IsLaunched) return false;
        if (Velocity.Y < 0) return false; // Moving up, can't hit paddle

        if (Bottom >= paddle.Top && Top <= paddle.Bottom &&
            Right >= paddle.Left && Left <= paddle.Right)
        {
            // Calculate hit position relative to paddle center (-1 to 1)
            var hitPos = (Position.X - paddle.Position.X) / (paddle.Size.X / 2);
            hitPos = Math.Clamp(hitPos, -0.9f, 0.9f);

            // Reflect with angle based on hit position
            var angle = hitPos * MathF.PI / 3 - MathF.PI / 2; // -60 to 60 degrees from vertical
            var speed = Math.Min(Velocity.Length() + SpeedIncrement, MaxSpeed);

            Velocity = new Vector2(
                MathF.Cos(angle) * speed,
                MathF.Sin(angle) * speed
            );

            Position.Y = paddle.Top - Radius;
            return true;
        }

        return false;
    }

    public void AttachToPaddle(Paddle paddle)
    {
        Position = new Vector2(paddle.Position.X, paddle.Top - Radius - 2);
        IsLaunched = false;
        Velocity = Vector2.Zero;
    }
}

/// <summary>
/// Types of bricks with different properties.
/// </summary>
public enum BrickType
{
    Normal,
    Hard,      // Takes 2 hits
    Unbreakable,
    PowerUp    // Drops a power-up when broken
}

/// <summary>
/// Represents a brick in the grid.
/// </summary>
public class Brick
{
    public Vector2 Position;
    public Vector2 Size = new(50f, 20f);
    public BrickType Type;
    public int HitsRemaining;
    public bool IsAlive = true;
    public int ColorIndex;

    public float Left => Position.X;
    public float Right => Position.X + Size.X;
    public float Top => Position.Y;
    public float Bottom => Position.Y + Size.Y;

    public Brick(Vector2 position, BrickType type, int colorIndex)
    {
        Position = position;
        Type = type;
        ColorIndex = colorIndex;
        HitsRemaining = type switch
        {
            BrickType.Hard => 2,
            BrickType.Unbreakable => int.MaxValue,
            _ => 1
        };
    }

    public int GetPoints() => Type switch
    {
        BrickType.Normal => 10,
        BrickType.Hard => 25,
        BrickType.PowerUp => 50,
        BrickType.Unbreakable => 0,
        _ => 10
    };

    public bool Hit()
    {
        if (Type == BrickType.Unbreakable) return false;

        HitsRemaining--;
        if (HitsRemaining <= 0)
        {
            IsAlive = false;
            return true;
        }
        return false;
    }
}

/// <summary>
/// Types of power-ups that can drop from bricks.
/// </summary>
public enum PowerUpType
{
    ExtraLife,
    WidePaddle,
    MultiBall,
    SlowBall
}

/// <summary>
/// Falling power-up pickup.
/// </summary>
public class PowerUp
{
    public Vector2 Position;
    public Vector2 Size = new(20f, 20f);
    public PowerUpType Type;
    public float FallSpeed = 150f;
    public bool IsActive = true;

    public float Left => Position.X - Size.X / 2;
    public float Right => Position.X + Size.X / 2;
    public float Top => Position.Y - Size.Y / 2;
    public float Bottom => Position.Y + Size.Y / 2;

    public void Update(float deltaTime)
    {
        Position.Y += FallSpeed * deltaTime;
    }

    public bool CheckPaddleCollision(Paddle paddle)
    {
        return Bottom >= paddle.Top && Top <= paddle.Bottom &&
               Right >= paddle.Left && Left <= paddle.Right;
    }
}

/// <summary>
/// Current state of the game.
/// </summary>
public enum GameState
{
    Ready,      // Ball attached to paddle, waiting to launch
    Playing,    // Active gameplay
    Paused,
    LevelComplete,
    GameOver,
    Victory     // All levels completed
}
