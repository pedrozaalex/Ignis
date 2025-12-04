using Ignis.Core;
using Ignis.Core.Scenery;
using Ignis.Core.Timing;
using Ignis.Gfx.Backends.OpenGL;
using Ignis.Samples;
using Silk.NET.Input;

// Available samples
var samples = new GraphicsSample[]
{
    new TriangleSample(),
};

var currentSampleIndex = 0;

// Create rendering server and engine loop
var server = new OpenGLRenderingServer();
var engineLoop = new EngineLoop
{
    TargetFixedStep = TimeSpan.FromSeconds(1.0 / 60.0),
    MaxFixedStepsPerFrame = 5
};

// Create window
var window = new Window(new WindowOptions
{
    Title = $"Ignis Samples - {samples[currentSampleIndex].Name}",
    Width = 1280,
    Height = 720,
    VSync = true,
    Resizable = true,
    Backend = WindowBackend.Auto,
    GraphicsBackend = GraphicsBackend.OpenGL
});

// Scene management
SampleContext? context = null;
SceneManager? sceneManager = null;
GraphicsSample GetCurrentSample() => (GraphicsSample)sceneManager!.CurrentScene!;

window.OnLoad += () =>
{
    server.Initialize(window);

    // Create context and scene manager
    context = new SampleContext(server, window.Width, window.Height);
    sceneManager = new SceneManager(context);

    // Load first sample
    sceneManager.LoadScene(samples[currentSampleIndex]);

    // Wire up engine loop events
    engineLoop.OnFixedUpdate += time => { sceneManager.Update(time); };

    engineLoop.OnRender += time => { GetCurrentSample().Render(time.Alpha); };

    PrintInfo();
};

window.OnUpdate += dt =>
{
    // Handle input before engine tick
    HandleInput();

    // Tick the engine loop (handles fixed update + render)
    engineLoop.Tick();
};

window.OnResize += (w, h) =>
{
    server.Resize(w, h);
    if (context != null)
    {
        context.Width = w;
        context.Height = h;
    }

    GetCurrentSample()?.OnResize(w, h);
};

window.OnClosing += () =>
{
    Console.WriteLine("Closing...");
    sceneManager?.CurrentScene?.OnExit();
    server.Dispose();
};

window.Run();

void HandleInput()
{
    var input = window.InputState;
    if (input == null) return;

    if (input.IsKeyPressed(Key.Escape))
    {
        window.Close();
        return;
    }

    // Pause with P
    if (input.IsKeyPressed(Key.P))
    {
        engineLoop.IsPaused = !engineLoop.IsPaused;
        Console.WriteLine(engineLoop.IsPaused ? "Paused" : "Resumed");
    }

    // Switch samples
    var switchSample = false;
    if (input.IsKeyPressed(Key.Right))
    {
        currentSampleIndex = (currentSampleIndex + 1) % samples.Length;
        switchSample = true;
    }
    else if (input.IsKeyPressed(Key.Left))
    {
        currentSampleIndex = (currentSampleIndex - 1 + samples.Length) % samples.Length;
        switchSample = true;
    }

    if (switchSample && sceneManager != null)
    {
        sceneManager.LoadScene(samples[currentSampleIndex]);
        window.Title = $"Ignis Samples - {GetCurrentSample().Name}";
        Console.WriteLine($"Switched to: {GetCurrentSample().Name}");
    }
}

void PrintInfo()
{
    Console.WriteLine("=== Ignis Samples ===");
    Console.WriteLine($"Platform: {Window.CurrentPlatform}");
    Console.WriteLine($"Backend: {server.Capabilities.BackendName}");
    Console.WriteLine();
    Console.WriteLine("Controls:");
    Console.WriteLine("  Left/Right Arrow - Switch samples");
    Console.WriteLine("  P - Pause/Resume");
    Console.WriteLine("  Escape - Exit");
    Console.WriteLine();
    Console.WriteLine("Samples:");
    for (var i = 0; i < samples.Length; i++)
        Console.WriteLine($"  [{i + 1}] {samples[i].Name}");
    Console.WriteLine();
    Console.WriteLine($"Loaded: {GetCurrentSample().Name}");
}