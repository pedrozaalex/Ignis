namespace Samples.Breakout.ECS;

/// <summary>
/// Game flow states for Breakout.
/// </summary>
public enum GamePhase
{
    Ready,
    Playing,
    Paused,
    LevelComplete,
    GameOver,
    Victory
}

/// <summary>
/// Central game state, held as a resource (not per-entity).
/// </summary>
public sealed class BreakoutState
{
    public GamePhase Phase = GamePhase.Ready;
    public int Score;
    public int Lives = 3;
    public int CurrentLevel = 1;

    // Power-up effect timers
    public float WidePaddleTimer;
    public float SlowBallTimer;
    public const float PowerUpDuration = 10f;

    public void Reset()
    {
        Phase = GamePhase.Ready;
        Score = 0;
        Lives = 3;
        CurrentLevel = 1;
        WidePaddleTimer = 0;
        SlowBallTimer = 0;
    }
}

