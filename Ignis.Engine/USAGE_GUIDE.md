Here is the `USAGE_GUIDE.md` tailored for building an editor using the Ignis Engine.

***

# Ignis Engine: Editor Development Guide

This guide documents the core systems and APIs required to build tools and editors using the Ignis Engine. The engine uses a **Reactive UI** paradigm (Signals & Effects) tightly integrated with a **Data-Oriented ECS** (Friflo).

## 1. Core Application Structure

To build an editor, you inherit from `IgnisGame`. This wraps the MonoGame `GraphicsDevice` and the headless `IgnisApp` core.

```csharp
using Ignis.Engine.Core;

public class MyEditor : IgnisGame
{
    private UIContext _uiContext;

    public MyEditor() : base(new IgnisApp(new EngineSettings {
        WindowTitle = "My Game Editor",
        WindowWidth = 1920,
        WindowHeight = 1080,
        VSync = true
    })) { }

    protected override void Initialize()
    {
        base.Initialize();
        
        // 1. Initialize UI Context
        _uiContext = new UIContext(GraphicsDevice, App.Input);
        _uiContext.SetGame(this); // Links FontSystem
        
        // 2. Build and Set Root UI
        var root = BuildEditorLayout();
        _uiContext.SetRoot(root);
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _uiContext.Update(gameTime); // Critical: Updates layout and input
    }

    protected override void OnRenderUI(SpriteBatch spriteBatch)
    {
        _uiContext.Draw(spriteBatch); // Renders the UI tree
    }
}
```

## 2. Reactive State Management

The UI relies on **Signals** rather than immediate mode rendering or event listeners. When a Signal changes, any UI component reading it updates automatically.

### Key Classes (`Ignis.Engine.Reactive`)

| Class | Description |
| :--- | :--- |
| `Signal<T>` | The atom of state. Stores a value and notifies observers on change. |
| `Computed<T>` | Derived state. Automatically re-calculates when dependencies change. |
| `SignalList<T>` | An observable list. Fires fine-grained events (Add/Remove/Move). |
| `Effect` | Runs an Action whenever accessed signals change. Used for side effects. |

### Creating 2-Way Bindings (Lenses)
For complex structs (like `Vector3` or ECS Components), use `.Lens()` to create a specific signal for a field.

```csharp
// Source of truth
Signal<Vector3> position = new(new Vector3(10, 0, 0));

// Create a lens for just the X component
// Getter: v => v.X
// Setter: (v, x) => new Vector3(x, v.Y, v.Z)
Signal<float> xSignal = position.Lens(v => v.X, (v, x) => v with { X = x });

// If UI updates xSignal, 'position' is updated automatically.
```

## 3. Building the UI

The UI is built declaratively using the `Ignis.Engine.UI.Elements` and `Ignis.Engine.UI.Widgets` namespaces.

### Layout Primitives
Use the static `Elements` class for fluent construction.

```csharp
using static Ignis.Engine.UI.Elements.Elements;

// Fluent API example
var layout = Panel()
    .Width(Units.Stretch(1))  // Flex grow
    .Height(Units.Stretch(1))
    .Padding(10)
    .Children(
        // Header
        Label("Inspector").FontSize(24),
        Rule(), // Separator line
        
        // Content
        ScrollView(
            Column(
                Button("Save", () => SaveProject()),
                Checkbox("Is Visible", _isVisibleSignal)
            ).Gap(5)
        )
    );
```

### Layout Units
*   `Units.Pixels(n)`: Absolute size.
*   `Units.Percentage(n)`: % of parent size.
*   `Units.Stretch(factor)`: Flex-like expansion (shares remaining space).
*   `Units.Auto`: Size to content.

### High-Level Editor Widgets
These are pre-built widgets specifically for editor workflows.

| Widget | Usage |
| :--- | :--- |
| `Window` | A draggable, floating panel with a title bar. |
| `Splitter` | Resizable divider between two views. Essential for Layouts (Hierarchy vs Viewport). |
| `PropertyGrid` | Container for labeled editors. |
| `Vector3Field` | Edits X, Y, Z floats simultaneously. |
| `NumberField<T>` | Draggable numeric input with buttons. |
| `Hierarchy<T>` | Tree view for Scene Graph. |
| `MenuBar` | Top-level application menu (File, Edit, etc). |

**Example: Building an Inspector Panel**
```csharp
var position = new Signal<Vector3>(Vector3.Zero);

var inspector = Panel()
    .Background(Color.DarkGray)
    .Children(
        new Vector3Field("Position", position),
        new NumberField<float>("Opacity", _opacitySignal),
        new Checkbox("Active", _activeSignal)
    );
```

## 4. ECS Integration (The Bridge)

To make an editor, you must bind UI Signals to ECS Components. Use `Ignis.Engine.ECS.Bridge`.

### ComponentSignal
Wraps an ECS Component property into a `Signal<T>`.

```csharp
// Get an entity from the ECS World
Entity entity = App.World.GetEntityById(selectedId);

// Create a bridge signal
// Note: This creates a Read/Write binding to the Entity's Position component
var posSignal = entity.ComponentSignal<Position>();

// Create a Vector3Field bound to that component
// The UI now edits the ECS directly
var transformEditor = new Vector3Field("Transform", 
    posSignal.Lens(p => p.value, (p, v) => new Position(v.X, v.Y, v.Z))
);
```

### Reactive Queries
To display a Hierarchy or Entity list, use `ReactiveQuery`. It acts as a `SignalList` that syncs with ECS Archetypes.

```csharp
// Query all entities with a NameComponent
var query = App.World.Query<NameComponent>();
var reactiveQuery = new ReactiveQuery(query);

// Bind to UI
var list = Bind.For(reactiveQuery, entity => {
    var name = entity.GetComponent<NameComponent>().Name;
    return Label(name).OnClick(() => SelectEntity(entity));
});
```

## 5. Input & Interactivity

The `InputManager` handles bubbling events, focus, and drag-and-drop.

### Shortcuts
Attach keyboard shortcuts to any container. They bubble up until handled.

```csharp
mainPanel.Shortcuts(s => s
    .Bind("Ctrl+S", () => SaveScene())
    .Bind("Ctrl+Z", () => UndoSystem.Undo())
    .Bind("Delete", () => DeleteSelection())
);
```

### Drag and Drop
Used for dragging assets into the scene or reordering hierarchy.

```csharp
// Draggable Source (e.g., Asset in Browser)
assetView.Draggable(payload: assetPath);

// Drop Target (e.g., Viewport)
viewportPanel.OnDragOver(evt => {
    // Visualize drop potential
    evt.Accept(); 
}).OnDrop(evt => {
    if (evt.Payload is string path) {
        InstantiatePrefab(path);
    }
});
```

## 6. Graphics & Viewport

The editor usually needs to render the 3D scene inside a UI panel.

1.  **RenderSystem**: Use `App.SimulationRoot` and `RenderSystem` to draw the 3D world.
2.  **Camera**: Ensure an editor camera exists in the ECS (`CameraComponent`).
3.  **UI Integration**: The `RenderSystem` draws to the backbuffer by default. In `IgnisGame.Draw`, call `OnRender3D` before `OnRenderUI`.

## 7. Asset Management

Use `AssetManager` to load resources for the editor (icons) or the game.

```csharp
// Loading an icon for the UI
var iconHandle = App.AssetManager.Load<Texture2D>("Icons/save.png");
var btn = new Button("Save").Icon(iconHandle.Asset);

// Async loading for heavy assets
var modelHandle = await App.AssetManager.LoadAsync<Model>("Models/Hero.xnb");
```

## Summary Checklist for Editor Creation

1.  **Setup**: Inherit `IgnisGame`, init `UIContext`.
2.  **Layout**: Use `Splitter` to create (Hierarchy | Viewport | Inspector).
3.  **Hierarchy**: Use `ReactiveQuery` to list entities.
4.  **Selection**: Create a `Signal<Entity?> _selectedEntity`.
5.  **Inspector**: Use `Bind.If(_selectedEntity, ...)` to show properties.
6.  **Properties**: Use `ComponentSignal` + `Lens` to bind UI widgets to selected entity components.
7.  **Input**: Add `Shortcuts` for productivity.