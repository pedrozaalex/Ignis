using System.Diagnostics;
using System.Numerics;
using Friflo.Engine.ECS;
using Ignis.Core;
using Ignis.Core.Scenery;
using Ignis.Core.Timing;
using Ignis.Graphics;
using Samples.Common;
using Samples.TowerDefense.Core;
using Samples.TowerDefense.ECS;
using Samples.TowerDefense.Services;
using Silk.NET.Input;

// Use ECS versions of types
using GamePhase = Samples.TowerDefense.ECS.GamePhase;
using EnemyType = Samples.TowerDefense.ECS.EnemyType;
using TurretType = Samples.TowerDefense.ECS.TurretType;

namespace Samples.TowerDefense.Scenes;

/// <summary>
/// Main gameplay scene for Tower Defense using ECS.
/// </summary>
public sealed class GameScene : Scene, ITowerDefenseScene
{
    private readonly TowerDefenseContext _context;
    private readonly SceneManager _sceneManager;

    // ECS
    private readonly EntityStore _store;
    private readonly TowerDefenseState _state;
    private TowerDefenseSystems? _systems;

    // Level data
    private LevelData _level = null!;
    private int _width;
    private int _height;

    // Queries for rendering
    private ArchetypeQuery<Transform2D, Turret, SpriteColor>? _turretRenderQuery;
    private ArchetypeQuery<Transform2D, Enemy, SpriteColor>? _enemyRenderQuery;
    private ArchetypeQuery<Transform2D, Projectile, SpriteColor>? _projectileRenderQuery;
    private ArchetypeQuery<Transform2D, Particle>? _particleRenderQuery;
    private ArchetypeQuery<LaserBeam>? _laserBeamRenderQuery;
    private ArchetypeQuery<FreezePulseRing>? _freezePulseRenderQuery;

    // FPS counter
    private readonly Stopwatch _fpsStopwatch = Stopwatch.StartNew();
    private int _fpsFrameCount;
    private int _displayFps;
    private float _displayFrameTimeMs;
    private const long FpsUpdateIntervalMs = 500;

    private const float TileSize = 60f;
    private const float PathWidth = 40f;

    public GameScene(TowerDefenseContext context, SceneManager sceneManager)
    {
        _context = context;
        _sceneManager = sceneManager;
        _width = context.Width;
        _height = context.Height;

        _store = new EntityStore();
        _state = new TowerDefenseState();
    }

    public override void OnEnter(EngineContext context)
    {
        var levelData = _context.Levels.GetLevel(_context.CurrentLevel);
        if (levelData == null)
        {
            _sceneManager.LoadScene(new MainMenuScene(_context, _sceneManager));
            return;
        }

        _level = levelData;
        _state.Reset(_level.StartingGold, _level.StartingLives);

        _systems = new TowerDefenseSystems(_store, _state, _context.Audio, _level);
        _systems.SetScreenSize(_width, _height);

        // Setup render queries
        _turretRenderQuery = _store.Query<Transform2D, Turret, SpriteColor>().AllTags(Tags.Get<TurretTag>());
        _enemyRenderQuery = _store.Query<Transform2D, Enemy, SpriteColor>().AllTags(Tags.Get<EnemyTag>());
        _projectileRenderQuery = _store.Query<Transform2D, Projectile, SpriteColor>().AllTags(Tags.Get<ProjectileTag>());
        _particleRenderQuery = _store.Query<Transform2D, Particle>().AllTags(Tags.Get<ParticleTag>());
        _laserBeamRenderQuery = _store.Query<LaserBeam>().AllTags(Tags.Get<LaserBeamTag>());
        _freezePulseRenderQuery = _store.Query<FreezePulseRing>().AllTags(Tags.Get<FreezePulseTag>());

        _context.Audio.PlayMusic("game_music");
    }

    public override void OnExit()
    {
        _context.Audio.StopMusic();
    }

    public override void Update(GameTime time)
    {
        var dt = time.DeltaTime;

        // Update FPS counter
        _fpsFrameCount++;
        var elapsedMs = _fpsStopwatch.ElapsedMilliseconds;
        if (elapsedMs >= FpsUpdateIntervalMs)
        {
            _displayFps = (int)(_fpsFrameCount * 1000L / elapsedMs);
            _displayFrameTimeMs = (float)elapsedMs / _fpsFrameCount;
            _fpsFrameCount = 0;
            _fpsStopwatch.Restart();
        }

        var input = _context.GetInput();

        // Handle back to menu
        if (input?.IsKeyPressed(Key.Escape) == true && _state.Phase == GamePhase.Build)
        {
            _sceneManager.LoadScene(new MainMenuScene(_context, _sceneManager));
            return;
        }

        // Handle victory/game over transitions
        if (_state.Phase == GamePhase.Victory || _state.Phase == GamePhase.GameOver)
        {
            if (input?.IsKeyPressed(Key.Enter) == true || input?.IsKeyPressed(Key.Space) == true || input?.IsKeyPressed(Key.Escape) == true)
            {
                if (_state.Phase == GamePhase.Victory)
                    _context.Settings.UnlockLevel(_context.CurrentLevel + 1);
                _sceneManager.LoadScene(new MainMenuScene(_context, _sceneManager));
                return;
            }
        }

        // Handle paused quit
        if (_state.Phase == GamePhase.Paused && input?.IsKeyPressed(Key.Q) == true)
        {
            _sceneManager.LoadScene(new MainMenuScene(_context, _sceneManager));
            return;
        }

        _systems?.Update(dt, input);

        // Sync state to context
        _context.Gold = _state.Gold;
        _context.Lives = _state.Lives;
        _context.TotalScore = _state.TotalScore;
    }

    public void Render(float alpha)
    {
        var server = _context.RenderingServer;

        var pass = new RenderPass
        {
            Target = RenderTargetHandle.Screen,
            ClearColor = new Color4(0.08f, 0.06f, 0.12f),
            ClearDepth = true,
            Viewport = new Rect(0, 0, _width, _height)
        };

        server.BeginPass(pass);

        var projection = Matrix4x4.CreateOrthographicOffCenter(0, _width, _height, 0, -1, 1);
        var commands = server.CreateCommandList();
        commands.SetPipeline(server.DefaultShader2D);
        commands.SetProjectionMatrix(projection);
        commands.SetViewMatrix(Matrix4x4.Identity);

        RenderGrid(commands);
        RenderPath(commands);
        RenderTurretRanges(commands);
        RenderFreezePulses(commands);
        RenderTurrets(commands);
        RenderEnemies(commands);
        RenderProjectiles(commands);
        RenderLaserBeams(commands);
        RenderParticles(commands);
        RenderUI(commands);
        RenderBuildUI(commands);

        server.Submit(commands);
        server.EndPass();
    }

    private void RenderGrid(IRenderCommandList commands)
    {
        var gridOffsetX = (_width - _level.GridWidth * TileSize) / 2f;
        var gridOffsetY = 80f;

        for (var y = 0; y < _level.GridHeight; y++)
        {
            for (var x = 0; x < _level.GridWidth; x++)
            {
                var cellX = gridOffsetX + x * TileSize;
                var cellY = gridOffsetY + y * TileSize;

                var isGround = IsValidGroundCell(x, y);
                var color = isGround
                    ? new Color4(0.12f, 0.1f, 0.18f, 1f)
                    : new Color4(0.08f, 0.06f, 0.12f, 0.5f);

                commands.DrawQuad(new Vector2(cellX + 1, cellY + 1), new Vector2(TileSize - 2, TileSize - 2), color);

                if (_state.HoveredCell.HasValue && _state.HoveredCell.Value == (x, y) && _state.Phase == GamePhase.Build)
                {
                    var hoverColor = _state.CanPlaceAtHovered
                        ? new Color4(0.2f, 0.5f, 0.3f, 0.4f)
                        : new Color4(0.5f, 0.2f, 0.2f, 0.4f);
                    commands.DrawQuad(new Vector2(cellX, cellY), new Vector2(TileSize, TileSize), hoverColor);
                }
            }
        }
    }

    private bool IsValidGroundCell(int x, int y)
    {
        var gridOffsetX = (_width - _level.GridWidth * TileSize) / 2f;
        var gridOffsetY = 80f;

        var cellCenter = new Vector2(
            gridOffsetX + x * TileSize + TileSize / 2f,
            gridOffsetY + y * TileSize + TileSize / 2f
        );

        for (var i = 0; i < _level.Path.Count - 1; i++)
        {
            var dist = DistanceToLineSegment(cellCenter, _level.Path[i], _level.Path[i + 1]);
            if (dist < PathWidth + TileSize / 3f) return false;
        }

        return true;
    }

    private static float DistanceToLineSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        var ap = point - a;
        var t = Math.Clamp(Vector2.Dot(ap, ab) / Vector2.Dot(ab, ab), 0f, 1f);
        var closest = a + ab * t;
        return Vector2.Distance(point, closest);
    }

    private void RenderPath(IRenderCommandList commands)
    {
        for (var i = 0; i < _level.Path.Count; i++)
        {
            var pos = _level.Path[i];
            var color = i == 0
                ? new Color4(0.3f, 0.6f, 0.3f, 1f)
                : i == _level.Path.Count - 1
                    ? new Color4(0.6f, 0.3f, 0.3f, 1f)
                    : new Color4(0.25f, 0.2f, 0.3f, 1f);

            commands.DrawQuad(pos - new Vector2(PathWidth / 2), new Vector2(PathWidth, PathWidth), color);
        }
    }

    private void RenderTurretRanges(IRenderCommandList commands)
    {
        if (_turretRenderQuery == null) return;
        if (!_state.HoveredCell.HasValue) return;

        var (hoverX, hoverY) = _state.HoveredCell.Value;
        var gridOffsetX = (_width - _level.GridWidth * TileSize) / 2f;
        var gridOffsetY = 80f;
        var hoverWorldPos = new Vector2(
            gridOffsetX + hoverX * TileSize + TileSize / 2f,
            gridOffsetY + hoverY * TileSize + TileSize / 2f
        );

        // Check if hovering over an existing turret
        bool foundTurret = false;
        foreach (var turretEntity in _turretRenderQuery.Entities)
        {
            var turret = turretEntity.GetComponent<Turret>();
            if (turret.GridX != hoverX || turret.GridY != hoverY) continue;
            
            foundTurret = true;
            var transform = turretEntity.GetComponent<Transform2D>();
            var range = turret.Range;
            
            var color = turret.Type switch
            {
                TurretType.Blaster => new Color4(0.3f, 0.6f, 0.9f, 0.25f),
                TurretType.Cannon => new Color4(0.9f, 0.5f, 0.2f, 0.25f),
                TurretType.Freezer => new Color4(0.4f, 0.8f, 0.9f, 0.25f),
                _ => new Color4(1f, 1f, 1f, 0.25f)
            };
            
            commands.DrawCircleFilled(transform.Position, range, color);
            break;
        }

        // Show placement preview range during build phase if hovering over valid empty cell
        if (foundTurret || _state.Phase != GamePhase.Build || !_state.CanPlaceAtHovered) return;
        
        var selectedType = _state.SelectedTurretType;
        var previewRange = selectedType switch
        {
            TurretType.Blaster => 120f,
            TurretType.Cannon => 150f,
            TurretType.Freezer => 100f,
            _ => 120f
        };
            
        var previewColor = selectedType switch
        {
            TurretType.Blaster => new Color4(0.3f, 0.6f, 0.9f, 0.2f),
            TurretType.Cannon => new Color4(0.9f, 0.5f, 0.2f, 0.2f),
            TurretType.Freezer => new Color4(0.4f, 0.8f, 0.9f, 0.2f),
            _ => new Color4(1f, 1f, 1f, 0.2f)
        };
            
        commands.DrawCircleFilled(hoverWorldPos, previewRange, previewColor);
    }

    private void RenderFreezePulses(IRenderCommandList commands)
    {
        if (_freezePulseRenderQuery == null) return;

        foreach (var pulseEntity in _freezePulseRenderQuery.Entities)
        {
            var pulse = pulseEntity.GetComponent<FreezePulseRing>();
            
            // Fade out as the pulse expands
            var progress = 1f - (pulse.Life / pulse.MaxLife);
            var alpha = (1f - progress) * 0.6f;
            var color = new Color4(0.4f, 0.8f, 1f, alpha);
            
            commands.DrawCircleFilled(pulse.Origin, pulse.CurrentRadius, color);
        }
    }

    private void RenderTurrets(IRenderCommandList commands)
    {
        if (_turretRenderQuery == null) return;

        foreach (var turretEntity in _turretRenderQuery.Entities)
        {
            var transform = turretEntity.GetComponent<Transform2D>();
            var color = turretEntity.GetComponent<SpriteColor>().Value;

            // Base
            const float baseSize = 30f;
            commands.DrawQuad(transform.Position - new Vector2(baseSize / 2), new Vector2(baseSize, baseSize), color);

            // Barrel
            const float barrelLength = 20f;
            var barrelDir = new Vector2(MathF.Cos(transform.Rotation), MathF.Sin(transform.Rotation));
            var barrelEnd = transform.Position + barrelDir * barrelLength;
            var barrelColor = new Color4(color.R * 0.8f, color.G * 0.8f, color.B * 0.8f, color.A);
            commands.DrawLine(transform.Position, barrelEnd, barrelColor, 8f);
        }
    }

    private void RenderEnemies(IRenderCommandList commands)
    {
        if (_enemyRenderQuery == null) return;

        foreach (var enemyEntity in _enemyRenderQuery.Entities)
        {
            var transform = enemyEntity.GetComponent<Transform2D>();
            var enemy = enemyEntity.GetComponent<Enemy>();
            var color = enemyEntity.GetComponent<SpriteColor>().Value;

            var size = enemy.Type == EnemyType.Boss ? 24f : 16f;

            // Body
            commands.DrawQuad(transform.Position - new Vector2(size / 2), new Vector2(size, size), color);

            // Health bar
            var healthBarWidth = size + 4f;
            var healthBarHeight = 4f;
            var healthPercent = (float)enemy.Health / enemy.MaxHealth;

            var barPos = transform.Position - new Vector2(healthBarWidth / 2, size / 2 + 8f);
            commands.DrawQuad(barPos, new Vector2(healthBarWidth, healthBarHeight), new Color4(0.2f, 0.2f, 0.2f, 1f));
            commands.DrawQuad(barPos, new Vector2(healthBarWidth * healthPercent, healthBarHeight), new Color4(0.2f, 0.8f, 0.2f, 1f));

            // Shield bar
            if (enemy.HasShield && enemy.Shield > 0)
            {
                var shieldPercent = (float)enemy.Shield / enemy.MaxShield;
                var shieldPos = barPos - new Vector2(0, 5f);
                commands.DrawQuad(shieldPos, new Vector2(healthBarWidth * shieldPercent, 3f), new Color4(0.3f, 0.6f, 1f, 1f));
            }

            // Slow indicator
            if (enemy.SlowTimer > 0)
            {
                commands.DrawQuad(transform.Position - new Vector2(size / 2 + 2), new Vector2(size + 4, size + 4),
                    new Color4(0.4f, 0.8f, 1f, 0.3f));
            }
        }
    }

    private void RenderProjectiles(IRenderCommandList commands)
    {
        if (_projectileRenderQuery == null) return;

        foreach (var projEntity in _projectileRenderQuery.Entities)
        {
            var transform = projEntity.GetComponent<Transform2D>();
            var projectile = projEntity.GetComponent<Projectile>();
            var color = projEntity.GetComponent<SpriteColor>().Value;

            var size = projectile.SourceType == TurretType.Cannon ? 8f : 5f;
            commands.DrawQuad(transform.Position - new Vector2(size / 2), new Vector2(size, size), color);
        }
    }

    private void RenderLaserBeams(IRenderCommandList commands)
    {
        if (_laserBeamRenderQuery == null) return;

        foreach (var beamEntity in _laserBeamRenderQuery.Entities)
        {
            var beam = beamEntity.GetComponent<LaserBeam>();
            
            // Fade out based on remaining life
            var alpha = beam.Life / beam.MaxLife;
            var color = beam.Color with { A = alpha };
            
            // Draw main beam
            commands.DrawLine(beam.Start, beam.End, color, 3f);
            
            // Draw inner bright core
            var coreColor = new Color4(1f, 1f, 1f, alpha * 0.8f);
            commands.DrawLine(beam.Start, beam.End, coreColor, 1.5f);
        }
    }

    private void RenderParticles(IRenderCommandList commands)
    {
        if (_particleRenderQuery == null) return;

        foreach (var particleEntity in _particleRenderQuery.Entities)
        {
            var transform = particleEntity.GetComponent<Transform2D>();
            var particle = particleEntity.GetComponent<Particle>();

            var alpha = particle.Life / particle.MaxLife;
            var color = particle.Color with { A = alpha };
            commands.DrawQuad(transform.Position - new Vector2(particle.Size / 2), new Vector2(particle.Size, particle.Size), color);
        }
    }

    private void RenderUI(IRenderCommandList commands)
    {
        // Top bar background
        commands.DrawQuad(Vector2.Zero, new Vector2(_width, 70f), new Color4(0.1f, 0.08f, 0.15f, 0.9f));

        // FPS counter
        UIRenderer.DrawText(commands, _context.Font, $"FPS: {_displayFps} ({_displayFrameTimeMs:F1}ms)", 10f, 5f, 12f, new Color4(0.6f, 0.6f, 0.6f, 1f));

        // Level info
        UIRenderer.DrawText(commands, _context.Font, $"Level {_context.CurrentLevel}: {_level.Name}", 20f, 20f, 18f, Color4.White);

        // Wave info
        var waveText = _state.Phase == GamePhase.Build
            ? $"Wave {_state.CurrentWave}/{_level.Waves.Count} - Press SPACE to start"
            : $"Wave {_state.CurrentWave}/{_level.Waves.Count}";
        UIRenderer.DrawText(commands, _context.Font, waveText, 20f, 45f, 14f, new Color4(0.7f, 0.7f, 0.8f, 1f));

        // Resources
        var rightX = _width - 250f;
        UIRenderer.DrawText(commands, _context.Font, $"Gold: {_state.Gold}", rightX, 15f, 18f, new Color4(1f, 0.85f, 0.2f, 1f));
        UIRenderer.DrawText(commands, _context.Font, $"Lives: {_state.Lives}", rightX, 38f, 18f, new Color4(1f, 0.4f, 0.4f, 1f));
        UIRenderer.DrawText(commands, _context.Font, $"Score: {_state.TotalScore}", rightX + 120f, 15f, 18f, Color4.White);

        // Phase-specific overlays
        switch (_state.Phase)
        {
            case GamePhase.Paused:
                RenderPauseOverlay(commands);
                break;
            case GamePhase.Victory:
                RenderVictoryOverlay(commands);
                break;
            case GamePhase.GameOver:
                RenderGameOverOverlay(commands);
                break;
        }
    }

    private void RenderBuildUI(IRenderCommandList commands)
    {
        if (_state.Phase != GamePhase.Build) return;

        var panelY = _height - 80f;
        commands.DrawQuad(new Vector2(0, panelY), new Vector2(_width, 80f), new Color4(0.1f, 0.08f, 0.15f, 0.9f));

        var turretTypes = new[] { TurretType.Blaster, TurretType.Cannon, TurretType.Freezer };
        var startX = _width / 2f - 150f;

        for (var i = 0; i < turretTypes.Length; i++)
        {
            var type = turretTypes[i];
            var cost = EntityFactory.GetTurretCost(type);
            var name = EntityFactory.GetTurretName(type);
            var x = startX + i * 100f;
            var isSelected = _state.SelectedTurretType == type;
            var canAfford = _state.Gold >= cost;

            var boxColor = isSelected
                ? new Color4(0.3f, 0.4f, 0.6f, 1f)
                : new Color4(0.15f, 0.15f, 0.25f, 1f);
            commands.DrawQuad(new Vector2(x, panelY + 10f), new Vector2(80f, 60f), boxColor);

            if (isSelected)
            {
                UIRenderer.DrawBorder(commands, x, panelY + 10f, 80f, 60f, 2f, new Color4(0.5f, 0.7f, 1f, 1f));
            }

            var iconColor = EntityFactory.GetTurretColor(type);
            commands.DrawQuad(new Vector2(x + 25f, panelY + 15f), new Vector2(30f, 18f), iconColor);

            var textColor = canAfford ? Color4.White : new Color4(0.5f, 0.3f, 0.3f, 1f);
            UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, $"[{i + 1}] {name}", x + 40f, panelY + 42f, 10f, textColor);
            UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, $"${cost}", x + 40f, panelY + 56f, 10f, new Color4(1f, 0.85f, 0.2f, canAfford ? 1f : 0.5f));
        }

        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "Left Click: Place | Right Click: Sell | 1-2-3: Select Turret",
            _width / 2f, panelY - 10f, 12f, new Color4(0.5f, 0.5f, 0.6f, 1f));
    }

    private void RenderPauseOverlay(IRenderCommandList commands)
    {
        commands.DrawQuad(Vector2.Zero, new Vector2(_width, _height), new Color4(0f, 0f, 0f, 0.7f));
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "PAUSED", _width / 2f, _height / 2f - 40f, 48f, Color4.White);
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "Press Space/Escape to Resume", _width / 2f, _height / 2f + 20f, 18f, new Color4(0.7f, 0.7f, 0.8f, 1f));
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "Press Q to Quit", _width / 2f, _height / 2f + 50f, 16f, new Color4(0.6f, 0.6f, 0.7f, 1f));
    }

    private void RenderVictoryOverlay(IRenderCommandList commands)
    {
        commands.DrawQuad(Vector2.Zero, new Vector2(_width, _height), new Color4(0f, 0.1f, 0f, 0.7f));
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "VICTORY!", _width / 2f, _height / 2f - 60f, 48f, new Color4(0.2f, 1f, 0.4f, 1f));
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, $"Level {_context.CurrentLevel} Complete", _width / 2f, _height / 2f - 10f, 24f, Color4.White);
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, $"Score: {_state.TotalScore}", _width / 2f, _height / 2f + 30f, 20f, Color4.White);
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "Press Enter to Continue", _width / 2f, _height / 2f + 80f, 16f, new Color4(0.7f, 0.7f, 0.8f, 1f));
    }

    private void RenderGameOverOverlay(IRenderCommandList commands)
    {
        commands.DrawQuad(Vector2.Zero, new Vector2(_width, _height), new Color4(0.1f, 0f, 0f, 0.7f));
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "GAME OVER", _width / 2f, _height / 2f - 60f, 48f, new Color4(1f, 0.3f, 0.3f, 1f));
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, $"Reached Wave {_state.CurrentWave}", _width / 2f, _height / 2f - 10f, 24f, Color4.White);
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, $"Final Score: {_state.TotalScore}", _width / 2f, _height / 2f + 30f, 20f, Color4.White);
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "Press Enter to Continue", _width / 2f, _height / 2f + 80f, 16f, new Color4(0.7f, 0.7f, 0.8f, 1f));
    }

    public void OnResize(int width, int height)
    {
        _width = width;
        _height = height;
        _systems?.SetScreenSize(width, height);
    }
}

