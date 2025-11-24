namespace Ignis.Engine.Reactive;

/// <summary>
///     Effect - Bridges signals to side effects.
///     Automatically re-runs when any accessed Signal changes.
/// </summary>
public class Effect : IObserver, IDisposable
{
    private readonly List<Signal<object>> _dependencies = [];
    private readonly Action _effect;
    private bool _isDisposed;

    public Effect(Action effect)
    {
        _effect = effect;
        Run();
    }

    public void Dispose()
    {
        _isDisposed = true;
        _dependencies.Clear();
    }

    public void OnDependencyChanged()
    {
        if (_isDisposed)
            return;

        Run();
    }

    private void Run()
    {
        if (_isDisposed)
            return;

        using (ReactiveContext.Track(this))
        {
            _effect();
        }
    }

    // Static factory for convenience
    public static Effect Create(Action effect)
    {
        return new Effect(effect);
    }
}