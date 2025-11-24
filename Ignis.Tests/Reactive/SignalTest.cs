using Ignis.Engine.Reactive;

namespace Ignis.Tests.Reactive;

/// <summary>
///     Tests for Signal&lt;T&gt; - The atomic state container
/// </summary>
public class SignalTests
{
    [Fact]
    public void Signal_Initialization_ReturnsCorrectValue()
    {
        // Arrange & Act
        var signal = new Signal<int>(10);

        // Assert
        Assert.Equal(10, signal.Value);
    }

    [Fact]
    public void Signal_ValueUpdate_NotifiesObservers()
    {
        // Arrange
        var signal = new Signal<int>(10);
        var notificationCount = 0;

        var effect = new Effect(() =>
        {
            _ = signal.Value;
            notificationCount++;
        });

        // Initial run
        Assert.Equal(1, notificationCount);

        // Act
        signal.Value = 20;

        // Assert
        Assert.Equal(20, signal.Value);
        Assert.Equal(2, notificationCount);
    }

    [Fact]
    public void Signal_SameValueWrite_DoesNotTriggerNotifications()
    {
        // Arrange
        var signal = new Signal<int>(10);
        var notificationCount = 0;

        var effect = new Effect(() =>
        {
            _ = signal.Value;
            notificationCount++;
        });

        Assert.Equal(1, notificationCount);

        // Act
        signal.Value = 10; // Same value

        // Assert - Should NOT trigger notification
        Assert.Equal(1, notificationCount);
    }

    [Fact]
    public void Signal_ImplicitConversion_ReturnsValue()
    {
        // Arrange
        Signal<int> signal = new(5);

        // Act
        int value = signal;

        // Assert
        Assert.Equal(5, value);
    }
}