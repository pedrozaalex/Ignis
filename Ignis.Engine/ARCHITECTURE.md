## 1. Project Overview
**Ignis** is a high-performance, data-oriented **3D game engine**. It combines the rendering of **MonoGame** with the data handling of **Friflo.Engine.ECS**.

The engine features a **Reactive Editor Architecture**. The UI is built on **Crucible**, a custom library using Signals (`Signal<T>`, `Computed<T>`, `Effect`) to create glitch-free, zero-boilerplate data binding between the UI and the ECS data.

## 2. Technology Stack
*   **Core**: .NET 8.0+
*   **Data**: [Friflo.Engine.ECS](https://friflo.gitbook.io/friflo.engine.ecs)
*   **Graphics**: [MonoGame 3.8.1+](https://monogame.net/)
*   **Reactivity**: **Crucible** (Custom Signal library defined in Phase 4).
    *   *Philosophy*: Synchronous, atomic propagation. No `INotifyPropertyChanged`.
*   **UI**: **Ignis.UI** (Declarative "Render-as-Function" framework over MonoGame SpriteBatch).

---

## 3. Project Layout & File Structure
The solution (`Ignis.sln`) layout is updated to include the Reactive core and the new UI structure.

```text
Ignis/
├── Ignis.Engine/
│   ├── Ignis.Engine.csproj
│   ├── Core/
│   │   ├── IgnisApp.cs
│   │   └── IgnisGame.cs
│   ├── ECS/
│   │   ├── Archetypes.cs
│   │   ├── Components/
│   │   ├── Systems/
│   │   └── Bridge/                     # (New) Connects ECS to Signals
│   │       ├── FrifloExtensions.cs     # .ComponentSignal<T>()
│   │       └── ReactiveQuery.cs        # SignalList<Entity> wrapper
│   ├── Reactive/                       # (New) The Crucible Library
│   │   ├── Signal.cs                   # The Atom of State
│   │   ├── Computed.cs                 # Derived State
│   │   ├── Effect.cs                   # Side Effects
│   │   └── SignalList.cs               # Observable Collections
│   ├── UI/                             # (Phase 4)
│   │   ├── Abstractions/
│   │   │   ├── IView.cs
│   │   │   └── ViewComponent.cs
│   │   ├── Core/
│   │   │   ├── UIContext.cs            # Renderer & Input Router
│   │   │   └── Bind.cs                 # Control Flow (If, For)
│   │   └── Elements/
│   │       ├── ElementBuilder.cs       # Static helpers (Row, Col, Label)
│   │       └── Primitives.cs           # Concrete Nodes (Box, Text)
│   └── Graphics/                       # (Phase 3)
│
├── Ignis.Samples/
│   └── SampleGame.cs
└── Ignis.Tests/
```

---

## 4. Core Architecture Modules

### 4.1. The "Headless" Core (`IgnisApp`)
Manages the ECS World and Simulation loop.

### 4.2. Scene Graph & Transform System
Uses Friflo events to drive the `TransformSystem`.

### 4.3. The Reactive Core (`Crucible`)
A library enabling declarative data flow.
*   **`Signal<T>`**: State container. Reading tracks dependencies; writing notifies observers.
*   **`Computed<T>`**: Pure derived state. Memoized and lazy. Updates only when dependencies change.
*   **`Effect`**: Bridges signals to side effects (e.g., rendering, logging, writing to legacy systems).

### 4.4. Declarative UI Framework (Phase 4)
Instead of managing a tree of objects manually, the UI is defined as **functions of state**.
*   **Composition**: UI is built by returning `IView` from functions.
    *   *Example*: `public IView Body() => Column(Label(nameSignal), Button("Click", onClick));`
*   **Binding**: UI elements accept `Signal<T>` instead of raw values. They subscribe automatically.
*   **Control Flow**: `Bind.If` and `Bind.For` replace C# `if/foreach` to handle DOM updates granularly without rebuilding the whole tree.

### 4.5. ECS-to-UI Bridge
Connecting Data-Oriented ECS (Structs/Chunks) to Object-Oriented UI (Signals).
*   **Problem**: ECS components are value types in arrays. You cannot hold a reference to them.
*   **Solution (`ComponentSignal`)**: A special Signal that holds an `Entity` ID.
    *   **Getter**: Calls `entity.GetComponent<T>()` directly from the store.
    *   **Setter**: Calls `entity.AddComponent<T>(val)` (or uses CommandBuffer if threaded).
    *   **Notification**: Since ECS doesn't fire events for every field write, the UI uses a **Polling Strategy** (via `UIContext.Update`) or hooks into "Archetype Changed" events for structural updates.

---

## 5. Implementation Specification

### 5.1. Module: Ignis.Engine.Reactive (The Crucible Core)

#### Class: `Signal<T>`
*   **State**: `T _value`, `List<IObserver> _observers`.
*   **Behavior**:
    *   `get`: Adds `CurrentObserver` to `_observers`. Returns `_value`.
    *   `set`: Updates `_value`. Iterates `_observers` calling `OnDependencyChanged()`.
*   **API**: Implicit conversion operator to `T` (read-only convenience).

#### Class: `Computed<T>`
*   **State**: `Func<T> _computer`, `T _cache`, `bool _isDirty`.
*   **Behavior**:
    *   Subscribes to all Signals accessed during `_computer` execution.
    *   When a dependency changes, marks `_isDirty = true`.
    *   Re-evaluates only when Read AND Dirty.

#### Class: `SignalList<T>`
*   **Purpose**: High-performance list for UI collections.
*   **Behavior**: Fires fine-grained events (`ItemAdded`, `ItemRemoved`, `ItemMoved`) rather than resetting the whole list.

### 5.2. Module: Ignis.Engine.ECS.Bridge

#### Class: `FrifloExtensions`
*   **Method**: `Signal<T> ComponentSignal<T>(this Entity entity)`
    *   Creates a `Signal<T>` where:
        *   `GetValue = () => entity.GetComponent<T>()`
        *   `SetValue = (v) => entity.AddComponent(v)`
    *   *Optimization*: If the UI runs every frame, `GetValue` is cheap (direct array access).

#### Class: `ReactiveQuery`
*   **Inherits**: `SignalList<Entity>`
*   **Constructor**: Accepts `EntityStore` and `Query`.
*   **Logic**:
    1.  Populates list from `Query.Entities`.
    2.  Subscribes to `store.OnEntityComponentAdded/Removed`.
    3.  If an entity enters the Query filter, `Add(entity)`. If it leaves, `Remove(entity)`.

### 5.3. Module: Ignis.Engine.UI (Phase 4)

#### Interface: `IView`
*   **Role**: Represents a node in the UI tree.
*   **Methods**:
    *   `void Draw(SpriteBatch sb, Rectangle bounds)`
    *   `void Mount(UIContext context)` (Called when added to live tree)
    *   `void Unmount()` (Called when removed; cleans up Signal subscriptions)

#### Static Class: `Elements` (The Builder API)
*   **Methods**:
    *   `IView Column(params IView[] children)`
    *   `IView Row(params IView[] children)`
    *   `IView Label(Signal<string> text)`
    *   `IView Button(string label, Action onClick)`
    *   `IView FloatField(string label, Signal<float> value)`
*   **Design**: These return concrete implementations (e.g., `LabelView`) that create `Effect`s in their `Mount` method to keep their internal state synced with the input Signals.

#### Class: `Bind` (Control Flow)
*   **`Bind.If(Signal<bool> condition, Func<IView> trueBuilder, Func<IView> falseBuilder)`**:
    *   Creates a view that swaps its child when `condition` changes.
    *   Ensures the old child is Unmounted (disposed) before the new one is Mounted.
*   **`Bind.For<T>(SignalList<T> list, Func<Signal<T>, IView> builder)`**:
    *   Maintains a dictionary of active Views mapped to list items.
    *   Efficiently inserts/removes Views from the layout without rebuilding unaffected items.

#### Class: `UIContext`
*   **Role**: The Root Renderer.
*   **Logic**:
    *   **Update**:
        1.  Polls Input (Mouse/Keyboard).
        2.  Traverses the `IView` tree to handle Hover/Click events.
        3.  (Optional) Calls `NotifyChanged()` on ECS signals if using a strict polling mode.
    *   **Draw**:
        1.  Calculates Flexbox-style layout (measure/arrange).
        2.  Draws the tree using the hybrid rendering strategy (see below).


#### 5.4. Module: Ignis.Engine.UI (Rendering Strategy)

**Rendering Architecture: The Hybrid Approach**
Ignis.UI employs a hybrid rendering strategy combining low-level primitive batching with traditional sprite rendering to support modern UI styling without requiring texture assets.

1.  **`PrimitiveBatch` (Low-Level Primitive Renderer)**:
    *   **Role**: GPU-accelerated shape renderer using dynamic vertex/index buffers.
    *   **Architecture**: Owns `VertexPositionColor[]` and `int[]` buffers, batches primitives between `Begin()`/`End()` calls.
    *   **API**: 
        *   `Begin(Matrix?)` - Initializes batch with orthographic projection
        *   `DrawFilledRectangle(Rectangle, Color)` - Draws solid quads
        *   `DrawBorder(Rectangle, float, Color)` - Draws rectangle outlines
        *   `DrawLine(Vector2, Vector2, float, Color)` - Draws lines as quads
        *   `DrawTriangle(Vector2, Vector2, Vector2, Color)` - Direct triangle rendering
        *   `DrawCircle(Vector2, float, Color, int)` - Triangle-fan circle approximation
        *   `DrawRoundedRectangle(Rectangle, float, Color)` - Composites rectangles and circle segments
        *   `End()` - Flushes batched geometry to GPU via `DrawUserIndexedPrimitives`
    *   **Performance**: Automatic buffer growth, flushes at 65k vertices or buffer capacity.
    *   **Effect**: Uses `BasicEffect` with vertex colors, orthographic projection.

2.  **`SpriteBatch` (Standard Text & Texture Rendering)**:
    *   Responsible for drawing Text (`SpriteFont`) and textured Icons/Images.
    *   Managed by MonoGame, handles its own batching and texture switches.

3.  **Widget Layer Composition**:
    *   **High-level widgets** (ProgressBar, Slider, Checkbox, Panel) now compose low-level primitives.
    *   Example: `ProgressBar.Draw()` calls:
        ```csharp
        batch.DrawFilledRectangle(bounds, BackgroundColor);
        batch.DrawFilledRectangle(fillBounds, FillColor);
        batch.DrawBorder(bounds, 1f, BorderColor);
        ```
    *   Widgets call `Context.PrimitiveBatch` directly—no SpriteBatch dependency for shapes.

4.  **`UIContext` Draw Loop**:
    *   **Sequence**:
        1. Calculate layout via `LayoutEngine.Layout()`
        2. Begin primitive batch: `_primitiveBatch.Begin()`
        3. Traverse view tree, each `IView.Draw()` adds primitives to batch
        4. Text rendering via `SpriteBatch` (managed separately, interleaved as needed)
        5. End primitive batch: `_primitiveBatch.End()` (flushes all geometry)
    *   **Batching Strategy**: All shape primitives batched together, minimizing draw calls and state switches.

**Design Rationale**:
*   **Decoupling**: Widgets no longer depend on SpriteBatch for shapes, enabling true procedural rendering.
*   **Flexibility**: Easy to add gradients, per-vertex colors, or custom shapes (arcs, polygons).
*   **Performance**: Single `DrawUserIndexedPrimitives` call per frame for all UI shapes (typical case).
*   **Extensibility**: Future support for textured primitives, anti-aliasing, or custom shaders via `BasicEffect` replacement.

---

## 6. Testing Strategy

### Unit Tests (`Ignis.Tests`)
*   **Reactive Logic**:
    *   `var a = new Signal<int>(1); var b = Computed.From(() => a * 2);`
    *   Assert `b.Value == 2`.
    *   `a.Value = 5;`
    *   Assert `b.Value == 10`.
*   **ECS Bridge**:
    *   Create Friflo Entity with `Position` (0,0,0).
    *   Create `Signal<Vector3> sig = entity.ComponentSignal<Position>()`.
    *   Set `sig.Value = new Vector3(1,0,0)`.
    *   Assert `entity.Position.X == 1` (Signal wrote to ECS).
    *   Set `entity.Position = new Vector3(0,5,0)` (Direct ECS write).
    *   Assert `sig.Value.Y == 5` (Signal read from ECS).

### Visual Verification (Sample Game)
*   **Inspector Window**:
    *   Select a 3D Cube.
    *   Show a `TransformInspector` (UI defined in C#).
    *   Change values in UI $\to$ Cube moves.
    *   Physics moves Cube $\to$ UI updates numbers.

## 7. Revised Roadmap

1.  **Phase 1**: Core Skeleton & Headless Setup.
2.  **Phase 2**: Test Suite & Reactive Core (`Crucible`).
    *   Implement `Signal`, `Computed`, `Effect`.
    *   Verify reactive graph topology.
3.  **Phase 3**: 3D Rendering Pipeline.
4.  **Phase 4**: **Reactive UI Framework**.
    *   **4.1**: Implement `UIContext`, `IView`, and `Elements` builder.
    *   **4.2**: Implement `Bind.If` and `Bind.For`.
    *   **4.3**: Implement `ECS Bridge` (`ComponentSignal`, `ReactiveQuery`).
    *   **4.4**: Build the `TransformInspector` sample.
5.  **Phase 5**: Input & Interaction (3D Picking).