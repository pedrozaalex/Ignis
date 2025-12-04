using Ignis.Gfx;
using Ignis.Gfx.Backends.OpenGL;
using Ignis.Gfx.Samples;
using Silk.NET.Input;

// Available samples
var samples = new ISample[]
{
    new TriangleSample(),
};

int currentSampleIndex = 0;
ISample currentSample = samples[currentSampleIndex];

// Create window
var window = new OpenGLWindow(new WindowOptions
{
    Title = $"Ignis.Gfx Samples - {currentSample.Name}",
    Width = 1280,
    Height = 720,
    VSync = true,
    Resizable = true,
    Backend = WindowBackend.Auto
});

window.OnLoad += () =>
{
    currentSample.Load(window.RenderingServer);
    
    Console.WriteLine("=== Ignis.Gfx Samples ===");
    Console.WriteLine($"Platform: {Window.CurrentPlatform}");
    Console.WriteLine($"Backend: {window.RenderingServer.Capabilities.BackendName}");
    Console.WriteLine();
    Console.WriteLine("Controls:");
    Console.WriteLine("  Left/Right Arrow - Switch samples");
    Console.WriteLine("  Escape - Exit");
    Console.WriteLine();
    Console.WriteLine("Samples:");
    for (int i = 0; i < samples.Length; i++)
    {
        Console.WriteLine($"  [{i + 1}] {samples[i].Name}");
    }
};

window.OnUpdate += (dt) =>
{
    // Use Window's managed InputState - EndFrame() is called automatically after this callback
    var input = window.InputState;
    
    if (input?.IsKeyPressed(Key.Escape) == true)
    {
        window.Close();
        return;
    }
    
    bool switchSample = false;
    if (input?.IsKeyPressed(Key.Right) == true)
    {
        currentSampleIndex = (currentSampleIndex + 1) % samples.Length;
        switchSample = true;
    }
    else if (input?.IsKeyPressed(Key.Left) == true)
    {
        currentSampleIndex = (currentSampleIndex - 1 + samples.Length) % samples.Length;
        switchSample = true;
    }
    
    if (switchSample)
    {
        currentSample.Dispose();
        currentSample = samples[currentSampleIndex];
        currentSample.Load(window.RenderingServer);
        window.Title = $"Ignis.Gfx Samples - {currentSample.Name}";
        Console.WriteLine($"Switched to: {currentSample.Name}");
    }
    
    currentSample.Update(dt);
};

window.OnRender += (dt) =>
{
    currentSample.Render(window.RenderingServer, window.Width, window.Height);
};

window.OnResize += (w, h) =>
{
    Console.WriteLine($"Window resized: {w}x{h}");
};

window.OnClosing += () =>
{
    Console.WriteLine("Closing...");
    currentSample.Dispose();
};

// Run the application
window.Run();
