namespace Ignis.Engine.Reactive
{
    /// <summary>
    /// SignalList&lt;T&gt; - Observable collection for UI.
    /// Fires fine-grained events (ItemAdded, ItemRemoved, ItemMoved) rather than resetting the whole list.
    /// </summary>
    public class SignalList<T>
    {
        private readonly List<T> _items = new();

        public event Action<T, int>? ItemAdded;
        public event Action<T, int>? ItemRemoved;
        public event Action<T, int, int>? ItemMoved;
        public event Action? Changed;

        public IReadOnlyList<T> Items => _items.AsReadOnly();

        public int Count => _items.Count;

        public T this[int index] => _items[index];

        public void Add(T item)
        {
            _items.Add(item);
            var index = _items.Count - 1;
            ItemAdded?.Invoke(item, index);
            Changed?.Invoke();
        }

        public void Insert(int index, T item)
        {
            _items.Insert(index, item);
            ItemAdded?.Invoke(item, index);
            Changed?.Invoke();
        }

        public bool Remove(T item)
        {
            var index = _items.IndexOf(item);
            if (index >= 0)
            {
                _items.RemoveAt(index);
                ItemRemoved?.Invoke(item, index);
                Changed?.Invoke();
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            var item = _items[index];
            _items.RemoveAt(index);
            ItemRemoved?.Invoke(item, index);
            Changed?.Invoke();
        }

        public void Move(int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex)
                return;

            var item = _items[oldIndex];
            _items.RemoveAt(oldIndex);
            _items.Insert(newIndex, item);
            ItemMoved?.Invoke(item, oldIndex, newIndex);
            Changed?.Invoke();
        }

        public void Clear()
        {
            while (_items.Count > 0)
            {
                RemoveAt(_items.Count - 1);
            }
        }

        public bool Contains(T item) => _items.Contains(item);

        public int IndexOf(T item) => _items.IndexOf(item);
    }
}

