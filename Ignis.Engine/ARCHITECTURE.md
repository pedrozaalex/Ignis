# Ignis Engine Architecture & Implementation Specification (3D)

## 1. Project Overview
**Ignis** is a high-performance, data-oriented **3D game engine** built on the .NET ecosystem. It combines the battle-tested rendering and platform capabilities of **MonoGame** with the high-performance data handling of **Friflo.Engine.ECS**.

The engine is designed with a strict separation of concerns to enable **Headless Testing**: logic runs independently of graphics context.

## 2. Technology Stack
*   **Core Framework**: .NET 8.0+
*   **Logic / Data**: [Friflo.Engine.ECS](https://friflo.gitbook.io/friflo.engine.ecs)
*   **Graphics / Platform**: [MonoGame 3.8.1+](https://monogame.net/)
    *   *Usage*: We leverage `Microsoft.Xna.Framework.Graphics.Model` for mesh data and `BasicEffect` for standard lighting/texturing to ensure robust, cross-platform rendering out of the box.
*   **UI**: Custom Native UI Framework (Retained Mode)

---

## 3. Project Layout & File Structure
The solution (`Ignis.sln`) adheres to the following physical directory structure.

```text
Ignis/
├── Ignis.Engine/                       # [Class Library] The Core Runtime
│   ├── Ignis.Engine.csproj
│   ├── Core/
│   │   ├── IgnisApp.cs                 # Headless Core
│   │   └── IgnisGame.cs                # MonoGame Wrapper
│   ├── ECS/
│   │   ├── Archetypes.cs
│   │   ├── Components/                 # Transform components
│   │   └── Systems/                    # TransformSystem
│   ├── Graphics/                       # (Phase 3)
│   │   ├── Components/
│   │   │   ├── MeshComponent.cs        # Holds Model reference
│   │   │   ├── MaterialComponent.cs    # Color, Texture overrides
│   │   │   └── CameraComponent.cs      # FOV, Near/Far planes
│   │   ├── Systems/
│   │   │   ├── RenderSystem.cs         # The Draw Loop
│   │   │   └── CameraSystem.cs         # View/Proj Matrix calculation
│   │   └── Lighting/
│   │       └── LightSettings.cs        # Ambient/Directional structs
│   ├── Input/                          # (Reserved for Phase 5)
│   └── UI/                             # (Reserved for Phase 4)
│
├── Ignis.Samples/                      # [Console App] Sandbox
│   ├── Content/                        # MonoGame Content Builder (.mgcb)
│   └── SampleGame.cs
│
├── Ignis.Tests/                        # [xUnit] Suite
└── Ignis.sln
```

---

## 4. Core Architecture Modules

### 4.1. The "Headless" Core (`IgnisApp`)
Manages the ECS World and Simulation loop.

### 4.2. Scene Graph & Transform System
Handles `Position`, `Rotation`, `Scale` $\to$ `WorldTransform` matrix propagation via reactive dirty flags.

### 4.3. 3D Rendering Pipeline (Phase 3)
The rendering system connects ECS data to the MonoGame Graphics Device.

*   **Philosophy**: The `IgnisApp` (Simulation) knows *nothing* about pixels. The `RenderSystem` (Presentation) reads the ECS state and issues draw calls to the GPU.
*   **Asset Strategy**:
    *   Components do not load files. They hold references to MonoGame `Model` or `Texture2D` objects that were loaded by the Game's `ContentManager`.
*   **Camera Architecture**:
    *   The camera is an Entity with a `CameraComponent` and standard `Transform`.
    *   This allows the camera to be parented to a player, animated via physics, or scripted easily.
*   **Render Loop**:
    1.  **Camera Resolve**: Find the active `CameraComponent`. Calculate `View` and `Projection`.
    2.  **Query**: Fetch all entities with (`MeshComponent` + `WorldTransform`).
    3.  **Draw**: Iterate entities. Apply `World` (from Entity), `View`, `Projection` (from Camera) to the Mesh's `BasicEffect`. Call `ModelMesh.Draw()`.

### 4.4. Custom UI Framework (Phase 4)
2D Overlay system (Retained Mode).

---

## 5. Implementation Specification

### 5.1. Module: Ignis.Engine.Core

#### Class: `IgnisGame`
*   **Integration**:
    *   Holds an instance of `RenderSystem`.
    *   **LoadContent**: Passes the `GraphicsDevice` to `RenderSystem` initialization.
    *   **Draw**: Calls `_renderSystem.Draw(_app.World)`.

### 5.2. Module: Ignis.Engine.Graphics (Phase 3 Requirement)

#### Class: `MeshComponent` (Struct)
*   **Purpose**: Links an entity to a 3D visual.
*   **Data**:
    *   `public Model ModelRef;` (Reference to loaded MonoGame Model)
    *   `public bool CastShadows;`
*   *Note*: Keeping `ModelRef` allows us to leverage MonoGame's pipeline for FBX/GLTF import, bone handling, and sub-meshes without writing a custom parser.

#### Class: `MaterialComponent` (Struct)
*   **Purpose**: Overrides default model properties.
*   **Data**:
    *   `public Color Color;` (Tint)
    *   `public Texture2D Texture;` (Optional override)
    *   `public float SpecularPower;`
    *   `public bool EnableLighting;`

#### Class: `CameraComponent` (Struct)
*   **Purpose**: Defines the "Lens".
*   **Data**:
    *   `public float FieldOfView;` (Default: 60 degrees)
    *   `public float NearPlane;` (Default: 0.1f)
    *   `public float FarPlane;` (Default: 1000f)
    *   `public float AspectRatio;`
    *   `public bool IsActive;` (To switch between cameras)
    *   **Transient Data**: `public Matrix ViewMatrix;`, `public Matrix ProjectionMatrix;` (Calculated by System).

#### Class: `CameraSystem`
*   **Role**: Calculates matrices based on Transform.
*   **Logic**:
    *   Query: Entities with `CameraComponent` + `Position` + `Rotation`.
    *   Loop:
        *   `Target = Position + Vector3.Transform(Vector3.Forward, Rotation)`
        *   `Up = Vector3.Transform(Vector3.Up, Rotation)`
        *   `View = Matrix.CreateLookAt(Position, Target, Up)`
        *   `Projection = Matrix.CreatePerspectiveFieldOfView(FOV, Aspect, Near, Far)`
    *   *Note*: This runs in `IgnisApp.Update` (Simulation) so logic (like "Is object visible?") can use the frustum.

#### Class: `RenderSystem`
*   **Role**: The bridge to GPU.
*   **Dependencies**: `GraphicsDevice`.
*   **Method `Draw(EntityStore world)`**:
    1.  **Find Camera**: Query `CameraComponent`. Find the first with `IsActive == true`. (Fallback to default if none).
    2.  **Global Settings**: Set `GraphicsDevice.DepthStencilState = DepthStencilState.Default`.
    3.  **Query Renderables**: `world.Query<MeshComponent, WorldTransform>()`.
    4.  **Iterate**:
        *   Get `Model` from `MeshComponent`.
        *   Get `World` matrix from `WorldTransform`.
        *   Get `Material` (optional).
        *   **Model Loop**: `foreach (var mesh in model.Meshes)`
            *   `foreach (BasicEffect effect in mesh.Effects)`
                *   `effect.World = mesh.ParentBone.Transform * worldMatrix`
                *   `effect.View = camera.View`
                *   `effect.Projection = camera.Projection`
                *   `effect.EnableDefaultLighting()` (if enabled)
                *   Apply `MaterialComponent` overrides (Color/Texture).
            *   `mesh.Draw()`

### 5.3. Module: Ignis.Samples

#### Class: `SampleGame`
*   **LoadContent**:
    *   `var cubeModel = Content.Load<Model>("Cube");`
*   **OnSetup**:
    *   **Camera**: Create Entity `Cam`. Add `CameraComponent`, `Position` (0, 5, 15), `Rotation` (Look at 0,0,0).
    *   **Object**: Create Entity `Cube`. Add `MeshComponent { ModelRef = cubeModel }`, `WorldTransform`.
*   **Verification**:
    *   Running the sample shows a 3D Cube.
    *   Moving the `Cam` entity in `Update` changes the view.

---

## 6. Testing Strategy

### Unit Tests (`Ignis.Tests`)
*   **Camera Math**:
    *   Create Camera at (0,0,10) looking at (0,0,0).
    *   Run `CameraSystem`.
    *   Assert `ViewMatrix` translation component corresponds to -10 Z.
*   **Missing Asset Handling**:
    *   Create entity with `MeshComponent` but `ModelRef = null`.
    *   (Integration test) Ensure `RenderSystem` checks for null and doesn't crash, perhaps logging a warning.

### Visual Verification
*   **Primitives**: Load a Cube, Sphere, and Torus. Arrange them in a line.
*   **Lighting**: Enable default lighting. Verify faces are shaded differently based on orientation.
*   **Camera**: Rotate the camera entity around the scene.

## 7. Roadmap

1.  **Phase 1**: Core Skeleton & Headless Setup.
2.  **Phase 2**: Test Suite (Hierarchy & Event logic).
3.  **Phase 3 (Rendering)**:
    *   **3.1**: Implement `CameraComponent` and `CameraSystem` (Math logic).
    *   **3.2**: Implement `MeshComponent` and `MaterialComponent`.
    *   **3.3**: Implement `RenderSystem` using MonoGame `Model.Draw`.
    *   **3.4**: Visual Sample: Spinning Cube with Orbiting Camera.
4.  **Phase 4**: UI Framework.
5.  **Phase 5**: Input & Interaction.