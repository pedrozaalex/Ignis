using Ignis.Engine.Reactive;

namespace Ignis.Tests.Reactive;

/// <summary>
///     Tests for Computed&lt;T&gt; - Derived state with lazy evaluation and memoization
/// </summary>
public class ComputedTests
{
    [Fact]
    public void Computed_BasicDerivation_CalculatesCorrectly()
    {
        // Arrange
        var a = new Signal<int>(1);
        var b = new Signal<int>(2);
        var sum = Computed<int>.From(() => a.Value + b.Value);

        // Assert
        Assert.Equal(3, sum.Value);
    }

    [Fact]
    public void Computed_LazyEvaluation_DoesNotComputeUntilRead()
    {
        // Arrange
        var signal = new Signal<int>(1);
        var computeCount = 0;
        var computed = Computed<int>.From(() =>
        {
            computeCount++;
            return signal.Value * 2;
        });

        // Act - Change signal but don't read computed
        signal.Value = 5;

        // Assert - Should not have computed yet
        Assert.Equal(0, computeCount);

        // Act - Now read the computed
        var result = computed.Value;

        // Assert
        Assert.Equal(1, computeCount);
        Assert.Equal(10, result);
    }

    [Fact]
    public void Computed_Memoization_CachesResultUntilDependencyChanges()
    {
        // Arrange
        var signal = new Signal<int>(5);
        var computeCount = 0;
        var computed = Computed<int>.From(() =>
        {
            computeCount++;
            return signal.Value * 2;
        });

        // Act - Read 10 times without changing signal
        for (var i = 0; i < 10; i++) _ = computed.Value;

        // Assert - Should only compute once
        Assert.Equal(1, computeCount);
    }

    [Fact]
    public void Computed_DynamicDependencies_TrackOnlyAccessedSignals()
    {
        // Arrange
        var condition = new Signal<bool>(true);
        var a = new Signal<int>(1);
        var b = new Signal<int>(100);
        var computeCount = 0;

        var computed = Computed<int>.From(() =>
        {
            computeCount++;
            return condition.Value ? a.Value : b.Value;
        });

        // Act & Assert - Initial read
        Assert.Equal(1, computed.Value);
        Assert.Equal(1, computeCount);

        // Act - Change b (not currently tracked)
        b.Value = 200;

        // Assert - Should NOT re-compute
        var result = computed.Value;
        Assert.Equal(1, computeCount);
        Assert.Equal(1, result);

        // Act - Switch condition to use b
        condition.Value = false;

        // Assert - Now it computes and tracks b
        Assert.Equal(200, computed.Value);
        Assert.Equal(2, computeCount);
    }

    [Fact]
    public void Computed_UpdatesDependencies_WhenSourceChanges()
    {
        // Arrange
        var a = new Signal<int>(2);
        var b = new Signal<int>(3);
        var sum = Computed<int>.From(() => a.Value + b.Value);

        Assert.Equal(5, sum.Value);

        // Act & Assert - Change a
        a.Value = 10;
        Assert.Equal(13, sum.Value);

        // Act & Assert - Change b
        b.Value = 7;
        Assert.Equal(17, sum.Value);
    }
}