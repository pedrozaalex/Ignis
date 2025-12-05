using System.Numerics;
using System.Runtime.CompilerServices;
using Friflo.Engine.ECS;

namespace Ignis.Physics;

/// <summary>
/// High-performance 2D collision system using spatial hashing for broad-phase.
/// </summary>
public sealed class CollisionSystem
{
    private readonly EntityStore _store;
    
    // Spatial hash grid
    private readonly Dictionary<long, List<int>> _grid = new();
    private readonly List<int> _entityList = new(); // Reused list for queries
    private float _cellSize;
    
    // Queries
    private readonly ArchetypeQuery<CircleCollider> _circleQuery;
    private readonly ArchetypeQuery<BoxCollider> _boxQuery;
    
    // Collision pair tracking to avoid duplicate checks
    private readonly HashSet<long> _checkedPairs = new();
    
    /// <summary>
    /// Creates a new collision system.
    /// </summary>
    /// <param name="store">The entity store to query.</param>
    /// <param name="cellSize">Spatial hash cell size. Should be ~2x the largest collider.</param>
    public CollisionSystem(EntityStore store, float cellSize = 64f)
    {
        _store = store;
        _cellSize = cellSize;
        
        _circleQuery = store.Query<CircleCollider>();
        _boxQuery = store.Query<BoxCollider>();
    }
    
    /// <summary>
    /// Updates the spatial hash grid with current entity positions.
    /// Call this once per frame before collision queries.
    /// </summary>
    public void UpdateSpatialHash(Func<Entity, Vector2> getPosition)
    {
        // Clear grid
        foreach (var cell in _grid.Values)
            cell.Clear();
        
        // Insert circle colliders
        foreach (var entity in _circleQuery.Entities)
        {
            var pos = getPosition(entity);
            var collider = entity.GetComponent<CircleCollider>();
            var worldPos = pos + collider.Offset;
            
            InsertIntoGrid(entity.Id, worldPos, collider.Radius);
        }
        
        // Insert box colliders
        foreach (var entity in _boxQuery.Entities)
        {
            var pos = getPosition(entity);
            var collider = entity.GetComponent<BoxCollider>();
            var worldPos = pos + collider.Offset;
            var halfSize = collider.Size * 0.5f;
            
            // Insert into all cells the box overlaps
            InsertBoxIntoGrid(entity.Id, worldPos, halfSize);
        }
    }
    
    /// <summary>
    /// Performs collision detection and populates CollisionEvents components.
    /// </summary>
    public void DetectCollisions(Func<Entity, Vector2> getPosition)
    {
        _checkedPairs.Clear();
        
        // Clear all collision events
        var eventsQuery = _store.Query<CollisionEvents>();
        foreach (var entity in eventsQuery.Entities)
        {
            entity.GetComponent<CollisionEvents>().Clear();
        }
        
        // Check circle vs circle
        foreach (var entityA in _circleQuery.Entities)
        {
            var posA = getPosition(entityA);
            var colliderA = entityA.GetComponent<CircleCollider>();
            var worldPosA = posA + colliderA.Offset;
            
            // Get potential colliders from spatial hash
            GetNearbyEntities(worldPosA, colliderA.Radius, _entityList);
            
            foreach (var entityBId in _entityList)
            {
                if (entityBId == entityA.Id) continue;
                if (!AddCheckedPair(entityA.Id, entityBId)) continue;
                
                var entityB = _store.GetEntityById(entityBId);
                if (entityB.IsNull) continue;
                
                // Check layer masks
                if (!CheckLayerCollision(entityA, entityB)) continue;
                
                var posB = getPosition(entityB);
                bool collision = false;
                
                if (entityB.HasComponent<CircleCollider>())
                {
                    var colliderB = entityB.GetComponent<CircleCollider>();
                    var worldPosB = posB + colliderB.Offset;
                    collision = CollisionDetection.CircleVsCircle(worldPosA, colliderA.Radius, worldPosB, colliderB.Radius);
                }
                else if (entityB.HasComponent<BoxCollider>())
                {
                    var colliderB = entityB.GetComponent<BoxCollider>();
                    var worldPosB = posB + colliderB.Offset;
                    collision = CollisionDetection.CircleVsBox(worldPosA, colliderA.Radius, worldPosB, colliderB.Size);
                }
                
                if (collision)
                {
                    RecordCollision(entityA, entityB);
                }
            }
        }
        
        // Check box vs box (for boxes not already checked via circle queries)
        foreach (var entityA in _boxQuery.Entities)
        {
            if (entityA.HasComponent<CircleCollider>()) continue; // Already handled
            
            var posA = getPosition(entityA);
            var colliderA = entityA.GetComponent<BoxCollider>();
            var worldPosA = posA + colliderA.Offset;
            var halfSizeA = colliderA.Size * 0.5f;
            
            // Get potential colliders from spatial hash
            GetNearbyEntitiesBox(worldPosA, halfSizeA, _entityList);
            
            foreach (var entityBId in _entityList)
            {
                if (entityBId == entityA.Id) continue;
                if (!AddCheckedPair(entityA.Id, entityBId)) continue;
                
                var entityB = _store.GetEntityById(entityBId);
                if (entityB.IsNull) continue;
                if (entityB.HasComponent<CircleCollider>()) continue; // Handled in circle pass
                
                if (!CheckLayerCollision(entityA, entityB)) continue;
                
                if (entityB.HasComponent<BoxCollider>())
                {
                    var posB = getPosition(entityB);
                    var colliderB = entityB.GetComponent<BoxCollider>();
                    var worldPosB = posB + colliderB.Offset;
                    
                    if (CollisionDetection.BoxVsBox(worldPosA, colliderA.Size, worldPosB, colliderB.Size))
                    {
                        RecordCollision(entityA, entityB);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Query for all entities within a circle.
    /// </summary>
    public void QueryCircle(Vector2 center, float radius, List<Entity> results, Func<Entity, Vector2> getPosition, uint layerMask = CollisionLayers.All)
    {
        results.Clear();
        GetNearbyEntities(center, radius, _entityList);
        
        foreach (var entityId in _entityList)
        {
            var entity = _store.GetEntityById(entityId);
            if (entity.IsNull) continue;
            
            // Check layer
            if (entity.HasComponent<CollisionLayer>())
            {
                var layer = entity.GetComponent<CollisionLayer>();
                if ((layer.Layer & layerMask) == 0) continue;
            }
            
            var pos = getPosition(entity);
            bool hit = false;
            
            if (entity.HasComponent<CircleCollider>())
            {
                var collider = entity.GetComponent<CircleCollider>();
                var worldPos = pos + collider.Offset;
                hit = CollisionDetection.CircleVsCircle(center, radius, worldPos, collider.Radius);
            }
            else if (entity.HasComponent<BoxCollider>())
            {
                var collider = entity.GetComponent<BoxCollider>();
                var worldPos = pos + collider.Offset;
                hit = CollisionDetection.CircleVsBox(center, radius, worldPos, collider.Size);
            }
            
            if (hit)
                results.Add(entity);
        }
    }
    
    /// <summary>
    /// Query for all entities within an AABB.
    /// </summary>
    public void QueryBox(Vector2 center, Vector2 size, List<Entity> results, Func<Entity, Vector2> getPosition, uint layerMask = CollisionLayers.All)
    {
        results.Clear();
        var halfSize = size * 0.5f;
        GetNearbyEntitiesBox(center, halfSize, _entityList);
        
        foreach (var entityId in _entityList)
        {
            var entity = _store.GetEntityById(entityId);
            if (entity.IsNull) continue;
            
            // Check layer
            if (entity.HasComponent<CollisionLayer>())
            {
                var layer = entity.GetComponent<CollisionLayer>();
                if ((layer.Layer & layerMask) == 0) continue;
            }
            
            var pos = getPosition(entity);
            bool hit = false;
            
            if (entity.HasComponent<CircleCollider>())
            {
                var collider = entity.GetComponent<CircleCollider>();
                var worldPos = pos + collider.Offset;
                hit = CollisionDetection.CircleVsBox(worldPos, collider.Radius, center, size);
            }
            else if (entity.HasComponent<BoxCollider>())
            {
                var collider = entity.GetComponent<BoxCollider>();
                var worldPos = pos + collider.Offset;
                hit = CollisionDetection.BoxVsBox(center, size, worldPos, collider.Size);
            }
            
            if (hit)
                results.Add(entity);
        }
    }
    
    // ─────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────────────
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private long GetCellKey(int x, int y) => ((long)x << 32) | (uint)y;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private (int x, int y) GetCell(Vector2 pos) => ((int)MathF.Floor(pos.X / _cellSize), (int)MathF.Floor(pos.Y / _cellSize));
    
    private void InsertIntoGrid(int entityId, Vector2 pos, float radius)
    {
        var minCell = GetCell(pos - new Vector2(radius));
        var maxCell = GetCell(pos + new Vector2(radius));
        
        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                var key = GetCellKey(x, y);
                if (!_grid.TryGetValue(key, out var list))
                {
                    list = new List<int>(8);
                    _grid[key] = list;
                }
                list.Add(entityId);
            }
        }
    }
    
    private void InsertBoxIntoGrid(int entityId, Vector2 pos, Vector2 halfSize)
    {
        var minCell = GetCell(pos - halfSize);
        var maxCell = GetCell(pos + halfSize);
        
        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                var key = GetCellKey(x, y);
                if (!_grid.TryGetValue(key, out var list))
                {
                    list = new List<int>(8);
                    _grid[key] = list;
                }
                list.Add(entityId);
            }
        }
    }
    
    private void GetNearbyEntities(Vector2 pos, float radius, List<int> results)
    {
        results.Clear();
        var minCell = GetCell(pos - new Vector2(radius));
        var maxCell = GetCell(pos + new Vector2(radius));
        
        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                var key = GetCellKey(x, y);
                if (_grid.TryGetValue(key, out var list))
                {
                    results.AddRange(list);
                }
            }
        }
    }
    
    private void GetNearbyEntitiesBox(Vector2 pos, Vector2 halfSize, List<int> results)
    {
        results.Clear();
        var minCell = GetCell(pos - halfSize);
        var maxCell = GetCell(pos + halfSize);
        
        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                var key = GetCellKey(x, y);
                if (_grid.TryGetValue(key, out var list))
                {
                    results.AddRange(list);
                }
            }
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AddCheckedPair(int idA, int idB)
    {
        // Ensure consistent ordering
        if (idA > idB) (idA, idB) = (idB, idA);
        var pairKey = ((long)idA << 32) | (uint)idB;
        return _checkedPairs.Add(pairKey);
    }
    
    private bool CheckLayerCollision(Entity entityA, Entity entityB)
    {
        var hasLayerA = entityA.HasComponent<CollisionLayer>();
        var hasLayerB = entityB.HasComponent<CollisionLayer>();
        
        if (!hasLayerA && !hasLayerB) return true; // Both default, collide
        if (!hasLayerA || !hasLayerB) return true; // One default, collide
        
        var layerA = entityA.GetComponent<CollisionLayer>();
        var layerB = entityB.GetComponent<CollisionLayer>();
        return layerA.CanCollideWith(layerB);
    }
    
    private void RecordCollision(Entity entityA, Entity entityB)
    {
        if (entityA.HasComponent<CollisionEvents>())
        {
            entityA.GetComponent<CollisionEvents>().Add(entityB.Id);
        }
        
        if (entityB.HasComponent<CollisionEvents>())
        {
            entityB.GetComponent<CollisionEvents>().Add(entityA.Id);
        }
    }
}

