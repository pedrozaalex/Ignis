using Ignis.Core.Timing;

namespace Ignis.Core;

/// <summary>
/// Implements the fixed timestep game loop with interpolation.
/// Decouples simulation speed from frame rate.
/// </summary>
public sealed class EngineLoop
{
    private readonly ITimeProvider _timeProvider;
    private DateTime _lastTickTime;
    private TimeSpan _accumulator;
    private TimeSpan _totalRealTime;
    private TimeSpan _totalGameTime;
    private bool _firstTick = true;
    
    /// <summary>Fixed timestep for physics/logic updates. Default: 1/60th second.</summary>
    public TimeSpan TargetFixedStep { get; set; } = TimeSpan.FromSeconds(1.0 / 60.0);
    
    /// <summary>Maximum fixed updates per frame to prevent spiral of death.</summary>
    public int MaxFixedStepsPerFrame { get; set; } = 5;
    
    /// <summary>When true, game time stops advancing but real time continues.</summary>
    public bool IsPaused { get; set; }
    
    /// <summary>Called for each fixed timestep update.</summary>
    public event Action<GameTime>? OnFixedUpdate;
    
    /// <summary>Called once per frame for rendering with interpolation alpha.</summary>
    public event Action<GameTime>? OnRender;
    
    public EngineLoop(ITimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? new SystemTimeProvider();
    }
    
    /// <summary>
    /// Process one frame. Call this from your main loop.
    /// </summary>
    public void Tick()
    {
        var currentTime = _timeProvider.UtcNow;
        
        if (_firstTick)
        {
            _lastTickTime = currentTime;
            _firstTick = false;
            return;
        }
        
        var frameTime = currentTime - _lastTickTime;
        _lastTickTime = currentTime;
        
        // Always advance real time
        _totalRealTime += frameTime;
        
        // Only accumulate for simulation if not paused
        if (!IsPaused)
        {
            _accumulator += frameTime;
        }
        
        // Fixed updates
        int steps = 0;
        while (_accumulator >= TargetFixedStep && steps < MaxFixedStepsPerFrame)
        {
            var fixedDelta = (float)TargetFixedStep.TotalSeconds;
            
            if (!IsPaused)
            {
                _totalGameTime += TargetFixedStep;
            }
            
            var time = new GameTime(_totalRealTime, _totalGameTime, fixedDelta, 0f);
            OnFixedUpdate?.Invoke(time);
            
            _accumulator -= TargetFixedStep;
            steps++;
        }
        
        // Cap accumulator to prevent spiral of death
        if (steps >= MaxFixedStepsPerFrame)
        {
            _accumulator = TimeSpan.Zero;
        }
        
        // Render with interpolation
        var alpha = (float)(_accumulator.TotalSeconds / TargetFixedStep.TotalSeconds);
        var renderTime = new GameTime(_totalRealTime, _totalGameTime, (float)frameTime.TotalSeconds, alpha);
        OnRender?.Invoke(renderTime);
    }
    
    /// <summary>
    /// Reset the loop state.
    /// </summary>
    public void Reset()
    {
        _firstTick = true;
        _accumulator = TimeSpan.Zero;
        _totalRealTime = TimeSpan.Zero;
        _totalGameTime = TimeSpan.Zero;
    }
}

