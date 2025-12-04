namespace Ignis.Core.Tests.Timing;

using Ignis.Core.Timing;

public class GameTimeTests
{
    [Fact]
    public void Default_HasZeroValues()
    {
        var time = new GameTime();
        
        Assert.Equal(TimeSpan.Zero, time.TotalRealTime);
        Assert.Equal(TimeSpan.Zero, time.TotalGameTime);
        Assert.Equal(0f, time.DeltaTime);
        Assert.Equal(0f, time.Alpha);
    }
    
    [Fact]
    public void Constructor_SetsAllValues()
    {
        var realTime = TimeSpan.FromSeconds(10);
        var gameTime = TimeSpan.FromSeconds(8);
        var delta = 0.016f;
        var alpha = 0.5f;
        
        var time = new GameTime(realTime, gameTime, delta, alpha);
        
        Assert.Equal(realTime, time.TotalRealTime);
        Assert.Equal(gameTime, time.TotalGameTime);
        Assert.Equal(delta, time.DeltaTime);
        Assert.Equal(alpha, time.Alpha);
    }
    
    [Fact]
    public void Alpha_ClampedBetweenZeroAndOne()
    {
        var time1 = new GameTime(TimeSpan.Zero, TimeSpan.Zero, 0f, -0.5f);
        var time2 = new GameTime(TimeSpan.Zero, TimeSpan.Zero, 0f, 1.5f);
        
        Assert.Equal(0f, time1.Alpha);
        Assert.Equal(1f, time2.Alpha);
    }
}

