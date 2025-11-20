namespace Ignis.Engine.Reactive
{
    /// <summary>
    /// Effect - Bridges signals to side effects.
    /// Automatically re-runs when any accessed Signal changes.
    /// </summary>
    public class Effect : IObserver, IDisposable
    {
        private readonly Action _effect;
        private readonly List<Signal<object>> _dependencies = [];
        private bool _isDisposed;

        public Effect(Action effect)
        {
            _effect = effect;
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

        public void OnDependencyChanged()
        {
            if (_isDisposed)
                return;

            Run();
        }

        public void Dispose()
        {
            _isDisposed = true;
            _dependencies.Clear();
        }

        // Static factory for convenience
        public static Effect Create(Action effect) => new(effect);
    }
}

