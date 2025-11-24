using Ignis.Engine.Reactive;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Engine.UI.Core;

/// <summary>
///     Control flow helpers for reactive UI (If, For).
/// </summary>
public static class Bind
{
    /// <summary>
    ///     Conditionally renders one of two views based on a signal.
    /// </summary>
    public static IView If(Signal<bool> condition, Func<IView> trueBuilder, Func<IView>? falseBuilder = null)
    {
        return new ConditionalView(condition, trueBuilder, falseBuilder);
    }

    /// <summary>
    ///     Conditionally renders one of two views based on a computed value.
    /// </summary>
    public static IView If(Computed<bool> condition, Func<IView> trueBuilder, Func<IView>? falseBuilder = null)
    {
        return new ConditionalViewComputed(condition, trueBuilder, falseBuilder);
    }

    /// <summary>
    ///     Renders a list of views from a SignalList, efficiently updating only changed items.
    /// </summary>
    public static IView For<T>(SignalList<T> list, Func<T, IView> builder)
    {
        return new ListView<T>(list, builder);
    }

    private class ConditionalView : ViewComponent, IViewContainer
    {
        private readonly Signal<bool> _condition;
        private readonly Func<IView>? _falseBuilder;
        private readonly Func<IView> _trueBuilder;
        private IView? _currentChild;

        public ConditionalView(Signal<bool> condition, Func<IView> trueBuilder, Func<IView>? falseBuilder)
        {
            _condition = condition;
            _trueBuilder = trueBuilder;
            _falseBuilder = falseBuilder;
        }

        public IEnumerable<IView> GetChildren()
        {
            if (_currentChild != null)
                yield return _currentChild;
        }

        protected override void OnMount()
        {
            CreateEffect(() =>
            {
                // Unmount old child
                if (_currentChild != null) _currentChild.Unmount();

                // Build new child based on condition
                _currentChild = _condition.Value ? _trueBuilder() : _falseBuilder?.Invoke();

                // Mount new child
                if (_currentChild != null && Context != null) _currentChild.Mount(Context);
            });
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // The child will be drawn by UIContext
        }
    }

    private class ConditionalViewComputed : ViewComponent, IViewContainer
    {
        private readonly Computed<bool> _condition;
        private readonly Func<IView>? _falseBuilder;
        private readonly Func<IView> _trueBuilder;
        private IView? _currentChild;

        public ConditionalViewComputed(Computed<bool> condition, Func<IView> trueBuilder, Func<IView>? falseBuilder)
        {
            _condition = condition;
            _trueBuilder = trueBuilder;
            _falseBuilder = falseBuilder;
        }

        public IEnumerable<IView> GetChildren()
        {
            if (_currentChild != null)
                yield return _currentChild;
        }

        protected override void OnMount()
        {
            CreateEffect(() =>
            {
                // Unmount old child
                if (_currentChild != null) _currentChild.Unmount();

                // Build new child based on condition
                _currentChild = _condition.Value ? _trueBuilder() : _falseBuilder?.Invoke();

                // Mount new child
                if (_currentChild != null && Context != null) _currentChild.Mount(Context);
            });
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // The child will be drawn by UIContext
        }
    }

    private class ListView<T> : ViewComponent, IViewContainer
    {
        private readonly Func<T, IView> _builder;
        private readonly List<IView> _children = [];
        private readonly SignalList<T> _list;

        public ListView(SignalList<T> list, Func<T, IView> builder)
        {
            _list = list;
            _builder = builder;
            Layout.LayoutType = LayoutType.Column; // Stack children vertically by default
        }

        public IEnumerable<IView> GetChildren()
        {
            return _children;
        }

        protected override void OnMount()
        {
            // Initial build
            foreach (var item in _list.Items)
            {
                var view = _builder(item);
                view.Mount(Context!);
                _children.Add(view);
            }

            // Subscribe to changes
            _list.ItemAdded += OnItemAdded;
            _list.ItemRemoved += OnItemRemoved;
            _list.ItemMoved += OnItemMoved;
        }

        protected override void OnUnmount()
        {
            _list.ItemAdded -= OnItemAdded;
            _list.ItemRemoved -= OnItemRemoved;
            _list.ItemMoved -= OnItemMoved;

            foreach (var child in _children) child.Unmount();
            _children.Clear();
        }

        private void OnItemAdded(T item, int index)
        {
            var view = _builder(item);
            if (Context != null) view.Mount(Context);
            _children.Insert(index, view);
        }

        private void OnItemRemoved(T item, int index)
        {
            var child = _children[index];
            child.Unmount();
            _children.RemoveAt(index);
        }

        private void OnItemMoved(T item, int oldIndex, int newIndex)
        {
            var child = _children[oldIndex];
            _children.RemoveAt(oldIndex);
            _children.Insert(newIndex, child);
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Children will be drawn by UIContext
        }
    }
}