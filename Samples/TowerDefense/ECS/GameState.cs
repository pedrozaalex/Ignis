using System.Numerics;

namespace Samples.TowerDefense.ECS;

/// <summary>
/// Game flow phases for Tower Defense.
/// </summary>
public enum GamePhase
{
    Build,
    Wave,
    Paused,
    Victory,
    GameOver
}

/// <summary>
/// Pending spawn entry for wave spawning.
/// </summary>
public struct PendingSpawn
{
    public EnemyType Type;
    public float SpawnTime;
}

/// <summary>
/// Central game state for Tower Defense, held as a resource.
/// </summary>
public sealed class TowerDefenseState
{
    public GamePhase Phase = GamePhase.Build;
    public int Gold = 100;
    public int Lives = 20;
    public int TotalScore;
    public int CurrentWave;
    public float WaveTimer;

    public List<PendingSpawn> PendingSpawns = [];
    public List<Vector2> Path = [];

    // UI state
    public TurretType SelectedTurretType = TurretType.Blaster;
    public Vector2 MousePosition;
    public (int x, int y)? HoveredCell;
    public bool CanPlaceAtHovered;

    public void Reset(int startingGold, int startingLives)
    {
        Phase = GamePhase.Build;
        Gold = startingGold;
        Lives = startingLives;
        TotalScore = 0;
        CurrentWave = 0;
        WaveTimer = 0;
        PendingSpawns.Clear();
        SelectedTurretType = TurretType.Blaster;
        HoveredCell = null;
        CanPlaceAtHovered = false;
    }
}

