using Ignis.Core;
using Ignis.Core.Scenery;
using Ignis.Core.Timing;
using Ignis.Gfx.Backends.OpenGL;
using Samples.Common;

var sample = new Samples.Scene3D.Scene3DSample();

var server = new OpenGLRenderingServer();
var engineLoop = new EngineLoop
{
    TargetFixedStep = TimeSpan.FromSeconds(1.0 / 60.0),
    MaxFixedStepsPerFrame = 5
};

var window = new Window(new WindowOptions
{
    Title = $"Ignis - {sample.Name}",
    Width = 1280,
    Height = 720,
    VSync = true,
    Resizable = true,
    Backend = WindowBackend.Auto,
    GraphicsBackend = GraphicsBackend.OpenGL
});

SampleContext? context = null;
SceneManager? sceneManager = null;

window.OnLoad += () =>
{
    server.Initialize(window);
    
    context = new SampleContext(server, window.Width, window.Height);
    context.InputProvider = () => window.InputState;
    
    sceneManager = new SceneManager(context);
    sceneManager.LoadScene(sample);
    
    engineLoop.OnFixedUpdate += time => sceneManager.Update(time);
    engineLoop.OnRender += time => sample.Render(time.Alpha);
    
    Console.WriteLine($"=== {sample.Name} ===");
    Console.WriteLine($"Backend: {server.Capabilities.BackendName}");
    Console.WriteLine();
    Console.WriteLine("Controls:");
    Console.WriteLine("  WASD - Move camera");
    Console.WriteLine("  Space/Shift - Up/Down");
    Console.WriteLine("  Right Click - Toggle mouse look");
    Console.WriteLine("  Ctrl - Speed boost");
    Console.WriteLine("  Escape - Exit");
};

window.OnUpdate += _ =>
{
    if (window.InputState?.IsKeyPressed(Silk.NET.Input.Key.Escape) == true)
    {
        window.Close();
        return;
    }
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
    sample.OnResize(w, h);
};

window.OnClosing += () =>
{
    sceneManager?.CurrentScene?.OnExit();
    server.Dispose();
};

window.Run();

