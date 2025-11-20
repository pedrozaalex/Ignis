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
        private readonly List<IObserver> _observers = [];

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

        /// <summary>
        /// Creates a lens (bidirectional binding) to a field of this signal's value.
        /// Useful for editing struct fields without boilerplate.
        /// </summary>
        /// <example>
        /// var posSignal = new Signal(new Vector3(1, 2, 3));
        /// var xSignal = posSignal.Lens(v => v.X, (v, x) => v with { X = x });
        /// xSignal.Value = 10; // Updates the X component of the vector
        /// </example>
        public Signal<TField> Lens<TField>(Func<T, TField> getter, Func<T, TField, T> setter)
        {
            return new LensSignal<T, TField>(this, getter, setter);
        }

        public override string ToString() => _value?.ToString() ?? "null";
    }

    /// <summary>
    /// A signal that acts as a bidirectional binding to a field of a parent signal.
    /// </summary>
    internal class LensSignal<TParent, TField> : Signal<TField>
    {
        private readonly Signal<TParent> _parent;
        private readonly Func<TParent, TField> _getter;
        private readonly Func<TParent, TField, TParent> _setter;

        public LensSignal(Signal<TParent> parent, Func<TParent, TField> getter, Func<TParent, TField, TParent> setter)
            : base(default!)
        {
            _parent = parent;
            _getter = getter;
            _setter = setter;
        }

        public new TField Value
        {
            get => _getter(_parent.Value);
            set => _parent.Value = _setter(_parent.Value, value);
        }
    }
}

