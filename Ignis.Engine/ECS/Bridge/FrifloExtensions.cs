using Friflo.Engine.ECS;
using Ignis.Engine.Reactive;

namespace Ignis.Engine.ECS.Bridge
{
    /// <summary>
    /// Extension methods to create reactive bridges between Friflo ECS and Signals.
    /// </summary>
    public static class FrifloExtensions
    {
        /// <summary>
        /// Creates a Signal that wraps an ECS component.
        /// Reading gets the component from the entity, writing updates it in the ECS.
        /// </summary>
        public static ComponentSignal<T> ComponentSignal<T>(this Entity entity) where T : struct, IComponent
        {
            return new ComponentSignal<T>(entity);
        }
    }

    /// <summary>
    /// A specialized Signal that reads/writes directly from/to an ECS Entity's component.
    /// Note: This uses a polling strategy - call Poll() to sync with ECS changes.
    /// </summary>
    public class ComponentSignal<T> : IObserver where T : struct, IComponent
    {
        private readonly Entity _entity;
        private readonly List<IObserver> _observers = [];
        private T _cachedValue;

        public ComponentSignal(Entity entity)
        {
            _entity = entity;
            if (!_entity.IsNull && _entity.HasComponent<T>())
            {
                _cachedValue = entity.GetComponent<T>();
            }
            else
            {
                _cachedValue = default!;
            }
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

                // Check if entity is still valid
                if (_entity.IsNull)
                {
                    return default!;
                }

                // Return cached value (call Poll() to sync with ECS)
                return _cachedValue;
            }
            set
            {
                // Check if entity is still valid
                if (_entity.IsNull)
                {
                    return;
                }

                if (EqualityComparer<T>.Default.Equals(_cachedValue, value))
                    return;

                _cachedValue = value;

                // Write to ECS using Set
                _entity.Set(value);

                NotifyObservers();
            }
        }

        private void NotifyObservers()
        {
            var observers = _observers.ToList();
            foreach (var observer in observers)
            {
                observer.OnDependencyChanged();
            }
        }

        public void OnDependencyChanged()
        {
            // External notification (e.g., from polling or ECS events)
            NotifyObservers();
        }

        public void Poll()
        {
            // Check if entity is still valid
            if (_entity.IsNull)
            {
                return;
            }

            // Check if component changed in ECS
            if (_entity.HasComponent<T>())
            {
                var current = _entity.GetComponent<T>();
                if (!EqualityComparer<T>.Default.Equals(_cachedValue, current))
                {
                    _cachedValue = current;
                    NotifyObservers();
                }
            }
        }

        // Implicit conversion
        public static implicit operator T(ComponentSignal<T> signal) => signal.Value;

        public override string ToString() => _cachedValue.ToString() ?? "null";
    }
}

