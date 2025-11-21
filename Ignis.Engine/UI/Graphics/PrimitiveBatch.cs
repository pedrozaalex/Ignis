using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ignis.Engine.UI.Graphics
{
    /// <summary>
    /// PrimitiveBatch - Low-level 2D primitive renderer using dynamic vertex/index buffers.
    /// Provides fundamental shape drawing (rectangles, lines, triangles, circles) for UI rendering.
    /// High-level widgets (progress bars, sliders, checkboxes) should compose these primitives.
    /// </summary>
    public class PrimitiveBatch : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly BasicEffect _basicEffect;
        
        // Dynamic vertex/index buffers for batching
        private VertexPositionColor[] _vertices;
        private int[] _indices;
        private int _vertexCount;
        private int _indexCount;
        
        private const int InitialVertexCapacity = 2048;
        private const int InitialIndexCapacity = 4096;
        private const int MaxVerticesPerBatch = 65535; // Max for 16-bit indices
        
        private bool _isDisposed;
        private bool _isDrawing;

        public PrimitiveBatch(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));

            // Initialize dynamic buffers
            _vertices = new VertexPositionColor[InitialVertexCapacity];
            _indices = new int[InitialIndexCapacity];

            // Create basic effect for primitive rendering
            _basicEffect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                TextureEnabled = false,
                LightingEnabled = false
            };
        }

        /// <summary>
        /// Begins a new primitive batch. Must be called before any Draw* methods.
        /// </summary>
        public void Begin(Matrix? transformMatrix = null)
        {
            if (_isDrawing)
                throw new InvalidOperationException("End must be called before Begin can be called again.");

            // Set up orthographic projection for 2D rendering
            // FIX: Changed ZNear from 0 to -1 to prevent clipping of geometry at Z=0
            var viewport = _graphicsDevice.Viewport;
            _basicEffect.World = transformMatrix ?? Matrix.Identity;
            _basicEffect.View = Matrix.Identity;
            _basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
                0, viewport.Width, viewport.Height, 0, -1f, 1f);

            _vertexCount = 0;
            _indexCount = 0;
            _isDrawing = true;
        }

        /// <summary>
        /// Ends the current batch and flushes all primitives to the GPU.
        /// </summary>
        public void End()
        {
            if (!_isDrawing)
                throw new InvalidOperationException("Begin must be called before End can be called.");

            Flush();
            _isDrawing = false;
        }

        /// <summary>
        /// Flushes all batched primitives to the GPU.
        /// </summary>
        private void Flush()
        {
            if (_vertexCount == 0 || _indexCount == 0)
                return;

            // FIX: Explicitly set render states to ensure visibility.
            // SpriteBatch might have set CullCounterClockwise, which could hide our primitives.
            // We don't need to restore these because SpriteBatch.End() resets its own state before drawing.
            var prevRasterizer = _graphicsDevice.RasterizerState;
            var prevBlend = _graphicsDevice.BlendState;
            var prevDepth = _graphicsDevice.DepthStencilState;

            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            _graphicsDevice.BlendState = BlendState.AlphaBlend;
            _graphicsDevice.DepthStencilState = DepthStencilState.None;

            try
            {
                // Apply effect and draw
                foreach (var pass in _basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    _graphicsDevice.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        _vertices,
                        0,
                        _vertexCount,
                        _indices,
                        0,
                        _indexCount / 3
                    );
                }
            }
            finally
            {
                // Reset counters
                _vertexCount = 0;
                _indexCount = 0;
                
                // Optionally restore states if needed, though usually safe not to in this batching context
                // _graphicsDevice.RasterizerState = prevRasterizer;
                // _graphicsDevice.BlendState = prevBlend;
                // _graphicsDevice.DepthStencilState = prevDepth;
            }
        }

        /// <summary>
        /// Ensures buffers have enough capacity for additional vertices/indices.
        /// </summary>
        private void EnsureCapacity(int additionalVertices, int additionalIndices)
        {
            // Check if we need to flush before adding more
            if (_vertexCount + additionalVertices > MaxVerticesPerBatch ||
                _indexCount + additionalIndices > _indices.Length)
            {
                Flush();
            }

            // Grow vertex buffer if needed
            while (_vertexCount + additionalVertices > _vertices.Length)
            {
                Array.Resize(ref _vertices, _vertices.Length * 2);
            }

            // Grow index buffer if needed
            while (_indexCount + additionalIndices > _indices.Length)
            {
                Array.Resize(ref _indices, _indices.Length * 2);
            }
        }

        /// <summary>
        /// Draws a filled rectangle (quad).
        /// </summary>
        public void DrawFilledRectangle(Rectangle bounds, Color color)
        {
            DrawFilledRectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height, color);
        }

        /// <summary>
        /// Draws a filled rectangle (quad) with explicit coordinates.
        /// </summary>
        public void DrawFilledRectangle(float x, float y, float width, float height, Color color)
        {
            if (!_isDrawing)
                throw new InvalidOperationException("Begin must be called before drawing.");

            EnsureCapacity(4, 6);

            int baseVertex = _vertexCount;

            // Add vertices (clockwise from top-left)
            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(x, y, 0), color);
            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(x + width, y, 0), color);
            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(x + width, y + height, 0), color);
            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(x, y + height, 0), color);

            // Add indices (two triangles)
            _indices[_indexCount++] = baseVertex;
            _indices[_indexCount++] = baseVertex + 1;
            _indices[_indexCount++] = baseVertex + 2;
            _indices[_indexCount++] = baseVertex;
            _indices[_indexCount++] = baseVertex + 2;
            _indices[_indexCount++] = baseVertex + 3;
        }

        /// <summary>
        /// Draws a rectangle border/outline with optional rounded corners.
        /// </summary>
        public void DrawBorder(Rectangle bounds, float thickness, Color color, float radius = 0f)
        {
            if (!_isDrawing)
                throw new InvalidOperationException("Begin must be called before drawing.");

            float x = bounds.X;
            float y = bounds.Y;
            float w = bounds.Width;
            float h = bounds.Height;

            if (radius <= 0)
            {
                // Simple rectangular border
                DrawFilledRectangle(x, y, w, thickness, color); // Top
                DrawFilledRectangle(x, y + h - thickness, w, thickness, color); // Bottom
                DrawFilledRectangle(x, y, thickness, h, color); // Left
                DrawFilledRectangle(x + w - thickness, y, thickness, h, color); // Right
                return;
            }

            // Clamp corner radius
            float maxRadius = Math.Min(w, h) * 0.5f;
            radius = Math.Min(radius, maxRadius);

            // Draw straight edge segments (avoiding corners)
            DrawFilledRectangle(x + radius, y, w - 2 * radius, thickness, color); // Top
            DrawFilledRectangle(x + radius, y + h - thickness, w - 2 * radius, thickness, color); // Bottom
            DrawFilledRectangle(x, y + radius, thickness, h - 2 * radius, color); // Left
            DrawFilledRectangle(x + w - thickness, y + radius, thickness, h - 2 * radius, color); // Right

            // Draw rounded corners as arc outlines
            int segments = 8;
            DrawArcOutline(new Vector2(x + radius, y + radius), radius, radius - thickness, color, segments, 180, 270); // Top-left
            DrawArcOutline(new Vector2(x + w - radius, y + radius), radius, radius - thickness, color, segments, 270, 360); // Top-right
            DrawArcOutline(new Vector2(x + w - radius, y + h - radius), radius, radius - thickness, color, segments, 0, 90); // Bottom-right
            DrawArcOutline(new Vector2(x + radius, y + h - radius), radius, radius - thickness, color, segments, 90, 180); // Bottom-left
        }

        /// <summary>
        /// Draws an arc outline (ring segment) between inner and outer radius.
        /// </summary>
        private void DrawArcOutline(Vector2 center, float outerRadius, float innerRadius, Color color, int segments, float startAngle, float endAngle)
        {
            if (segments < 1) segments = 1;

            EnsureCapacity((segments + 1) * 2, segments * 6);
            int baseVertex = _vertexCount;

            float startRad = MathHelper.ToRadians(startAngle);
            float endRad = MathHelper.ToRadians(endAngle);
            float angleStep = (endRad - startRad) / segments;

            // Generate vertices for outer and inner arc
            for (int i = 0; i <= segments; i++)
            {
                float angle = startRad + angleStep * i;
                var direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                
                // Outer vertex
                _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(center + direction * outerRadius, 0), color);
                // Inner vertex
                _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(center + direction * innerRadius, 0), color);
            }

            // Generate quad indices for each segment
            for (int i = 0; i < segments; i++)
            {
                int outerCurrent = baseVertex + i * 2;
                int innerCurrent = baseVertex + i * 2 + 1;
                int outerNext = baseVertex + (i + 1) * 2;
                int innerNext = baseVertex + (i + 1) * 2 + 1;

                // First triangle
                _indices[_indexCount++] = outerCurrent;
                _indices[_indexCount++] = outerNext;
                _indices[_indexCount++] = innerCurrent;
                
                // Second triangle
                _indices[_indexCount++] = innerCurrent;
                _indices[_indexCount++] = outerNext;
                _indices[_indexCount++] = innerNext;
            }
        }

        /// <summary>
        /// Draws a line between two points.
        /// </summary>
        public void DrawLine(Vector2 start, Vector2 end, float thickness, Color color)
        {
            if (!_isDrawing)
                throw new InvalidOperationException("Begin must be called before drawing.");

            var direction = end - start;
            var length = direction.Length();
            if (length < 0.001f) return;

            direction.Normalize();
            var perpendicular = new Vector2(-direction.Y, direction.X) * (thickness * 0.5f);

            EnsureCapacity(4, 6);
            int baseVertex = _vertexCount;

            // Create quad along the line
            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(start + perpendicular, 0), color);
            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(start - perpendicular, 0), color);
            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(end - perpendicular, 0), color);
            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(end + perpendicular, 0), color);

            _indices[_indexCount++] = baseVertex;
            _indices[_indexCount++] = baseVertex + 1;
            _indices[_indexCount++] = baseVertex + 2;
            _indices[_indexCount++] = baseVertex;
            _indices[_indexCount++] = baseVertex + 2;
            _indices[_indexCount++] = baseVertex + 3;
        }

        /// <summary>
        /// Draws a filled triangle.
        /// </summary>
        public void DrawTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Color color)
        {
            if (!_isDrawing)
                throw new InvalidOperationException("Begin must be called before drawing.");

            EnsureCapacity(3, 3);
            int baseVertex = _vertexCount;

            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(p1, 0), color);
            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(p2, 0), color);
            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(p3, 0), color);

            _indices[_indexCount++] = baseVertex;
            _indices[_indexCount++] = baseVertex + 1;
            _indices[_indexCount++] = baseVertex + 2;
        }

        /// <summary>
        /// Draws a filled circle using triangle fan approximation.
        /// </summary>
        public void DrawCircle(Vector2 center, float radius, Color color, int segments = 32)
        {
            if (!_isDrawing)
                throw new InvalidOperationException("Begin must be called before drawing.");

            if (segments < 3) segments = 3;

            EnsureCapacity(segments + 1, segments * 3);
            int baseVertex = _vertexCount;

            // Center vertex
            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(center, 0), color);

            // Circle vertices
            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * MathHelper.TwoPi;
                var offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(center + offset, 0), color);
            }

            // Indices for triangle fan
            for (int i = 0; i < segments; i++)
            {
                _indices[_indexCount++] = baseVertex; // Center
                _indices[_indexCount++] = baseVertex + i + 1;
                _indices[_indexCount++] = baseVertex + i + 2;
            }
        }

        /// <summary>
        /// Draws a rounded rectangle by composing rectangles and circle segments.
        /// </summary>
        public void DrawRoundedRectangle(Rectangle bounds, float cornerRadius, Color color)
        {
            if (!_isDrawing)
                throw new InvalidOperationException("Begin must be called before drawing.");

            if (cornerRadius <= 0)
            {
                DrawFilledRectangle(bounds, color);
                return;
            }

            // Clamp corner radius
            float maxRadius = Math.Min(bounds.Width, bounds.Height) * 0.5f;
            cornerRadius = Math.Min(cornerRadius, maxRadius);

            float x = bounds.X;
            float y = bounds.Y;
            float w = bounds.Width;
            float h = bounds.Height;
            float r = cornerRadius;

            // Draw central cross (horizontal and vertical bars)
            DrawFilledRectangle(x + r, y, w - 2 * r, h, color); // Horizontal bar
            DrawFilledRectangle(x, y + r, r, h - 2 * r, color); // Left vertical
            DrawFilledRectangle(x + w - r, y + r, r, h - 2 * r, color); // Right vertical

            // Draw corner circles
            DrawCircleSegment(new Vector2(x + r, y + r), r, color, 8, 180, 270); // Top-left
            DrawCircleSegment(new Vector2(x + w - r, y + r), r, color, 8, 270, 360); // Top-right
            DrawCircleSegment(new Vector2(x + w - r, y + h - r), r, color, 8, 0, 90); // Bottom-right
            DrawCircleSegment(new Vector2(x + r, y + h - r), r, color, 8, 90, 180); // Bottom-left
        }

        /// <summary>
        /// Draws a segment of a circle (arc) as a filled triangle fan.
        /// </summary>
        private void DrawCircleSegment(Vector2 center, float radius, Color color, int segments, float startAngle, float endAngle)
        {
            if (segments < 1) segments = 1;

            EnsureCapacity(segments + 2, segments * 3);
            int baseVertex = _vertexCount;

            // Center vertex
            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(center, 0), color);

            // Arc vertices
            float startRad = MathHelper.ToRadians(startAngle);
            float endRad = MathHelper.ToRadians(endAngle);
            float angleStep = (endRad - startRad) / segments;

            for (int i = 0; i <= segments; i++)
            {
                float angle = startRad + angleStep * i;
                var offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(center + offset, 0), color);
            }

            // Indices
            for (int i = 0; i < segments; i++)
            {
                _indices[_indexCount++] = baseVertex;
                _indices[_indexCount++] = baseVertex + i + 1;
                _indices[_indexCount++] = baseVertex + i + 2;
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _basicEffect?.Dispose();
                _isDisposed = true;
            }
        }
    }
}

