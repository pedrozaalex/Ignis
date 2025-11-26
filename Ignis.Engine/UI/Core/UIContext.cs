using FontStashSharp;
using Ignis.Engine.Core;
using Ignis.Engine.Input;
using Ignis.Engine.UI.Graphics;
using Ignis.Engine.UI.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Engine.UI.Core;

/// <summary>
///     UIContext - The Root Renderer and coordinator for the UI system.
///     Manages the view tree, layout calculation, and rendering.
/// </summary>
public class UIContext : ILayoutNode, ILayoutCache, IDisposable
{
    private static int _nextInstanceId;
    private readonly Dictionary<object, Rectangle> _bounds = new();
    private readonly Dictionary<long, Rectangle> _boundsById = new();
    private readonly GraphicsDevice? _graphicsDevice;
    private readonly int _instanceId;
    private bool _isDisposed;
    private IView? _root;

    public UIContext(GraphicsDevice? graphicsDevice, IInputProvider inputProvider, SpriteFontBase? defaultFont = null,
        Theme? theme = null)
    {
        _instanceId = Interlocked.Increment(ref _nextInstanceId);
        Console.WriteLine($"[UIContext #{_instanceId}] Created");

        _graphicsDevice = graphicsDevice;
        DefaultFont = defaultFont;
        if (theme != null) Theme = theme;
        if (graphicsDevice != null) PrimitiveBatch = new PrimitiveBatch(graphicsDevice);

        // InputManager shares the same bounds dictionary indexed by element ID
        Input = new InputManager(_boundsById, inputProvider);
    }

    public PrimitiveBatch? PrimitiveBatch { get; }
    public SpriteFontBase? DefaultFont { get; private set; }

    public InputManager Input { get; }

    public IgnisGame? Game { get; private set; }
    public Theme Theme { get; set; } = Theme.Dark;

    public void Dispose()
    {
        if (!_isDisposed)
        {
            PrimitiveBatch?.Dispose();
            _root?.Unmount();
            _isDisposed = true;
        }
    }

    // ILayoutCache implementation
    public void SetBounds(object node, float posX, float posY, float width, float height)
    {
        // During Layout phase, these are RELATIVE coordinates (x,y relative to parent)
        // We store them temporarily in _bounds.
        // The subsequent ResolveAbsolutePositions pass will convert them to absolute.
        var rect = new Rectangle((int)posX, (int)posY, (int)width, (int)height);
        _bounds[node] = rect;
        
        // We don't need to update _boundsById here yet if we only rely on Input after ResolveAbsolutePositions.
        // However, updating it does no harm as long as we fix it before Input reads it.
        if (node is IView view) _boundsById[view.Layout.ElementId] = rect;
    }

    float ILayoutCache.GetWidth(object node)
    {
        return _bounds.TryGetValue(node, out var rect) ? rect.Width : 0;
    }

    float ILayoutCache.GetHeight(object node)
    {
        return _bounds.TryGetValue(node, out var rect) ? rect.Height : 0;
    }

    public float GetPosX(object node)
    {
        return _bounds.TryGetValue(node, out var rect) ? rect.X : 0;
    }

    public float GetPosY(object node)
    {
        return _bounds.TryGetValue(node, out var rect) ? rect.Y : 0;
    }

    // ILayoutNode implementation
    public IEnumerable<object> GetChildren(object node)
    {
        if (node is IViewContainer container)
            return container.GetChildren();
        return [];
    }

    public bool IsVisible(object node)
    {
        return (node as IView)?.Layout.Visible ?? true;
    }

    public LayoutType GetLayoutType(object node)
    {
        return (node as IView)?.Layout.LayoutType ?? LayoutType.Column;
    }

    public PositionType GetPositionType(object node)
    {
        return (node as IView)?.Layout.PositionType ?? PositionType.Relative;
    }

    public Alignment GetAlignment(object node)
    {
        return (node as IView)?.Layout.Alignment ?? Alignment.TopLeft;
    }

    Units ILayoutNode.GetWidth(object node)
    {
        return (node as IView)?.Layout.Width ?? Units.Auto;
    }

    Units ILayoutNode.GetHeight(object node)
    {
        return (node as IView)?.Layout.Height ?? Units.Auto;
    }

    public Units GetMinWidth(object node)
    {
        return (node as IView)?.Layout.MinWidth ?? Units.Auto;
    }

    public Units GetMinHeight(object node)
    {
        return (node as IView)?.Layout.MinHeight ?? Units.Auto;
    }

    public Units GetMaxWidth(object node)
    {
        return (node as IView)?.Layout.MaxWidth ?? Units.Auto;
    }

    public Units GetMaxHeight(object node)
    {
        return (node as IView)?.Layout.MaxHeight ?? Units.Auto;
    }

    public Units GetLeft(object node)
    {
        return (node as IView)?.Layout.Left ?? Units.Auto;
    }

    public Units GetRight(object node)
    {
        return (node as IView)?.Layout.Right ?? Units.Auto;
    }

    public Units GetTop(object node)
    {
        return (node as IView)?.Layout.Top ?? Units.Auto;
    }

    public Units GetBottom(object node)
    {
        return (node as IView)?.Layout.Bottom ?? Units.Auto;
    }

    public Units GetPaddingLeft(object node)
    {
        return (node as IView)?.Layout.PaddingLeft ?? Units.Pixels(0);
    }

    public Units GetPaddingRight(object node)
    {
        return (node as IView)?.Layout.PaddingRight ?? Units.Pixels(0);
    }

    public Units GetPaddingTop(object node)
    {
        return (node as IView)?.Layout.PaddingTop ?? Units.Pixels(0);
    }

    public Units GetPaddingBottom(object node)
    {
        return (node as IView)?.Layout.PaddingBottom ?? Units.Pixels(0);
    }

    public Units GetBorderLeft(object node)
    {
        return Units.Pixels(0);
    }

    public Units GetBorderRight(object node)
    {
        return Units.Pixels(0);
    }

    public Units GetBorderTop(object node)
    {
        return Units.Pixels(0);
    }

    public Units GetBorderBottom(object node)
    {
        return Units.Pixels(0);
    }

    public Units GetChildLeft(object node)
    {
        return Units.Auto;
    }

    public Units GetChildTop(object node)
    {
        return Units.Auto;
    }

    public Units GetRowGap(object node)
    {
        return ((IView)node).Layout.RowGap;
    }

    public Units GetColumnGap(object node)
    {
        return ((IView)node).Layout.ColumnGap;
    }

    public List<Units> GetGridRows(object node)
    {
        return [];
    }

    public List<Units> GetGridColumns(object node)
    {
        return [];
    }

    public int GetRowStart(object node)
    {
        return 0;
    }

    public int GetRowSpan(object node)
    {
        return 1;
    }

    public int GetColumnStart(object node)
    {
        return 0;
    }

    public int GetColumnSpan(object node)
    {
        return 1;
    }

    public (float width, float height)? MeasureContent(object node, float? knownWidth, float? knownHeight)
    {
        return (node as IView)?.Measure(knownWidth, knownHeight);
    }

    /// <summary>
    ///     Sets the game instance reference for accessing FontSystem and other game-level resources.
    /// </summary>
    public void SetGame(IgnisGame game)
    {
        Game = game;
    }

    /// <summary>
    ///     Sets the default font for the UI context.
    /// </summary>
    public void SetDefaultFont(SpriteFontBase font)
    {
        DefaultFont = font;
        Console.WriteLine(
            $"[UIContext #{_instanceId}] SetDefaultFont called. Font is now: {(DefaultFont != null ? "SET" : "NULL")}");
    }

    /// <summary>
    ///     Sets the root view for the UI tree.
    /// </summary>
    public void SetRoot(IView view)
    {
        if (_root != null) _root.Unmount();

        _root = view;
        _root.Mount(this);
        Input.SetRoot(_root);
    }

    /// <summary>
    ///     Updates the UI (handles input, polls reactive queries, etc.)
    /// </summary>
    public void Update(GameTime gameTime)
    {
        if (_root == null || _graphicsDevice == null)
            return;

        // 1. Input: Process input based on LAST frame's layout.
        //    Events here may modify the structure (add/remove elements).
        Input.Update();

        // 2. Clear old bounds to avoid stale data leaking.
        _bounds.Clear();
        _boundsById.Clear();

        // 3. Layout: Calculate RELATIVE layout for the new structure.
        var viewport = _graphicsDevice.Viewport;
        LayoutEngine.Layout(_root, this, this, viewport.Width, viewport.Height);

        // 4. Resolve Absolute Positions: Convert relative layout to absolute screen coordinates
        //    so Draw() has the correct final positions.
        if (_bounds.TryGetValue(_root, out var rootRect))
        {
            // Root position is already absolute (0,0 or whatever LayoutEngine decided)
            // We pass 0,0 as the parent offset for the root.
            ResolveAbsolutePositions(_root, Vector2.Zero);
        }
    }

    private void ResolveAbsolutePositions(object node, Vector2 parentPos)
    {
        if (!_bounds.TryGetValue(node, out var relRect)) return;

        // Calculate Absolute Position
        var absX = parentPos.X + relRect.X;
        var absY = parentPos.Y + relRect.Y;
        var absRect = new Rectangle((int)absX, (int)absY, relRect.Width, relRect.Height);

        // Update bounds with Absolute Position
        _bounds[node] = absRect;
        if (node is IView view) _boundsById[view.Layout.ElementId] = absRect;

        // Recurse for children
        foreach (var child in GetChildren(node))
        {
            ResolveAbsolutePositions(child, new Vector2(absX, absY));
        }
    }

    /// <summary>
    ///     Performs layout calculation and draws the UI.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch)
    {
        if (_root == null || _graphicsDevice == null)
            return;

        // Debug: Check font state at draw time
        if (DefaultFont == null)
            Console.WriteLine($"[UIContext #{_instanceId}.Draw] WARNING: DefaultFont is NULL at draw time!");

        // Layout was already calculated in Update(), just draw now
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
            foreach (var child in container.GetChildren())
                DrawView(spriteBatch, child);
    }

    public Rectangle GetBounds(object node)
    {
        return _bounds.TryGetValue(node, out var rect) ? rect : Rectangle.Empty;
    }
}

/// <summary>
///     Interface for views that contain children.
/// </summary>
public interface IViewContainer
{
    IEnumerable<IView> GetChildren();
}