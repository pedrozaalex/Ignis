namespace Ignis.Core.Tests;

using Ignis.Core;
using Ignis.Core.Timing;

public class EngineLoopTests
{
    [Fact]
    public void FixedUpdate_CalledAtTargetRate()
    {
        var timeProvider = new MockTimeProvider(DateTime.UtcNow);
        var loop = new EngineLoop(timeProvider)
        {
            TargetFixedStep = TimeSpan.FromSeconds(1.0 / 60.0) // 60 FPS
        };
        
        int fixedUpdateCount = 0;
        loop.OnFixedUpdate += _ => fixedUpdateCount++;
        
        // First tick establishes baseline (no time elapsed yet)
        loop.Tick();
        Assert.Equal(0, fixedUpdateCount);
        
        // Advance time by 1/30th of a second (should trigger 2 fixed updates at 60 FPS)
        timeProvider.Advance(TimeSpan.FromSeconds(1.0 / 30.0));
        loop.Tick();
        
        Assert.Equal(2, fixedUpdateCount);
    }
    
    [Fact]
    public void Render_CalledOncePerTick_AfterBaseline()
    {
        var timeProvider = new MockTimeProvider(DateTime.UtcNow);
        var loop = new EngineLoop(timeProvider);
        
        int renderCount = 0;
        loop.OnRender += _ => renderCount++;
        
        // First tick establishes baseline (no render)
        loop.Tick();
        Assert.Equal(0, renderCount);
        
        // Each subsequent tick should render once
        timeProvider.Advance(TimeSpan.FromSeconds(0.016));
        loop.Tick();
        Assert.Equal(1, renderCount);
        
        timeProvider.Advance(TimeSpan.FromSeconds(0.016));
        loop.Tick();
        Assert.Equal(2, renderCount);
        
        timeProvider.Advance(TimeSpan.FromSeconds(0.016));
        loop.Tick();
        Assert.Equal(3, renderCount);
    }
    
    [Fact]
    public void Alpha_ProvidedToRender_BetweenZeroAndOne()
    {
        var timeProvider = new MockTimeProvider(DateTime.UtcNow);
        var loop = new EngineLoop(timeProvider)
        {
            TargetFixedStep = TimeSpan.FromSeconds(1.0 / 60.0)
        };
        
        float? capturedAlpha = null;
        loop.OnRender += time => capturedAlpha = time.Alpha;
        
        // First tick establishes baseline
        loop.Tick();
        
        // Advance by half a fixed step
        timeProvider.Advance(TimeSpan.FromSeconds(1.0 / 120.0));
        loop.Tick();
        
        Assert.NotNull(capturedAlpha);
        Assert.True(capturedAlpha >= 0f && capturedAlpha <= 1f);
    }
    
    [Fact]
    public void MaxFixedStepsPerFrame_PreventsSpiral()
    {
        var timeProvider = new MockTimeProvider(DateTime.UtcNow);
        var loop = new EngineLoop(timeProvider)
        {
            TargetFixedStep = TimeSpan.FromSeconds(1.0 / 60.0),
            MaxFixedStepsPerFrame = 5
        };
        
        int fixedUpdateCount = 0;
        loop.OnFixedUpdate += _ => fixedUpdateCount++;
        
        // First tick establishes baseline
        loop.Tick();
        
        // Simulate a huge lag spike (1 second at 60 FPS would be 60 updates)
        timeProvider.Advance(TimeSpan.FromSeconds(1.0));
        loop.Tick();
        
        // Should be capped at MaxFixedStepsPerFrame
        Assert.Equal(5, fixedUpdateCount);
    }
    
    [Fact]
    public void Pause_StopsGameTimeButNotRealTime()
    {
        var timeProvider = new MockTimeProvider(DateTime.UtcNow);
        var loop = new EngineLoop(timeProvider)
        {
            TargetFixedStep = TimeSpan.FromSeconds(1.0 / 60.0)
        };
        
        GameTime? lastFixedTime = null;
        GameTime? lastRenderTime = null;
        loop.OnFixedUpdate += time => lastFixedTime = time;
        loop.OnRender += time => lastRenderTime = time;
        
        // First tick establishes baseline
        loop.Tick();
        
        // Second tick triggers update
        timeProvider.Advance(TimeSpan.FromSeconds(1.0 / 60.0));
        loop.Tick();
        
        Assert.NotNull(lastFixedTime);
        var gameTimeBeforePause = lastFixedTime.Value.TotalGameTime;
        var realTimeBeforePause = lastRenderTime!.Value.TotalRealTime;
        
        // Pause and tick - while paused, no fixed updates run but render still does
        loop.IsPaused = true;
        timeProvider.Advance(TimeSpan.FromSeconds(0.5));
        loop.Tick();
        
        // Game time should not advance (no fixed updates when paused)
        Assert.Equal(gameTimeBeforePause, lastFixedTime.Value.TotalGameTime);
        // Real time should advance (checked via render)
        Assert.True(lastRenderTime.Value.TotalRealTime > realTimeBeforePause);
    }
}

