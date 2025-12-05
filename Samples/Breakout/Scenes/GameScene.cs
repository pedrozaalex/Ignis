using System.Numerics;
using Ignis.Core;
using Ignis.Core.Scenery;
using Ignis.Core.Timing;
using Ignis.Graphics;
using Samples.Breakout.Core;
using Samples.Breakout.Services;
using Samples.Common;
using Silk.NET.Input;

namespace Samples.Breakout.Scenes;

/// <summary>
/// Main gameplay scene for Breakout.
/// </summary>
public sealed class GameScene : Scene, IBreakoutScene
{
    private readonly BreakoutContext _context;
    private readonly SceneManager _sceneManager;

    // Game objects
    private Paddle _paddle = null!;
    private Ball _ball = null!;
    private List<Brick> _bricks = [];
    private List<PowerUp> _powerUps = [];

    private GameState _state = GameState.Ready;
    private int _width;
    private int _height;

    // Power-up timers
    private float _widePaddleTimer;
    private float _slowBallTimer;
    private const float PowerUpDuration = 10f;

    // Visual effects
    private readonly List<ParticleEffect> _particles = [];

    // Brick colors by row
    private static readonly Color4[] BrickColors =
    [
        new(0.9f, 0.2f, 0.2f, 1f),  // Red
        new(0.9f, 0.5f, 0.2f, 1f),  // Orange
        new(0.9f, 0.9f, 0.2f, 1f),  // Yellow
        new(0.2f, 0.9f, 0.2f, 1f),  // Green
        new(0.2f, 0.5f, 0.9f, 1f),  // Blue
        new(0.7f, 0.2f, 0.9f, 1f)   // Purple
    ];

    public GameScene(BreakoutContext context, SceneManager sceneManager)
    {
        _context = context;
        _sceneManager = sceneManager;
        _width = context.Width;
        _height = context.Height;
    }

    public override void OnEnter(EngineContext context)
    {
        InitializeLevel();
        _context.Audio.PlayMusic("game_music");
    }

    public override void OnExit()
    {
        _context.Audio.StopMusic();
    }

    private void InitializeLevel()
    {
        // Create paddle
        _paddle = new Paddle
        {
            Position = new Vector2(_width / 2f, _height - 50f)
        };

        // Create ball attached to paddle
        _ball = new Ball();
        _ball.AttachToPaddle(_paddle);

        // Load bricks for current level
        _bricks = _context.Levels.CreateBricksForLevel(_context.CurrentLevel, _width, 80f);
        _powerUps.Clear();
        _particles.Clear();

        _state = GameState.Ready;
        _widePaddleTimer = 0;
        _slowBallTimer = 0;
    }

    public override void Update(GameTime time)
    {
        var dt = time.DeltaTime;
        var input = _context.GetInput();
        if (input == null) return;

        // Handle pause
        if (input.IsKeyPressed(Key.Escape))
        {
            if (_state == GameState.Playing)
                _state = GameState.Paused;
            else if (_state == GameState.Paused)
                _state = GameState.Playing;
            else if (_state == GameState.Ready)
                _sceneManager.LoadScene(new MainMenuScene(_context, _sceneManager));
        }

        // Update based on state
        switch (_state)
        {
            case GameState.Ready:
                UpdateReady(dt, input);
                break;
            case GameState.Playing:
                UpdatePlaying(dt, input);
                break;
            case GameState.Paused:
                if (input.IsKeyPressed(Key.Enter))
                    _state = GameState.Playing;
                break;
            case GameState.LevelComplete:
                if (input.IsKeyPressed(Key.Enter) || input.IsKeyPressed(Key.Space))
                    AdvanceToNextLevel();
                break;
            case GameState.GameOver:
            case GameState.Victory:
                if (input.IsKeyPressed(Key.Enter) || input.IsKeyPressed(Key.Space))
                    _sceneManager.LoadScene(new LeaderboardScene(_context, _sceneManager, true, _context.Score, _context.CurrentLevel));
                break;
        }

        // Update particles
        UpdateParticles(dt);
    }

    private void UpdateReady(float dt, InputState input)
    {
        UpdatePaddleMovement(dt, input);
        _ball.AttachToPaddle(_paddle);

        if (input.IsKeyPressed(Key.Space))
        {
            _ball.Launch();
            _state = GameState.Playing;
            _context.Audio.PlaySfx(AudioService.SfxBallLaunch);
        }
    }

    private void UpdatePlaying(float dt, InputState input)
    {
        UpdatePaddleMovement(dt, input);
        UpdatePowerUpTimers(dt);

        // Apply slow ball effect
        var effectiveDt = _slowBallTimer > 0 ? dt * 0.6f : dt;

        // Update ball and check for wall bounces
        if (_ball.Update(effectiveDt, _width, _height))
        {
            _context.Audio.PlaySfx(AudioService.SfxWallBounce);
        }

        // Check if ball fell below screen
        if (_ball.Top > _height)
        {
            LoseLife();
            return;
        }

        // Check paddle collision
        if (_ball.CheckPaddleCollision(_paddle))
        {
            _context.Audio.PlaySfx(AudioService.SfxPaddleHit);
        }

        // Check brick collisions
        CheckBrickCollisions();

        // Update power-ups
        UpdatePowerUps(dt);

        // Check win condition
        if (_bricks.All(b => !b.IsAlive || b.Type == BrickType.Unbreakable))
        {
            _state = GameState.LevelComplete;
            _context.Audio.PlaySfx(AudioService.SfxLevelComplete);
        }
    }

    private void UpdatePaddleMovement(float dt, InputState input)
    {
        var moveDir = 0f;
        if (input.IsKeyDown(Key.Left) || input.IsKeyDown(Key.A))
            moveDir -= 1f;
        if (input.IsKeyDown(Key.Right) || input.IsKeyDown(Key.D))
            moveDir += 1f;

        _paddle.Position.X += moveDir * _paddle.Speed * dt;
        _paddle.ClampToScreen(_width);
    }

    private void UpdatePowerUpTimers(float dt)
    {
        if (_widePaddleTimer > 0)
        {
            _widePaddleTimer -= dt;
            if (_widePaddleTimer <= 0)
                _paddle.Size.X = 100f; // Reset to normal
        }

        if (_slowBallTimer > 0)
        {
            _slowBallTimer -= dt;
        }
    }

    private void CheckBrickCollisions()
    {
        foreach (var brick in _bricks.Where(b => b.IsAlive))
        {
            if (!CheckBallBrickCollision(_ball, brick)) continue;

            if (brick.Hit())
            {
                _context.Score += brick.GetPoints();
                _context.Audio.PlaySfx(AudioService.SfxBrickBreak);
                SpawnParticles(brick.Position + brick.Size / 2, BrickColors[brick.ColorIndex % BrickColors.Length]);

                // Chance to spawn power-up
                if (brick.Type == BrickType.PowerUp)
                {
                    SpawnPowerUp(brick.Position + brick.Size / 2);
                }
            }
            else
            {
                _context.Audio.PlaySfx(AudioService.SfxBrickHit);
            }
            break; // Only handle one collision per frame
        }
    }

    private bool CheckBallBrickCollision(Ball ball, Brick brick)
    {
        if (ball.Right < brick.Left || ball.Left > brick.Right ||
            ball.Bottom < brick.Top || ball.Top > brick.Bottom)
            return false;

        // Determine collision side
        var overlapLeft = ball.Right - brick.Left;
        var overlapRight = brick.Right - ball.Left;
        var overlapTop = ball.Bottom - brick.Top;
        var overlapBottom = brick.Bottom - ball.Top;

        var minOverlapX = Math.Min(overlapLeft, overlapRight);
        var minOverlapY = Math.Min(overlapTop, overlapBottom);

        if (minOverlapX < minOverlapY)
        {
            ball.Velocity.X = -ball.Velocity.X;
        }
        else
        {
            ball.Velocity.Y = -ball.Velocity.Y;
        }

        return true;
    }

    private void UpdatePowerUps(float dt)
    {
        foreach (var powerUp in _powerUps.Where(p => p.IsActive))
        {
            powerUp.Update(dt);

            if (powerUp.Bottom > _height)
            {
                powerUp.IsActive = false;
                continue;
            }

            if (powerUp.CheckPaddleCollision(_paddle))
            {
                ApplyPowerUp(powerUp.Type);
                powerUp.IsActive = false;
                _context.Audio.PlaySfx(AudioService.SfxPowerUp);
            }
        }

        _powerUps.RemoveAll(p => !p.IsActive);
    }

    private void SpawnPowerUp(Vector2 position)
    {
        var types = Enum.GetValues<PowerUpType>();
        var type = types[Random.Shared.Next(types.Length)];

        _powerUps.Add(new PowerUp
        {
            Position = position,
            Type = type
        });
    }

    private void ApplyPowerUp(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.ExtraLife:
                _context.Lives++;
                break;
            case PowerUpType.WidePaddle:
                _paddle.Size.X = 150f;
                _widePaddleTimer = PowerUpDuration;
                break;
            case PowerUpType.SlowBall:
                _slowBallTimer = PowerUpDuration;
                break;
            case PowerUpType.MultiBall:
                // Simple implementation - just add points
                _context.Score += 100;
                break;
        }
    }

    private void LoseLife()
    {
        _context.Lives--;
        _context.Audio.PlaySfx(AudioService.SfxLifeLost);

        if (_context.Lives <= 0)
        {
            _state = GameState.GameOver;
            _context.Audio.PlaySfx(AudioService.SfxGameOver);
        }
        else
        {
            _ball.AttachToPaddle(_paddle);
            _state = GameState.Ready;
        }
    }

    private void AdvanceToNextLevel()
    {
        _context.CurrentLevel++;

        if (_context.CurrentLevel > _context.Levels.LevelCount)
        {
            _state = GameState.Victory;
        }
        else
        {
            InitializeLevel();
        }
    }

    private void SpawnParticles(Vector2 position, Color4 color)
    {
        for (var i = 0; i < 8; i++)
        {
            var angle = Random.Shared.NextSingle() * MathF.PI * 2;
            var speed = 100f + Random.Shared.NextSingle() * 150f;
            _particles.Add(new ParticleEffect
            {
                Position = position,
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed,
                Color = color,
                Life = 0.5f
            });
        }
    }

    private void UpdateParticles(float dt)
    {
        foreach (var p in _particles)
        {
            p.Position += p.Velocity * dt;
            p.Velocity *= 0.98f;
            p.Life -= dt;
        }
        _particles.RemoveAll(p => p.Life <= 0);
    }

    public void Render(float alpha)
    {
        var server = _context.RenderingServer;

        var pass = new RenderPass
        {
            Target = RenderTargetHandle.Screen,
            ClearColor = new Color4(0.02f, 0.02f, 0.05f),
            ClearDepth = true,
            Viewport = new Rect(0, 0, _width, _height)
        };

        server.BeginPass(pass);

        var projection = Matrix4x4.CreateOrthographicOffCenter(0, _width, _height, 0, -1, 1);
        var commands = server.CreateCommandList();
        commands.SetPipeline(server.DefaultShader2D);
        commands.SetProjectionMatrix(projection);
        commands.SetViewMatrix(Matrix4x4.Identity);

        // Draw particles (behind everything)
        foreach (var p in _particles)
        {
            var size = 4f * p.Life * 2;
            var color = new Color4(p.Color.R, p.Color.G, p.Color.B, p.Life);
            commands.DrawQuad(p.Position - new Vector2(size / 2), new Vector2(size), color);
        }

        // Draw bricks
        foreach (var brick in _bricks.Where(b => b.IsAlive))
        {
            var color = brick.Type switch
            {
                BrickType.Unbreakable => new Color4(0.4f, 0.4f, 0.4f, 1f),
                BrickType.Hard when brick.HitsRemaining == 1 =>
                    new Color4(BrickColors[brick.ColorIndex % BrickColors.Length].R * 0.6f,
                               BrickColors[brick.ColorIndex % BrickColors.Length].G * 0.6f,
                               BrickColors[brick.ColorIndex % BrickColors.Length].B * 0.6f, 1f),
                _ => BrickColors[brick.ColorIndex % BrickColors.Length]
            };

            commands.DrawQuad(brick.Position, brick.Size - new Vector2(2), color);

            // Draw border for hard bricks
            if (brick.Type == BrickType.Hard)
            {
                commands.DrawQuad(brick.Position, new Vector2(brick.Size.X - 2, 2), new Color4(1f, 1f, 1f, 0.3f));
            }
        }

        // Draw power-ups
        foreach (var powerUp in _powerUps)
        {
            var color = powerUp.Type switch
            {
                PowerUpType.ExtraLife => new Color4(0.2f, 0.9f, 0.2f, 1f),
                PowerUpType.WidePaddle => new Color4(0.2f, 0.5f, 0.9f, 1f),
                PowerUpType.SlowBall => new Color4(0.9f, 0.9f, 0.2f, 1f),
                PowerUpType.MultiBall => new Color4(0.9f, 0.2f, 0.9f, 1f),
                _ => Color4.White
            };
            commands.DrawQuad(
                powerUp.Position - powerUp.Size / 2,
                powerUp.Size,
                color
            );
        }

        // Draw paddle
        var paddleColor = _widePaddleTimer > 0
            ? new Color4(0.2f, 0.6f, 1f, 1f)
            : new Color4(0.8f, 0.8f, 0.9f, 1f);
        commands.DrawQuad(
            new Vector2(_paddle.Left, _paddle.Top),
            _paddle.Size,
            paddleColor
        );

        // Draw ball
        var ballColor = _slowBallTimer > 0
            ? new Color4(1f, 1f, 0.5f, 1f)
            : Color4.White;
        commands.DrawQuad(
            new Vector2(_ball.Left, _ball.Top),
            new Vector2(_ball.Radius * 2),
            ballColor
        );

        // Draw UI
        DrawUI(commands);

        // Draw overlays
        DrawStateOverlay(commands);

        server.Submit(commands);
        server.EndPass();
    }

    private void DrawUI(IRenderCommandList commands)
    {
        if (!_context.Font.IsValid) return;

        // Score
        commands.DrawText(_context.Font, $"Score: {_context.Score}",
            new Vector2(10, 10), 20f, Color4.White);

        // Level
        var levelText = $"Level {_context.CurrentLevel}";
        var (levelWidth, _) = _context.RenderingServer.MeasureText(_context.Font, levelText, 20f);
        commands.DrawText(_context.Font, levelText,
            new Vector2(_width / 2f - levelWidth / 2, 10), 20f, Color4.White);

        // Lives
        var livesText = $"Lives: {_context.Lives}";
        var (livesWidth, _) = _context.RenderingServer.MeasureText(_context.Font, livesText, 20f);
        commands.DrawText(_context.Font, livesText,
            new Vector2(_width - livesWidth - 10, 10), 20f, Color4.White);

        // Power-up indicators
        var indicatorY = 40f;
        if (_widePaddleTimer > 0)
        {
            commands.DrawText(_context.Font, $"Wide Paddle: {_widePaddleTimer:F1}s",
                new Vector2(10, indicatorY), 14f, new Color4(0.2f, 0.6f, 1f, 1f));
            indicatorY += 18f;
        }
        if (_slowBallTimer > 0)
        {
            commands.DrawText(_context.Font, $"Slow Ball: {_slowBallTimer:F1}s",
                new Vector2(10, indicatorY), 14f, new Color4(1f, 1f, 0.5f, 1f));
        }
    }

    private void DrawStateOverlay(IRenderCommandList commands)
    {
        if (!_context.Font.IsValid) return;

        switch (_state)
        {
            case GameState.Ready:
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "Press SPACE to Launch", _width / 2f, _height - 100f, 24f, Color4.Yellow);
                break;
            case GameState.Paused:
                DrawOverlayBackground(commands);
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "PAUSED", _width / 2f, _height / 2f - 30f, 48f, Color4.White);
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "Press ENTER to Resume", _width / 2f, _height / 2f + 30f, 20f, Color4.Gray);
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "Press ESC to Quit", _width / 2f, _height / 2f + 60f, 16f, Color4.Gray);
                break;
            case GameState.LevelComplete:
                DrawOverlayBackground(commands);
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "LEVEL COMPLETE!", _width / 2f, _height / 2f - 30f, 48f, new Color4(0.2f, 0.9f, 0.2f, 1f));
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "Press ENTER to Continue", _width / 2f, _height / 2f + 30f, 20f, Color4.Gray);
                break;
            case GameState.GameOver:
                DrawOverlayBackground(commands);
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "GAME OVER", _width / 2f, _height / 2f - 30f, 48f, new Color4(0.9f, 0.2f, 0.2f, 1f));
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, $"Final Score: {_context.Score}", _width / 2f, _height / 2f + 30f, 24f, Color4.White);
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "Press ENTER to Continue", _width / 2f, _height / 2f + 70f, 20f, Color4.Gray);
                break;
            case GameState.Victory:
                DrawOverlayBackground(commands);
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "VICTORY!", _width / 2f, _height / 2f - 30f, 48f, new Color4(1f, 0.8f, 0.2f, 1f));
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, $"Final Score: {_context.Score}", _width / 2f, _height / 2f + 30f, 24f, Color4.White);
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "Press ENTER to Continue", _width / 2f, _height / 2f + 70f, 20f, Color4.Gray);
                break;
        }
    }

    private void DrawOverlayBackground(IRenderCommandList commands)
    {
        commands.DrawQuad(Vector2.Zero, new Vector2(_width, _height), new Color4(0, 0, 0, 0.7f));
    }

    public void OnResize(int width, int height)
    {
        _width = width;
        _height = height;
    }
}

/// <summary>
/// Simple particle for visual effects.
/// </summary>
internal class ParticleEffect
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Color4 Color;
    public float Life;
}
