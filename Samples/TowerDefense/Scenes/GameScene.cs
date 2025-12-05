using System.Numerics;
using Ignis.Core;
using Ignis.Core.Scenery;
using Ignis.Core.Timing;
using Ignis.Graphics;
using Samples.Common;
using Samples.TowerDefense.Core;
using Samples.TowerDefense.Services;
using Silk.NET.Input;

namespace Samples.TowerDefense.Scenes;

/// <summary>
/// Main gameplay scene for Tower Defense.
/// Features turret placement and enemy waves.
/// </summary>
public sealed class GameScene : Scene, ITowerDefenseScene
{
    private readonly TowerDefenseContext _context;
    private readonly SceneManager _sceneManager;

    // Level data
    private LevelData _level = null!;
    private int _width;
    private int _height;

    // Game state
    private GamePhase _phase = GamePhase.Build;
    private int _currentWave;
    private float _waveTimer;

    // Game objects
    private readonly List<Turret> _turrets = [];
    private readonly List<Enemy> _enemies = [];
    private readonly List<Projectile> _projectiles = [];
    private readonly List<ParticleEffect> _particles = [];

    // Spawn tracking
    private readonly List<PendingSpawn> _pendingSpawns = [];

    // UI state
    private TurretType _selectedTurretType = TurretType.Blaster;
    private Vector2 _mousePos;
    private (int x, int y)? _hoveredCell;
    private bool _canPlaceAtHovered;

    // Constants
    private const float TileSize = 60f;
    private const float PathWidth = 40f;

    public GameScene(TowerDefenseContext context, SceneManager sceneManager)
    {
        _context = context;
        _sceneManager = sceneManager;
        _width = context.Width;
        _height = context.Height;
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
        _context.Gold = _level.StartingGold;
        _context.Lives = _level.StartingLives;

        _turrets.Clear();
        _enemies.Clear();
        _projectiles.Clear();
        _particles.Clear();
        _pendingSpawns.Clear();

        _currentWave = 0;
        _phase = GamePhase.Build;

        _context.Audio.PlayMusic("game_music");
    }

    public override void OnExit()
    {
        _context.Audio.StopMusic();
    }

    public override void Update(GameTime time)
    {
        var dt = time.DeltaTime;
        var input = _context.GetInput();
        if (input == null) return;

        UpdateMousePosition(input);

        switch (_phase)
        {
            case GamePhase.Build:
                UpdateBuildPhase(dt, input);
                break;
            case GamePhase.Wave:
                UpdateWavePhase(dt, input);
                break;
            case GamePhase.Paused:
                if (input.IsKeyPressed(Key.Escape) || input.IsKeyPressed(Key.Space))
                    _phase = GamePhase.Wave;
                if (input.IsKeyPressed(Key.Q))
                    _sceneManager.LoadScene(new MainMenuScene(_context, _sceneManager));
                break;
            case GamePhase.Victory:
            case GamePhase.GameOver:
                if (input.IsKeyPressed(Key.Enter) || input.IsKeyPressed(Key.Space) || input.IsKeyPressed(Key.Escape))
                {
                    _sceneManager.LoadScene(new MainMenuScene(_context, _sceneManager));
                }
                break;
        }

        // Update particles
        foreach (var p in _particles)
            p.Update(dt);
        _particles.RemoveAll(p => !p.IsAlive);
    }

    private void UpdateMousePosition(InputState input)
    {
        _mousePos = input.MousePosition;

        // Calculate grid cell under mouse
        var gridOffsetX = (_width - _level.GridWidth * TileSize) / 2f;
        var gridOffsetY = 80f;

        var cellX = (int)((_mousePos.X - gridOffsetX) / TileSize);
        var cellY = (int)((_mousePos.Y - gridOffsetY) / TileSize);

        if (cellX >= 0 && cellX < _level.GridWidth && cellY >= 0 && cellY < _level.GridHeight)
        {
            _hoveredCell = (cellX, cellY);
            _canPlaceAtHovered = CanPlaceTurret(cellX, cellY);
        }
        else
        {
            _hoveredCell = null;
            _canPlaceAtHovered = false;
        }
    }

    private void UpdateBuildPhase(float dt, InputState input)
    {
        // Turret type selection
        if (input.IsKeyPressed(Key.Number1)) _selectedTurretType = TurretType.Blaster;
        if (input.IsKeyPressed(Key.Number2)) _selectedTurretType = TurretType.Cannon;
        if (input.IsKeyPressed(Key.Number3)) _selectedTurretType = TurretType.Freezer;

        // Place turret with left click
        if (input.IsMousePressed(MouseButton.Left) && _hoveredCell.HasValue && _canPlaceAtHovered)
        {
            TryPlaceTurret(_hoveredCell.Value.x, _hoveredCell.Value.y);
        }

        // Sell turret with right click
        if (input.IsMousePressed(MouseButton.Right) && _hoveredCell.HasValue)
        {
            TrySellTurret(_hoveredCell.Value.x, _hoveredCell.Value.y);
        }

        // Start wave
        if (input.IsKeyPressed(Key.Space))
        {
            StartNextWave();
        }

        // Pause / back to menu
        if (input.IsKeyPressed(Key.Escape))
        {
            _sceneManager.LoadScene(new MainMenuScene(_context, _sceneManager));
        }
    }

    private void UpdateWavePhase(float dt, InputState input)
    {
        // Pause
        if (input.IsKeyPressed(Key.Escape))
        {
            _phase = GamePhase.Paused;
            return;
        }

        // Update wave timer and spawns
        _waveTimer += dt;
        UpdateSpawns();

        // Update turrets
        foreach (var turret in _turrets)
        {
            turret.Update(dt);
            UpdateTurretTargeting(turret);

            if (turret.CanFire && turret.Target != null)
            {
                FireTurret(turret);
            }
        }

        // Update projectiles
        foreach (var proj in _projectiles)
        {
            proj.Update(dt);
            if (!proj.IsAlive && proj.Target.IsAlive)
            {
                HitEnemy(proj);
            }
        }
        _projectiles.RemoveAll(p => !p.IsAlive);

        // Update enemies
        foreach (var enemy in _enemies.Where(e => e.IsAlive))
        {
            enemy.Update(dt);
            MoveEnemyAlongPath(enemy, dt);

            if (enemy.ReachedEnd)
            {
                _context.Lives--;
                _context.Audio.PlaySfx(AudioService.SfxEnemyReachEnd);
                enemy.IsAlive = false;

                if (_context.Lives <= 0)
                {
                    _phase = GamePhase.GameOver;
                    _context.Audio.PlaySfx(AudioService.SfxGameOver);
                    return;
                }
            }
        }
        _enemies.RemoveAll(e => !e.IsAlive);

        // Check wave complete
        if (_pendingSpawns.Count == 0 && _enemies.Count == 0)
        {
            CompleteWave();
        }
    }

    private void StartNextWave()
    {
        _currentWave++;
        if (_currentWave > _level.Waves.Count)
        {
            // All waves complete
            _phase = GamePhase.Victory;
            _context.Settings.UnlockLevel(_context.CurrentLevel + 1);
            _context.Audio.PlaySfx(AudioService.SfxVictory);
            return;
        }

        var wave = _level.Waves[_currentWave - 1];
        _waveTimer = 0;
        _pendingSpawns.Clear();

        // Queue all spawns for this wave
        foreach (var spawn in wave.Spawns)
        {
            for (var i = 0; i < spawn.Count; i++)
            {
                _pendingSpawns.Add(new PendingSpawn
                {
                    Type = spawn.Type,
                    SpawnTime = spawn.Delay + i * spawn.Interval
                });
            }
        }

        _pendingSpawns.Sort((a, b) => a.SpawnTime.CompareTo(b.SpawnTime));
        _phase = GamePhase.Wave;
        _context.Audio.PlaySfx(AudioService.SfxWaveStart);
    }

    private void UpdateSpawns()
    {
        while (_pendingSpawns.Count > 0 && _pendingSpawns[0].SpawnTime <= _waveTimer)
        {
            var spawn = _pendingSpawns[0];
            _pendingSpawns.RemoveAt(0);

            var definition = EnemyDefinition.Get(spawn.Type);
            var startPos = _level.Path[0];
            var enemy = new Enemy(definition, startPos);
            _enemies.Add(enemy);
        }
    }

    private void MoveEnemyAlongPath(Enemy enemy, float dt)
    {
        if (enemy.PathIndex >= _level.Path.Count - 1)
        {
            enemy.ReachedEnd = true;
            return;
        }

        var current = _level.Path[enemy.PathIndex];
        var next = _level.Path[enemy.PathIndex + 1];
        var segment = next - current;
        var segmentLength = segment.Length();

        var distanceToMove = enemy.EffectiveSpeed * dt;
        var progressDistance = enemy.PathProgress * segmentLength + distanceToMove;

        while (progressDistance >= segmentLength && enemy.PathIndex < _level.Path.Count - 1)
        {
            progressDistance -= segmentLength;
            enemy.PathIndex++;

            if (enemy.PathIndex >= _level.Path.Count - 1)
            {
                enemy.Position = _level.Path[^1];
                enemy.ReachedEnd = true;
                return;
            }

            current = _level.Path[enemy.PathIndex];
            next = _level.Path[enemy.PathIndex + 1];
            segment = next - current;
            segmentLength = segment.Length();
        }

        enemy.PathProgress = progressDistance / segmentLength;
        enemy.Position = Vector2.Lerp(current, next, enemy.PathProgress);
    }

    private void UpdateTurretTargeting(Turret turret)
    {
        // Check if current target is still valid
        if (turret.Target != null)
        {
            if (!turret.Target.IsAlive ||
                Vector2.Distance(turret.Position, turret.Target.Position) > turret.Definition.Range)
            {
                turret.Target = null;
            }
        }

        // Find new target if needed
        if (turret.Target == null)
        {
            Enemy? bestTarget = null;
            var bestProgress = -1f;

            foreach (var enemy in _enemies.Where(e => e.IsAlive))
            {
                var dist = Vector2.Distance(turret.Position, enemy.Position);
                if (dist > turret.Definition.Range) continue;

                // Prioritize enemies furthest along the path
                var progress = enemy.PathIndex + enemy.PathProgress;
                if (progress > bestProgress)
                {
                    bestProgress = progress;
                    bestTarget = enemy;
                }
            }

            turret.Target = bestTarget;
        }

        // Update rotation to face target
        if (turret.Target != null)
        {
            var dir = turret.Target.Position - turret.Position;
            turret.Rotation = MathF.Atan2(dir.Y, dir.X);
        }
    }

    private void FireTurret(Turret turret)
    {
        turret.Fire();

        var proj = new Projectile(turret, turret.Target!);
        _projectiles.Add(proj);

        var sfx = turret.Definition.Type switch
        {
            TurretType.Blaster => AudioService.SfxBlasterFire,
            TurretType.Cannon => AudioService.SfxCannonFire,
            TurretType.Freezer => AudioService.SfxFreezerFire,
            _ => AudioService.SfxBlasterFire
        };
        _context.Audio.PlaySfx(sfx);
    }

    private void HitEnemy(Projectile proj)
    {
        var def = proj.Source.Definition;

        // Splash damage
        if (def.SplashRadius > 0)
        {
            foreach (var enemy in _enemies.Where(e => e.IsAlive))
            {
                var dist = Vector2.Distance(proj.Position, enemy.Position);
                if (dist <= def.SplashRadius)
                {
                    var falloff = 1f - (dist / def.SplashRadius) * 0.5f;
                    var damage = (int)(def.Damage * falloff);
                    ApplyDamageToEnemy(enemy, damage, def.SlowAmount, def.SlowDuration);
                }
            }
            SpawnExplosion(proj.Position);
            _context.Audio.PlaySfx(AudioService.SfxExplosion);
        }
        else
        {
            ApplyDamageToEnemy(proj.Target, def.Damage, def.SlowAmount, def.SlowDuration);
        }
    }

    private void ApplyDamageToEnemy(Enemy enemy, int damage, float slowAmount, float slowDuration)
    {
        var wasAlive = enemy.IsAlive;
        enemy.TakeDamage(damage, slowAmount, slowDuration);

        if (wasAlive && !enemy.IsAlive)
        {
            // Enemy killed
            _context.Gold += enemy.Definition.GoldReward;
            _context.TotalScore += enemy.Definition.ScoreValue;
            _context.Audio.PlaySfx(AudioService.SfxEnemyDeath);

            SpawnDeathEffect(enemy.Position);

            // Boss spawns minions on death
            if (enemy.Definition.SpawnsOnDeath)
            {
                for (var i = 0; i < enemy.Definition.SpawnCount; i++)
                {
                    var grunt = new Enemy(EnemyDefinition.Grunt, enemy.Position);
                    grunt.PathIndex = enemy.PathIndex;
                    grunt.PathProgress = enemy.PathProgress;
                    _enemies.Add(grunt);
                }
            }
        }
        else if (wasAlive)
        {
            _context.Audio.PlaySfx(AudioService.SfxEnemyHit);
        }
    }

    private void CompleteWave()
    {
        var wave = _level.Waves[_currentWave - 1];
        _context.Gold += wave.GoldBonus;
        _context.Audio.PlaySfx(AudioService.SfxWaveComplete);

        if (_currentWave >= _level.Waves.Count)
        {
            _phase = GamePhase.Victory;
            _context.Settings.UnlockLevel(_context.CurrentLevel + 1);
            _context.Audio.PlaySfx(AudioService.SfxVictory);
        }
        else
        {
            _phase = GamePhase.Build;
        }
    }

    private bool CanPlaceTurret(int x, int y)
    {
        // Check if cell is valid ground
        if (!IsValidGroundCell(x, y)) return false;

        // Check if turret already exists
        if (_turrets.Any(t => t.GridX == x && t.GridY == y)) return false;

        // Check if player has enough gold
        var cost = TurretDefinition.Get(_selectedTurretType).Cost;
        if (_context.Gold < cost) return false;

        return true;
    }

    private bool IsValidGroundCell(int x, int y)
    {
        // For simplicity, check if cell is not on the path
        var gridOffsetX = (_width - _level.GridWidth * TileSize) / 2f;
        var gridOffsetY = 80f;

        var cellCenter = new Vector2(
            gridOffsetX + x * TileSize + TileSize / 2f,
            gridOffsetY + y * TileSize + TileSize / 2f
        );

        // Check distance from path
        for (var i = 0; i < _level.Path.Count - 1; i++)
        {
            var dist = DistanceToLineSegment(cellCenter, _level.Path[i], _level.Path[i + 1]);
            if (dist < PathWidth + TileSize / 3f) return false;
        }

        return true;
    }

    private float DistanceToLineSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        var ap = point - a;
        var t = Math.Clamp(Vector2.Dot(ap, ab) / Vector2.Dot(ab, ab), 0f, 1f);
        var closest = a + ab * t;
        return Vector2.Distance(point, closest);
    }

    private void TryPlaceTurret(int x, int y)
    {
        var definition = TurretDefinition.Get(_selectedTurretType);
        if (_context.Gold < definition.Cost)
        {
            _context.Audio.PlaySfx(AudioService.SfxNotEnoughGold);
            return;
        }

        var gridOffsetX = (_width - _level.GridWidth * TileSize) / 2f;
        var gridOffsetY = 80f;

        var worldPos = new Vector2(
            gridOffsetX + x * TileSize + TileSize / 2f,
            gridOffsetY + y * TileSize + TileSize / 2f
        );

        var turret = new Turret(definition, x, y, worldPos);
        _turrets.Add(turret);
        _context.Gold -= definition.Cost;
        _context.Audio.PlaySfx(AudioService.SfxTurretPlace);
    }

    private void TrySellTurret(int x, int y)
    {
        var turret = _turrets.FirstOrDefault(t => t.GridX == x && t.GridY == y);
        if (turret == null) return;

        _turrets.Remove(turret);
        _context.Gold += turret.Definition.SellValue;
        _context.Audio.PlaySfx(AudioService.SfxTurretSell);
    }

    private void SpawnExplosion(Vector2 position)
    {
        var effect = new ParticleEffect
        {
            Position = position,
            Lifetime = 0.5f,
            MaxLifetime = 0.5f,
            Color = new Color4(1f, 0.5f, 0.2f, 1f)
        };

        for (var i = 0; i < 12; i++)
        {
            var angle = i * MathF.PI * 2 / 12;
            var speed = 100f + Random.Shared.NextSingle() * 100f;
            effect.Particles.Add(new Particle
            {
                Position = position,
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed,
                Life = 0.5f,
                Size = 6f,
                Color = new Color4(1f, 0.6f, 0.2f, 1f)
            });
        }

        _particles.Add(effect);
    }

    private void SpawnDeathEffect(Vector2 position)
    {
        var effect = new ParticleEffect
        {
            Position = position,
            Lifetime = 0.4f,
            MaxLifetime = 0.4f
        };

        for (var i = 0; i < 8; i++)
        {
            var angle = Random.Shared.NextSingle() * MathF.PI * 2;
            var speed = 50f + Random.Shared.NextSingle() * 80f;
            effect.Particles.Add(new Particle
            {
                Position = position,
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed,
                Life = 0.4f,
                Size = 4f,
                Color = new Color4(0.8f, 0.2f, 0.2f, 1f)
            });
        }

        _particles.Add(effect);
    }

    #region Rendering

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

        // Render layers
        RenderGrid(commands);
        RenderPath(commands);
        RenderTurrets(commands);
        RenderEnemies(commands);
        RenderProjectiles(commands);
        RenderParticles(commands);
        RenderUI(commands);
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

                // Hover highlight
                if (_hoveredCell.HasValue && _hoveredCell.Value == (x, y) && _phase == GamePhase.Build)
                {
                    var hoverColor = _canPlaceAtHovered
                        ? new Color4(0.2f, 0.5f, 0.3f, 0.4f)
                        : new Color4(0.5f, 0.2f, 0.2f, 0.4f);
                    commands.DrawQuad(new Vector2(cellX, cellY), new Vector2(TileSize, TileSize), hoverColor);
                }
            }
        }
    }

    private void RenderPath(IRenderCommandList commands)
    {
        // Draw path as connected lines
        for (var i = 0; i < _level.Path.Count - 1; i++)
        {
            var a = _level.Path[i];
            var b = _level.Path[i + 1];

            var dir = Vector2.Normalize(b - a);
            var perp = new Vector2(-dir.Y, dir.X) * PathWidth / 2f;

            // Draw as quad
            var color = new Color4(0.2f, 0.15f, 0.25f, 1f);
            var length = Vector2.Distance(a, b);

            // Simplified: draw rectangles along path
            var midpoint = (a + b) / 2f;
            var angle = MathF.Atan2(dir.Y, dir.X);

            // For simplicity, draw circles at waypoints and lines between
            commands.DrawQuad(a - new Vector2(PathWidth / 2), new Vector2(PathWidth, PathWidth), color);
        }

        // Draw waypoint markers
        for (var i = 0; i < _level.Path.Count; i++)
        {
            var pos = _level.Path[i];
            var color = i == 0
                ? new Color4(0.3f, 0.6f, 0.3f, 1f)  // Start - green
                : i == _level.Path.Count - 1
                    ? new Color4(0.6f, 0.3f, 0.3f, 1f)  // End - red
                    : new Color4(0.25f, 0.2f, 0.3f, 1f);

            commands.DrawQuad(pos - new Vector2(PathWidth / 2), new Vector2(PathWidth, PathWidth), color);
        }
    }

    private void RenderTurrets(IRenderCommandList commands)
    {
        foreach (var turret in _turrets)
        {
            var color = turret.Definition.Type switch
            {
                TurretType.Blaster => new Color4(0.3f, 0.6f, 0.9f, 1f),
                TurretType.Cannon => new Color4(0.9f, 0.5f, 0.2f, 1f),
                TurretType.Freezer => new Color4(0.4f, 0.8f, 0.9f, 1f),
                _ => Color4.White
            };

            // Base
            var baseSize = 30f;
            commands.DrawQuad(turret.Position - new Vector2(baseSize / 2), new Vector2(baseSize, baseSize), color);

            // Barrel (simplified as a line toward target)
            var barrelLength = 20f;
            var barrelEnd = turret.Position + new Vector2(MathF.Cos(turret.Rotation), MathF.Sin(turret.Rotation)) * barrelLength;
            var barrelDir = Vector2.Normalize(barrelEnd - turret.Position);
            var barrelPerp = new Vector2(-barrelDir.Y, barrelDir.X) * 4f;

            commands.DrawQuad(turret.Position - barrelPerp, new Vector2(barrelLength, 8f), new Color4(color.R * 0.8f, color.G * 0.8f, color.B * 0.8f, color.A));

            // Range indicator when selected (simplified)
            if (_phase == GamePhase.Build && _hoveredCell.HasValue &&
                _hoveredCell.Value == (turret.GridX, turret.GridY))
            {
                // Just show a text with range info
            }
        }
    }

    private void RenderEnemies(IRenderCommandList commands)
    {
        foreach (var enemy in _enemies.Where(e => e.IsAlive))
        {
            var color = enemy.Definition.Type switch
            {
                EnemyType.Grunt => new Color4(0.7f, 0.3f, 0.3f, 1f),
                EnemyType.Scout => new Color4(0.9f, 0.7f, 0.2f, 1f),
                EnemyType.Tank => new Color4(0.4f, 0.4f, 0.5f, 1f),
                EnemyType.Shielded => new Color4(0.3f, 0.5f, 0.8f, 1f),
                EnemyType.Boss => new Color4(0.6f, 0.2f, 0.6f, 1f),
                _ => Color4.White
            };

            var size = enemy.Definition.Type == EnemyType.Boss ? 24f : 16f;

            // Body
            commands.DrawQuad(enemy.Position - new Vector2(size / 2), new Vector2(size, size), color);

            // Health bar
            var healthBarWidth = size + 4f;
            var healthBarHeight = 4f;
            var healthPercent = (float)enemy.Health / enemy.Definition.MaxHealth;

            var barPos = enemy.Position - new Vector2(healthBarWidth / 2, size / 2 + 8f);
            commands.DrawQuad(barPos, new Vector2(healthBarWidth, healthBarHeight), new Color4(0.2f, 0.2f, 0.2f, 1f));
            commands.DrawQuad(barPos, new Vector2(healthBarWidth * healthPercent, healthBarHeight), new Color4(0.2f, 0.8f, 0.2f, 1f));

            // Shield bar
            if (enemy.Definition.HasShield && enemy.Shield > 0)
            {
                var shieldPercent = (float)enemy.Shield / enemy.Definition.ShieldHealth;
                var shieldPos = barPos - new Vector2(0, 5f);
                commands.DrawQuad(shieldPos, new Vector2(healthBarWidth * shieldPercent, 3f), new Color4(0.3f, 0.6f, 1f, 1f));
            }

            // Slow indicator
            if (enemy.SlowTimer > 0)
            {
                commands.DrawQuad(enemy.Position - new Vector2(size / 2 + 2), new Vector2(size + 4, size + 4),
                    new Color4(0.4f, 0.8f, 1f, 0.3f));
            }
        }
    }

    private void RenderProjectiles(IRenderCommandList commands)
    {
        foreach (var proj in _projectiles)
        {
            var color = proj.Source.Definition.Type switch
            {
                TurretType.Blaster => new Color4(0.5f, 0.8f, 1f, 1f),
                TurretType.Cannon => new Color4(1f, 0.6f, 0.2f, 1f),
                TurretType.Freezer => new Color4(0.6f, 0.9f, 1f, 1f),
                _ => Color4.White
            };

            var size = proj.Source.Definition.Type == TurretType.Cannon ? 8f : 5f;
            commands.DrawQuad(proj.Position - new Vector2(size / 2), new Vector2(size, size), color);
        }
    }

    private void RenderParticles(IRenderCommandList commands)
    {
        foreach (var effect in _particles)
        {
            foreach (var p in effect.Particles)
            {
                var alpha = p.Life / 0.5f;
                var color = p.Color with { A = alpha };
                commands.DrawQuad(p.Position - new Vector2(p.Size / 2), new Vector2(p.Size, p.Size), color);
            }
        }
    }

    private void RenderUI(IRenderCommandList commands)
    {
        // Top bar background
        commands.DrawQuad(Vector2.Zero, new Vector2(_width, 70f), new Color4(0.1f, 0.08f, 0.15f, 0.9f));

        // Level info
        UIRenderer.DrawText(commands, _context.Font, $"Level {_context.CurrentLevel}: {_level.Name}", 20f, 20f, 18f, Color4.White);

        // Wave info
        var waveText = _phase == GamePhase.Build
            ? $"Wave {_currentWave}/{_level.Waves.Count} - Press SPACE to start"
            : $"Wave {_currentWave}/{_level.Waves.Count}";
        UIRenderer.DrawText(commands, _context.Font, waveText, 20f, 45f, 14f, new Color4(0.7f, 0.7f, 0.8f, 1f));

        // Resources (right side)
        var rightX = _width - 250f;

        UIRenderer.DrawText(commands, _context.Font, $"Gold: {_context.Gold}", rightX, 15f, 18f, new Color4(1f, 0.85f, 0.2f, 1f));
        UIRenderer.DrawText(commands, _context.Font, $"Lives: {_context.Lives}", rightX, 38f, 18f, new Color4(1f, 0.4f, 0.4f, 1f));
        UIRenderer.DrawText(commands, _context.Font, $"Score: {_context.TotalScore}", rightX + 120f, 15f, 18f, Color4.White);

        // Phase-specific UI
        switch (_phase)
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
        if (_phase != GamePhase.Build) return;

        // Turret selection panel (bottom)
        var panelY = _height - 80f;
        commands.DrawQuad(new Vector2(0, panelY), new Vector2(_width, 80f), new Color4(0.1f, 0.08f, 0.15f, 0.9f));

        var turretTypes = new[] { TurretType.Blaster, TurretType.Cannon, TurretType.Freezer };
        var startX = _width / 2f - 150f;

        for (var i = 0; i < turretTypes.Length; i++)
        {
            var type = turretTypes[i];
            var def = TurretDefinition.Get(type);
            var x = startX + i * 100f;
            var isSelected = _selectedTurretType == type;
            var canAfford = _context.Gold >= def.Cost;

            // Box
            var boxColor = isSelected
                ? new Color4(0.3f, 0.4f, 0.6f, 1f)
                : new Color4(0.15f, 0.15f, 0.25f, 1f);
            commands.DrawQuad(new Vector2(x, panelY + 10f), new Vector2(80f, 60f), boxColor);

            if (isSelected)
            {
                UIRenderer.DrawBorder(commands, x, panelY + 10f, 80f, 60f, 2f, new Color4(0.5f, 0.7f, 1f, 1f));
            }

            // Turret icon color
            var iconColor = type switch
            {
                TurretType.Blaster => new Color4(0.3f, 0.6f, 0.9f, 1f),
                TurretType.Cannon => new Color4(0.9f, 0.5f, 0.2f, 1f),
                TurretType.Freezer => new Color4(0.4f, 0.8f, 0.9f, 1f),
                _ => Color4.White
            };
            commands.DrawQuad(new Vector2(x + 25f, panelY + 20f), new Vector2(30f, 20f), iconColor);

            // Name and cost
            var textColor = canAfford ? Color4.White : new Color4(0.5f, 0.3f, 0.3f, 1f);
            UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, $"[{i + 1}]", x + 40f, panelY + 52f, 10f, new Color4(0.6f, 0.6f, 0.7f, 1f));
            UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, def.Name, x + 40f, panelY + 65f, 12f, textColor);
            UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, $"${def.Cost}", x + 40f, panelY + 78f, 10f, new Color4(1f, 0.85f, 0.2f, canAfford ? 1f : 0.5f));
        }

        // Instructions
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "Left Click: Place | Right Click: Sell | 1-2-3: Select Turret",
            _width / 2f, panelY - 10f, 12f, new Color4(0.5f, 0.5f, 0.6f, 1f));
    }

    private void RenderPauseOverlay(IRenderCommandList commands)
    {
        commands.DrawQuad(Vector2.Zero, new Vector2(_width, _height), new Color4(0f, 0f, 0f, 0.7f));
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "PAUSED", _width / 2f, _height / 2f - 40f, 48f, Color4.White);
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "Press Space/Escape to Resume", _width / 2f, _height / 2f + 20f, 18f,
            new Color4(0.7f, 0.7f, 0.8f, 1f));
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "Press Q to Quit", _width / 2f, _height / 2f + 50f, 16f,
            new Color4(0.6f, 0.6f, 0.7f, 1f));
    }

    private void RenderVictoryOverlay(IRenderCommandList commands)
    {
        commands.DrawQuad(Vector2.Zero, new Vector2(_width, _height), new Color4(0f, 0.1f, 0f, 0.7f));
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "VICTORY!", _width / 2f, _height / 2f - 60f, 48f, new Color4(0.2f, 1f, 0.4f, 1f));
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, $"Level {_context.CurrentLevel} Complete", _width / 2f, _height / 2f - 10f, 24f, Color4.White);
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, $"Score: {_context.TotalScore}", _width / 2f, _height / 2f + 30f, 20f, Color4.White);
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "Press Enter to Continue", _width / 2f, _height / 2f + 80f, 16f,
            new Color4(0.7f, 0.7f, 0.8f, 1f));
    }

    private void RenderGameOverOverlay(IRenderCommandList commands)
    {
        commands.DrawQuad(Vector2.Zero, new Vector2(_width, _height), new Color4(0.1f, 0f, 0f, 0.7f));
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "GAME OVER", _width / 2f, _height / 2f - 60f, 48f, new Color4(1f, 0.3f, 0.3f, 1f));
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, $"Reached Wave {_currentWave}", _width / 2f, _height / 2f - 10f, 24f, Color4.White);
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, $"Final Score: {_context.TotalScore}", _width / 2f, _height / 2f + 30f, 20f, Color4.White);
        UIRenderer.DrawCenteredText(commands, _context.RenderingServer, _context.Font, "Press Enter to Continue", _width / 2f, _height / 2f + 80f, 16f,
            new Color4(0.7f, 0.7f, 0.8f, 1f));
    }

    #endregion

    public void OnResize(int width, int height)
    {
        _width = width;
        _height = height;
    }

    private struct PendingSpawn
    {
        public EnemyType Type;
        public float SpawnTime;
    }
}
