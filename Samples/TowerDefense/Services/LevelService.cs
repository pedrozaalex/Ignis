using System.Numerics;
using Samples.TowerDefense.Core;

namespace Samples.TowerDefense.Services;

/// <summary>
/// Manages level data for Tower Defense.
/// Contains 10 hand-crafted levels with increasing difficulty.
/// </summary>
public sealed class LevelService
{
    private readonly List<LevelData> _levels = [];

    public int LevelCount => _levels.Count;

    public void LoadLevels()
    {
        _levels.Clear();

        // Level 1: Tutorial - Simple straight path
        _levels.Add(CreateLevel1());

        // Level 2: L-shaped path
        _levels.Add(CreateLevel2());

        // Level 3: S-curve
        _levels.Add(CreateLevel3());

        // Level 4: Split path (enemies can take two routes)
        _levels.Add(CreateLevel4());

        // Level 5: Spiral inward
        _levels.Add(CreateLevel5());

        // Level 6: Cross pattern
        _levels.Add(CreateLevel6());

        // Level 7: Maze-like
        _levels.Add(CreateLevel7());

        // Level 8: Long winding path
        _levels.Add(CreateLevel8());

        // Level 9: Multiple spawn points
        _levels.Add(CreateLevel9());

        // Level 10: Final challenge - complex layout
        _levels.Add(CreateLevel10());
    }

    public LevelData? GetLevel(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > _levels.Count)
            return null;
        return _levels[levelNumber - 1];
    }

    #region Level Definitions

    private LevelData CreateLevel1()
    {
        // Simple horizontal path across the middle
        var path = new List<Vector2>
        {
            new(0, 360),
            new(1280, 360)
        };

        return new LevelData
        {
            LevelNumber = 1,
            Name = "First Contact",
            Path = path,
            StartingGold = 150,
            StartingLives = 25,
            Waves =
            [
                CreateWave(1, [(EnemyType.Grunt, 5, 0f, 1f)]),
                CreateWave(2, [(EnemyType.Grunt, 8, 0f, 0.8f)]),
                CreateWave(3, [(EnemyType.Grunt, 5, 0f, 1f), (EnemyType.Scout, 3, 3f, 0.5f)])
            ]
        };
    }

    private LevelData CreateLevel2()
    {
        // L-shaped path
        var path = new List<Vector2>
        {
            new(0, 100),
            new(800, 100),
            new(800, 620),
            new(1280, 620)
        };

        return new LevelData
        {
            LevelNumber = 2,
            Name = "Corner Defense",
            Path = path,
            StartingGold = 125,
            StartingLives = 22,
            Waves =
            [
                CreateWave(1, [(EnemyType.Grunt, 8, 0f, 0.8f)]),
                CreateWave(2, [(EnemyType.Scout, 6, 0f, 0.4f)]),
                CreateWave(3, [(EnemyType.Grunt, 6, 0f, 1f), (EnemyType.Scout, 4, 2f, 0.5f)]),
                CreateWave(4, [(EnemyType.Tank, 2, 0f, 2f), (EnemyType.Grunt, 8, 1f, 0.6f)])
            ]
        };
    }

    private LevelData CreateLevel3()
    {
        // S-curve
        var path = new List<Vector2>
        {
            new(0, 150),
            new(400, 150),
            new(400, 360),
            new(880, 360),
            new(880, 570),
            new(1280, 570)
        };

        return new LevelData
        {
            LevelNumber = 3,
            Name = "Serpent's Path",
            Path = path,
            StartingGold = 125,
            StartingLives = 20,
            Waves =
            [
                CreateWave(1, [(EnemyType.Grunt, 10, 0f, 0.7f)]),
                CreateWave(2, [(EnemyType.Scout, 8, 0f, 0.4f), (EnemyType.Grunt, 5, 2f, 0.8f)]),
                CreateWave(3, [(EnemyType.Tank, 3, 0f, 2f)]),
                CreateWave(4, [(EnemyType.Shielded, 4, 0f, 1.5f), (EnemyType.Scout, 6, 2f, 0.4f)]),
                CreateWave(5, [(EnemyType.Tank, 2, 0f, 3f), (EnemyType.Shielded, 3, 1f, 1.5f), (EnemyType.Grunt, 10, 3f, 0.5f)])
            ]
        };
    }

    private LevelData CreateLevel4()
    {
        // Converging paths
        var path = new List<Vector2>
        {
            new(0, 200),
            new(300, 200),
            new(500, 360),
            new(780, 360),
            new(980, 200),
            new(1280, 200)
        };

        return new LevelData
        {
            LevelNumber = 4,
            Name = "Crossroads",
            Path = path,
            StartingGold = 120,
            StartingLives = 20,
            Waves =
            [
                CreateWave(1, [(EnemyType.Grunt, 12, 0f, 0.6f)]),
                CreateWave(2, [(EnemyType.Scout, 10, 0f, 0.3f)]),
                CreateWave(3, [(EnemyType.Shielded, 5, 0f, 1.2f)]),
                CreateWave(4, [(EnemyType.Tank, 4, 0f, 2f), (EnemyType.Scout, 8, 1f, 0.4f)]),
                CreateWave(5, [(EnemyType.Grunt, 8, 0f, 0.5f), (EnemyType.Shielded, 4, 2f, 1f), (EnemyType.Tank, 2, 4f, 2f)])
            ]
        };
    }

    private LevelData CreateLevel5()
    {
        // Spiral inward
        var path = new List<Vector2>
        {
            new(0, 100),
            new(1100, 100),
            new(1100, 620),
            new(180, 620),
            new(180, 280),
            new(640, 280),
            new(640, 440)
        };

        return new LevelData
        {
            LevelNumber = 5,
            Name = "The Spiral",
            Path = path,
            StartingGold = 115,
            StartingLives = 18,
            Waves =
            [
                CreateWave(1, [(EnemyType.Grunt, 15, 0f, 0.5f)]),
                CreateWave(2, [(EnemyType.Scout, 12, 0f, 0.3f), (EnemyType.Grunt, 8, 2f, 0.6f)]),
                CreateWave(3, [(EnemyType.Tank, 5, 0f, 1.5f)]),
                CreateWave(4, [(EnemyType.Shielded, 6, 0f, 1f), (EnemyType.Scout, 10, 2f, 0.3f)]),
                CreateWave(5, [(EnemyType.Boss, 1, 0f, 0f)]),
                CreateWave(6, [(EnemyType.Grunt, 10, 0f, 0.4f), (EnemyType.Tank, 3, 2f, 1.5f), (EnemyType.Shielded, 4, 4f, 1f)])
            ]
        };
    }

    private LevelData CreateLevel6()
    {
        // Cross pattern
        var path = new List<Vector2>
        {
            new(0, 360),
            new(480, 360),
            new(640, 200),
            new(800, 360),
            new(1280, 360)
        };

        return new LevelData
        {
            LevelNumber = 6,
            Name = "Divergence",
            Path = path,
            StartingGold = 110,
            StartingLives = 18,
            Waves =
            [
                CreateWave(1, [(EnemyType.Scout, 15, 0f, 0.25f)]),
                CreateWave(2, [(EnemyType.Grunt, 12, 0f, 0.5f), (EnemyType.Scout, 8, 3f, 0.3f)]),
                CreateWave(3, [(EnemyType.Shielded, 8, 0f, 0.8f)]),
                CreateWave(4, [(EnemyType.Tank, 4, 0f, 2f), (EnemyType.Shielded, 4, 2f, 1f)]),
                CreateWave(5, [(EnemyType.Boss, 1, 0f, 0f), (EnemyType.Scout, 10, 3f, 0.3f)]),
                CreateWave(6, [(EnemyType.Tank, 3, 0f, 1.5f), (EnemyType.Shielded, 5, 2f, 0.8f), (EnemyType.Grunt, 15, 4f, 0.3f)])
            ]
        };
    }

    private LevelData CreateLevel7()
    {
        // Maze-like zigzag
        var path = new List<Vector2>
        {
            new(0, 100),
            new(300, 100),
            new(300, 300),
            new(100, 300),
            new(100, 500),
            new(500, 500),
            new(500, 200),
            new(800, 200),
            new(800, 620),
            new(1100, 620),
            new(1100, 300),
            new(1280, 300)
        };

        return new LevelData
        {
            LevelNumber = 7,
            Name = "The Labyrinth",
            Path = path,
            StartingGold = 105,
            StartingLives = 16,
            Waves =
            [
                CreateWave(1, [(EnemyType.Grunt, 18, 0f, 0.4f)]),
                CreateWave(2, [(EnemyType.Scout, 15, 0f, 0.2f)]),
                CreateWave(3, [(EnemyType.Tank, 6, 0f, 1.2f), (EnemyType.Grunt, 10, 2f, 0.5f)]),
                CreateWave(4, [(EnemyType.Shielded, 8, 0f, 0.7f), (EnemyType.Scout, 12, 2f, 0.25f)]),
                CreateWave(5, [(EnemyType.Boss, 1, 0f, 0f), (EnemyType.Tank, 3, 2f, 1.5f)]),
                CreateWave(6, [(EnemyType.Shielded, 6, 0f, 0.6f), (EnemyType.Tank, 4, 2f, 1f), (EnemyType.Scout, 15, 4f, 0.2f)]),
                CreateWave(7, [(EnemyType.Boss, 2, 0f, 5f), (EnemyType.Grunt, 20, 2f, 0.3f)])
            ]
        };
    }

    private LevelData CreateLevel8()
    {
        // Long winding path
        var path = new List<Vector2>
        {
            new(0, 600),
            new(200, 600),
            new(200, 150),
            new(450, 150),
            new(450, 550),
            new(700, 550),
            new(700, 200),
            new(950, 200),
            new(950, 500),
            new(1280, 500)
        };

        return new LevelData
        {
            LevelNumber = 8,
            Name = "Endless March",
            Path = path,
            StartingGold = 100,
            StartingLives = 15,
            Waves =
            [
                CreateWave(1, [(EnemyType.Scout, 20, 0f, 0.2f)]),
                CreateWave(2, [(EnemyType.Grunt, 15, 0f, 0.4f), (EnemyType.Tank, 5, 3f, 1f)]),
                CreateWave(3, [(EnemyType.Shielded, 10, 0f, 0.5f)]),
                CreateWave(4, [(EnemyType.Tank, 8, 0f, 0.8f)]),
                CreateWave(5, [(EnemyType.Boss, 1, 0f, 0f), (EnemyType.Shielded, 6, 2f, 0.6f)]),
                CreateWave(6, [(EnemyType.Scout, 20, 0f, 0.15f), (EnemyType.Tank, 5, 2f, 0.8f)]),
                CreateWave(7, [(EnemyType.Shielded, 8, 0f, 0.4f), (EnemyType.Boss, 1, 3f, 0f), (EnemyType.Grunt, 15, 5f, 0.3f)])
            ]
        };
    }

    private LevelData CreateLevel9()
    {
        // Complex path with loops
        var path = new List<Vector2>
        {
            new(0, 360),
            new(200, 360),
            new(200, 150),
            new(500, 150),
            new(500, 500),
            new(300, 500),
            new(300, 360),
            new(640, 360),
            new(800, 200),
            new(1000, 200),
            new(1000, 500),
            new(1280, 500)
        };

        return new LevelData
        {
            LevelNumber = 9,
            Name = "Temporal Storm",
            Path = path,
            StartingGold = 100,
            StartingLives = 12,
            Waves =
            [
                CreateWave(1, [(EnemyType.Grunt, 20, 0f, 0.3f), (EnemyType.Scout, 15, 2f, 0.2f)]),
                CreateWave(2, [(EnemyType.Tank, 8, 0f, 0.7f), (EnemyType.Shielded, 6, 2f, 0.5f)]),
                CreateWave(3, [(EnemyType.Boss, 2, 0f, 4f)]),
                CreateWave(4, [(EnemyType.Scout, 25, 0f, 0.12f), (EnemyType.Tank, 5, 1f, 0.8f)]),
                CreateWave(5, [(EnemyType.Shielded, 12, 0f, 0.35f), (EnemyType.Boss, 1, 3f, 0f)]),
                CreateWave(6, [(EnemyType.Tank, 10, 0f, 0.5f), (EnemyType.Shielded, 8, 2f, 0.4f)]),
                CreateWave(7, [(EnemyType.Boss, 2, 0f, 3f), (EnemyType.Grunt, 25, 2f, 0.2f), (EnemyType.Scout, 20, 4f, 0.15f)]),
                CreateWave(8, [(EnemyType.Boss, 3, 0f, 2f), (EnemyType.Tank, 8, 3f, 0.5f)])
            ]
        };
    }

    private LevelData CreateLevel10()
    {
        // Final challenge - long complex path
        var path = new List<Vector2>
        {
            new(0, 100),
            new(400, 100),
            new(400, 300),
            new(200, 300),
            new(200, 500),
            new(600, 500),
            new(600, 200),
            new(900, 200),
            new(900, 600),
            new(700, 600),
            new(700, 400),
            new(1100, 400),
            new(1100, 150),
            new(1280, 150)
        };

        return new LevelData
        {
            LevelNumber = 10,
            Name = "Temporal Nexus",
            Path = path,
            StartingGold = 120,
            StartingLives = 10,
            Waves =
            [
                CreateWave(1, [(EnemyType.Grunt, 25, 0f, 0.25f), (EnemyType.Scout, 20, 2f, 0.15f)]),
                CreateWave(2, [(EnemyType.Tank, 10, 0f, 0.6f), (EnemyType.Shielded, 8, 2f, 0.4f)]),
                CreateWave(3, [(EnemyType.Boss, 2, 0f, 3f), (EnemyType.Scout, 25, 2f, 0.1f)]),
                CreateWave(4, [(EnemyType.Shielded, 15, 0f, 0.25f), (EnemyType.Tank, 8, 2f, 0.5f)]),
                CreateWave(5, [(EnemyType.Scout, 30, 0f, 0.08f), (EnemyType.Grunt, 20, 1f, 0.2f), (EnemyType.Tank, 5, 3f, 0.6f)]),
                CreateWave(6, [(EnemyType.Boss, 3, 0f, 2.5f), (EnemyType.Shielded, 10, 2f, 0.3f)]),
                CreateWave(7, [(EnemyType.Tank, 12, 0f, 0.4f), (EnemyType.Scout, 25, 2f, 0.1f), (EnemyType.Shielded, 10, 4f, 0.25f)]),
                CreateWave(8, [(EnemyType.Boss, 2, 0f, 2f), (EnemyType.Tank, 10, 2f, 0.4f), (EnemyType.Shielded, 12, 4f, 0.25f)]),
                CreateWave(9, [(EnemyType.Scout, 40, 0f, 0.06f), (EnemyType.Grunt, 30, 1f, 0.15f)]),
                CreateWave(10, [(EnemyType.Boss, 5, 0f, 1.5f), (EnemyType.Tank, 15, 3f, 0.3f), (EnemyType.Shielded, 15, 5f, 0.2f)])
            ]
        };
    }

    private WaveData CreateWave(int waveNumber, List<(EnemyType type, int count, float delay, float interval)> spawns)
    {
        var wave = new WaveData
        {
            WaveNumber = waveNumber,
            GoldBonus = 20 + waveNumber * 5
        };

        foreach (var (type, count, delay, interval) in spawns)
        {
            wave.Spawns.Add(new SpawnEntry
            {
                Type = type,
                Count = count,
                Delay = delay,
                Interval = interval
            });
        }

        return wave;
    }

    #endregion
}
