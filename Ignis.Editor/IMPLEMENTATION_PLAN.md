# **Project Plan: Ignis Editor (Reactive Architecture & Roadmap)**

## **1. Executive Summary**

This document outlines the architectural plan for the **Ignis Editor**, the primary content creation tool for the Ignis Engine.
The architecture adapts the traditional Model-View-ViewModel (MVVM) pattern into a **Reactive-Data-Oriented** approach. It leverages the engine's native `Signal<T>` system to bridge the gap between the `Friflo.Engine.ECS` data (Model) and the `Ignis.Engine.UI` (View), ensuring zero-latency updates and loose coupling.

## **2. High-Level Architecture (The Reactive Bridge)**

### **2.1. The Model (Ignis.Engine.Core)**

The Model is the headless engine core (`IgnisApp`) and the ECS data.

*   **Components:** `IgnisApp`, `EntityStore` (Friflo), `SystemRoot`.
*   **Responsibilities:**
  *   Storing the Scene Graph (Entities and Components like `Position`, `MeshComponent`).
  *   Running the Simulation Loop (`TransformSystem`, `RenderSystem`).
  *   Asset loading via `AssetManager`.
*   **Constraint:** The Model runs blindly; it does not know if it is running a game or being edited.

### **2.2. The Bridge (Replacing the ViewModel)**

Instead of traditional ViewModels, Ignis uses **Reactive Bridges** located in `Ignis.Engine.ECS.Bridge`. This layer binds ECS data to UI Signals.

*   **Responsibilities:**
  *   **Reactive Queries:** Using `ReactiveQuery` (wraps `ArchetypeQuery`) to provide auto-updating `SignalList<Entity>` for the Hierarchy.
  *   **Component Signals:** Using `ComponentSignal<T>` to bind a specific ECS Component field (e.g., `Position.value`) to a UI `Signal<Vector3>`.
  *   **Lenses:** Using `.Lens()` to break complex structs (like `Vector3`) into atomic float signals for UI sliders.
  *   **Selection State:** A global `Signal<Entity?>` managed by the Editor shell.

### **2.3. The View (Ignis.Engine.UI)**

The visual layer built with `ViewComponent` and `Primitives`.

*   **Components:** `UIContext`, `Window`, `Panel`, `Splitter`.
*   **Responsibilities:**
  *   Declarative UI construction using the Fluent API (e.g., `Elements.Column(...)`).
  *   Rendering via `PrimitiveBatch` (shapes) and `SpriteBatch` (text/icons).
  *   Input handling via `InputManager` and `EventHandlers`.
  *   **The Viewport:** A specific `Panel` where the `RenderSystem` draws the 3D scene.

## **3. Core Subsystems & Modules**

### **3.1. The Editor Shell (IgnisGame)**

The container class (inheriting `IgnisGame`) that manages the editor lifecycle.

*   **UI Context:** Manages the `UIContext` and the root layout (likely a `Splitter` configuration).
*   **Editor Loop:** Overrides `Update` to step the `IgnisApp` (Model) and `UIContext` (View).
*   **Input Arbitration:** Distinguishes between UI input (clicking a button) and Viewport input (orbiting the camera).

### **3.2. The Viewport Module**

The bridge between MonoGame rendering and the UI layout.

*   **View:** A `Panel` reserved for 3D rendering.
*   **Logic:**
  *   **Render Pass:** In `IgnisGame.Draw`, the `RenderSystem` is invoked. The `GraphicsDevice.Viewport` is set to match the screen rectangle of the Viewport Panel calculated by the layout engine.
  *   **Editor Camera:** A unique `Entity` with `CameraComponent` used only by the editor, separate from game cameras.
  *   **Gizmos:** A custom render pass using `PrimitiveBatch` to draw 3D lines/arrows on top of the scene for manipulation.

### **3.3. The Scene Hierarchy Module**

*   **View:** The `Hierarchy<T>` widget (already prototyped in Samples).
*   **Bridge:** Connects a `ReactiveQuery` (tracking all entities) to the UI.
*   **Interaction:**
  *   Clicking a node updates the global `_selectedEntity` Signal.
  *   Drag-and-Drop uses the `InputManager` drag events to reparent entities in the `EntityStore`.

### **3.4. The Inspector Module (Property Grid)**

The dynamic editing system.

*   **View:** A `PropertyGrid` widget containing `Vector3Field`, `NumberField`, `Checkbox`, etc.
*   **Logic:**
  *   Listens to `_selectedEntity`.
  *   **Reflection Strategy:** When selection changes, iterate `entity.GetComponents()`.
  *   **Binding:** For each component, create `ComponentSignal<T>` wrappers.
  *   **Factory:** Map types to widgets (e.g., `System.Numerics.Vector3` $\rightarrow$ `Vector3Field`).

### **3.5. The Asset Browser Module**

*   **View:** The `AssetBrowser<T>` widget.
*   **Logic:**
  *   Uses `System.IO.FileSystemWatcher` to track the "Content" directory.
  *   Updates a `SignalList<FileInfo>`.
  *   Uses `AssetManager` to generate thumbnails for recognized types (Textures).
  *   Enables `Draggable` on items, with the payload being the file path.

## **4. Cross-Cutting Concerns**

### **4.1. The Command System (Undo/Redo)**

Since Signals allow direct modification, we need an interception layer for Undo.

*   **Implementation:**
  *   Create a `CommandHistory` class.
  *   Wrap writes to `ComponentSignal` in a command: `SetComponentCommand<T>(Entity e, T oldValue, T newValue)`.
  *   The UI widgets should accept an `Action<T> onCommit` or similar to push these commands, rather than setting the Signal directly if Undo is desired.

### **4.2. Embedded Hosting (Single Process)**

Unlike complex IPC architectures, Ignis runs as a single .NET process.

*   **Architecture:** The Editor *is* the Game, but with extra UI overlays.
*   **Play Mode:** When "Play" is clicked, the Editor creates a snapshot of the `EntityStore`. When stopped, it restores that snapshot to revert changes made by game logic.

### **4.3. Dirty State & Serialization**

*   **Serialization:** Use Friflo's built-in JSON serialization to save the `EntityStore` to scene files.
*   **Dirty Flags:** The `ComponentSignal` already tracks changes. The Editor Shell listens to these changes to mark the "Scene" as unsaved (adding an `*` to the title bar).

## **5. Development Roadmap**

### **Phase 1: The Layout Framework (Visual)**
*   **Goal:** A solid docking/layout system.
*   **Tasks:**
  *   Refine `Splitter` to support nesting and resizing limits.
  *   Implement `MenuBar` for "File", "Edit", "View".
  *   Create the main Editor Shell layout (Left: Hierarchy, Center: Viewport, Right: Inspector, Bottom: Assets).

### **Phase 2: Data Binding & Selection**
*   **Goal:** Bi-directional link between ECS and UI.
*   **Tasks:**
  *   Finalize `ReactiveQuery` to handle Entity creation/deletion automatically.
  *   Implement the `SelectionSystem` (Global Signal).
  *   Connect `Hierarchy` widget to the `EntityStore` via the bridge.

### **Phase 3: The Reflection Inspector**
*   **Goal:** Edit any component without writing custom UI code for it.
*   **Tasks:**
  *   Create a `ComponentInspectorFactory` that uses Reflection.
  *   Map standard types (`int`, `float`, `bool`, `Vector3`, `Color`) to existing Widgets (`NumberField`, `Checkbox`, `Vector3Field`).
  *   Implement `ComponentSignal` read/write logic for these widgets.

### **Phase 4: Viewport Interaction (Gizmos)**
*   **Goal:** Manipulate objects in 3D.
*   **Tasks:**
  *   Implement `Raycasting` (Screen-to-World) using `Viewport.Unproject`.
  *   Create a `GizmoSystem` that renders axis arrows using `PrimitiveBatch`.
  *   Implement "Pick" logic: Clicking in Viewport $\rightarrow$ Updates Selection Signal $\rightarrow$ Updates Inspector.

### **Phase 5: Asset Integration**
*   **Goal:** Drag-and-drop workflow.
*   **Tasks:**
  *   Implement `AssetBrowser` scanning logic.
  *   Implement Drag-and-Drop handlers on the Viewport (e.g., dragging a `.obj` creates an Entity with `MeshComponent`).

## **6. Technical Stack**

*   **Language:** C# (.NET 8).
*   **Engine Core:** Friflo.Engine.ECS.
*   **Rendering:** MonoGame (DesktopGL).
*   **UI Framework:** Ignis.Engine.UI (Custom Reactive).
*   **Format:** Scenes saved as JSON (via ECS serialization). Assets built via MGCB (MonoGame Content Builder).