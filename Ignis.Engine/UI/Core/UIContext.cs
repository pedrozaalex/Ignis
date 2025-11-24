using FontStashSharp;
using Ignis.Engine.Core;
using Ignis.Engine.UI.Abstractions;
using Ignis.Engine.UI.Graphics;
using Ignis.Engine.UI.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Engine.UI.Core
{
    /// <summary>
    /// UIContext - The Root Renderer and coordinator for the UI system.
    /// Manages the view tree, layout calculation, and rendering.
    /// </summary>
    public class UIContext : ILayoutNode, ILayoutCache, IDisposable
    {
        private readonly GraphicsDevice? _graphicsDevice;
        private IView? _root;
        private readonly Dictionary<object, Rectangle> _bounds = new();
        private readonly Dictionary<long, Rectangle> _boundsById = new();
        private bool _isDisposed;
        private SpriteFontBase? _defaultFont;
        private readonly int _instanceId;
        private static int _nextInstanceId;
        
        private readonly InputManager _inputManager;

        public PrimitiveBatch? PrimitiveBatch { get; }
        public SpriteFontBase? DefaultFont => _defaultFont;
        public InputManager Input => _inputManager;
        public IgnisGame? Game { get; private set; }
        public Theme Theme { get; set; } = Theme.Dark;

        public UIContext(GraphicsDevice? graphicsDevice, SpriteFontBase? defaultFont = null, Theme? theme = null)
        {
            _instanceId = Interlocked.Increment(ref _nextInstanceId);
            Console.WriteLine($"[UIContext #{_instanceId}] Created");
            
            _graphicsDevice = graphicsDevice;
            _defaultFont = defaultFont;
            if (theme != null) Theme = theme;
            if (graphicsDevice != null)
            {
                PrimitiveBatch = new PrimitiveBatch(graphicsDevice);
            }
            
            // InputManager shares the same bounds dictionary indexed by element ID
            _inputManager = new InputManager(_boundsById);
        }

        /// <summary>
        /// Sets the game instance reference for accessing FontSystem and other game-level resources.
        /// </summary>
        public void SetGame(IgnisGame game)
        {
            Game = game;
        }

        /// <summary>
        /// Sets the default font for the UI context.
        /// </summary>
        public void SetDefaultFont(SpriteFontBase font)
        {
            _defaultFont = font;
            Console.WriteLine($"[UIContext #{_instanceId}] SetDefaultFont called. Font is now: {(_defaultFont != null ? "SET" : "NULL")}");
        }

        /// <summary>
        /// Sets the root view for the UI tree.
        /// </summary>
        public void SetRoot(IView view)
        {
            if (_root != null)
            {
                _root.Unmount();
            }

            _root = view;
            _root.Mount(this);
            _inputManager.SetRoot(_root);
        }

        /// <summary>
        /// Updates the UI (handles input, polls reactive queries, etc.)
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Process input events
            _inputManager.Update();
        }

        /// <summary>
        /// Performs layout calculation and draws the UI.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (_root == null || _graphicsDevice == null)
                return;

            // Debug: Check font state at draw time
            if (_defaultFont == null)
            {
                Console.WriteLine($"[UIContext #{_instanceId}.Draw] WARNING: DefaultFont is NULL at draw time!");
            }

            // Get viewport dimensions for layout constraint
            var viewport = _graphicsDevice.Viewport;

            // Calculate layout with viewport as constraint
            LayoutEngine.Layout(_root, this, this, viewport.Width, viewport.Height);

            // Start both batches - primitives render independently, text uses SpriteBatch
            PrimitiveBatch?.Begin();
            spriteBatch.Begin();

            // Single pass - draw the entire tree
            // Widgets use PrimitiveBatch for shapes, SpriteBatch for text
            DrawView(spriteBatch, _root);

            // End both batches
            // FIX: PrimitiveBatch must end (flush to GPU) BEFORE SpriteBatch.
            // This ensures backgrounds/panels are drawn first, and Text is drawn on top.
            PrimitiveBatch?.End();
            spriteBatch.End();
        }

        private void DrawView(SpriteBatch spriteBatch, IView view)
        {
            if (!view.Layout.Visible)
                return;

            var bounds = GetBounds(view);
            view.Draw(spriteBatch, bounds);

            // Recursively draw children
            if (view is IViewContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    DrawView(spriteBatch, child);
                }
            }
        }

        // ILayoutCache implementation
        public void SetBounds(object node, float posX, float posY, float width, float height)
        {
            var rect = new Rectangle((int)posX, (int)posY, (int)width, (int)height);
            _bounds[node] = rect;
            
            // Also store by element ID for InputManager
            if (node is IView view)
            {
                _boundsById[view.Layout.ElementId] = rect;
            }
        }

        float ILayoutCache.GetWidth(object node) => _bounds.TryGetValue(node, out var rect) ? rect.Width : 0;
        float ILayoutCache.GetHeight(object node) => _bounds.TryGetValue(node, out var rect) ? rect.Height : 0;
        public float GetPosX(object node) => _bounds.TryGetValue(node, out var rect) ? rect.X : 0;
        public float GetPosY(object node) => _bounds.TryGetValue(node, out var rect) ? rect.Y : 0;

        public Rectangle GetBounds(object node) => _bounds.TryGetValue(node, out var rect) ? rect : Rectangle.Empty;

        // ILayoutNode implementation
        public IEnumerable<object> GetChildren(object node)
        {
            if (node is IViewContainer container)
                return container.GetChildren();
            return [];
        }

        public bool IsVisible(object node) => (node as IView)?.Layout.Visible ?? true;
        public LayoutType GetLayoutType(object node) => (node as IView)?.Layout.LayoutType ?? LayoutType.Column;
        public PositionType GetPositionType(object node) => (node as IView)?.Layout.PositionType ?? PositionType.Relative;
        public Alignment GetAlignment(object node) => (node as IView)?.Layout.Alignment ?? Alignment.TopLeft;
        
        Units ILayoutNode.GetWidth(object node) => (node as IView)?.Layout.Width ?? Units.Auto;
        Units ILayoutNode.GetHeight(object node) => (node as IView)?.Layout.Height ?? Units.Auto;
        public Units GetMinWidth(object node) => (node as IView)?.Layout.MinWidth ?? Units.Auto;
        public Units GetMinHeight(object node) => (node as IView)?.Layout.MinHeight ?? Units.Auto;
        public Units GetMaxWidth(object node) => (node as IView)?.Layout.MaxWidth ?? Units.Auto;
        public Units GetMaxHeight(object node) => (node as IView)?.Layout.MaxHeight ?? Units.Auto;
        
        public Units GetLeft(object node) => (node as IView)?.Layout.Left ?? Units.Auto;
        public Units GetRight(object node) => (node as IView)?.Layout.Right ?? Units.Auto;
        public Units GetTop(object node) => (node as IView)?.Layout.Top ?? Units.Auto;
        public Units GetBottom(object node) => (node as IView)?.Layout.Bottom ?? Units.Auto;
        
        public Units GetPaddingLeft(object node) => (node as IView)?.Layout.PaddingLeft ?? Units.Pixels(0);
        public Units GetPaddingRight(object node) => (node as IView)?.Layout.PaddingRight ?? Units.Pixels(0);
        public Units GetPaddingTop(object node) => (node as IView)?.Layout.PaddingTop ?? Units.Pixels(0);
        public Units GetPaddingBottom(object node) => (node as IView)?.Layout.PaddingBottom ?? Units.Pixels(0);
        
        public Units GetBorderLeft(object node) => Units.Pixels(0);
        public Units GetBorderRight(object node) => Units.Pixels(0);
        public Units GetBorderTop(object node) => Units.Pixels(0);
        public Units GetBorderBottom(object node) => Units.Pixels(0);
        
        public Units GetChildLeft(object node) => Units.Auto;
        public Units GetChildTop(object node) => Units.Auto;
        
        public Units GetRowGap(object node) => ((IView)node).Layout.RowGap;
        public Units GetColumnGap(object node) => ((IView)node).Layout.ColumnGap;

        public List<Units> GetGridRows(object node) => [];
        public List<Units> GetGridColumns(object node) => [];
        public int GetRowStart(object node) => 0;
        public int GetRowSpan(object node) => 1;
        public int GetColumnStart(object node) => 0;
        public int GetColumnSpan(object node) => 1;

        public (float width, float height)? MeasureContent(object node, float? knownWidth, float? knownHeight)
        {
            return (node as IView)?.Measure(knownWidth, knownHeight);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                PrimitiveBatch?.Dispose();
                _root?.Unmount();
                _isDisposed = true;
            }
        }
    }

    /// <summary>
    /// Interface for views that contain children.
    /// </summary>
    public interface IViewContainer
    {
        IEnumerable<IView> GetChildren();
    }
}