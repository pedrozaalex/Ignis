namespace Ignis.Core.Events;

/// <summary>
/// Decoupled pub/sub communication between systems.
/// Prefer struct-based messages to avoid GC pressure.
/// </summary>
public sealed class EventBus
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();
    
    /// <summary>
    /// Subscribe to events of type T.
    /// </summary>
    public void Subscribe<T>(Action<T> callback)
    {
        var type = typeof(T);
        if (!_subscribers.TryGetValue(type, out var list))
        {
            list = new List<Delegate>();
            _subscribers[type] = list;
        }
        list.Add(callback);
    }
    
    /// <summary>
    /// Unsubscribe from events of type T.
    /// </summary>
    public void Unsubscribe<T>(Action<T> callback)
    {
        var type = typeof(T);
        if (_subscribers.TryGetValue(type, out var list))
        {
            list.Remove(callback);
        }
    }
    
    /// <summary>
    /// Publish an event to all subscribers.
    /// </summary>
    public void Publish<T>(T message)
    {
        var type = typeof(T);
        if (_subscribers.TryGetValue(type, out var list))
        {
            // Iterate over a copy to allow modifications during publish
            foreach (var subscriber in list.ToArray())
            {
                ((Action<T>)subscriber)(message);
            }
        }
    }
    
    /// <summary>
    /// Remove all subscribers.
    /// </summary>
    public void Clear()
    {
        _subscribers.Clear();
    }
}

