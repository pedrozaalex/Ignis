using Ignis.Engine.Reactive;
using Ignis.Engine.UI.Core;
using Ignis.Engine.UI.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReactiveEffect = Ignis.Engine.Reactive.Effect;

namespace Ignis.Engine.UI.Abstractions
{
    /// <summary>
    /// Base class for view components with reactive capabilities.
    /// </summary>
    public abstract class ViewComponent : IView
    {
        private UIContext? _context;
        private readonly List<ReactiveEffect> _effects = [];

        public IViewLayout Layout { get; } = new ViewLayout();
        public EventHandlers EventHandlers { get; } = new();
        public ShortcutCollection Shortcuts { get; } = new();

        protected UIContext? Context => _context;

        public abstract void Draw(SpriteBatch spriteBatch, Rectangle bounds);

        public virtual void Mount(UIContext context)
        {
            _context = context;
            OnMount();
        }

        public virtual void Unmount()
        {
            OnUnmount();
            
            // Dispose all effects
            foreach (var effect in _effects)
            {
                effect.Dispose();
            }
            _effects.Clear();
            
            _context = null;
        }

        public virtual (float width, float height)? Measure(float? availableWidth, float? availableHeight)
        {
            return null; // No intrinsic size by default
        }

        /// <summary>
        /// Override to set up reactive effects when mounted.
        /// </summary>
        protected virtual void OnMount() { }

        /// <summary>
        /// Override to clean up when unmounted.
        /// </summary>
        protected virtual void OnUnmount() { }

        /// <summary>
        /// Helper to create an effect that will be automatically cleaned up on unmount.
        /// </summary>
        protected ReactiveEffect CreateEffect(Action action)
        {
            var effect = new ReactiveEffect(action);
            _effects.Add(effect);
            return effect;
        }
    }
}

