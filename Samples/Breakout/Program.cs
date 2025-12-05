using Ignis.Core;
using Ignis.Core.Scenery;
using Ignis.Graphics.Backends.OpenGL;
using Samples.Breakout.Core;
using Samples.Breakout.Scenes;

// Create game context with shared services
var renderingServer = new OpenGLRenderingServer();
var engineLoop = new EngineLoop
{
    TargetFixedStep = TimeSpan.FromSeconds(1.0 / 60.0),
    MaxFixedStepsPerFrame = 5
};

var window = new Window(new WindowOptions
{
    Title = "Ignis - Breakout",
    Width = 1280,
    Height = 720,
    VSync = true,
    Resizable = true,
    Backend = WindowBackend.Auto,
    GraphicsBackend = GraphicsBackend.OpenGL
});

BreakoutContext? gameContext = null;
SceneManager? sceneManager = null;
IBreakoutScene? currentScene = null;

window.OnLoad += () =>
{
    renderingServer.Initialize(window);

    gameContext = new BreakoutContext(renderingServer, window.Width, window.Height, window);
    gameContext.Initialize();
    gameContext.Audio.Initialize();
    gameContext.Audio.UpdateFromSettings(gameContext.Settings);

    sceneManager = new SceneManager(gameContext);

    // Start with main menu
    var mainMenu = new MainMenuScene(gameContext, sceneManager);
    currentScene = mainMenu;
    sceneManager.LoadScene(mainMenu);

    engineLoop.OnFixedUpdate += time =>
    {
        sceneManager.Update(time);
        gameContext.Audio.Update();
    };

    engineLoop.OnRender += time =>
    {
        currentScene?.Render(time.Alpha);
    };

    Console.WriteLine("=== Breakout Sample ===");
    Console.WriteLine($"Backend: {renderingServer.Capabilities.BackendName}");
    Console.WriteLine("Controls:");
    Console.WriteLine("  Arrow Keys / A,D - Move paddle");
    Console.WriteLine("  Space - Launch ball / Pause");
    Console.WriteLine("  Escape - Back / Exit");
};

window.OnUpdate += _ =>
{
    engineLoop.Tick();

    // Update current scene reference when scene changes
    if (sceneManager?.CurrentScene is IBreakoutScene bs)
        currentScene = bs;
};

window.OnResize += (w, h) =>
{
    renderingServer.Resize(w, h);
    if (gameContext != null)
    {
        gameContext.Width = w;
        gameContext.Height = h;
    }
    currentScene?.OnResize(w, h);
};

window.OnClosing += () =>
{
    gameContext?.SaveSettings();
    sceneManager?.CurrentScene?.OnExit();
    renderingServer.Dispose();
};

window.Run();
