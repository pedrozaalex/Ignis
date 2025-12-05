# Ignis Engine - AI Coding Instructions

## Project Overview
Ignis is a modular C# game engine built on .NET 8+. It emphasizes separation of concerns, high performance, and a "code-first" approach.
- **Ignis.Core**: Core application loop, windowing (Silk.NET), input, and timing.
- **Ignis.Graphics**: Backend-agnostic rendering abstraction.
- **Ignis.Graphics.Backends.OpenGL**: OpenGL 3.3+ implementation.
- **Ignis.Physics**: 2D physics system using `Friflo.Engine.ECS`.
- **CrucibleUI**: A standalone, high-performance UI layout engine.

## Architecture & Patterns

### 1. The Game Loop (`Ignis.Core`)
- **Fixed Timestep**: Logic runs in `OnFixedUpdate` (default 60Hz).
- **Interpolation**: Rendering runs in `OnRender` with an alpha value (0.0-1.0) for interpolating between states.
- **Usage**:
  ```csharp
  var loop = new EngineLoop();
  loop.OnFixedUpdate += (time) => { /* Physics/Logic */ };
  loop.OnRender += (time) => { /* Draw(time.Alpha) */ };
  ```

### 2. Entity Component System (ECS)
- The engine uses **Friflo.Engine.ECS** for game state management.
- **Systems**: Logic should be implemented as systems that query the `EntityStore`.
- **Physics**: `Ignis.Physics.CollisionSystem` integrates with the ECS store.

### 3. Rendering (`Ignis.Graphics`)
- **Abstraction**: Do not call OpenGL/Vulkan directly in game code. Use `IRenderingServer`.
- **Command Lists**: Record drawing commands into `RenderCommandList` and submit them to the server.
- **Resources**: Use handles (`MeshHandle`, `TextureHandle`) to manage GPU assets.

### 4. User Interface (`CrucibleUI`)
- **Layout Engine**: `CrucibleUI` is a pure layout solver. It calculates rectangles but doesn't render them.
- **Integration**: You must traverse the computed layout tree and issue rendering commands (e.g., via `Ignis.Graphics`) to draw the UI.

## Development Workflow

### Build & Test
- **Build Solution**: `dotnet build Ignis.sln`
- **Run Tests**: `dotnet test` (Includes `CrucibleUI.Tests` and `Ignis.Core.Tests`)
- **Run Sample**: `dotnet run --project Samples/Breakout/Breakout.csproj`

### Key Dependencies
- **Silk.NET**: Used for Windowing, Input, and OpenGL bindings.
- **Friflo.Engine.ECS**: The ECS framework.
- **System.Numerics**: Standard math library (Vector2, Vector3, Matrix4x4).

## Coding Conventions
- **Manual Composition**: Prefer manual dependency injection over IoC containers. See `Samples/Breakout/Program.cs` for how to wire up `Window`, `EngineLoop`, and `RenderingServer`.
- **Math**: Use `System.Numerics` types.
- **Disposal**: Ensure `IDisposable` resources (Textures, Meshes, Windows) are properly disposed.
