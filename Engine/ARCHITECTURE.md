Here is the updated **Ignis Engine Architecture & Implementation Specification**.

Changes from the previous version:
1.  **Project Layout**: Added specific files for the new hierarchy and transform logic (`TransformComponents.cs`, `TransformSystem.cs`).
2.  **Architecture Modules**: Added a **"Scene Graph & Transform System"** section explaining the "Dirty Flag" + "Recursive Propagation" strategy.
3.  **Phase 3**: Detailed the implementation of `TransformSystem` to handle parent-child matrix concatenation efficiently.

***

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
│   │   ├── Components/
│   │   │   ├── ComponentTypes.cs       # (General Registry)
│   │   │   └── TransformComponents.cs  # LocalTransform, WorldTransform, DirtyTag
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

### 4.2. Scene Graph & Transform System (New)
To support 3D hierarchy efficiently, we employ a **Recursive Dirty Propagation** strategy.
*   **Goal**: Update `WorldMatrix` only when necessary, respecting parent-child relationships.
*   **Data**:
    *   **`LocalTransform`**: Relative Position (Vec3), Rotation (Quat), Scale (Vec3). Edited by gameplay code.
    *   **`WorldTransform`**: The calculated Matrix4x4 used for rendering. Read-only.
    *   **`TransformDirty` (Tag)**: A tag added automatically when `LocalTransform` is modified.
*   **Logic (`TransformSystem`)**:
    1.  **Identify Roots**: Query all entities that have a Transform but *no* Parent.
    2.  **Parallel Recursion**: Iterate over Roots. For each Root, call `UpdateRecursive(entity, parentMatrix, parentIsDirty)`.
    3.  **Optimization**:
        *   If `parentIsDirty` is `true`, we **must** recalculate the child, even if the child isn't dirty.
        *   If `parentIsDirty` is `false` AND `child.IsDirty` is `false`, **skip** calculation and recurse to grandchildren.
        *   This avoids O(N) matrix multiplications for static objects.

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
    *   **Constructor**: Instantiates World and Systems.
    *   **Method `Update(double dt)`**: Executing `SimulationRoot`.

#### Class: `IgnisGame`
*   **File**: `Ignis.Engine/Core/IgnisGame.cs`
*   **Requirements**:
    *   Standard MonoGame setup (`GraphicsDeviceManager`).
    *   **Draw Logic**:
        1.  Reset Render State (Depth/Blend).
        2.  Call `OnRender3D`.
        3.  Call `OnRenderUI` (SpriteBatch).

### 5.2. Module: Ignis.Engine.ECS

#### File: `ECS/Components/TransformComponents.cs`
*   **Struct**: `public struct LocalTransform : IComponent`
    *   `Vector3 Position`, `Quaternion Rotation`, `Vector3 Scale`.
    *   *Helper*: `static LocalTransform Default => ...`
*   **Struct**: `public struct WorldTransform : IComponent`
    *   `Matrix4x4 Value`.
*   **Struct**: `public struct TransformDirty : ITag { }`
    *   *Note*: Used to flag entities needing update.

#### File: `ECS/Systems/TransformSystem.cs`
*   **Class**: `public class TransformSystem : QuerySystem`
*   **Role**: Implements the hierarchy traversal.
*   **Requirements**:
    *   **Query**: Entities with `LocalTransform` and **without** `Parent` (Roots).
    *   **Method `OnUpdate()`**:
        *   Iterate Roots.
        *   Call `ProcessNode(entity, Matrix4x4.Identity, false)`.
    *   **Method `ProcessNode(Entity entity, Matrix parentMatrix, bool parentDirty)`**:
        1.  Check if `entity` has `TransformDirty` tag.
        2.  `bool needsUpdate = parentDirty || isSelfDirty`.
        3.  If `needsUpdate`:
            *   Calculate `localMatrix = Matrix.Create...`
            *   `worldMatrix = localMatrix * parentMatrix`
            *   `entity.SetComponent(new WorldTransform { Value = worldMatrix })`
            *   `entity.RemoveTag<TransformDirty>()`
        4.  **Recurse**: `foreach (var child in entity.Children) ProcessNode(child, worldMatrix, needsUpdate)`.

### 5.3. Module: Ignis.Samples

#### Class: `SampleGame`
*   **Requirements**:
    *   **Setup**:
        *   Create `Root` entity (Position: 0,0,0).
        *   Create `Child` entity (Position: 10,0,0). `Root.AddChild(Child)`.
        *   Create `GrandChild` entity (Position: 0,5,0). `Child.AddChild(GrandChild)`.
    *   **Update**:
        *   Rotate `Root` entity every frame.
    *   **Verification**:
        *   Verify `Child` and `GrandChild` World Matrices are moving in circles, proving the hierarchy system works.

---

## 6. Testing Strategy

### Unit Tests (`Ignis.Tests`)
*   **Hierarchy Logic**:
    1.  Create `A` (Root), `B` (Child).
    2.  Move `A`.
    3.  Run `TransformSystem`.
    4.  Assert `B.WorldTransform` has changed to match `A`'s offset.
*   **Optimization Check**:
    1.  Run a frame where nothing moves.
    2.  Assert `WorldTransform` values are unchanged and no expensive matrix math logs occurred (if logging enabled).

### Visual Verification
*   **Planetary System**: Run `Ignis.Samples` with a "Sun, Earth, Moon" setup. Visual confirmation that the Moon orbits the Earth while the Earth orbits the Sun.

## 7. Roadmap
1.  **Phase 1**: Core Skeleton & Headless Setup.
2.  **Phase 2**: **Test Suite** (Hierarchy Logic verification).
3.  **Phase 3**: **Transform System** (Implementation of the Recursive Dirty Propagation).
4.  **Phase 4**: **3D Rendering** (Mesh/Shader).
5.  **Phase 5**: **UI Overlay**.