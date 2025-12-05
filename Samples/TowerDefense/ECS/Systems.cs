using System.Numerics;
using Friflo.Engine.ECS;
using Ignis.Core;
using Ignis.Graphics;
using Ignis.Physics;
using Samples.TowerDefense.Core;
using Samples.TowerDefense.Services;
using Silk.NET.Input;

namespace Samples.TowerDefense.ECS;

/// <summary>
/// ECS systems for Tower Defense gameplay.
/// </summary>
public sealed class TowerDefenseSystems
{
    private readonly EntityStore _store;
    private readonly TowerDefenseState _state;
    private readonly AudioService _audio;
    private readonly LevelData _level;
    private readonly EntityFactory _factory;

    // Queries
    private readonly ArchetypeQuery<Transform2D, Turret> _turretQuery;
    private readonly ArchetypeQuery<Transform2D, Enemy> _enemyQuery;
    private readonly ArchetypeQuery<Transform2D, Velocity2D, Projectile> _projectileQuery;
    private readonly ArchetypeQuery<Transform2D, Velocity2D, Particle> _particleQuery;
    private readonly ArchetypeQuery<LaserBeam> _laserBeamQuery;
    private readonly ArchetypeQuery<FreezePulseRing> _freezePulseQuery;

    private float _screenWidth;
    private float _screenHeight;
    private const float TileSize = 60f;
    private const float PathWidth = 40f;

    public TowerDefenseSystems(EntityStore store, TowerDefenseState state, AudioService audio, LevelData level)
    {
        _store = store;
        _state = state;
        _audio = audio;
        _level = level;
        _factory = new EntityFactory(store);

        _turretQuery = store.Query<Transform2D, Turret>().AllTags(Tags.Get<TurretTag>());
        _enemyQuery = store.Query<Transform2D, Enemy>().AllTags(Tags.Get<EnemyTag>());
        _projectileQuery = store.Query<Transform2D, Velocity2D, Projectile>().AllTags(Tags.Get<ProjectileTag>());
        _particleQuery = store.Query<Transform2D, Velocity2D, Particle>().AllTags(Tags.Get<ParticleTag>());
        _laserBeamQuery = store.Query<LaserBeam>().AllTags(Tags.Get<LaserBeamTag>());
        _freezePulseQuery = store.Query<FreezePulseRing>().AllTags(Tags.Get<FreezePulseTag>());

        _state.Path = level.Path;
    }

    public void SetScreenSize(float width, float height)
    {
        _screenWidth = width;
        _screenHeight = height;
    }

    public void Update(float dt, InputState? input)
    {
        if (input == null) return;

        UpdateMousePosition(input);

        switch (_state.Phase)
        {
            case GamePhase.Build:
                UpdateBuildPhase(dt, input);
                break;
            case GamePhase.Wave:
                UpdateWavePhase(dt, input);
                break;
            case GamePhase.Paused:
                if (input.IsKeyPressed(Key.Escape) || input.IsKeyPressed(Key.Space))
                    _state.Phase = GamePhase.Wave;
                break;
            case GamePhase.Victory:
            case GamePhase.GameOver:
                // Handled by scene
                break;
        }

        UpdateParticles(dt);
        UpdateLaserBeams(dt);
        UpdateFreezePulses(dt);
        CleanupDeadEntities();
    }

    private void UpdateMousePosition(InputState input)
    {
        _state.MousePosition = input.MousePosition;

        var gridOffsetX = (_screenWidth - _level.GridWidth * TileSize) / 2f;
        var gridOffsetY = 80f;

        var cellX = (int)((_state.MousePosition.X - gridOffsetX) / TileSize);
        var cellY = (int)((_state.MousePosition.Y - gridOffsetY) / TileSize);

        if (cellX >= 0 && cellX < _level.GridWidth && cellY >= 0 && cellY < _level.GridHeight)
        {
            _state.HoveredCell = (cellX, cellY);
            _state.CanPlaceAtHovered = CanPlaceTurret(cellX, cellY);
        }
        else
        {
            _state.HoveredCell = null;
            _state.CanPlaceAtHovered = false;
        }
    }

    private void UpdateBuildPhase(float dt, InputState input)
    {
        if (input.IsKeyPressed(Key.Number1)) _state.SelectedTurretType = TurretType.Blaster;
        if (input.IsKeyPressed(Key.Number2)) _state.SelectedTurretType = TurretType.Cannon;
        if (input.IsKeyPressed(Key.Number3)) _state.SelectedTurretType = TurretType.Freezer;

        if (input.IsMousePressed(MouseButton.Left) && _state.HoveredCell.HasValue && _state.CanPlaceAtHovered)
            TryPlaceTurret(_state.HoveredCell.Value.x, _state.HoveredCell.Value.y);

        if (input.IsMousePressed(MouseButton.Right) && _state.HoveredCell.HasValue)
            TrySellTurret(_state.HoveredCell.Value.x, _state.HoveredCell.Value.y);

        if (input.IsKeyPressed(Key.Space))
            StartNextWave();
    }

    private void UpdateWavePhase(float dt, InputState input)
    {
        if (input.IsKeyPressed(Key.Escape))
        {
            _state.Phase = GamePhase.Paused;
            return;
        }

        _state.WaveTimer += dt;
        UpdateSpawns();
        UpdateTurrets(dt);
        UpdateProjectiles(dt);
        UpdateEnemies(dt);

        // Check wave complete
        if (_state.PendingSpawns.Count == 0 && _enemyQuery.Count == 0)
            CompleteWave();
    }

    private void StartNextWave()
    {
        _state.CurrentWave++;
        if (_state.CurrentWave > _level.Waves.Count)
        {
            _state.Phase = GamePhase.Victory;
            _audio.PlaySfx(AudioService.SfxVictory);
            return;
        }

        var wave = _level.Waves[_state.CurrentWave - 1];
        _state.WaveTimer = 0;
        _state.PendingSpawns.Clear();

        foreach (var spawn in wave.Spawns)
        {
            for (var i = 0; i < spawn.Count; i++)
            {
                _state.PendingSpawns.Add(new PendingSpawn
                {
                    Type = spawn.Type,
                    SpawnTime = spawn.Delay + i * spawn.Interval
                });
            }
        }

        _state.PendingSpawns.Sort((a, b) => a.SpawnTime.CompareTo(b.SpawnTime));
        _state.Phase = GamePhase.Wave;
        _audio.PlaySfx(AudioService.SfxWaveStart);
    }

    private void UpdateSpawns()
    {
        while (_state.PendingSpawns.Count > 0 && _state.PendingSpawns[0].SpawnTime <= _state.WaveTimer)
        {
            var spawn = _state.PendingSpawns[0];
            _state.PendingSpawns.RemoveAt(0);

            var startPos = _state.Path[0];
            _factory.CreateEnemy(spawn.Type, startPos);
        }
    }

    private void UpdateTurrets(float dt)
    {
        // Collect targeting changes to apply after iteration (avoid StructuralChangeException)
        var targetChanges = new List<(Entity turret, Entity? newTarget)>();
        // Collect laser kills to apply after iteration
        var laserKills = new List<(Entity entity, Vector2 pos)>();

        foreach (var turretEntity in _turretQuery.Entities)
        {
            ref var transform = ref turretEntity.GetComponent<Transform2D>();
            ref var turret = ref turretEntity.GetComponent<Turret>();

            if (turret.FireCooldown > 0)
                turret.FireCooldown -= dt;

            // Freezer turrets use aura - handle separately
            if (turret.Type == TurretType.Freezer)
            {
                UpdateFreezerAura(turretEntity, ref transform, ref turret, dt);
                continue;
            }

            // Collect targeting updates (read-only check, no structural changes)
            var newTarget = ComputeTurretTarget(turretEntity, ref transform, ref turret);
            targetChanges.Add((turretEntity, newTarget));

            // Fire if ready and has valid current target
            if (turret.FireCooldown <= 0 && turretEntity.HasComponent<TargetLink>())
            {
                var targetLink = turretEntity.GetComponent<TargetLink>();
                var targetEntity = targetLink.Target;
                if (!targetEntity.IsNull && targetEntity.Tags.Has<EnemyTag>() && !targetEntity.Tags.Has<Dead>())
                {
                    var killed = FireTurret(turretEntity, ref transform, ref turret, targetEntity);
                    if (killed.HasValue)
                        laserKills.Add(killed.Value);
                }
            }
        }

        // Apply targeting changes outside iteration (safe for structural changes)
        foreach (var (turretEntity, newTarget) in targetChanges)
        {
            if (newTarget.HasValue)
            {
                if (turretEntity.HasComponent<TargetLink>())
                {
                    // Update existing target link
                    turretEntity.GetComponent<TargetLink>().Target = newTarget.Value;
                }
                else
                {
                    turretEntity.AddComponent(new TargetLink(newTarget.Value));
                }

                // Update rotation toward target
                ref var transform = ref turretEntity.GetComponent<Transform2D>();
                var targetPos = newTarget.Value.GetComponent<Transform2D>().Position;
                var dir = targetPos - transform.Position;
                transform.Rotation = MathF.Atan2(dir.Y, dir.X);
            }
            else if (turretEntity.HasComponent<TargetLink>())
            {
                turretEntity.RemoveComponent<TargetLink>();
            }
        }

        // Process laser kills outside iteration
        foreach (var (entity, pos) in laserKills)
        {
            if (entity.IsNull || entity.Tags.Has<Dead>()) continue;
            entity.AddTag<Dead>();
            
            // Spawn death particles
            for (var i = 0; i < 6; i++)
            {
                var angle = Random.Shared.NextSingle() * MathF.PI * 2;
                var speed = 40f + Random.Shared.NextSingle() * 60f;
                var velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
                _factory.CreateParticle(pos, velocity, new Color4(0.4f, 0.7f, 1f, 1f), 3f, 0.3f);
            }
        }
    }

    /// <summary>
    /// Updates the freezer turret - emits slowing pulses periodically.
    /// </summary>
    private void UpdateFreezerAura(Entity turretEntity, ref Transform2D transform, ref Turret turret, float dt)
    {
        if (!turretEntity.HasComponent<FreezeAura>()) return;
        
        ref var aura = ref turretEntity.GetComponent<FreezeAura>();
        aura.PulseCooldown -= dt;
        
        // Emit pulse when cooldown reaches zero
        if (aura.PulseCooldown <= 0)
        {
            aura.PulseCooldown = aura.PulseInterval;
            
            // Create visual pulse ring
            _factory.CreateFreezePulseRing(transform.Position, aura.Radius, 0.6f);
            
            // Apply slow to all enemies in range immediately
            foreach (var enemyEntity in _enemyQuery.Entities)
            {
                if (enemyEntity.Tags.Has<Dead>()) continue;
                
                var enemyPos = enemyEntity.GetComponent<Transform2D>().Position;
                var dist = Vector2.Distance(transform.Position, enemyPos);
                
                if (dist <= aura.Radius)
                {
                    ref var enemy = ref enemyEntity.GetComponent<Enemy>();
                    enemy.SlowAmount = MathF.Max(enemy.SlowAmount, aura.SlowAmount);
                    enemy.SlowTimer = MathF.Max(enemy.SlowTimer, aura.SlowDuration);
                }
            }
            
            _audio.PlaySfx(AudioService.SfxFreezerFire);
        }
    }

    /// <summary>
    /// Computes the best target for a turret without modifying any components.
    /// Returns null if no valid target found.
    /// </summary>
    private Entity? ComputeTurretTarget(Entity turretEntity, ref Transform2D transform, ref Turret turret)
    {
        // Check if current target is still valid
        if (turretEntity.HasComponent<TargetLink>())
        {
            var targetLink = turretEntity.GetComponent<TargetLink>();
            var target = targetLink.Target;
            
            if (!target.IsNull && !target.Tags.Has<Dead>())
            {
                var targetPos = target.GetComponent<Transform2D>().Position;
                if (Vector2.Distance(transform.Position, targetPos) <= turret.Range)
                {
                    // Current target still valid
                    return target;
                }
            }
            // Target invalid or out of range, find new one
        }

        // Find new target - prioritize enemies furthest along path
        Entity? bestTarget = null;
        float bestProgress = -1f;

        foreach (var enemyEntity in _enemyQuery.Entities)
        {
            if (enemyEntity.Tags.Has<Dead>()) continue;
            
            var enemyPos = enemyEntity.GetComponent<Transform2D>().Position;
            var dist = Vector2.Distance(transform.Position, enemyPos);
            if (dist > turret.Range) continue;

            var enemy = enemyEntity.GetComponent<Enemy>();
            var progress = enemy.PathIndex + enemy.PathProgress;
            if (progress > bestProgress)
            {
                bestProgress = progress;
                bestTarget = enemyEntity;
            }
        }

        return bestTarget;
    }

    /// <summary>
    /// Fires a turret at the target. Returns killed enemy info for deferred processing (laser kills).
    /// </summary>
    private (Entity entity, Vector2 pos)? FireTurret(Entity turretEntity, ref Transform2D transform, ref Turret turret, Entity target)
    {
        turret.FireCooldown = 1f / turret.FireRate;
        
        var targetPos = target.GetComponent<Transform2D>().Position;
        (Entity, Vector2)? killed = null;

        if (turret.Type == TurretType.Blaster)
        {
            // Blaster shoots instant laser - damage target immediately
            killed = FireLaser(ref transform, ref turret, target, targetPos);
        }
        else
        {
            // Cannon shoots projectile
            var direction = targetPos - transform.Position;
            _factory.CreateProjectile(transform.Position, direction, turret);
        }

        var sfx = turret.Type switch
        {
            TurretType.Blaster => AudioService.SfxBlasterFire,
            TurretType.Cannon => AudioService.SfxCannonFire,
            _ => AudioService.SfxBlasterFire
        };
        _audio.PlaySfx(sfx);
        
        return killed;
    }

    /// <summary>
    /// Fires an instant laser beam at the target. Returns killed enemy info for deferred processing.
    /// </summary>
    private (Entity entity, Vector2 pos)? FireLaser(ref Transform2D transform, ref Turret turret, Entity target, Vector2 targetPos)
    {
        // Create visual laser beam
        var laserColor = new Color4(0.4f, 0.7f, 1f, 1f);
        _factory.CreateLaserBeam(transform.Position, targetPos, laserColor, 0.12f);
        
        // Apply damage immediately
        if (!target.IsNull && target.HasComponent<Enemy>() && !target.Tags.Has<Dead>())
        {
            ref var enemy = ref target.GetComponent<Enemy>();
            
            if (enemy.Shield > 0)
            {
                var shieldDamage = Math.Min(enemy.Shield, turret.Damage);
                enemy.Shield -= shieldDamage;
                var remaining = turret.Damage - shieldDamage;
                enemy.Health -= remaining;
                enemy.ShieldRegenCooldown = 3f;
            }
            else
            {
                enemy.Health -= turret.Damage;
            }
            
            if (enemy.Health <= 0)
            {
                enemy.Health = 0;
                _state.Gold += enemy.GoldReward;
                _state.TotalScore += enemy.ScoreValue;
                _audio.PlaySfx(AudioService.SfxEnemyDeath);
                
                // Return killed enemy for deferred Dead tag processing
                return (target, targetPos);
            }
            else
            {
                _audio.PlaySfx(AudioService.SfxEnemyHit);
            }
        }
        
        return null;
    }

    private void UpdateProjectiles(float dt)
    {
        var projectilesToRemove = new List<Entity>();
        var enemiesToKill = new List<(Entity entity, Vector2 pos, Enemy enemy)>();
        var explosions = new List<Vector2>();
        var processedEnemies = new HashSet<int>(); // Avoid hitting same enemy twice per frame

        foreach (var projEntity in _projectileQuery.Entities)
        {
            ref var transform = ref projEntity.GetComponent<Transform2D>();
            ref var velocity = ref projEntity.GetComponent<Velocity2D>();
            var projectile = projEntity.GetComponent<Projectile>();

            // Move projectile by velocity
            transform.Position += velocity.Value * dt;

            // Check if projectile is out of bounds
            if (transform.Position.X < -100 || transform.Position.X > _screenWidth + 100 ||
                transform.Position.Y < -100 || transform.Position.Y > _screenHeight + 100)
            {
                projectilesToRemove.Add(projEntity);
                continue;
            }

            // Get projectile collider
            var projRadius = projEntity.HasComponent<CircleCollider>() 
                ? projEntity.GetComponent<CircleCollider>().Radius 
                : 5f;

            // Check collision with all enemies
            Entity? hitEnemy = null;
            Vector2 hitPos = transform.Position;

            foreach (var enemyEntity in _enemyQuery.Entities)
            {
                if (enemyEntity.Tags.Has<Dead>()) continue;

                var enemyTransform = enemyEntity.GetComponent<Transform2D>();
                var enemyRadius = enemyEntity.HasComponent<CircleCollider>()
                    ? enemyEntity.GetComponent<CircleCollider>().Radius
                    : 8f;

                // Circle vs circle collision
                if (CollisionDetection.CircleVsCircle(
                    transform.Position, projRadius,
                    enemyTransform.Position, enemyRadius))
                {
                    hitEnemy = enemyEntity;
                    hitPos = enemyTransform.Position;
                    break;
                }
            }

            if (!hitEnemy.HasValue) continue;

            // Process hit
            if (projectile.SplashRadius > 0)
            {
                // Splash damage - hit all enemies in radius
                foreach (var enemyEntity in _enemyQuery.Entities)
                {
                    if (enemyEntity.IsNull || enemyEntity.Tags.Has<Dead>()) continue;
                    if (processedEnemies.Contains(enemyEntity.Id)) continue;

                    var enemyPos = enemyEntity.GetComponent<Transform2D>().Position;
                    var dist = Vector2.Distance(transform.Position, enemyPos);
                    
                    if (dist <= projectile.SplashRadius)
                    {
                        var falloff = 1f - (dist / projectile.SplashRadius) * 0.5f;
                        var damage = (int)(projectile.Damage * falloff);
                        var killed = ApplyDamageToEnemy(enemyEntity, damage, projectile.SlowAmount, projectile.SlowDuration);
                        
                        if (killed)
                        {
                            var enemy = enemyEntity.GetComponent<Enemy>();
                            enemiesToKill.Add((enemyEntity, enemyPos, enemy));
                        }
                        processedEnemies.Add(enemyEntity.Id);
                    }
                }
                explosions.Add(transform.Position);
                _audio.PlaySfx(AudioService.SfxExplosion);
            }
            else
            {
                // Single target damage
                var killed = ApplyDamageToEnemy(hitEnemy.Value, projectile.Damage, projectile.SlowAmount, projectile.SlowDuration);
                if (killed)
                {
                    var enemy = hitEnemy.Value.GetComponent<Enemy>();
                    enemiesToKill.Add((hitEnemy.Value, hitPos, enemy));
                }
            }

            projectilesToRemove.Add(projEntity);
        }

        // Apply deferred changes outside iteration
        foreach (var proj in projectilesToRemove)
            proj.AddTag<Dead>();

        foreach (var pos in explosions)
            SpawnExplosion(pos);

        ProcessKilledEnemies(enemiesToKill);
    }

    /// <summary>
    /// Applies damage to an enemy. Returns true if the enemy was killed.
    /// Does NOT add Dead tag - caller must handle that outside query loop.
    /// </summary>
    private bool ApplyDamageToEnemy(Entity enemyEntity, int damage, float slowAmount, float slowDuration)
    {
        if (enemyEntity.IsNull) return false;
        if (!enemyEntity.HasComponent<Enemy>()) return false;

        ref var enemy = ref enemyEntity.GetComponent<Enemy>();

        // Already dead
        if (enemy.Health <= 0) return false;

        var wasAlive = enemy.Health > 0;

        if (slowAmount > 0 && slowDuration > 0)
        {
            enemy.SlowAmount = MathF.Max(enemy.SlowAmount, slowAmount);
            enemy.SlowTimer = MathF.Max(enemy.SlowTimer, slowDuration);
        }

        if (enemy.Shield > 0)
        {
            var shieldDamage = Math.Min(enemy.Shield, damage);
            enemy.Shield -= shieldDamage;
            damage -= shieldDamage;
            enemy.ShieldRegenCooldown = 3f;
        }

        enemy.Health -= damage;
        if (enemy.Health <= 0)
        {
            enemy.Health = 0;
            return wasAlive; // Return true if this kill counts
        }

        if (wasAlive)
            _audio.PlaySfx(AudioService.SfxEnemyHit);

        return false;
    }

    /// <summary>
    /// Processes killed enemies after iteration is complete.
    /// </summary>
    private void ProcessKilledEnemies(List<(Entity entity, Vector2 pos, Enemy enemy)> killed)
    {
        foreach (var (entity, pos, enemy) in killed)
        {
            if (entity.Tags.Has<Dead>()) continue; // Already processed

            entity.AddTag<Dead>();
            _state.Gold += enemy.GoldReward;
            _state.TotalScore += enemy.ScoreValue;
            _audio.PlaySfx(AudioService.SfxEnemyDeath);

            SpawnDeathEffect(pos);

            if (enemy.SpawnsOnDeath)
            {
                for (var i = 0; i < enemy.SpawnCount; i++)
                {
                    var grunt = _factory.CreateEnemy(EnemyType.Grunt, pos);
                    ref var gruntEnemy = ref grunt.GetComponent<Enemy>();
                    gruntEnemy.PathIndex = enemy.PathIndex;
                    gruntEnemy.PathProgress = enemy.PathProgress;
                }
            }
        }
    }

    private void UpdateEnemies(float dt)
    {
        var enemiesToRemove = new List<Entity>();

        foreach (var enemyEntity in _enemyQuery.Entities)
        {
            ref var transform = ref enemyEntity.GetComponent<Transform2D>();
            ref var enemy = ref enemyEntity.GetComponent<Enemy>();

            // Update slow timer
            if (enemy.SlowTimer > 0)
            {
                enemy.SlowTimer -= dt;
                if (enemy.SlowTimer <= 0)
                    enemy.SlowAmount = 0;
            }

            // Regenerate shield
            if (enemy.HasShield && enemy.Shield < enemy.MaxShield)
            {
                if (enemy.ShieldRegenCooldown > 0)
                    enemy.ShieldRegenCooldown -= dt;
                else
                    enemy.Shield = Math.Min(enemy.MaxShield, enemy.Shield + (int)(enemy.ShieldRegenRate * dt));
            }

            // Move along path
            MoveEnemyAlongPath(ref transform, ref enemy, dt);

            if (enemy.ReachedEnd)
            {
                _state.Lives--;
                _audio.PlaySfx(AudioService.SfxEnemyReachEnd);
                enemiesToRemove.Add(enemyEntity);

                if (_state.Lives <= 0)
                {
                    _state.Phase = GamePhase.GameOver;
                    _audio.PlaySfx(AudioService.SfxGameOver);
                    break;
                }
            }
        }

        // Apply deferred removals
        foreach (var entity in enemiesToRemove)
            entity.AddTag<Dead>();
    }

    private void MoveEnemyAlongPath(ref Transform2D transform, ref Enemy enemy, float dt)
    {
        if (enemy.PathIndex >= _state.Path.Count - 1)
        {
            enemy.ReachedEnd = true;
            return;
        }

        var current = _state.Path[enemy.PathIndex];
        var next = _state.Path[enemy.PathIndex + 1];
        var segment = next - current;
        var segmentLength = segment.Length();

        var distanceToMove = enemy.EffectiveSpeed * dt;
        var progressDistance = enemy.PathProgress * segmentLength + distanceToMove;

        while (progressDistance >= segmentLength && enemy.PathIndex < _state.Path.Count - 1)
        {
            progressDistance -= segmentLength;
            enemy.PathIndex++;

            if (enemy.PathIndex >= _state.Path.Count - 1)
            {
                transform.Position = _state.Path[^1];
                enemy.ReachedEnd = true;
                return;
            }

            current = _state.Path[enemy.PathIndex];
            next = _state.Path[enemy.PathIndex + 1];
            segment = next - current;
            segmentLength = segment.Length();
        }

        enemy.PathProgress = progressDistance / segmentLength;
        transform.Position = Vector2.Lerp(current, next, enemy.PathProgress);
    }

    private void CompleteWave()
    {
        var wave = _level.Waves[_state.CurrentWave - 1];
        _state.Gold += wave.GoldBonus;
        _audio.PlaySfx(AudioService.SfxWaveComplete);

        // Clean up any remaining projectiles
        ClearProjectiles();

        if (_state.CurrentWave >= _level.Waves.Count)
        {
            _state.Phase = GamePhase.Victory;
            _audio.PlaySfx(AudioService.SfxVictory);
        }
        else
        {
            _state.Phase = GamePhase.Build;
        }
    }

    /// <summary>
    /// Removes all projectiles from the world.
    /// </summary>
    private void ClearProjectiles()
    {
        // Collect first to avoid StructuralChangeException during iteration
        var toRemove = _projectileQuery.Entities.ToArray();
        foreach (var proj in toRemove)
            proj.AddTag<Dead>();
    }

    private bool CanPlaceTurret(int x, int y)
    {
        if (!IsValidGroundCell(x, y)) return false;

        foreach (var turretEntity in _turretQuery.Entities)
        {
            var turret = turretEntity.GetComponent<Turret>();
            if (turret.GridX == x && turret.GridY == y)
                return false;
        }

        var cost = EntityFactory.GetTurretCost(_state.SelectedTurretType);
        return _state.Gold >= cost;
    }

    private bool IsValidGroundCell(int x, int y)
    {
        var gridOffsetX = (_screenWidth - _level.GridWidth * TileSize) / 2f;
        var gridOffsetY = 80f;

        var cellCenter = new Vector2(
            gridOffsetX + x * TileSize + TileSize / 2f,
            gridOffsetY + y * TileSize + TileSize / 2f
        );

        for (var i = 0; i < _state.Path.Count - 1; i++)
        {
            var dist = DistanceToLineSegment(cellCenter, _state.Path[i], _state.Path[i + 1]);
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

    private void TryPlaceTurret(int x, int y)
    {
        var cost = EntityFactory.GetTurretCost(_state.SelectedTurretType);
        if (_state.Gold < cost)
        {
            _audio.PlaySfx(AudioService.SfxNotEnoughGold);
            return;
        }

        var gridOffsetX = (_screenWidth - _level.GridWidth * TileSize) / 2f;
        var gridOffsetY = 80f;

        var worldPos = new Vector2(
            gridOffsetX + x * TileSize + TileSize / 2f,
            gridOffsetY + y * TileSize + TileSize / 2f
        );

        _factory.CreateTurret(_state.SelectedTurretType, x, y, worldPos);
        _state.Gold -= cost;
        _audio.PlaySfx(AudioService.SfxTurretPlace);
    }

    private void TrySellTurret(int x, int y)
    {
        Entity? toSell = null;
        int sellValue = 0;

        foreach (var turretEntity in _turretQuery.Entities)
        {
            var turret = turretEntity.GetComponent<Turret>();
            if (turret.GridX == x && turret.GridY == y)
            {
                toSell = turretEntity;
                sellValue = turret.SellValue;
                break;
            }
        }

        if (toSell.HasValue)
        {
            toSell.Value.AddTag<Dead>();
            _state.Gold += sellValue;
            _audio.PlaySfx(AudioService.SfxTurretSell);
        }
    }

    private void SpawnExplosion(Vector2 position)
    {
        for (var i = 0; i < 12; i++)
        {
            var angle = i * MathF.PI * 2 / 12;
            var speed = 100f + Random.Shared.NextSingle() * 100f;
            var velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
            _factory.CreateParticle(position, velocity, new Color4(1f, 0.6f, 0.2f, 1f), 6f, 0.5f);
        }
    }

    private void SpawnDeathEffect(Vector2 position)
    {
        for (var i = 0; i < 8; i++)
        {
            var angle = Random.Shared.NextSingle() * MathF.PI * 2;
            var speed = 50f + Random.Shared.NextSingle() * 80f;
            var velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
            _factory.CreateParticle(position, velocity, new Color4(0.8f, 0.2f, 0.2f, 1f), 4f, 0.4f);
        }
    }

    private void UpdateParticles(float dt)
    {
        var toRemove = new List<Entity>();

        foreach (var particleEntity in _particleQuery.Entities)
        {
            ref var transform = ref particleEntity.GetComponent<Transform2D>();
            ref var velocity = ref particleEntity.GetComponent<Velocity2D>();
            ref var particle = ref particleEntity.GetComponent<Particle>();

            transform.Position += velocity.Value * dt;
            velocity.Value *= 0.98f;
            particle.Life -= dt;

            if (particle.Life <= 0)
                toRemove.Add(particleEntity);
        }

        foreach (var e in toRemove)
            e.AddTag<Dead>();
    }

    private void UpdateLaserBeams(float dt)
    {
        var toRemove = _laserBeamQuery.Entities
            .Where(e => {
                ref var beam = ref e.GetComponent<LaserBeam>();
                beam.Life -= dt;
                return beam.Life <= 0;
            })
            .ToArray();

        foreach (var e in toRemove)
            e.AddTag<Dead>();
    }

    private void UpdateFreezePulses(float dt)
    {
        var toRemove = _freezePulseQuery.Entities
            .Where(e => {
                ref var pulse = ref e.GetComponent<FreezePulseRing>();
                pulse.Life -= dt;
                // Expand radius based on time
                var progress = 1f - (pulse.Life / pulse.MaxLife);
                pulse.CurrentRadius = pulse.MaxRadius * progress;
                return pulse.Life <= 0;
            })
            .ToArray();

        foreach (var e in toRemove)
            e.AddTag<Dead>();
    }

    private void CleanupDeadEntities()
    {
        var deadQuery = _store.Query().AllTags(Tags.Get<Dead>());
        var buffer = _store.GetCommandBuffer();

        foreach (var entity in deadQuery.Entities)
            buffer.DeleteEntity(entity.Id);

        buffer.Playback();
    }
}

