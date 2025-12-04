using System.Runtime.InteropServices;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Ignis.Core;

/// <summary>
/// Detected platform for the current runtime.
/// </summary>
public enum Platform
{
    Windows,
    Linux,
    MacOS,
    Unknown
}

/// <summary>
/// Preferred windowing backend.
/// </summary>
public enum WindowBackend
{
    /// <summary>Automatically select the best backend for the platform.</summary>
    Auto,
    /// <summary>Use GLFW (works on Windows, Linux, macOS).</summary>
    GLFW,
    /// <summary>Use SDL2 (works on Windows, Linux, macOS, and more).</summary>
    SDL
}

/// <summary>
/// Graphics API to use with the window.
/// </summary>
public enum GraphicsBackend
{
    /// <summary>OpenGL 3.3 Core Profile</summary>
    OpenGL,
    /// <summary>No graphics API (headless window for compute, etc.)</summary>
    None
}

/// <summary>
/// Configuration options for creating a window.
/// </summary>
public struct WindowOptions
{
    public string Title;
    public int Width;
    public int Height;
    public bool VSync;
    public bool Resizable;
    public bool Fullscreen;
    public int Samples;
    public WindowBackend Backend;
    public GraphicsBackend GraphicsBackend;
    
    public static WindowOptions Default => new()
    {
        Title = "Ignis Application",
        Width = 1280,
        Height = 720,
        VSync = true,
        Resizable = true,
        Fullscreen = false,
        Samples = 0,
        Backend = WindowBackend.Auto,
        GraphicsBackend = GraphicsBackend.OpenGL
    };
}

/// <summary>
/// Cross-platform window using Silk.NET.Windowing.
/// Supports Windows, Linux, and macOS via GLFW or SDL backends.
/// </summary>
public sealed class Window : IDisposable
{
    private readonly IWindow _nativeWindow;
    private readonly GraphicsBackend _graphicsBackend;
    private IInputContext? _inputContext;
    private InputState? _inputState;
    private bool _disposed;
    
    /// <summary>The current platform.</summary>
    public static Platform CurrentPlatform { get; } = DetectPlatform();
    
    /// <summary>The underlying Silk.NET window for advanced usage.</summary>
    public IWindow NativeWindow => _nativeWindow;
    
    /// <summary>
    /// Managed input state for the window. EndFrame() is called automatically after OnUpdate.
    /// </summary>
    public InputState? InputState => _inputState;
    
    /// <summary>Current window width in pixels.</summary>
    public int Width => _nativeWindow.Size.X;
    
    /// <summary>Current window height in pixels.</summary>
    public int Height => _nativeWindow.Size.Y;
    
    /// <summary>Window title.</summary>
    public string Title
    {
        get => _nativeWindow.Title;
        set => _nativeWindow.Title = value;
    }
    
    /// <summary>Whether VSync is enabled.</summary>
    public bool VSync
    {
        get => _nativeWindow.VSync;
        set => _nativeWindow.VSync = value;
    }
    
    /// <summary>Time elapsed since last frame in seconds.</summary>
    public double DeltaTime { get; private set; }
    
    /// <summary>Total elapsed time since window creation in seconds.</summary>
    public double TotalTime { get; private set; }
    
    /// <summary>Current frames per second.</summary>
    public double FramesPerSecond => _nativeWindow.FramesPerSecond;
    
    /// <summary>Whether the window is currently focused.</summary>
    public bool IsFocused => !_nativeWindow.IsClosing;
    
    // Events
    public event Action? OnLoad;
    public event Action<double>? OnUpdate;
    public event Action<double>? OnRender;
    public event Action<int, int>? OnResize;
    public event Action? OnClosing;
    
    /// <summary>
    /// Creates a new cross-platform window with the specified options.
    /// </summary>
    public Window(WindowOptions options)
    {
        _graphicsBackend = options.GraphicsBackend;
        RegisterWindowingBackend(options.Backend);
        
        var silkOptions = Silk.NET.Windowing.WindowOptions.Default;
        silkOptions.Title = options.Title;
        silkOptions.Size = new Vector2D<int>(options.Width, options.Height);
        silkOptions.VSync = options.VSync;
        silkOptions.WindowBorder = options.Resizable ? WindowBorder.Resizable : WindowBorder.Fixed;
        silkOptions.WindowState = options.Fullscreen ? WindowState.Fullscreen : WindowState.Normal;
        silkOptions.Samples = options.Samples;
        silkOptions.API = GetGraphicsApi(options.GraphicsBackend);
        
        _nativeWindow = Silk.NET.Windowing.Window.Create(silkOptions);
        
        _nativeWindow.Load += HandleLoad;
        _nativeWindow.Update += HandleUpdate;
        _nativeWindow.Render += HandleRender;
        _nativeWindow.FramebufferResize += HandleResize;
        _nativeWindow.Closing += HandleClosing;
    }
    
    /// <summary>
    /// Creates a new window with default options.
    /// </summary>
    public Window() : this(WindowOptions.Default) { }
    
    /// <summary>
    /// Creates a new window with title and size.
    /// </summary>
    public Window(string title, int width, int height) : this(new WindowOptions
    {
        Title = title,
        Width = width,
        Height = height,
        VSync = true,
        Resizable = true,
        Fullscreen = false,
        Samples = 0,
        Backend = WindowBackend.Auto,
        GraphicsBackend = GraphicsBackend.OpenGL
    }) { }
    
    private static GraphicsAPI GetGraphicsApi(GraphicsBackend backend) => backend switch
    {
        GraphicsBackend.OpenGL => new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(3, 3)),
        GraphicsBackend.None => GraphicsAPI.None,
        _ => GraphicsAPI.Default
    };
    
    /// <summary>
    /// Starts the window and enters the main loop.
    /// This method blocks until the window is closed.
    /// </summary>
    public void Run() => _nativeWindow.Run();
    
    /// <summary>
    /// Closes the window.
    /// </summary>
    public void Close() => _nativeWindow.Close();
    
    /// <summary>
    /// Sets the window to fullscreen mode.
    /// </summary>
    public void SetFullscreen(bool fullscreen)
    {
        _nativeWindow.WindowState = fullscreen ? WindowState.Fullscreen : WindowState.Normal;
    }
    
    /// <summary>
    /// Sets the window size.
    /// </summary>
    public void SetSize(int width, int height)
    {
        _nativeWindow.Size = new Vector2D<int>(width, height);
    }
    
    /// <summary>
    /// Centers the window on the screen.
    /// </summary>
    public void Center()
    {
        var monitor = _nativeWindow.Monitor;
        if (monitor != null)
        {
            var bounds = monitor.Bounds;
            var x = bounds.Origin.X + (bounds.Size.X - _nativeWindow.Size.X) / 2;
            var y = bounds.Origin.Y + (bounds.Size.Y - _nativeWindow.Size.Y) / 2;
            _nativeWindow.Position = new Vector2D<int>(x, y);
        }
    }
    
    private void HandleLoad()
    {
        _inputContext = _nativeWindow.CreateInput();
        _inputState = new InputState(_inputContext);
        OnLoad?.Invoke();
    }
    
    private void HandleUpdate(double deltaTime)
    {
        DeltaTime = deltaTime;
        TotalTime += deltaTime;
        OnUpdate?.Invoke(deltaTime);
        
        // Clear per-frame input states AFTER update processing
        _inputState?.EndFrame();
    }
    
    private void HandleRender(double deltaTime)
    {
        OnRender?.Invoke(deltaTime);
    }
    
    private void HandleResize(Vector2D<int> size)
    {
        OnResize?.Invoke(size.X, size.Y);
    }
    
    private void HandleClosing()
    {
        OnClosing?.Invoke();
    }
    
    private static Platform DetectPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Platform.Windows;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return Platform.Linux;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return Platform.MacOS;
        return Platform.Unknown;
    }
    
    private static void RegisterWindowingBackend(WindowBackend backend)
    {
        switch (backend)
        {
            case WindowBackend.GLFW:
            case WindowBackend.Auto:
            default:
                Silk.NET.Windowing.Glfw.GlfwWindowing.RegisterPlatform();
                break;
            case WindowBackend.SDL:
                throw new PlatformNotSupportedException(
                    "SDL backend requires the Silk.NET.Windowing.Sdl package. " +
                    "Add it to your project or use WindowBackend.GLFW instead.");
        }
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        _inputState?.Dispose();
        _inputContext?.Dispose();
        _nativeWindow.Dispose();
        
        GC.SuppressFinalize(this);
    }
}

