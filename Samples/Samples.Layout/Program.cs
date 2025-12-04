using Ignis.Core;
using Ignis.Core.Scenery;
using Ignis.Core.Timing;
using Ignis.Graphics.Backends.OpenGL;
using Samples.Common;

var sample = new Samples.Layout.LayoutSample();

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
    sceneManager = new SceneManager(context);
    sceneManager.LoadScene(sample);
    
    engineLoop.OnFixedUpdate += time => sceneManager.Update(time);
    engineLoop.OnRender += time => sample.Render(time.Alpha);
    
    Console.WriteLine($"=== {sample.Name} Sample ===");
    Console.WriteLine($"Backend: {server.Capabilities.BackendName}");
    Console.WriteLine("Press Escape to exit");
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

