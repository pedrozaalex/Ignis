using Ignis.Core;
using Ignis.Core.Scenery;
using Ignis.Core.Timing;
using Ignis.Graphics.Backends.OpenGL;
using Samples.TowerDefense.Core;
using Samples.TowerDefense.Scenes;
using Samples.Common;

// Create game context with shared services
var server = new OpenGLRenderingServer();
var engineLoop = new EngineLoop
{
    TargetFixedStep = TimeSpan.FromSeconds(1.0 / 60.0),
    MaxFixedStepsPerFrame = 5
};

var window = new Window(new WindowOptions
{
    Title = "Ignis - Tower Defense",
    Width = 1280,
    Height = 720,
    VSync = true,
    Resizable = true,
    Backend = WindowBackend.Auto,
    GraphicsBackend = GraphicsBackend.OpenGL
});

TowerDefenseContext? gameContext = null;
SceneManager? sceneManager = null;
ITowerDefenseScene? currentScene = null;

window.OnLoad += () =>
{
    server.Initialize(window);

    gameContext = new TowerDefenseContext(server, window.Width, window.Height, window);
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

    Console.WriteLine("=== Tower Defense: Temporal Rift ===");
    Console.WriteLine($"Backend: {server.Capabilities.BackendName}");
    Console.WriteLine("Controls:");
    Console.WriteLine("  Mouse - Select/Place turrets");
    Console.WriteLine("  1,2,3 - Select turret type");
    Console.WriteLine("  Space - Start wave / Activate time rift");
    Console.WriteLine("  Escape - Pause / Back");
};

window.OnUpdate += _ =>
{
    engineLoop.Tick();

    // Update current scene reference when scene changes
    if (sceneManager?.CurrentScene is ITowerDefenseScene td)
        currentScene = td;
};

window.OnResize += (w, h) =>
{
    server.Resize(w, h);
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
    server.Dispose();
};

window.Run();
