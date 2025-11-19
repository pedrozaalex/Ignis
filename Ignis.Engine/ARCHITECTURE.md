# Ignis Engine Architecture & Implementation Specification (3D)

## 1. Project Overview
**Ignis** is a high-performance, data-oriented **3D game engine** built on the .NET ecosystem. It combines the battle-tested rendering and platform capabilities of **MonoGame** with the high-performance data handling of **Friflo.Engine.ECS**.

The engine is designed with a strict separation of concerns to enable **Headless Testing**: logic runs independently of graphics context, allowing the 3D simulation (physics, AI, transforms) to be verified without opening a window.

## 2. Technology Stack
*   **Core Framework**: .NET 8.0+
*   **Logic / Data**: [Friflo.Engine.ECS](https://friflo.gitbook.io/friflo.engine.ecs) (Archetype-based Entity Component System)
*   **Graphics / Platform**: [MonoGame 3.8.1+](https://monogame.net/) (Windowing, Graphics Device, Audio, Input, HLSL Shaders)
*   **Serialization**: `Friflo.Json.Fliox` (For ECS state debugging and saving)
*   **UI**: Custom Native UI Framework (2D Screen-Space Overlay)

---

## 3. Project Layout & File Structure (Phase 1 + 3)
The solution (`Ignis.sln`) adheres to the following physical directory structure.

```text
Ignis/
├── Ignis.Engine/                       # [Class Library] The Core Runtime
│   ├── Ignis.Engine.csproj             # Dependencies: MonoGame.DesktopGL, Friflo.Engine.ECS
│   ├── Core/
│   │   ├── EngineSettings.cs           # Configuration POCO
│   │   ├── IgnisApp.cs                 # Headless Core (manages Friflo World)
│   │   └── IgnisGame.cs                # MonoGame Wrapper (manages GraphicsDevice)
│   ├── ECS/
│   │   ├── Archetypes.cs               # Standard Archetypes (e.g., GameObject)
│   │   ├── Components/
│   │   │   ├── ComponentTypes.cs       # (General Registry)
│   │   │   └── TransformComponents.cs  # WorldTransform (Matrix), TransformDirty (Tag)
│   │   ├── Systems/
│   │   │   └── TransformSystem.cs      # Recursive World Matrix Calculation
│   │   └── SystemGroups.cs             # Enums/Constants for system sorting
│   ├── Graphics/                       # (Reserved for Phase 3: Meshes, Camera3D)
│   ├── Input/                          # (Reserved for Phase 5)
│   └── UI/                             # (Reserved for Phase 4)
│
├── Ignis.Samples/                      # [Console App] Sandbox/Reference Game
│   └── SampleGame.cs                   # Concrete implementation
│
├── Ignis.Tests/                        # [xUnit] Unit & Integration Suite
└── Ignis.sln
```

---

## 4. Core Architecture Modules

### 4.1. The "Headless" Core (`IgnisApp`)
*   **Role**: Manages the ECS World (`EntityStore`) and the 3D simulation loop.
*   **Usage**: Used by `IgnisGame` (Visual) and `Ignis.Tests` (Headless).

### 4.2. Scene Graph & Transform System
To support 3D hierarchy efficiently, we employ a **Reactive Dirty Propagation** strategy using Friflo's built-in components and event system.

*   **Data**:
    *   **Built-in Components**: `Position` (Vec3), `Rotation` (Quat), `Scale3` (Vec3).
    *   **`WorldTransform`**: The calculated Matrix4x4 used for rendering. Read-only.
    *   **`TransformDirty` (Tag)**: Added automatically via event hooks when built-in components change.
*   **Archetype**:
    *   **`GameObject`**: An archetype ensuring `Position`, `Rotation`, `Scale3`, `WorldTransform`, and `TransformDirty` are present on creation.
*   **Reactivity (`IgnisApp` Setup)**:
    *   Register `store.OnComponentChanged` events for `Position`, `Rotation`, and `Scale3`.
    *   **Handler**: When any of these change, add the `TransformDirty` tag to the entity.
*   **Logic (`TransformSystem`)**:
    1.  **Identify Roots**: Query all entities that have `WorldTransform` but *no* Parent.
    2.  **Parallel Recursion**: Iterate over Roots.
    3.  **Optimization**:
        *   If `parentIsDirty` is `true`, we **must** recalculate the child.
        *   If `parentIsDirty` is `false` AND `child.Has<TransformDirty>()` is `false`, **skip** calculation and recurse to grandchildren.
        *   **Completion**: After recalculating, remove the `TransformDirty` tag.

### 4.3. The Visual Wrapper (`IgnisGame`)
*   **Role**: Inherits from MonoGame’s `Game` class.
*   **Lifecycle**:
    *   `Update()`: Polls input $\to$ Steps `IgnisApp`.
    *   `Draw()`: Clears screen $\to$ Render 3D World (using `WorldTransform` matrices) $\to$ Render 2D UI Overlay.

---

## 5. Implementation Specification

### 5.1. Module: Ignis.Engine.Core

#### Class: `IgnisApp`
*   **File**: `Ignis.Engine/Core/IgnisApp.cs`
*   **Requirements**:
    *   **Properties**: `EntityStore World`, `SystemGroup SimulationRoot`.
    *   **Constructor**:
        *   Instantiate `World`.
        *   **Event Registration**:
            ```csharp
            World.OnComponentChanged<Position>(OnTransformChanged);
            World.OnComponentChanged<Rotation>(OnTransformChanged);
            World.OnComponentChanged<Scale3>(OnTransformChanged);
            ```
        *   **Method `OnTransformChanged`**: `eventArgs.Entity.AddTag<TransformDirty>();`
    *   **Method `Update(double dt)`**: Execute `SimulationRoot`.

#### Class: `IgnisGame`
*   **File**: `Ignis.Engine/Core/IgnisGame.cs`
*   **Requirements**:
    *   Standard MonoGame setup (`GraphicsDeviceManager`).
    *   **Draw Logic**:
        1.  Reset Render State.
        2.  Call `OnRender3D` (virtual).
        3.  Call `OnRenderUI` (virtual).

### 5.2. Module: Ignis.Engine.ECS

#### File: `ECS/Archetypes.cs`
*   **Static Class**: `Archetypes`
*   **Requirements**:
    *   Define standard definitions for entity creation.
    *   `public static Archetype GameObject => ...` (Includes `Position`, `Rotation`, `Scale3`, `WorldTransform`, `TransformDirty`).
    *   *Rationale*: Ensures new entities don't default to (0,0,0) without the ability to move.

#### File: `ECS/Components/TransformComponents.cs`
*   **Struct**: `public struct WorldTransform : IComponent`
    *   `Matrix4x4 Value`.
*   **Struct**: `public struct TransformDirty : ITag { }`
    *   *Note*: Used to flag entities needing update.

#### File: `ECS/Systems/TransformSystem.cs`
*   **Class**: `public class TransformSystem : QuerySystem`
*   **Role**: Implements the hierarchy traversal.
*   **Requirements**:
    *   **Query**: Entities with `WorldTransform` and **without** `Parent` (Roots).
    *   **Method `OnUpdate()`**:
        *   Iterate Roots.
        *   Call `ProcessNode(entity, Matrix4x4.Identity, false)`.
    *   **Method `ProcessNode(Entity entity, Matrix parentMatrix, bool parentDirty)`**:
        1.  `bool isSelfDirty = entity.HasTag<TransformDirty>()`.
        2.  `bool needsUpdate = parentDirty || isSelfDirty`.
        3.  If `needsUpdate`:
            *   **Read Native**: `var pos = entity.Position; var rot = entity.Rotation; var scale = entity.Scale3;`
            *   Calculate `localMatrix = Matrix.CreateScale(scale) * Matrix.CreateFromQuaternion(rot) * Matrix.CreateTranslation(pos)`.
            *   `worldMatrix = localMatrix * parentMatrix`.
            *   `entity.GetComponent<WorldTransform>().Value = worldMatrix`.
            *   `entity.RemoveTag<TransformDirty>()`.
        4.  **Recurse**: `foreach (var child in entity.Children) ProcessNode(child, worldMatrix, needsUpdate)`.

### 5.3. Module: Ignis.Samples

#### Class: `SampleGame`
*   **Requirements**:
    *   **Setup**:
        *   Create `Root` using `Archetypes.GameObject`.
        *   Create `Child` using `Archetypes.GameObject`. `Root.AddChild(Child)`.
        *   Set `Child.Position = new Vector3(10, 0, 0)`.
    *   **Update**:
        *   Modify `Root.Rotation` every frame.
    *   **Verification**:
        *   Observe `Child` orbiting via its `WorldTransform` matrix.

---

## 6. Testing Strategy

### Unit Tests (`Ignis.Tests`)
*   **Event Reactivity**:
    1.  Create entity `A` (GameObject).
    2.  Assert `A` has `TransformDirty` (initial state).
    3.  Run `TransformSystem` (should clear dirty tag).
    4.  Set `A.Position = new Vector3(1,1,1)`.
    5.  Assert `A` has `TransformDirty` tag again (proving event listener works).
*   **Hierarchy Propagation**:
    1.  Create chain `Root -> Child`.
    2.  Move `Root`.
    3.  Run System.
    4.  Assert `Child.WorldTransform` contains the translation.

### Visual Verification
*   **Solar System**: Run `Ignis.Samples`. Create a "Sun", "Earth", and "Moon" hierarchy. Rotating the Sun should carry the Earth, and rotating the Earth should carry the Moon, confirming matrix multiplication order and recursion.

## 7. Roadmap
1.  **Phase 1**: Core Skeleton & Headless Setup.
2.  **Phase 2**: **Test Suite** (Focus on Event listeners and Dirty Flag logic).
3.  **Phase 3**: **Transform System** (Recursive calculation using built-in components).
4.  **Phase 4**: **3D Rendering** (Mesh/Shader).
5.  **Phase 5**: **UI Overlay**.