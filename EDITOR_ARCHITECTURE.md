# Ignis Editor - Master Design & Implementation Guide

**Target Audience**: Junior to Mid-level Engineers
**Goal**: Build a functional Game Editor for the Ignis Engine using `CrucibleUI`.

---

## 1. Executive Summary

We are building **Ignis.Editor**, a standalone application that allows users to:
1.  **View** the game scene in a window (Viewport).
2.  **Inspect** the Entity Component System (Hierarchy).
3.  **Edit** component properties (Inspector).
4.  **Manage** assets (Asset Browser).

**Key Architecture Decision**: "The Editor is a Game".
The editor itself is an Ignis application. It has an `EngineLoop`, a `Window`, and uses `Ignis.Graphics` to render. The "Game" being edited is rendered into a texture (Frame Buffer) and displayed as a UI Widget.

---

## 2. Project Initialization (Step-by-Step)

### 2.1. Create the Project
Run these commands in your terminal at the root of the repository:

```powershell
# 1. Create the project
dotnet new console -n Ignis.Editor

# 2. Add it to the solution
dotnet sln add Ignis.Editor/Ignis.Editor.csproj

# 3. Add references to core libraries
dotnet add Ignis.Editor/Ignis.Editor.csproj reference Ignis.Core/Ignis.Core.csproj
dotnet add Ignis.Editor/Ignis.Editor.csproj reference Ignis.Graphics/Ignis.Graphics.csproj
dotnet add Ignis.Editor/Ignis.Editor.csproj reference Ignis.Graphics.Backends.OpenGL/Ignis.Graphics.Backends.OpenGL.csproj
dotnet add Ignis.Editor/Ignis.Editor.csproj reference Ignis.Physics/Ignis.Physics.csproj
dotnet add Ignis.Editor/Ignis.Editor.csproj reference CrucibleUI/CrucibleUI.csproj
# If CrucibleUI.Widgets is separate:
# dotnet add Ignis.Editor/Ignis.Editor.csproj reference CrucibleUI.Widgets/CrucibleUI.Widgets.csproj
```

### 2.2. Folder Structure
Create these folders inside `Ignis.Editor/`:
```
Ignis.Editor/
├── Core/           # App entry, Loop, Global Context
├── Rendering/      # UI Rendering Logic
├── Input/          # Input Mapping
├── Styling/        # Themes and Color Definitions
├── Widgets/        # Editor-specific Widgets
│   ├── Common/     # (TextInput, Splitter, etc.)
│   ├── Panels/     # (Inspector, Hierarchy)
│   └── Viewport/   # (Game View)
└── Commands/       # Undo/Redo System
└── Services/       # (Selection, AssetDatabase)
```

---

## 3. Phase 1: The Application Shell

**Goal**: Get a window on screen with a colored background and a basic UI panel.

### 3.1. The Editor Application (`Core/EditorApp.cs`)
This class replaces the standard `Program.cs` logic. It owns the Window and the Loop.

```csharp
using Ignis.Core;
using Ignis.Graphics;
using Ignis.Graphics.Backends.OpenGL;
using CrucibleUI;
using CrucibleUI.Widgets;
using Ignis.Editor.Styling; // For Theme

namespace Ignis.Editor.Core;

public class EditorApp : IDisposable
{
    public Window Window { get; }
    public EngineLoop Loop { get; }
    public IRenderingServer Server { get; }
    
    // UI State
    private Widget _root;
    private WidgetInputHandler _uiInput;
    private CrucibleRenderer _uiRenderer;
    private WidgetCache _layoutCache;
    private FontHandle _uiFont;

    public EditorApp()
    {
        // 1. Init Engine
        Window = new Window("Ignis Editor", 1600, 900);
        Server = new OpenGLRenderingServer();
        Server.Initialize(Window.Handle, 1600, 900);
        
        // 2. Load Resources
        // TODO: Replace with actual path to a TTF file
        _uiFont = Server.CreateFontFromFile("Assets/Fonts/SegoeUI.ttf");

        // 3. Init UI
        _root = new Panel()
            .Width(Units.Stretch(1))
            .Height(Units.Stretch(1))
            .Background(ThemeColor.Background); // Uses Theme System
            
        _uiInput = new WidgetInputHandler(_root);
        _uiRenderer = new CrucibleRenderer(Server, _uiFont);
        _layoutCache = new WidgetCache();

        // 4. Init Loop
        Loop = new EngineLoop();
        Loop.OnUpdate += Update;
        Loop.OnRender += Render;
        
        // 5. Handle Resize
        Window.Resize += (w, h) => Server.Resize(w, h);
        
        // 6. Hook Input (See Phase 2)
        HookInputEvents();
    }

    public void Run() => Window.Run(Loop);

    private void HookInputEvents()
    {
        // Direct hook into Silk.NET input via Window (requires exposing InputContext from Window or InputState)
        // Assuming we extend InputState to expose events:
        Window.InputState.OnKeyDown += key => _uiInput.HandleKeyDown(key);
        Window.InputState.OnTextInput += c => _uiInput.HandleTextInput(c);
    }

    private void Update(float dt)
    {
        // Mouse Input
        var input = Window.InputState;
        _uiInput.HandleMouseMove(input.MousePosition.X, input.MousePosition.Y);
        
        if (input.IsMousePressed(MouseButton.Left)) 
            _uiInput.HandleMouseDown(input.MousePosition.X, input.MousePosition.Y);
            
        if (input.IsMouseReleased(MouseButton.Left)) 
            _uiInput.HandleMouseUp(input.MousePosition.X, input.MousePosition.Y);

        // Layout Pass
        WidgetSubLayout sub = default;
        LayoutEngine.Compute(_root, _layoutCache, _root, ref sub, Window.Width, Window.Height);
    }

    private void Render(float alpha)
    {
        var cmd = Server.CreateCommandList();
        cmd.BeginPass(new RenderPass { ClearColor = Color4.Black });
        
        // Render UI
        _uiRenderer.Render(_root, cmd);
        
        cmd.EndPass();
        Server.Submit(cmd);
        Server.Present();
    }

    public void Dispose()
    {
        Server.DestroyFont(_uiFont);
        Server.Dispose();
        Window.Dispose();
    }
}
```

### 3.2. The UI Renderer (`Rendering/CrucibleRenderer.cs`)
Handles rendering of Widgets, including Text and Theme resolution.

```csharp
using System.Numerics;
using Ignis.Graphics;
using CrucibleUI.Widgets;
using Ignis.Editor.Styling;

namespace Ignis.Editor.Rendering;

public class CrucibleRenderer
{
    private readonly IRenderingServer _server;
    private readonly FontHandle _defaultFont;

    public CrucibleRenderer(IRenderingServer server, FontHandle defaultFont)
    {
        _server = server;
        _defaultFont = defaultFont;
    }

    public void Render(Widget root, IRenderCommandList cmd)
    {
        RenderRecursive(root, cmd);
    }

    private void RenderRecursive(Widget widget, IRenderCommandList cmd)
    {
        if (!widget.IsVisible) return;

        // 1. Get Computed Bounds
        var x = widget.ComputedX;
        var y = widget.ComputedY;
        var w = widget.ComputedWidth;
        var h = widget.ComputedHeight;
        var rect = new Rect(x, y, w, h);

        // 2. Resolve Colors
        var bgColor = ResolveColor(widget, widget.BackgroundColor);
        var borderColor = ResolveColor(widget, widget.BorderColorValue);

        // 3. Draw Background
        if (bgColor.A > 0)
        {
            if (widget.CornerRadiusValue > 0)
                cmd.DrawRoundedQuad(rect.Position, rect.Size, bgColor, widget.CornerRadiusValue);
            else
                cmd.DrawQuad(rect.Position, rect.Size, bgColor);
        }
        
        // 4. Draw Border
        if (borderColor.A > 0)
        {
             cmd.DrawQuadOutline(rect.Position, rect.Size, borderColor);
        }

        // 5. Draw Text (if Label)
        if (widget is Label label && !string.IsNullOrEmpty(label.Text))
        {
            var textColor = ResolveColor(widget, label.TextColor); // Assuming Label has TextColor property
            // TODO: Implement DrawText in IRenderCommandList or use FontRenderer directly
            // cmd.DrawText(_defaultFont, label.Text, rect.Position + label.Padding, label.FontSize, textColor);
        }

        // 6. Recurse
        foreach (var child in widget.ChildWidgets)
        {
            RenderRecursive(child, cmd);
        }
    }

    // Cascading Theme Resolution
    private Color4 ResolveColor(Widget widget, StyleColor styleColor)
    {
        if (!styleColor.IsSemantic) return styleColor.DirectColor;
        if (styleColor.Token == ThemeColor.None) return Color4.Transparent;

        // Walk up the tree to find a LocalTheme
        var current = widget;
        while (current != null)
        {
            if (current.LocalTheme != null && current.LocalTheme.Colors.TryGetValue(styleColor.Token, out var c))
            {
                return c;
            }
            current = current.Parent;
        }

        // Fallback to Global Theme
        if (Theme.Current.Colors.TryGetValue(styleColor.Token, out var globalColor))
            return globalColor;

        return Color4.Magenta; // Error color
    }
}
```

---

## 4. Phase 2: Styling & Theming System

**Goal**: Implement a robust, cascading theme system.

### 4.1. Theme Tokens (`Styling/ThemeColor.cs`)
Expand tokens to cover states and severity.

```csharp
public enum ThemeColor
{
    None,
    
    // Base Surfaces
    Background,         // Main app background
    Surface,            // Panel background
    SurfaceHighlight,   // Hovered panel / Header
    SurfaceActive,      // Pressed panel / Active tab
    
    // Text
    Text,               // Primary text
    TextDim,            // Secondary/Label text
    TextSelected,       // Text on selection
    
    // Accents
    Primary,            // Main action color (Blue)
    PrimaryHover,
    PrimaryActive,
    
    // Functional
    Success,            // Green
    Warning,            // Yellow
    Error,              // Red
    
    // UI Elements
    Border,
    Selection,          // Selection rectangle
    Separator
}
```

### 4.2. The Theme Class (`Styling/Theme.cs`)
```csharp
public class Theme
{
    public static Theme Current { get; set; } = Dark; // Global default

    public Dictionary<ThemeColor, Color4> Colors { get; } = new();

    public static Theme Dark { get; } = new()
    {
        Colors = {
            [ThemeColor.Background] = new(0.10f, 0.10f, 0.10f, 1f),
            [ThemeColor.Surface]    = new(0.18f, 0.18f, 0.18f, 1f),
            [ThemeColor.Text]       = new(0.90f, 0.90f, 0.90f, 1f),
            [ThemeColor.Primary]    = new(0.20f, 0.40f, 0.80f, 1f),
            // ... fill other colors
        }
    };

    public static Theme Light { get; } = new()
    {
        Colors = {
            [ThemeColor.Background] = new(0.90f, 0.90f, 0.90f, 1f),
            [ThemeColor.Surface]    = new(0.80f, 0.80f, 0.80f, 1f),
            [ThemeColor.Text]       = new(0.10f, 0.10f, 0.10f, 1f),
            [ThemeColor.Primary]    = new(0.30f, 0.50f, 0.90f, 1f),
            // ... fill other colors
        }
    };
}
```

### 4.3. Integrating into `Widget`
Modify `CrucibleUI/Widgets/Widget.cs` to support `StyleColor` and `LocalTheme`.

1.  **Add `StyleColor` Struct**:
    ```csharp
    public readonly struct StyleColor
    {
        public readonly bool IsSemantic;
        public readonly ThemeColor Token;
        public readonly Color4 DirectColor;
        // Implicit conversions from ThemeColor and Color4...
    }
    ```

2.  **Update Widget Properties**:
    ```csharp
    public abstract class Widget {
        public Theme? LocalTheme { get; set; } // For scoping
        public StyleColor BackgroundColor { get; protected set; } = ThemeColor.None;
        public StyleColor BorderColorValue { get; protected set; } = ThemeColor.None;
        
        // Fluent Builders
        public T Background<T>(StyleColor color) where T : Widget {
            BackgroundColor = color;
            return (T)this;
        }
    }
    ```

3.  **Theme Scope Helper**:
    Create `ThemeScopeWidget` (or just use `Panel` with `LocalTheme` set) to apply a theme to a subtree.

---

## 5. Phase 3: Input & Core Extensions

**Goal**: Fix input plumbing and extend core classes.

### 5.1. Extend `InputState`
Modify `Ignis.Core/InputState.cs` to expose text events.
*   **Action**: Add `public event Action<char> OnTextInput;` and `public event Action<Key> OnKeyDown;`.
*   **Implementation**: Hook into `IInputContext.Keyboards[0].KeyChar` and `KeyDown`.

### 5.2. Update `WidgetInputHandler`
Ensure it propagates events to the focused widget.
```csharp
public void HandleTextInput(char c) => _focusedWidget?.OnTextInput(c);
public void HandleKeyDown(Key k) => _focusedWidget?.OnKeyDown(k);
```

---

## 6. Phase 4: The Viewport

**Goal**: Robust rendering of the game scene.

### 6.1. `ViewportWidget` Implementation
Handle resize safety and UV flipping.

```csharp
public class ViewportWidget : Widget
{
    public RenderTargetHandle RenderTarget { get; private set; }
    private IRenderingServer _server;

    public ViewportWidget(IRenderingServer server)
    {
        _server = server;
        // Initialize with 1x1 to avoid 0x0 errors
        RenderTarget = _server.CreateRenderTarget(new RenderTargetDesc(1, 1));
    }

    public void CheckResize()
    {
        int w = (int)ComputedWidth;
        int h = (int)ComputedHeight;

        // Guard against minimized window or invalid layout
        if (w <= 0 || h <= 0) return;

        if (w != RenderTarget.Width || h != RenderTarget.Height)
        {
            _server.DestroyRenderTarget(RenderTarget);
            RenderTarget = _server.CreateRenderTarget(new RenderTargetDesc(w, h));
        }
    }
}
```

### 6.2. Rendering with UV Flip
In `CrucibleRenderer`, when drawing the viewport:
```csharp
if (widget is ViewportWidget viewport)
{
    var texture = _server.GetRenderTargetTexture(viewport.RenderTarget);
    
    // OpenGL RenderTargets are often upside-down relative to UI.
    // Use a DrawSprite overload that accepts UVs, or a specific flag.
    // UVs: TopLeft(0, 1), BottomRight(1, 0) flips Y.
    cmd.DrawSprite(texture, rect.Position, rect.Size, Color4.White, flipY: true);
}
```

---

## 7. Phase 5: Editor Logic & Commands

**Goal**: Implement Undo/Redo and ECS interaction.

### 7.1. Command System (`Commands/`)
All edits must go through commands.

```csharp
public interface IEditorCommand
{
    void Execute();
    void Undo();
}

public class CommandHistory
{
    private Stack<IEditorCommand> _undoStack = new();
    private Stack<IEditorCommand> _redoStack = new();

    public void Execute(IEditorCommand cmd)
    {
        cmd.Execute();
        _undoStack.Push(cmd);
        _redoStack.Clear();
    }

    public void Undo() { /* ... */ }
    public void Redo() { /* ... */ }
}
```

### 7.2. Concrete Commands
*   **`SetComponentFieldCommand<T>`**: Uses Reflection to set a value on a component. Stores `Entity`, `ComponentType`, `FieldInfo`, `OldValue`, `NewValue`.
*   **`CreateEntityCommand`**: Creates entity, stores its ID. Undo deletes it.
*   **`DeleteEntityCommand`**: Serializes entity state, then deletes it. Undo restores it from state.

### 7.3. ECS Refresh Strategy
For `HierarchyWidget`:
*   **Option A (Polling)**: Check `EntityStore.Count` or a version number every frame. If changed, rebuild UI. Simple, robust.
*   **Option B (Events)**: Subscribe to `EntityStore.OnEntityCreated` / `OnEntityDestroyed`. More complex but efficient.
*   **Recommendation**: Start with Polling (every 10-30 frames) for simplicity.

---

## 8. Implementation Roadmap

1.  **Setup**: Create Project, References, Folder Structure.
2.  **Core**: Implement `EditorApp` and `CrucibleRenderer` (Basic).
3.  **Styling**: Implement `Theme`, `StyleColor`, and update `Widget`.
4.  **Input**: Extend `InputState` and `WidgetInputHandler`.
5.  **Viewport**: Implement `ViewportWidget` with resize guards.
6.  **ECS UI**: Implement `Hierarchy` and `Inspector` using `SelectionService`.
7.  **Commands**: Implement `CommandHistory` and wire Inspector to use `SetComponentFieldCommand`.
