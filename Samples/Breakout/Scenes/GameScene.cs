using System.Numerics;
using Friflo.Engine.ECS;
using Ignis.Core;
using Ignis.Core.Scenery;
using Ignis.Core.Timing;
using Ignis.Graphics;
using Samples.Breakout.Core;
using Samples.Breakout.ECS;
using Samples.Breakout.Services;
using Samples.Common;
using Silk.NET.Input;

namespace Samples.Breakout.Scenes;

/// <summary>
/// Main gameplay scene for Breakout using ECS.
/// </summary>
public sealed class GameScene : Scene, IBreakoutScene
{
    private readonly BreakoutContext _context;
    private readonly SceneManager _sceneManager;

    // ECS
    private readonly EntityStore _store;
    private readonly BreakoutState _state;
    private readonly BreakoutSystems _systems;

    // Queries for rendering
    private readonly ArchetypeQuery<Transform2D, Size, SpriteColor> _paddleRenderQuery;
    private readonly ArchetypeQuery<Transform2D, Radius, SpriteColor> _ballRenderQuery;
    private readonly ArchetypeQuery<Transform2D, Size, Brick, SpriteColor> _brickRenderQuery;
    private readonly ArchetypeQuery<Transform2D, Size, PowerUp, SpriteColor> _powerUpRenderQuery;
    private readonly ArchetypeQuery<Transform2D, Particle, SpriteColor> _particleRenderQuery;

    private int _width;
    private int _height;

    public GameScene(BreakoutContext context, SceneManager sceneManager)
    {
        _context = context;
        _sceneManager = sceneManager;
        _width = context.Width;
        _height = context.Height;

        _store = new EntityStore();
        _state = new BreakoutState();
        _systems = new BreakoutSystems(_store, _state, context.Audio);

        // Setup render queries
        _paddleRenderQuery = _store.Query<Transform2D, Size, SpriteColor>().AllTags(Tags.Get<PaddleTag>());
        _ballRenderQuery = _store.Query<Transform2D, Radius, SpriteColor>().AllTags(Tags.Get<BallTag>());
        _brickRenderQuery = _store.Query<Transform2D, Size, Brick, SpriteColor>().AllTags(Tags.Get<BrickTag>());
        _powerUpRenderQuery = _store.Query<Transform2D, Size, PowerUp, SpriteColor>().AllTags(Tags.Get<PowerUpTag>());
        _particleRenderQuery = _store.Query<Transform2D, Particle, SpriteColor>().AllTags(Tags.Get<ParticleTag>());
    }

    public override void OnEnter(EngineContext context)
    {
        _state.CurrentLevel = _context.CurrentLevel;
        _state.Score = _context.Score;
        _state.Lives = _context.Lives;

        InitializeLevel();
        _context.Audio.PlayMusic("game_music");
    }

    public override void OnExit()
    {
        _context.Audio.StopMusic();
        // Sync state back to context
        _context.Score = _state.Score;
        _context.Lives = _state.Lives;
        _context.CurrentLevel = _state.CurrentLevel;
    }

    private void InitializeLevel()
    {
        _systems.ClearLevel();
        _systems.SetScreenSize(_width, _height);

        // Create paddle
        EntityFactory.CreatePaddle(_store, _width, _height);

        // Create ball (will attach to paddle)
        var paddleTop = _height - 50f - 7.5f; // paddle Y - half height
        EntityFactory.CreateBall(_store, new Vector2(_width / 2f, paddleTop - 10f));

        // Create bricks
        _context.Levels.CreateBricksForLevel(_store, _state.CurrentLevel, _width, 80f);

        _state.Phase = GamePhase.Ready;
        _state.WidePaddleTimer = 0;
        _state.SlowBallTimer = 0;
    }

    public override void Update(GameTime time)
    {
        var dt = time.DeltaTime;
        var input = _context.GetInput();

        // Handle global input
        if (input?.IsKeyPressed(Key.Escape) == true)
        {
            switch (_state.Phase)
            {
                case GamePhase.Playing:
                    _state.Phase = GamePhase.Paused;
                    break;
                case GamePhase.Paused:
                    _state.Phase = GamePhase.Playing;
                    break;
                case GamePhase.Ready:
                    _sceneManager.LoadScene(new MainMenuScene(_context, _sceneManager));
                    return;
            }
        }

        // Handle state transitions
        switch (_state.Phase)
        {
            case GamePhase.LevelComplete:
                if (input?.IsKeyPressed(Key.Enter) == true || input?.IsKeyPressed(Key.Space) == true)
                    AdvanceToNextLevel();
                break;
            case GamePhase.GameOver:
            case GamePhase.Victory:
                if (input?.IsKeyPressed(Key.Enter) == true || input?.IsKeyPressed(Key.Space) == true)
                    _sceneManager.LoadScene(new LeaderboardScene(_context, _sceneManager, true, _state.Score, _state.CurrentLevel));
                break;
        }

        // Run ECS systems
        _systems.Update(dt, input);

        // Sync state to context for UI
        _context.Score = _state.Score;
        _context.Lives = _state.Lives;
    }

    private void AdvanceToNextLevel()
    {
        _state.CurrentLevel++;
        _context.CurrentLevel = _state.CurrentLevel;

        if (_state.CurrentLevel > _context.Levels.LevelCount)
        {
            _state.Phase = GamePhase.Victory;
        }
        else
        {
            InitializeLevel();
        }
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

        RenderParticles(commands);
        RenderBricks(commands);
        RenderPowerUps(commands);
        RenderPaddle(commands);
        RenderBall(commands);
        RenderUI(commands);
        RenderStateOverlay(commands);

        server.Submit(commands);
        server.EndPass();
    }

    private void RenderParticles(IRenderCommandList commands)
    {
        foreach (var (transforms, particles, colors, _) in _particleRenderQuery.Chunks)
        {
            var posSpan = transforms.Span;
            var particleSpan = particles.Span;
            var colorSpan = colors.Span;

            for (int i = 0; i < transforms.Length; i++)
            {
                var pos = posSpan[i].Position;
                var particle = particleSpan[i];
                var baseColor = colorSpan[i].Value;

                var size = 4f * particle.Life * 2;
                var color = new Color4(baseColor.R, baseColor.G, baseColor.B, particle.Life);
                commands.DrawQuad(pos - new Vector2(size / 2), new Vector2(size), color);
            }
        }
    }

    private void RenderBricks(IRenderCommandList commands)
    {
        foreach (var (transforms, sizes, bricks, colors, _) in _brickRenderQuery.Chunks)
        {
            var posSpan = transforms.Span;
            var sizeSpan = sizes.Span;
            var brickSpan = bricks.Span;
            var colorSpan = colors.Span;

            for (int i = 0; i < transforms.Length; i++)
            {
                var pos = posSpan[i].Position;
                var size = sizeSpan[i].Value;
                var brick = brickSpan[i];
                var color = colorSpan[i].Value;

                commands.DrawQuad(pos, size - new Vector2(2), color);

                // Draw border for hard bricks
                if (brick.Type == BrickType.Hard)
                {
                    commands.DrawQuad(pos, new Vector2(size.X - 2, 2), new Color4(1f, 1f, 1f, 0.3f));
                }
            }
        }
    }

    private void RenderPowerUps(IRenderCommandList commands)
    {
        foreach (var (transforms, sizes, _, colors, _) in _powerUpRenderQuery.Chunks)
        {
            var posSpan = transforms.Span;
            var sizeSpan = sizes.Span;
            var colorSpan = colors.Span;

            for (int i = 0; i < transforms.Length; i++)
            {
                var pos = posSpan[i].Position;
                var size = sizeSpan[i].Value;
                var color = colorSpan[i].Value;

                commands.DrawQuad(pos - size / 2, size, color);
            }
        }
    }

    private void RenderPaddle(IRenderCommandList commands)
    {
        var paddleColor = _state.WidePaddleTimer > 0
            ? new Color4(0.2f, 0.6f, 1f, 1f)
            : new Color4(0.8f, 0.8f, 0.9f, 1f);

        foreach (var (transforms, sizes, _, _) in _paddleRenderQuery.Chunks)
        {
            var posSpan = transforms.Span;
            var sizeSpan = sizes.Span;

            for (int i = 0; i < transforms.Length; i++)
            {
                var pos = posSpan[i].Position;
                var size = sizeSpan[i].Value;
                var left = pos.X - size.X / 2;
                var top = pos.Y - size.Y / 2;
                commands.DrawQuad(new Vector2(left, top), size, paddleColor);
            }
        }
    }

    private void RenderBall(IRenderCommandList commands)
    {
        var ballColor = _state.SlowBallTimer > 0
            ? new Color4(1f, 1f, 0.5f, 1f)
            : Color4.White;

        foreach (var (transforms, radii, _, _) in _ballRenderQuery.Chunks)
        {
            var posSpan = transforms.Span;
            var radiusSpan = radii.Span;

            for (int i = 0; i < transforms.Length; i++)
            {
                var pos = posSpan[i].Position;
                var radius = radiusSpan[i].Value;
                commands.DrawQuad(new Vector2(pos.X - radius, pos.Y - radius), new Vector2(radius * 2), ballColor);
            }
        }
    }

    private void RenderUI(IRenderCommandList commands)
    {
        if (!_context.Font.IsValid) return;

        // Score
        commands.DrawText(_context.Font, $"Score: {_state.Score}",
            new Vector2(10, 10), 20f, Color4.White);

        // Level
        var levelText = $"Level {_state.CurrentLevel}";
        var (levelWidth, _) = _context.RenderingServer.MeasureText(_context.Font, levelText, 20f);
        commands.DrawText(_context.Font, levelText,
            new Vector2(_width / 2f - levelWidth / 2, 10), 20f, Color4.White);

        // Lives
        var livesText = $"Lives: {_state.Lives}";
        var (livesWidth, _) = _context.RenderingServer.MeasureText(_context.Font, livesText, 20f);
        commands.DrawText(_context.Font, livesText,
            new Vector2(_width - livesWidth - 10, 10), 20f, Color4.White);

        // Power-up indicators
        var indicatorY = 40f;
        if (_state.WidePaddleTimer > 0)
        {
            commands.DrawText(_context.Font, $"Wide Paddle: {_state.WidePaddleTimer:F1}s",
                new Vector2(10, indicatorY), 14f, new Color4(0.2f, 0.6f, 1f, 1f));
            indicatorY += 18f;
        }
        if (_state.SlowBallTimer > 0)
        {
            commands.DrawText(_context.Font, $"Slow Ball: {_state.SlowBallTimer:F1}s",
                new Vector2(10, indicatorY), 14f, new Color4(1f, 1f, 0.5f, 1f));
        }
    }

    private void RenderStateOverlay(IRenderCommandList commands)
    {
        if (!_context.Font.IsValid) return;

        switch (_state.Phase)
        {
            case GamePhase.Ready:
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font,
                    "Press SPACE to Launch", _width / 2f, _height - 100f, 24f, Color4.Yellow);
                break;
            case GamePhase.Paused:
                DrawOverlayBackground(commands);
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font,
                    "PAUSED", _width / 2f, _height / 2f - 30f, 48f, Color4.White);
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font,
                    "Press ENTER to Resume", _width / 2f, _height / 2f + 30f, 20f, Color4.Gray);
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font,
                    "Press ESC to Quit", _width / 2f, _height / 2f + 60f, 16f, Color4.Gray);
                break;
            case GamePhase.LevelComplete:
                DrawOverlayBackground(commands);
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font,
                    "LEVEL COMPLETE!", _width / 2f, _height / 2f - 30f, 48f, new Color4(0.2f, 0.9f, 0.2f, 1f));
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font,
                    "Press ENTER to Continue", _width / 2f, _height / 2f + 30f, 20f, Color4.Gray);
                break;
            case GamePhase.GameOver:
                DrawOverlayBackground(commands);
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font,
                    "GAME OVER", _width / 2f, _height / 2f - 30f, 48f, new Color4(0.9f, 0.2f, 0.2f, 1f));
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font,
                    $"Final Score: {_state.Score}", _width / 2f, _height / 2f + 30f, 24f, Color4.White);
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font,
                    "Press ENTER to Continue", _width / 2f, _height / 2f + 70f, 20f, Color4.Gray);
                break;
            case GamePhase.Victory:
                DrawOverlayBackground(commands);
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font,
                    "VICTORY!", _width / 2f, _height / 2f - 30f, 48f, new Color4(1f, 0.8f, 0.2f, 1f));
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font,
                    $"Final Score: {_state.Score}", _width / 2f, _height / 2f + 30f, 24f, Color4.White);
                UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font,
                    "Press ENTER to Continue", _width / 2f, _height / 2f + 70f, 20f, Color4.Gray);
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
        _systems.SetScreenSize(width, height);
    }
}

