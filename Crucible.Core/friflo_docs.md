Here is a summarized guide to the Friflo Engine ECS, condensing the key features and usage examples into a single reference document.

-----

# Friflo Engine ECS - Features Summary

Friflo ECS is a high-performance, archetype-based Entity Component System for C\#. [cite_start]It focuses on memory efficiency, cache locality, and simple API design[cite: 350, 353].

## 1\. Core Concepts: Entities & Store

The `EntityStore` acts as an in-memory database containing all entities. [cite_start]An `Entity` has a unique ID and acts as a container for components, tags, and scripts[cite: 96, 97].

**Usage:**

```csharp
var store = new EntityStore();
var entity = store.CreateEntity();

// Delete an entity
entity.DeleteEntity();

// Check if deleted
if (entity.IsNull) { /* ... */ }
```

[cite_start][cite: 98, 100, 102]

## 2\. Components & Tags

  * **Components:** Structs containing data. [cite_start]Adding a component of an existing type updates the value[cite: 118, 120].
  * **Tags:** Empty structs used for flagging entities. [cite_start]They store no data[cite: 138].

**Usage:**

```csharp
struct Position : IComponent { public int x, y, z; }
struct EnemyTag : ITag { }

// Add/Get components
entity.AddComponent(new Position { x = 10, y = 20 });
ref Position pos = ref entity.GetComponent<Position>();

// Add tags
entity.AddTag<EnemyTag>();
```

[cite_start][cite: 121, 138, 142]

## 3\. Batch Operations

[cite_start]To improve performance and minimize archetype fragmentation (structural changes), components should be added or removed in batches rather than one by one[cite: 1, 18].

**Creation Batch:**

```csharp
// Create entity with multiple components in one step
store.CreateEntity(new Position(1, 2, 3), new Transform(), Tags.Get<MyTag>());
```

[cite_start][cite: 5, 9]

**Modification Batch:**

```csharp
// Apply multiple changes to an existing entity at once
entity.Batch()
    .Add(new Position(1, 2, 3))
    .AddTag<MyTag>()
    .Apply();
```

[cite_start][cite: 39, 40]

## 4\. Queries

Queries retrieve entities based on their component composition. [cite_start]They allow for iteration over data efficiently[cite: 350, 362].

**Basic Query:**

```csharp
var query = store.Query<Position, Velocity>()
                 .AllTags(Tags.Get<EnemyTag>())
                 .WithoutAnyTags(Tags.Get<DeadTag>());

foreach (var (pos, vel, entity) in query.Entities.Data) {
    pos.value += vel.value;
}
```

[cite_start][cite: 362, 384]

**Structural Changes & CommandBuffers:**
[cite_start]Modifying the structure (adding/removing components) inside a query loop throws a `StructuralChangeException`[cite: 386]. Use a `CommandBuffer` to record changes and apply them later.

```csharp
var buffer = store.GetCommandBuffer();
foreach (var entity in query.Entities) {
    // Record change
    buffer.AddComponent(entity.Id, new EntityName("Updated"));
}
buffer.Playback(); // Apply changes on main thread
```

[cite_start][cite: 394, 406, 408]

## 5\. Query Optimization

For high-performance scenarios, Friflo offers several optimization techniques:

  * [cite_start]**Boosted Query:** Uses `query.Each(new JobStruct())` to avoid delegate allocation overhead[cite: 290].
  * [cite_start]**Chunks & SIMD:** Access data as memory chunks (`Span`), enabling vectorization (SIMD)[cite: 303, 332].
  * [cite_start]**Parallel Jobs:** Execute queries across multiple CPU cores[cite: 312].

**SIMD Example:**

```csharp
foreach (var (components, entities) in query.Chunks) {
    var values = components.AsSpan256<int>(); // Vectorized span
    // Process using Vector256<int> ...
}
```

[cite_start][cite: 339, 345]

## 6\. Component Indexing

[cite_start]Standard components allow O(1) lookups of entities based on a specific field value, similar to a database index[cite: 58, 60].

**Usage:**

```csharp
struct PlayerId : IIndexedComponent<int> {
    public int id;
    public int GetIndexedValue() => id;
}

var index = store.ComponentIndex<PlayerId, int>();
// Lookup entity in O(1)
var entities = index[42]; 
```

[cite_start][cite: 62, 69, 70]

## 7\. Systems

Systems provide a structured way to organize logic. [cite_start]They can be grouped in a `SystemRoot`, enabling performance monitoring and ordered execution[cite: 538, 540].

**Usage:**

```csharp
class MoveSystem : QuerySystem<Position, Velocity>
{
    protected override void OnUpdate() {
        // Iterate query efficiently
        Query.ForEachEntity((ref Position p, ref Velocity v, Entity e) => {
             p.value += v.value;
        });
    }
}

// Setup
var root = new SystemRoot(store) { new MoveSystem() };
root.Update(default);
```

[cite_start][cite: 548, 551]

## 8\. Relations (Inventory / Data Sets)

Unlike standard components, multiple `IRelation` items of the same type can be added to a single entity. [cite_start]This is useful for inventories or buffs[cite: 419, 430].

**Usage:**

```csharp
struct InventoryItem : IRelation<int> { 
    public int itemId; 
    public int count;
    public int GetRelationKey() => itemId;
}

entity.AddRelation(new InventoryItem { itemId = 101, count = 5 });
entity.AddRelation(new InventoryItem { itemId = 102, count = 1 });

// Retrieve specific item
var item = entity.GetRelation<InventoryItem, int>(101);
```

[cite_start][cite: 435, 438, 441]

## 9\. Relationships (Graph Links)

Relationships model directed links between entities (e.g., "Attacking", "ParentOf"). [cite_start]They update automatically if a target entity is deleted[cite: 465, 478].

  * **LinkComponent:** One link per type (e.g., `Target`).
  * **LinkRelation:** Multiple links per type (e.g., `Alliance`).

**Usage:**

```csharp
struct AttackTarget : ILinkComponent { 
    public Entity target;
    public Entity GetIndexedValue() => target; 
}

// Link entity 1 to entity 2
entity1.AddComponent(new AttackTarget { target = entity2 });

// Get all entities targeting entity 2 (Reverse lookup)
var attackers = entity2.GetIncomingLinks<AttackTarget>();
```

[cite_start][cite: 501, 505, 507]

## 10\. Events & Signals

  * [cite_start]**Events:** Subscribe to changes like `OnComponentChanged`, `OnTagsChanged`, or `OnChildEntitiesChanged`[cite: 219, 224].
  * [cite_start]**Signals:** Send custom events to entities without polling state every frame[cite: 261].

**Usage:**

```csharp
entity.OnComponentChanged += ev => {
    Console.WriteLine($"Action: {ev.Action}, Component: {ev.Type}");
};
```

[cite_start][cite: 233]

## 11\. Hierarchy & Scripts

  * **Hierarchy:** Entities can be parented to others. [cite_start]Removing a parent removes the children[cite: 173, 176].
  * [cite_start]**Scripts:** Standard C\# classes that can be attached to entities for OOP-style behavior (`Start`/`Update`), though generally slower than systems[cite: 160, 161].

**Usage:**

```csharp
rootEntity.AddChild(childEntity);
```

[cite_start][cite: 177]

## 12\. JSON Serialization

[cite_start]Friflo ECS has built-in support for serializing entities and components to JSON[cite: 206].

**Usage:**

```csharp
var serializer = new EntitySerializer();
serializer.WriteStore(store, fileStream);
```

[cite_start][cite: 213, 214]