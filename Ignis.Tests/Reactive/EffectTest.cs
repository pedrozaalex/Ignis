using Ignis.Engine.Reactive;
using Xunit;

namespace Ignis.Tests.Reactive;

/// <summary>
/// Tests for Effect - Side effects with automatic dependency tracking
/// </summary>
public class EffectTests
{
    [Fact]
    public void Effect_ExecutesImmediatelyOnCreation()
    {
        // Arrange
        var executionCount = 0;

        // Act
        var effect = new Effect(() => executionCount++);

        // Assert
        Assert.Equal(1, executionCount);
    }

    [Fact]
    public void Effect_ReExecutes_WhenDependencyChanges()
    {
        // Arrange
        var signal = new Signal<int>(10);
        var loggedValue = 0;

        var effect = new Effect(() =>
        {
            loggedValue = signal.Value;
        });

        Assert.Equal(10, loggedValue);

        // Act
        signal.Value = 20;

        // Assert
        Assert.Equal(20, loggedValue);
    }

    [Fact]
    public void Effect_Disposal_StopsExecution()
    {
        // Arrange
        var signal = new Signal<int>(10);
        var executionCount = 0;

        var effect = new Effect(() =>
        {
            _ = signal.Value;
            executionCount++;
        });

        Assert.Equal(1, executionCount);

        // Act
        effect.Dispose();
        signal.Value = 20;

        // Assert - Should NOT execute after disposal
        Assert.Equal(1, executionCount);
    }
}

