namespace Ignis.Engine.Reactive;

/// <summary>
///     Computed&lt;T&gt; - Derived State.
///     Pure derived state that is memoized and lazy. Updates only when dependencies change.
/// </summary>
public class Computed<T> : IObserver
{
    private readonly Func<T> _computer;
    private readonly List<IObserver> _observers = [];
    private T _cache;
    private bool _isDirty = true;

    public Computed(Func<T> computer)
    {
        _computer = computer;
        _cache = default!;
    }

    public T Value
    {
        get
        {
            if (_isDirty) Recompute();

            // Track this computed as a dependency
            var observer = ReactiveContext.CurrentObserver;
            if (observer != null && !_observers.Contains(observer)) _observers.Add(observer);

            return _cache;
        }
    }

    public void OnDependencyChanged()
    {
        if (_isDirty)
            return; // Already dirty

        _isDirty = true;
        NotifyObservers();
    }

    private void Recompute()
    {
        using (ReactiveContext.Track(this))
        {
            _cache = _computer();
        }

        _isDirty = false;
    }

    private void NotifyObservers()
    {
        var observers = _observers.ToList();
        foreach (var observer in observers) observer.OnDependencyChanged();
    }

    // Static factory for convenience
    public static Computed<T> From(Func<T> computer)
    {
        return new Computed<T>(computer);
    }

    // Implicit conversion
    public static implicit operator T(Computed<T> computed)
    {
        return computed.Value;
    }

    public override string ToString()
    {
        return _cache?.ToString() ?? "null";
    }
}