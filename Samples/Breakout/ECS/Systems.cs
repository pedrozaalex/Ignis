using System.Numerics;
using Friflo.Engine.ECS;
using Ignis.Core;
using Ignis.Graphics;
using Samples.Breakout.Services;
using Silk.NET.Input;

namespace Samples.Breakout.ECS;

/// <summary>
/// ECS systems for Breakout gameplay logic.
/// </summary>
public sealed class BreakoutSystems
{
    private readonly EntityStore _store;
    private readonly BreakoutState _state;
    private readonly AudioService _audio;

    // Queries cached for performance
    private readonly ArchetypeQuery<Transform2D, Size, PaddleSpeed> _paddleQuery;
    private readonly ArchetypeQuery<Transform2D, Velocity2D, Radius, BallState> _ballQuery;
    private readonly ArchetypeQuery<Transform2D, Size, Brick> _brickQuery;
    private readonly ArchetypeQuery<Transform2D, Size, Velocity2D, PowerUp> _powerUpQuery;
    private readonly ArchetypeQuery<Transform2D, Velocity2D, Particle, SpriteColor> _particleQuery;

    private float _screenWidth;
    private float _screenHeight;

    public BreakoutSystems(EntityStore store, BreakoutState state, AudioService audio)
    {
        _store = store;
        _state = state;
        _audio = audio;

        _paddleQuery = store.Query<Transform2D, Size, PaddleSpeed>().AllTags(Tags.Get<PaddleTag>());
        _ballQuery = store.Query<Transform2D, Velocity2D, Radius, BallState>().AllTags(Tags.Get<BallTag>());
        _brickQuery = store.Query<Transform2D, Size, Brick>().AllTags(Tags.Get<BrickTag>());
        _powerUpQuery = store.Query<Transform2D, Size, Velocity2D, PowerUp>().AllTags(Tags.Get<PowerUpTag>());
        _particleQuery = store.Query<Transform2D, Velocity2D, Particle, SpriteColor>().AllTags(Tags.Get<ParticleTag>());
    }

    public void SetScreenSize(float width, float height)
    {
        _screenWidth = width;
        _screenHeight = height;
    }

    public void Update(float dt, InputState? input)
    {
        if (input == null) return;

        switch (_state.Phase)
        {
            case GamePhase.Ready:
                UpdatePaddleMovement(dt, input);
                AttachBallToPaddle();
                if (input.IsKeyPressed(Key.Space))
                    LaunchBall();
                break;

            case GamePhase.Playing:
                UpdatePaddleMovement(dt, input);
                UpdatePowerUpTimers(dt);
                UpdateBallPhysics(dt);
                CheckBrickCollisions();
                UpdatePowerUps(dt);
                CheckWinCondition();
                break;

            case GamePhase.Paused:
                if (input.IsKeyPressed(Key.Enter))
                    _state.Phase = GamePhase.Playing;
                break;

            case GamePhase.LevelComplete:
            case GamePhase.GameOver:
            case GamePhase.Victory:
                // Handled by scene for transitions
                break;
        }

        UpdateParticles(dt);
        CleanupDeadEntities();
    }

    private void UpdatePaddleMovement(float dt, InputState input)
    {
        var moveDir = 0f;
        if (input.IsKeyDown(Key.Left) || input.IsKeyDown(Key.A)) moveDir -= 1f;
        if (input.IsKeyDown(Key.Right) || input.IsKeyDown(Key.D)) moveDir += 1f;

        foreach (var (transforms, sizes, speeds, _) in _paddleQuery.Chunks)
        {
            var posSpan = transforms.Span;
            var sizeSpan = sizes.Span;
            var speedSpan = speeds.Span;

            for (int i = 0; i < transforms.Length; i++)
            {
                ref var pos = ref posSpan[i];
                var size = sizeSpan[i];
                var speed = speedSpan[i];

                pos.Position.X += moveDir * speed.Value * dt;

                // Clamp to screen
                var halfWidth = size.Value.X / 2;
                pos.Position.X = Math.Clamp(pos.Position.X, halfWidth, _screenWidth - halfWidth);
            }
        }
    }

    private void AttachBallToPaddle()
    {
        Vector2 paddlePos = default;
        float paddleTop = 0;

        foreach (var (transforms, sizes, _, _) in _paddleQuery.Chunks)
        {
            if (transforms.Length > 0)
            {
                paddlePos = transforms.Span[0].Position;
                paddleTop = paddlePos.Y - sizes.Span[0].Value.Y / 2;
            }
        }

        foreach (var (transforms, velocities, radii, states, _) in _ballQuery.Chunks)
        {
            var posSpan = transforms.Span;
            var velSpan = velocities.Span;
            var radiusSpan = radii.Span;
            var stateSpan = states.Span;

            for (int i = 0; i < transforms.Length; i++)
            {
                if (!stateSpan[i].IsLaunched)
                {
                    posSpan[i].Position = new Vector2(paddlePos.X, paddleTop - radiusSpan[i].Value - 2);
                    velSpan[i].Value = Vector2.Zero;
                }
            }
        }
    }

    private void LaunchBall()
    {
        foreach (var (_, velocities, _, states, _) in _ballQuery.Chunks)
        {
            var velSpan = velocities.Span;
            var stateSpan = states.Span;

            for (int i = 0; i < velocities.Length; i++)
            {
                if (!stateSpan[i].IsLaunched)
                {
                    const float angle = -MathF.PI / 2;
                    velSpan[i].Value = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * BallState.MinSpeed;
                    stateSpan[i].IsLaunched = true;
                }
            }
        }

        _state.Phase = GamePhase.Playing;
        _audio.PlaySfx(AudioService.SfxBallLaunch);
    }

    private void UpdatePowerUpTimers(float dt)
    {
        if (_state.WidePaddleTimer > 0)
        {
            _state.WidePaddleTimer -= dt;
            if (_state.WidePaddleTimer <= 0)
                SetPaddleWidth(100f); // Reset to normal
        }

        if (_state.SlowBallTimer > 0)
            _state.SlowBallTimer -= dt;
    }

    private void SetPaddleWidth(float width)
    {
        foreach (var (_, sizes, _, entities) in _paddleQuery.Chunks)
        {
            for (int i = 0; i < sizes.Length; i++)
            {
                sizes.Span[i].Value.X = width;
            }
        }
    }

    private void UpdateBallPhysics(float dt)
    {
        // Apply slow ball effect
        var effectiveDt = _state.SlowBallTimer > 0 ? dt * 0.6f : dt;

        // Get paddle bounds for collision
        Vector2 paddlePos = default;
        Vector2 paddleSize = default;
        foreach (var (transforms, sizes, _, _) in _paddleQuery.Chunks)
        {
            if (transforms.Length > 0)
            {
                paddlePos = transforms.Span[0].Position;
                paddleSize = sizes.Span[0].Value;
            }
        }

        foreach (var (transforms, velocities, radii, states, _) in _ballQuery.Chunks)
        {
            var posSpan = transforms.Span;
            var velSpan = velocities.Span;
            var radiusSpan = radii.Span;
            var stateSpan = states.Span;

            for (int i = 0; i < transforms.Length; i++)
            {
                if (!stateSpan[i].IsLaunched) continue;

                ref var pos = ref posSpan[i];
                ref var vel = ref velSpan[i];
                var radius = radiusSpan[i].Value;

                // Move ball
                pos.Position += vel.Value * effectiveDt;

                // Wall collisions
                if (pos.Position.X - radius <= 0)
                {
                    pos.Position.X = radius;
                    vel.Value.X = MathF.Abs(vel.Value.X);
                    _audio.PlaySfx(AudioService.SfxWallBounce);
                }
                else if (pos.Position.X + radius >= _screenWidth)
                {
                    pos.Position.X = _screenWidth - radius;
                    vel.Value.X = -MathF.Abs(vel.Value.X);
                    _audio.PlaySfx(AudioService.SfxWallBounce);
                }

                if (pos.Position.Y - radius <= 0)
                {
                    pos.Position.Y = radius;
                    vel.Value.Y = MathF.Abs(vel.Value.Y);
                    _audio.PlaySfx(AudioService.SfxWallBounce);
                }

                // Ball fell below screen
                if (pos.Position.Y - radius > _screenHeight)
                {
                    LoseLife();
                    return;
                }

                // Paddle collision
                if (vel.Value.Y > 0) // Moving down
                {
                    var paddleLeft = paddlePos.X - paddleSize.X / 2;
                    var paddleRight = paddlePos.X + paddleSize.X / 2;
                    var paddleTop = paddlePos.Y - paddleSize.Y / 2;
                    var paddleBottom = paddlePos.Y + paddleSize.Y / 2;

                    if (pos.Position.Y + radius >= paddleTop && pos.Position.Y - radius <= paddleBottom &&
                        pos.Position.X + radius >= paddleLeft && pos.Position.X - radius <= paddleRight)
                    {
                        // Calculate hit position (-1 to 1)
                        var hitPos = (pos.Position.X - paddlePos.X) / (paddleSize.X / 2);
                        hitPos = Math.Clamp(hitPos, -0.9f, 0.9f);

                        // Reflect with angle based on hit position
                        var angle = hitPos * MathF.PI / 3 - MathF.PI / 2;
                        var speed = Math.Min(vel.Value.Length() + BallState.SpeedIncrement, BallState.MaxSpeed);

                        vel.Value = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
                        pos.Position.Y = paddleTop - radius;

                        _audio.PlaySfx(AudioService.SfxPaddleHit);
                    }
                }
            }
        }
    }

    private void LoseLife()
    {
        _state.Lives--;
        _audio.PlaySfx(AudioService.SfxLifeLost);

        if (_state.Lives <= 0)
        {
            _state.Phase = GamePhase.GameOver;
            _audio.PlaySfx(AudioService.SfxGameOver);
        }
        else
        {
            // Reset ball
            foreach (var (_, _, _, states, _) in _ballQuery.Chunks)
            {
                for (int i = 0; i < states.Length; i++)
                    states.Span[i].IsLaunched = false;
            }
            _state.Phase = GamePhase.Ready;
        }
    }

    private void CheckBrickCollisions()
    {
        // Get ball data
        Vector2 ballPos = default;
        Vector2 ballVel = default;
        float ballRadius = 0;
        bool hasLaunchedBall = false;

        foreach (var (transforms, velocities, radii, states, _) in _ballQuery.Chunks)
        {
            for (int i = 0; i < transforms.Length; i++)
            {
                if (states.Span[i].IsLaunched)
                {
                    ballPos = transforms.Span[i].Position;
                    ballVel = velocities.Span[i].Value;
                    ballRadius = radii.Span[i].Value;
                    hasLaunchedBall = true;
                    break;
                }
            }
        }

        if (!hasLaunchedBall) return;

        var ballLeft = ballPos.X - ballRadius;
        var ballRight = ballPos.X + ballRadius;
        var ballTop = ballPos.Y - ballRadius;
        var ballBottom = ballPos.Y + ballRadius;

        // Check each brick - iterate through entities, not chunks, for easy entity access
        Entity? hitBrick = null;
        Vector2 hitBrickPos = default;
        Vector2 hitBrickSize = default;
        int hitColorIndex = 0;
        BrickType hitType = default;
        bool brickDestroyed = false;

        foreach (var entity in _brickQuery.Entities)
        {
            ref var transform = ref entity.GetComponent<Transform2D>();
            ref var size = ref entity.GetComponent<Size>();
            ref var brick = ref entity.GetComponent<Brick>();

            var brickPos = transform.Position;
            var brickSize = size.Value;

            var brickRight = brickPos.X + brickSize.X;
            var brickBottom = brickPos.Y + brickSize.Y;

            // AABB collision
            if (ballRight < brickPos.X || ballLeft > brickRight ||
                ballBottom < brickPos.Y || ballTop > brickBottom)
                continue;

            // Collision detected - determine side
            var overlapLeft = ballRight - brickPos.X;
            var overlapRight = brickRight - ballLeft;
            var overlapTop = ballBottom - brickPos.Y;
            var overlapBottom = brickBottom - ballTop;

            var minOverlapX = Math.Min(overlapLeft, overlapRight);
            var minOverlapY = Math.Min(overlapTop, overlapBottom);

            // Reflect ball
            if (minOverlapX < minOverlapY)
                ReflectBallX();
            else
                ReflectBallY();

            // Handle brick hit
            if (brick.Type == BrickType.Unbreakable)
            {
                _audio.PlaySfx(AudioService.SfxBrickHit);
            }
            else
            {
                brick.HitsRemaining--;
                if (brick.HitsRemaining <= 0)
                {
                    _state.Score += brick.GetPoints();
                    _audio.PlaySfx(AudioService.SfxBrickBreak);

                    hitBrick = entity;
                    hitBrickPos = brickPos + brickSize / 2;
                    hitBrickSize = brickSize;
                    hitColorIndex = brick.ColorIndex;
                    hitType = brick.Type;
                    brickDestroyed = true;
                }
                else
                {
                    _audio.PlaySfx(AudioService.SfxBrickHit);
                    entity.GetComponent<SpriteColor>().Value =
                        EntityFactory.GetBrickColor(brick.Type, brick.ColorIndex, brick.HitsRemaining);
                }
            }

            break; // Only one collision per frame
        }

        // Process destroyed brick outside iteration
        if (brickDestroyed && hitBrick.HasValue)
        {
            var color = EntityFactory.GetBrickColor(hitType, hitColorIndex);
            SpawnBrickParticles(hitBrickPos, color);

            if (hitType == BrickType.PowerUp)
                SpawnRandomPowerUp(hitBrickPos);

            hitBrick.Value.AddTag<Dead>();
        }
    }

    private void ReflectBallX()
    {
        foreach (var (_, velocities, _, states, _) in _ballQuery.Chunks)
        {
            for (int i = 0; i < velocities.Length; i++)
            {
                if (states.Span[i].IsLaunched)
                    velocities.Span[i].Value.X = -velocities.Span[i].Value.X;
            }
        }
    }

    private void ReflectBallY()
    {
        foreach (var (_, velocities, _, states, _) in _ballQuery.Chunks)
        {
            for (int i = 0; i < velocities.Length; i++)
            {
                if (states.Span[i].IsLaunched)
                    velocities.Span[i].Value.Y = -velocities.Span[i].Value.Y;
            }
        }
    }

    private void SpawnBrickParticles(Vector2 position, Color4 color)
    {
        for (int i = 0; i < 8; i++)
        {
            var angle = Random.Shared.NextSingle() * MathF.PI * 2;
            var speed = 100f + Random.Shared.NextSingle() * 150f;
            var velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
            EntityFactory.CreateParticle(_store, position, velocity, color, 0.5f);
        }
    }

    private void SpawnRandomPowerUp(Vector2 position)
    {
        var types = Enum.GetValues<PowerUpType>();
        var type = types[Random.Shared.Next(types.Length)];
        EntityFactory.CreatePowerUp(_store, position, type);
    }

    private void UpdatePowerUps(float dt)
    {
        // Get paddle bounds
        Vector2 paddlePos = default;
        Vector2 paddleSize = default;
        foreach (var (transforms, sizes, _, _) in _paddleQuery.Chunks)
        {
            if (transforms.Length > 0)
            {
                paddlePos = transforms.Span[0].Position;
                paddleSize = sizes.Span[0].Value;
            }
        }

        var paddleLeft = paddlePos.X - paddleSize.X / 2;
        var paddleRight = paddlePos.X + paddleSize.X / 2;
        var paddleTop = paddlePos.Y - paddleSize.Y / 2;
        var paddleBottom = paddlePos.Y + paddleSize.Y / 2;

        var entitiesToRemove = new List<Entity>();

        foreach (var entity in _powerUpQuery.Entities)
        {
            ref var pos = ref entity.GetComponent<Transform2D>();
            ref var size = ref entity.GetComponent<Size>();
            ref var vel = ref entity.GetComponent<Velocity2D>();
            var powerUp = entity.GetComponent<PowerUp>();

            // Move power-up
            pos.Position += vel.Value * dt;

            // Check if fell off screen
            if (pos.Position.Y - size.Value.Y / 2 > _screenHeight)
            {
                entitiesToRemove.Add(entity);
                continue;
            }

            // Check paddle collision
            var puLeft = pos.Position.X - size.Value.X / 2;
            var puRight = pos.Position.X + size.Value.X / 2;
            var puTop = pos.Position.Y - size.Value.Y / 2;
            var puBottom = pos.Position.Y + size.Value.Y / 2;

            if (puBottom >= paddleTop && puTop <= paddleBottom &&
                puRight >= paddleLeft && puLeft <= paddleRight)
            {
                ApplyPowerUp(powerUp.Type);
                entitiesToRemove.Add(entity);
                _audio.PlaySfx(AudioService.SfxPowerUp);
            }
        }

        foreach (var entity in entitiesToRemove)
            entity.AddTag<Dead>();
    }

    private void ApplyPowerUp(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.ExtraLife:
                _state.Lives++;
                break;
            case PowerUpType.WidePaddle:
                SetPaddleWidth(150f);
                _state.WidePaddleTimer = BreakoutState.PowerUpDuration;
                break;
            case PowerUpType.SlowBall:
                _state.SlowBallTimer = BreakoutState.PowerUpDuration;
                break;
            case PowerUpType.MultiBall:
                _state.Score += 100;
                break;
        }
    }

    private void UpdateParticles(float dt)
    {
        var entitiesToRemove = new List<Entity>();

        foreach (var entity in _particleQuery.Entities)
        {
            ref var pos = ref entity.GetComponent<Transform2D>();
            ref var vel = ref entity.GetComponent<Velocity2D>();
            ref var particle = ref entity.GetComponent<Particle>();

            pos.Position += vel.Value * dt;
            vel.Value *= particle.Drag;
            particle.Life -= dt;

            if (particle.Life <= 0)
                entitiesToRemove.Add(entity);
        }

        foreach (var entity in entitiesToRemove)
            entity.AddTag<Dead>();
    }

    private void CheckWinCondition()
    {
        bool hasBreakableBricks = false;

        foreach (var (_, _, bricks, _) in _brickQuery.Chunks)
        {
            for (int i = 0; i < bricks.Length; i++)
            {
                if (bricks.Span[i].Type != BrickType.Unbreakable)
                {
                    hasBreakableBricks = true;
                    break;
                }
            }
            if (hasBreakableBricks) break;
        }

        if (!hasBreakableBricks)
        {
            _state.Phase = GamePhase.LevelComplete;
            _audio.PlaySfx(AudioService.SfxLevelComplete);
        }
    }

    private void CleanupDeadEntities()
    {
        var deadQuery = _store.Query().AllTags(Tags.Get<Dead>());
        var buffer = _store.GetCommandBuffer();

        foreach (var entity in deadQuery.Entities)
            buffer.DeleteEntity(entity.Id);

        buffer.Playback();
    }

    public void ClearLevel()
    {
        var buffer = _store.GetCommandBuffer();

        foreach (var entity in _brickQuery.Entities)
            buffer.DeleteEntity(entity.Id);
        foreach (var entity in _powerUpQuery.Entities)
            buffer.DeleteEntity(entity.Id);
        foreach (var entity in _particleQuery.Entities)
            buffer.DeleteEntity(entity.Id);

        buffer.Playback();
    }
}

