namespace Ignis.Core.Timing;

/// <summary>
/// Snapshot of time passed to systems during updates.
/// </summary>
public readonly struct GameTime
{
    /// <summary>Wall clock time since engine start.</summary>
    public TimeSpan TotalRealTime { get; }
    
    /// <summary>Simulation time (pausable).</summary>
    public TimeSpan TotalGameTime { get; }
    
    /// <summary>Time since last tick in seconds.</summary>
    public float DeltaTime { get; }
    
    /// <summary>
    /// Value between 0.0 and 1.0 representing interpolation between 
    /// two fixed physics ticks (for smooth rendering).
    /// </summary>
    public float Alpha { get; }
    
    public GameTime(TimeSpan totalRealTime, TimeSpan totalGameTime, float deltaTime, float alpha)
    {
        TotalRealTime = totalRealTime;
        TotalGameTime = totalGameTime;
        DeltaTime = deltaTime;
        Alpha = Math.Clamp(alpha, 0f, 1f);
    }
}

