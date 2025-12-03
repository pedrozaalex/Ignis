# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Priorities

1. **Terseness**: Write concise code and straightforward code without unnecessary verbosity. DO NOT add frivolous comments or documentation. Comments should only be added when they provide essential context that is not obvious from the code itself.
2. **Clarity**: Ensure code is easy to understand with clear naming and structure.
3. **Maintainability**: Write code that is easy to maintain and extend in the future.
4. **Testability**: Ensure code is testable and includes appropriate tests.

## Project Overview

**Ignis** is a high-performance, data-oriented 3D game engine combining MonoGame rendering with Friflo.Engine.ECS. It features a **Reactive Editor Architecture** using Signals for glitch-free data binding between UI and ECS.

Key technologies:
- **.NET 10.0** - C# with nullable reference types enabled
- **Friflo.Engine.ECS** - Data-oriented entity component system
- **MonoGame 3.8.5** - Cross-platform graphics framework
- **FontStashSharp** - Dynamic TrueType font rendering with optimal scaling
- **Crucible** - Custom reactive library (Signal<T>, Computed<T>, Effect)

## Build and Run Commands

### Building
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build Ignis.Engine/Ignis.Engine.csproj
dotnet build Ignis.Samples/Ignis.Samples.csproj
dotnet build Ignis.Tests/Ignis.Tests.csproj
```

### Running
```bash
# Run samples (interactive menu)
dotnet run --project Ignis.Samples

# Run specific sample directly (modify Program.cs or pass arguments if supported)
cd Ignis.Samples
dotnet run
```

### Testing
```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run specific test
dotnet test --filter "FullyQualifiedName~TestName"
```

## High-Level Architecture

### Two-Tier Application Model

**IgnisApp (Headless Core)**
- Manages the ECS World (`EntityStore`)
- Runs the simulation loop via `SystemRoot`
- Provides `AssetManager` for resource loading
- Provides `InputService` for keyboard/mouse
- Can run without graphics (for tests/headless servers)

**IgnisGame (Visual Wrapper)**
- Inherits from MonoGame's `Game` class
- Wraps an `IgnisApp` instance
- Manages `GraphicsDevice` and rendering
- Provides `FontSystem` for dynamic font loading
- Splits rendering into `OnRender3D()` and `OnRenderUI()` hooks

### Reactive State Management (Crucible)

The engine uses **Signals** for reactive state, not immediate mode or manual event listeners:

- **Signal<T>**: Atomic state container. Reading tracks dependencies, writing notifies observers.
- **Computed<T>**: Derived state that memoizes and updates only when dependencies change.
- **Effect**: Runs side effects when accessed signals change.
- **SignalList<T>**: Observable collection with fine-grained events (Add/Remove/Move).
- **Signal.Lens()**: Creates bidirectional binding to struct fields for editing Vector3, Quaternion, etc.

### ECS-to-UI Bridge

Connecting data-oriented ECS (structs in arrays) to object-oriented UI (Signals):

- **ComponentSignal<T>**: A Signal that wraps an Entity ID, reading/writing components on demand
  - Getter: `entity.GetComponent<T>()`
  - Setter: `entity.AddComponent<T>(value)`
- **ReactiveQuery**: A `SignalList<Entity>` that syncs with ECS queries, firing events when entities enter/leave

### Declarative UI Framework

UI is built as **functions of state** using fluent builders:

```csharp
using static Ignis.Engine.UI.Elements.Elements;

var layout = Panel()
    .Width(Units.Stretch(1))
    .Height(Units.Pixels(400))
    .Padding(10)
    .Children(
        Label("Inspector").FontSize(24),
        Rule(), // Separator
        Button("Save", OnSave)
    );
```

**Control Flow**:
- `Bind.If(condition, trueBuilder, falseBuilder)`: Conditional rendering
- `Bind.For(signalList, itemBuilder)`: Efficient list rendering

**Layout Units**:
- `Units.Pixels(n)`: Absolute size
- `Units.Percentage(n)`: Percentage of parent
- `Units.Stretch(factor)`: Flex-like expansion
- `Units.Auto`: Size to content

### Hybrid Rendering Architecture

**PrimitiveBatch** (Low-level shape rendering):
- GPU-accelerated using dynamic vertex/index buffers
- API: `DrawFilledRectangle`, `DrawBorder`, `DrawLine`, `DrawCircle`, `DrawRoundedRectangle`
- Used for panels, borders, sliders, progress bars

**SpriteBatch** (Text & texture rendering):
- Standard MonoGame rendering for text and images
- Handles FontStashSharp font rendering

**Draw Loop**:
1. Calculate layout via `LayoutEngine.Layout()`
2. Start both batches: `PrimitiveBatch.Begin()` and `SpriteBatch.Begin()`
3. Traverse view tree once, widgets use appropriate batch
4. End both batches: `spriteBatch.End()`, `PrimitiveBatch.End()`

### Font Rendering

Uses **FontStashSharp** instead of MonoGame's content pipeline:

- Fonts loaded from TrueType files at runtime (no MGCB build step)
- `FontSystem.GetFont(size)` creates fonts at any size on-demand
- Configured with optimal scaling parameters:
  - `FontResolutionFactor = 2.0f` for crisp scaling
  - `KernelWidth/Height = 2` for enhanced anti-aliasing
- All font parameters use `SpriteFontBase?` (not `SpriteFont?`)
- Default font automatically loaded in `IgnisGame.LoadContent()`

### Theme System

Centralized color palette via `Theme` record:

```csharp
// Properties: PrimaryColor, BackgroundColor, SurfaceColor, BorderColor, TextColor, DefaultFont
context.Theme = Theme.Dark;  // or Theme.Light
```

Widgets use nullable colors that fall back to theme colors:
```csharp
var panel = new Panel { BackgroundColor = null }; // Uses Theme.SurfaceColor
var panel2 = new Panel { BackgroundColor = Color.Red }; // Explicit override
```

## Project Structure

```
Ignis/
├── Ignis.Engine/          # Core engine library
│   ├── Core/              # IgnisApp, IgnisGame, EngineSettings
│   ├── ECS/               # Components, Systems, Archetypes
│   │   └── Bridge/        # ComponentSignal, ReactiveQuery
│   ├── Reactive/          # Signal, Computed, Effect, SignalList
│   ├── UI/                # Declarative UI framework
│   │   ├── Core/          # IView, UIContext, Bind
│   │   ├── Elements/      # Builder API (Row, Column, Label, Button)
│   │   └── Widgets/       # High-level widgets (Panel, Splitter, Hierarchy)
│   ├── Graphics/          # RenderSystem, CameraSystem, Components
│   ├── Input/             # InputService, IInputProvider
│   └── Assets/            # AssetManager, DefaultFontProvider
├── Ignis.Samples/         # Sample applications
│   └── Program.cs         # Interactive sample launcher
└── Ignis.Tests/           # xUnit test suite
```

## Development Guidelines

### Code Style (from .github/copilot-instructions.md)

- **DO NOT** create markdown docs for every change
- **DO** write clear code with descriptive names
- **DO** include brief comments where additional context helps
- **DO** update ARCHITECTURE.md for significant architectural changes

### Creating a New Game/Editor

1. Inherit from `IgnisGame`:
```csharp
public class MyGame : IgnisGame
{
    private UIContext? _uiContext;

    public MyGame() : base(new EngineSettings
    {
        WindowTitle = "My Game",
        WindowWidth = 1920,
        WindowHeight = 1080
    }) { }

    protected override void Initialize()
    {
        base.Initialize();
        _uiContext = new UIContext(GraphicsDevice, App.Input);
        if (DefaultFont != null)
            _uiContext.SetDefaultFont(DefaultFont);
        _uiContext.SetRoot(BuildUI());
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _uiContext?.Update(gameTime);
    }

    protected override void OnRenderUI(SpriteBatch spriteBatch)
    {
        _uiContext?.Draw(spriteBatch);
    }
}
```

2. Add systems to `App.SimulationRoot` in `Initialize()` or `LoadContent()`
3. Build UI declaratively using `Elements` builders
4. Use Signals for reactive state management

### Binding UI to ECS

```csharp
// Get entity
Entity entity = App.World.GetEntityById(id);

// Create component signal
var posSignal = entity.ComponentSignal<Position>();

// Create lens for editing Vector3.X
var xSignal = posSignal.Lens(
    p => p.value.X,
    (p, x) => new Position(new Vector3(x, p.value.Y, p.value.Z))
);

// Bind to UI
var editor = new Vector3Field("Position", posSignal.Lens(
    p => p.value,
    (p, v) => new Position(v)
));
```

### Working with Lists

```csharp
// Reactive list
var items = new SignalList<string>();

// Bind to UI
var listView = Bind.For(items, item =>
    Label(item).OnClick(() => SelectItem(item))
);

// Modifications automatically update UI
items.Add("New Item");
items.Remove(items[0]);
```

### Keyboard Shortcuts

```csharp
mainPanel.Shortcuts(s => s
    .Bind("Ctrl+S", SaveFile)
    .Bind("Ctrl+Z", Undo)
    .Bind("Delete", DeleteSelection)
);
```

## Common Patterns

### Editor Inspector Pattern
```csharp
var selectedEntity = new Signal<Entity?>(null);

var inspector = Bind.If(
    selectedEntity.Map(e => e != null),
    () => {
        var entity = selectedEntity.Value!;
        return Column(
            new Vector3Field("Position", entity.ComponentSignal<Position>()),
            new NumberField<float>("Scale", entity.ComponentSignal<Scale>())
        );
    },
    () => Label("No selection")
);
```

### Hierarchy Panel Pattern
```csharp
var sceneNodes = new SignalList<TreeNode<string>>();
var selectedNode = new Signal<string?>(null);

var hierarchy = new Hierarchy<string>(
    sceneNodes,
    node => node,  // Display name getter
    selectedNode   // Selection signal
)
{
    Layout = { Height = Units.Stretch(1) }
};
```

### Splitter Layout Pattern
```csharp
var mainLayout = new Splitter(
    leftPanel,   // Hierarchy
    new Splitter(
        centerPanel,  // Viewport
        rightPanel,   // Inspector
        isVertical: false
    ) { SplitRatio = 0.7f },
    isVertical: false
) { SplitRatio = 0.2f };
```

## Testing Approach

- **Reactive Logic**: Test Signal propagation and Computed memoization
- **ECS Bridge**: Test ComponentSignal reads/writes sync with entity components
- **Visual Verification**: Use sample games to verify UI behavior

See USAGE_GUIDE.md and ARCHITECTURE.md for detailed documentation on specific systems.
