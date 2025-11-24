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
        private Computed<WidgetState>? _currentState;

        public IViewLayout Layout { get; } = new ViewLayout();
        public EventHandlers EventHandlers { get; } = new();
        public ShortcutCollection Shortcuts { get; } = new();

        protected UIContext? Context => _context;
        
        /// <summary>
        /// Reactive computed property that tracks the current widget state (hover, active, focused).
        /// </summary>
        public WidgetState CurrentState => _currentState?.Value ?? WidgetState.Normal;

        public abstract void Draw(SpriteBatch spriteBatch, Rectangle bounds);

        public virtual void Mount(UIContext context)
        {
            _context = context;
            
            // Initialize CurrentState computed property
            _currentState = Computed<WidgetState>.From(() =>
            {
                if (_context == null) return WidgetState.Normal;
                
                var state = WidgetState.Normal;
                var elementId = Layout.ElementId;
                
                if (_context.Input.HoveredElementId.Value == elementId)
                    state |= WidgetState.Hovered;
                    
                if (_context.Input.ActiveElementId.Value == elementId)
                    state |= WidgetState.Active;
                    
                if (_context.Input.FocusedElementId.Value == elementId)
                    state |= WidgetState.Focused;
                
                return state;
            });
            
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

