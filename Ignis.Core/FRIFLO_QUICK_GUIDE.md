# Friflo ECS - Distilled Guide for Game Engine Development

This document is a distilled summary of the Friflo ECS documentation, focusing on advanced use cases, performance optimizations, and features relevant to game engine architecture.

## 1. Core Concepts & Architecture

*   **Entity**: A lightweight struct with a unique `Id`. Acts as a container for components, tags, scripts, and children.
*   **Component**: Pure data structs implementing `IComponent`. Stored in contiguous memory (Archetypes).
*   **Archetype**: A unique combination of component types and tags. Similar to a SQL Table.
*   **EntityStore**: The "World" or database containing entities. Multiple stores can exist independently.
*   **System**: Logic that processes entities based on queries. Optional but recommended for structure.

## 2. High-Performance Entity Management

### Batching & Bulk Operations
Minimize structural changes (archetype moves) by performing operations in batches.

*   **Batch Creation**: Create entities with multiple components in one go.
    ```csharp
    store.CreateEntity(new Position(), new Velocity(), Tags.Get<MyTag>());
    ```
*   **Bulk Creation**: Create thousands of identical entities instantly.
    ```csharp
    var archetype = store.GetArchetype(ComponentTypes.Get<Position>(), Tags.Get<MyTag>());
    var entities = archetype.CreateEntities(100_000);
    ```
*   **Entity Batch**: Apply multiple changes to a single entity with one structural change.
    ```csharp
    entity.Batch().Add(new Position()).AddTag<MyTag>().Apply();
    ```
*   **Bulk Batch**: Apply the same batch of changes to a list of entities or a query result.
    ```csharp
    var batch = new EntityBatch();
    batch.Add(new Position());
    store.Entities.ApplyBatch(batch); // or list.ApplyBatch(batch)
    ```

### Fast Component Access (`Entity.Data`)
When accessing multiple components on the same entity, use `Entity.Data` to avoid repeated lookup overhead.
```csharp
var data = entity.Data;
ref var pos = ref data.Get<Position>();
ref var vel = ref data.Get<Velocity>();
```

### Structural Changes & CommandBuffer
*   **StructuralChangeException**: Thrown if you add/remove components/tags inside a query loop.
*   **CommandBuffer**: Use this to record changes during iteration and apply them later (`Playback()`). Thread-safe version: `CommandBuffer.Synced`.

## 3. Advanced Querying & Optimization

### Query Basics
```csharp
var query = store.Query<Position, Velocity>()
    .AllTags(Tags.Get<Active>())
    .WithoutAnyTags(Tags.Get<Disabled>())
    .WithDisabled(); // Include disabled entities in result

// Basic iteration (fast)
foreach (var (positions, velocities, entities) in query.Chunks) { ... }
```

### Optimization Levels
1.  **Boosted Query (`IEach`)**: ~3x faster than standard iteration. Requires `Friflo.Engine.ECS.Boost`.
    ```csharp
    struct MoveJob : IEach<Position, Velocity> {
        public void Execute(ref Position p, ref Velocity v) => p.value += v.value;
    }
    query.Each(new MoveJob());
    ```
2.  **Parallel Query Job**: Multithreaded execution. Best for heavy arithmetic.
    ```csharp
    var job = query.ForEach((positions, entities) => { ... });
    job.RunParallel();
    ```
3.  **SIMD / Vectorization**: Process components using hardware vectors (AVX/SSE).
    ```csharp
    foreach (var (chunk, entities) in query.Chunks) {
        var span = chunk.AsSpan256<int>();
        // Process span with Vector256<int>
    }
    ```

## 4. Indexing & Lookups (O(1))

Friflo supports O(1) lookups for component values, similar to SQL indices.

*   **IIndexedComponent**: Define a component with an indexed field.
    ```csharp
    struct PlayerID : IIndexedComponent<int> {
        public int value;
        public int GetIndexedValue() => value;
    }
    ```
*   **Usage**:
    ```csharp
    var index = store.ComponentIndex<PlayerID, int>();
    var entity = index[42]; // O(1) lookup
    ```
*   **Range Queries**: `store.Query().ValueInRange<PlayerID, int>(10, 20);`

## 5. Relations & Relationships

### Relations (One-to-Many of same type)
Add multiple components of the same type to an entity, distinguished by a key.
*   **Interface**: `IRelation<TKey>`
*   **Usage**: Inventory systems, buffs/debuffs.
    ```csharp
    entity.AddRelation(new Buff { type = BuffType.Poison, duration = 10 });
    entity.AddRelation(new Buff { type = BuffType.Fire, duration = 5 });
    ```

### Relationships (Entity Links)
Model directed graphs (Attack targets, Parent/Child, Ownership).

*   **Link Component (`ILinkComponent`)**: Single link per type.
    ```csharp
    struct Target : ILinkComponent { public Entity value; ... }
    entity.AddComponent(new Target { value = enemy });
    ```
*   **Link Relation (`ILinkRelation`)**: Multiple links to different entities.
    ```csharp
    struct Alliance : ILinkRelation { public Entity target; ... }
    entity.AddRelation(new Alliance { target = factionA });
    ```
*   **Bidirectional Access**: `targetEntity.GetIncomingLinks<Target>()` returns all entities targeting it (O(1)).

## 6. Systems Architecture

*   **SystemRoot**: The root container for systems. Handles update loops.
*   **QuerySystem**: Base class for systems processing a query.
    ```csharp
    class MoveSystem : QuerySystem<Position, Velocity> {
        protected override void OnUpdate() {
            Query.Each(new MoveJob());
        }
    }
    ```
*   **Performance Monitoring**: Built-in perf logging (`root.GetPerfLog()`) tracks execution time and allocations per system.

### System Lifecycle
Systems can override additional methods for fine-grained control:
```csharp
protected override void OnUpdateGroupBegin() { } // Before any system in group updates
protected override void OnUpdate()           { } // Per-store update
protected override void OnUpdateGroupEnd()   { } // After all systems in group update
```

## 7. Events & Signals

*   **Entity Events**: Subscribe to `OnComponentChanged`, `OnTagsChanged`, `OnChildEntitiesChanged`.
*   **Event Recorder**: Record events to process them later in a query (e.g., "Process all entities that gained `Damage` component this frame").
    ```csharp
    query.EventFilter.ComponentAdded<Damage>();
    if (query.HasEvent(entity.Id)) { ... }
    ```
*   **Signals**: Lightweight, struct-based messaging for occasional events (e.g., Collision).
    ```csharp
    entity.AddSignalHandler<CollisionSignal>(sig => ...);
    entity.EmitSignal(new CollisionSignal { other = wall });
    ```

## 8. Game Engine Specifics

*   **Hierarchy**: Built-in parent/child support.
    ```csharp
    parent.AddChild(child);
    foreach (var child in parent.ChildEntities) { ... }
    ```
*   **Copying & Cloning**:
    ```csharp
    var clone = entity.CloneEntity(); // Create duplicate
    entity.CopyEntity(targetEntity);  // Copy components/tags to target
    ```
*   **Serialization**: JSON serialization support for entities and stores.
    ```csharp
    serializer.WriteStore(store, stream);
    ```
    Attributes: `[ComponentKey("name")]`, `[Ignore]`, `[Serialize("alias")]`.
*   **Native AOT**: Fully supported. Requires registering component types manually if not using reflection-based fallback.
*   **Unity Integration**: Available as a package, supports Editor integration.

## 9. Performance Guidelines

1.  **Prefer Structs**: Use structs for components and relations to avoid GC.
2.  **Avoid Structural Changes in Loops**: Use `CommandBuffer`.
3.  **Use Batching**: For creating/modifying multiple entities.
4.  **Limit Indexed/Relation Counts**: Keep duplicates/relations per entity < 100 for optimal O(N) insertion performance.
5.  **Vectorization**: Use `Chunk` iteration and `AsSpan` for heavy math.
