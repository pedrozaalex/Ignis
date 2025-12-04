namespace Ignis.Core.Timing;

/// <summary>
/// Abstracts time access for testability.
/// </summary>
public interface ITimeProvider
{
    DateTime UtcNow { get; }
}

/// <summary>
/// Uses the real system clock.
/// </summary>
public sealed class SystemTimeProvider : ITimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}

/// <summary>
/// Allows controlling time in tests.
/// </summary>
public sealed class MockTimeProvider : ITimeProvider
{
    private DateTime _currentTime;
    
    public MockTimeProvider(DateTime startTime)
    {
        _currentTime = startTime;
    }
    
    public DateTime UtcNow => _currentTime;
    
    public void Advance(TimeSpan duration)
    {
        _currentTime = _currentTime.Add(duration);
    }
    
    public void Set(DateTime time)
    {
        _currentTime = time;
    }
}

