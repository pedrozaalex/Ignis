using System.Numerics;
using Samples.TowerDefense.ECS;

namespace Samples.TowerDefense.Core;

// ─────────────────────────────────────────────────────────────────────────────
// Enemy type enum needed by LevelService for spawning
// ─────────────────────────────────────────────────────────────────────────────

// ─────────────────────────────────────────────────────────────────────────────
// Level data structures
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Spawn instruction for a wave.
/// </summary>
public struct SpawnEntry
{
    public EnemyType Type;
    public float Delay;
    public int Count;
    public float Interval;
}

/// <summary>
/// A single wave of enemies.
/// </summary>
public class WaveData
{
    public int WaveNumber { get; init; }
    public List<SpawnEntry> Spawns { get; init; } = [];
    public int GoldBonus { get; init; }
}

/// <summary>
/// Complete level definition.
/// </summary>
public class LevelData
{
    public int LevelNumber { get; init; }
    public string Name { get; init; } = "";
    public int GridWidth { get; init; } = 16;
    public int GridHeight { get; init; } = 9;
    public List<Vector2> Path { get; init; } = [];
    public List<WaveData> Waves { get; init; } = [];
    public int StartingGold { get; init; } = 100;
    public int StartingLives { get; init; } = 20;
}

