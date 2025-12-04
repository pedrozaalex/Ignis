namespace Ignis.Core.Tests.Timing;

using Ignis.Core.Timing;

public class TimeProviderTests
{
    [Fact]
    public void SystemTimeProvider_ReturnsCurrentTime()
    {
        var provider = new SystemTimeProvider();
        var before = DateTime.UtcNow;
        var result = provider.UtcNow;
        var after = DateTime.UtcNow;
        
        Assert.True(result >= before && result <= after);
    }
    
    [Fact]
    public void MockTimeProvider_ReturnsFixedTime()
    {
        var fixedTime = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var provider = new MockTimeProvider(fixedTime);
        
        Assert.Equal(fixedTime, provider.UtcNow);
    }
    
    [Fact]
    public void MockTimeProvider_CanAdvanceTime()
    {
        var startTime = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var provider = new MockTimeProvider(startTime);
        
        provider.Advance(TimeSpan.FromSeconds(5));
        
        Assert.Equal(startTime.AddSeconds(5), provider.UtcNow);
    }
}

