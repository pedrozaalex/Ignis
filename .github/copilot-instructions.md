```instructions
DO NOT waste tokens creating markdown docs detailing every change you make.
Instead, focus on writing clear and concise code with descriptive names for functions and variables.
If you feel additional context is necessary, include brief comments directly in the code where appropriate.

If there were significant changes to the overall architecture then update the ARCHITECTURE.md file accordingly.
```

## Copilot / Agent Guidance (Ignis)

Follow these concise, actionable rules to be productive in this repository.

1. Build & run
   - Build entire solution: `dotnet build Ignis.sln`
   - Run samples: `dotnet run --project Ignis.Samples`
   - Run tests: `dotnet test`

2. Big picture (what to know first)
   - `Ignis.Engine/Core/IgnisApp.cs`: headless core, ECS world, simulation loop.
   - `Ignis.Engine/Core/IgnisGame.cs`: MonoGame wrapper for rendering and `FontSystem`.
   - `Ignis.Engine/Reactive/`: Crucible reactive primitives (`Signal<T>`, `Computed`, `Effect`, `SignalList`).
   - `Ignis.Engine/ECS/Bridge/`: bridge code (`ComponentSignal`, `ReactiveQuery`, `FrifloExtensions`) connecting ECS to Signals.
   - `Ignis.Engine/UI/` and `UIContext.cs`: declarative UI, layout, hybrid rendering (`PrimitiveBatch` + `SpriteBatch`).

3. Core patterns to use and preserve
   - Reactive-first: prefer `Signal<T>` over mutable shared state. Use `Lens()` for struct fields (Vector3, etc.).
   - ECS-to-UI: use `entity.ComponentSignal<T>()` to bind components to UI widgets; do not cache component structs—read/write via the bridge.
   - UI builders: use `Elements` (e.g. `Panel()`, `Label()`, `Button()`) and `Bind.If` / `Bind.For` for control flow.
   - Layout safety: `Fill`/`Units.Stretch` are applied automatically in many containers; set explicit size only when necessary to avoid zero-size interactive elements.

4. File & code conventions
   - Keep code terse and descriptive: small, focused methods and expressive names.
   - Avoid large prose docs in PRs; add brief inline comments only when intent is non-obvious.
   - Update `Ignis.Engine/ARCHITECTURE.md` when changing major modules or control flow.

5. Important development workflows & checks
   - To debug UI layout issues enable `EngineSettings.DebugUI = true` (see `UIContext` behavior and zero-size detection in `Ignis.Engine/ARCHITECTURE.md`).
   - For editor features, prefer using `Ignis.Samples` as a quick interactive verifier (`dotnet run --project Ignis.Samples`).
   - Tests should focus on reactive propagation (`Signal`, `Computed`) and ECS bridge correctness (`ComponentSignal`, `ReactiveQuery`).

6. Examples (copyable snippets)
   - Component -> UI binding:
     ```csharp
     var pos = entity.ComponentSignal<Position>();
     var editor = new Vector3Field("Position", pos.Lens(p => p.value, (p, v) => new Position(v)));
     ```
   - Reactive list binding:
     ```csharp
     var q = new ReactiveQuery(App.World.Query<NameComponent>());
     var view = Bind.For(q, e => Label(e.GetComponent<NameComponent>().Name));
     ```

7. What to avoid
   - Do not introduce global mutable singletons for state that should be a `Signal` or live in the ECS world.
   - Do not read component arrays once and assume they never change—use the `ReactiveQuery` / bridge APIs.

8. Key files to reference when changing systems
   - `Ignis.Engine/Core/IgnisApp.cs`, `Ignis.Engine/Core/IgnisGame.cs`
   - `Ignis.Engine/Reactive/Signal.cs`, `Computed.cs`, `SignalList.cs`
   - `Ignis.Engine/ECS/Bridge/FrifloExtensions.cs`, `ReactiveQuery.cs`
   - `Ignis.Engine/UI/UIContext.cs`, `UI/Elements/ElementBuilder.cs`, `UI/Graphics/*` (PrimitiveBatch)

If anything here is unclear or you want more examples (tests, UI snippets, or a short checklist for PR reviews), tell me which area to expand.
DO NOT waste tokens creating markdown docs detailing every change you make.
Instead, focus on writing clear and concise code with descriptive names for functions and variables.
If you feel additional context is necessary, include brief comments directly in the code where appropriate.

If there were significant changes to the overall architecture then update the ARCHITECTURE.md file accordingly.