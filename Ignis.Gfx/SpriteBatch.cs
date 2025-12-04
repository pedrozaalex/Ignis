using System.Numerics;

namespace Ignis.Gfx;

/// <summary>
/// Batches 2D sprite draw calls into efficient batches.
/// Groups sprites by texture and flushes when texture changes or capacity is reached.
/// </summary>
public class SpriteBatch
{
    private const int MaxSpritesPerBatch = 2048;
    private const int VerticesPerSprite = 4;
    private const int IndicesPerSprite = 6;
    
    private readonly IRenderingServer _server;
    private readonly Vertex2D[] _vertices;
    private readonly uint[] _indices;
    private int _spriteCount;
    private TextureHandle _currentTexture;
    private bool _begun;
    private Matrix4x4 _projection;
    private Matrix4x4 _view;
    
    public SpriteBatch(IRenderingServer server)
    {
        _server = server;
        _vertices = new Vertex2D[MaxSpritesPerBatch * VerticesPerSprite];
        _indices = new uint[MaxSpritesPerBatch * IndicesPerSprite];
        
        // Pre-fill index buffer pattern (0,1,2,0,2,3 repeated)
        for (int i = 0; i < MaxSpritesPerBatch; i++)
        {
            uint baseVertex = (uint)(i * 4);
            int baseIndex = i * 6;
            _indices[baseIndex + 0] = baseVertex + 0;
            _indices[baseIndex + 1] = baseVertex + 1;
            _indices[baseIndex + 2] = baseVertex + 2;
            _indices[baseIndex + 3] = baseVertex + 0;
            _indices[baseIndex + 4] = baseVertex + 2;
            _indices[baseIndex + 5] = baseVertex + 3;
        }
    }
    
    /// <summary>Begin a sprite batch with the given projection (typically orthographic).</summary>
    public void Begin(Matrix4x4 projection, Matrix4x4? view = null)
    {
        if (_begun)
            throw new InvalidOperationException("SpriteBatch.Begin called without matching End");
        
        _begun = true;
        _projection = projection;
        _view = view ?? Matrix4x4.Identity;
        _spriteCount = 0;
        _currentTexture = TextureHandle.Invalid;
    }
    
    /// <summary>Draw a textured sprite.</summary>
    public void Draw(TextureHandle texture, Vector2 position, Vector2 size, Color4 color)
    {
        Draw(texture, position, size, new Rect(0, 0, 1, 1), color, 0f, Vector2.Zero);
    }
    
    /// <summary>Draw a textured sprite with rotation.</summary>
    public void Draw(TextureHandle texture, Vector2 position, Vector2 size, Color4 color, float rotation, Vector2 origin)
    {
        Draw(texture, position, size, new Rect(0, 0, 1, 1), color, rotation, origin);
    }
    
    /// <summary>Draw a sprite region (atlas support) with full control.</summary>
    public void Draw(TextureHandle texture, Vector2 position, Vector2 size, Rect srcRect, Color4 color, float rotation, Vector2 origin)
    {
        if (!_begun)
            throw new InvalidOperationException("SpriteBatch.Draw called without Begin");
        
        // Flush if texture changes or batch is full
        if (_currentTexture != texture && _currentTexture.IsValid)
            Flush();
        
        if (_spriteCount >= MaxSpritesPerBatch)
            Flush();
        
        _currentTexture = texture;
        
        // Calculate rotated corners
        var cos = MathF.Cos(rotation);
        var sin = MathF.Sin(rotation);
        
        // Corners relative to origin
        var topLeft = new Vector2(-origin.X, -origin.Y);
        var topRight = new Vector2(size.X - origin.X, -origin.Y);
        var bottomRight = new Vector2(size.X - origin.X, size.Y - origin.Y);
        var bottomLeft = new Vector2(-origin.X, size.Y - origin.Y);
        
        // Rotate and translate
        Vector2 RotatePoint(Vector2 p) => new(
            p.X * cos - p.Y * sin + position.X,
            p.X * sin + p.Y * cos + position.Y
        );
        
        var v0 = RotatePoint(topLeft);
        var v1 = RotatePoint(topRight);
        var v2 = RotatePoint(bottomRight);
        var v3 = RotatePoint(bottomLeft);
        
        // UV coordinates from source rectangle
        var uvLeft = srcRect.X;
        var uvTop = srcRect.Y;
        var uvRight = srcRect.X + srcRect.Width;
        var uvBottom = srcRect.Y + srcRect.Height;
        
        int baseVertex = _spriteCount * 4;
        _vertices[baseVertex + 0] = new Vertex2D(v0, new Vector2(uvLeft, uvTop), color);
        _vertices[baseVertex + 1] = new Vertex2D(v1, new Vector2(uvRight, uvTop), color);
        _vertices[baseVertex + 2] = new Vertex2D(v2, new Vector2(uvRight, uvBottom), color);
        _vertices[baseVertex + 3] = new Vertex2D(v3, new Vector2(uvLeft, uvBottom), color);
        
        _spriteCount++;
    }
    
    /// <summary>End the batch and flush remaining sprites.</summary>
    public void End()
    {
        if (!_begun)
            throw new InvalidOperationException("SpriteBatch.End called without Begin");
        
        Flush();
        _begun = false;
    }
    
    private void Flush()
    {
        if (_spriteCount == 0)
            return;
        
        // Backend would upload vertices here and draw
        // This is a placeholder - actual implementation depends on backend
        
        _spriteCount = 0;
    }
    
    /// <summary>Create an orthographic projection for 2D rendering.</summary>
    public static Matrix4x4 CreateOrthographic(float width, float height, bool topLeftOrigin = true)
    {
        if (topLeftOrigin)
        {
            // 0,0 at top-left, Y increases downward (typical UI coordinates)
            return Matrix4x4.CreateOrthographicOffCenter(0, width, height, 0, 0, 1);
        }
        else
        {
            // 0,0 at bottom-left, Y increases upward (OpenGL style)
            return Matrix4x4.CreateOrthographicOffCenter(0, width, 0, height, 0, 1);
        }
    }
}

