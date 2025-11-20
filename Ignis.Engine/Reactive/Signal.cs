namespace Ignis.Engine.Reactive
{
    /// <summary>
    /// The Observer interface for tracking dependencies.
    /// </summary>
    public interface IObserver
    {
        void OnDependencyChanged();
    }

    /// <summary>
    /// Context for tracking the current observer during reactive computations.
    /// </summary>
    public static class ReactiveContext
    {
        [ThreadStatic]
        private static IObserver? _currentObserver;

        public static IObserver? CurrentObserver => _currentObserver;

        public static IDisposable Track(IObserver observer)
        {
            var previous = _currentObserver;
            _currentObserver = observer;
            return new ObserverScope(previous);
        }

        private class ObserverScope : IDisposable
        {
            private readonly IObserver? _previous;

            public ObserverScope(IObserver? previous)
            {
                _previous = previous;
            }

            public void Dispose()
            {
                _currentObserver = _previous;
            }
        }
    }

    /// <summary>
    /// Signal&lt;T&gt; - The Atom of State.
    /// A state container that tracks dependencies on read and notifies observers on write.
    /// </summary>
    public class Signal<T>
    {
        private T _value;
        private readonly List<IObserver> _observers = new();

        public Signal(T initialValue)
        {
            _value = initialValue;
        }

        public T Value
        {
            get
            {
                // Track dependency
                var observer = ReactiveContext.CurrentObserver;
                if (observer != null && !_observers.Contains(observer))
                {
                    _observers.Add(observer);
                }
                return _value;
            }
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value))
                    return;

                _value = value;
                NotifyObservers();
            }
        }

        private void NotifyObservers()
        {
            // Copy to avoid modification during iteration
            var observers = _observers.ToList();
            foreach (var observer in observers)
            {
                observer.OnDependencyChanged();
            }
        }

        public void Unsubscribe(IObserver observer)
        {
            _observers.Remove(observer);
        }

        // Implicit conversion for convenience
        public static implicit operator T(Signal<T> signal) => signal.Value;

        public override string ToString() => _value?.ToString() ?? "null";
    }
}

