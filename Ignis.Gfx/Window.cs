using System.Runtime.InteropServices;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Ignis.Gfx;

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
    
    public static WindowOptions Default => new()
    {
        Title = "Ignis Application",
        Width = 1280,
        Height = 720,
        VSync = true,
        Resizable = true,
        Fullscreen = false,
        Samples = 0,
        Backend = WindowBackend.Auto
    };
}

/// <summary>
/// Cross-platform window wrapper using Silk.NET.Windowing.
/// Supports Windows, Linux, and macOS via GLFW or SDL backends.
/// </summary>
public abstract class Window : IDisposable
{
    protected readonly IWindow NativeWindow;
    protected IInputContext? InputContext;
    private InputState? _inputState;
    private bool _disposed;
    
    /// <summary>The current platform.</summary>
    public static Platform CurrentPlatform { get; } = DetectPlatform();
    
    /// <summary>The underlying rendering server (set by derived class).</summary>
    public abstract IRenderingServer RenderingServer { get; }
    
    /// <summary>
    /// Managed input state for the window. EndFrame() is called automatically after OnUpdate.
    /// Use this instead of creating your own InputState to avoid timing issues.
    /// </summary>
    public InputState? InputState => _inputState;
    
    /// <summary>Current window width in pixels.</summary>
    public int Width => NativeWindow.Size.X;
    
    /// <summary>Current window height in pixels.</summary>
    public int Height => NativeWindow.Size.Y;
    
    /// <summary>Window title.</summary>
    public string Title
    {
        get => NativeWindow.Title;
        set => NativeWindow.Title = value;
    }
    
    /// <summary>Whether VSync is enabled.</summary>
    public bool VSync
    {
        get => NativeWindow.VSync;
        set => NativeWindow.VSync = value;
    }
    
    /// <summary>Time elapsed since last frame in seconds.</summary>
    public double DeltaTime { get; private set; }
    
    /// <summary>Total elapsed time since window creation in seconds.</summary>
    public double TotalTime { get; private set; }
    
    /// <summary>Current frames per second.</summary>
    public double FramesPerSecond => NativeWindow.FramesPerSecond;
    
    /// <summary>Whether the window is currently focused.</summary>
    public bool IsFocused => !NativeWindow.IsClosing;
    
    /// <summary>The input context for keyboard/mouse/gamepad.</summary>
    public IInputContext? Input => InputContext;
    
    // Events
    public event Action? OnLoad;
    public event Action<double>? OnUpdate;
    public event Action<double>? OnRender;
    public event Action<int, int>? OnResize;
    public event Action? OnClosing;
    
    /// <summary>
    /// Creates a new cross-platform window with the specified options.
    /// </summary>
    protected Window(WindowOptions options)
    {
        RegisterWindowingBackend(options.Backend);
        
        var silkOptions = Silk.NET.Windowing.WindowOptions.Default;
        silkOptions.Title = options.Title;
        silkOptions.Size = new Vector2D<int>(options.Width, options.Height);
        silkOptions.VSync = options.VSync;
        silkOptions.WindowBorder = options.Resizable ? WindowBorder.Resizable : WindowBorder.Fixed;
        silkOptions.WindowState = options.Fullscreen ? WindowState.Fullscreen : WindowState.Normal;
        silkOptions.Samples = options.Samples;
        silkOptions.API = GetGraphicsApi();
        
        NativeWindow = Silk.NET.Windowing.Window.Create(silkOptions);
        
        NativeWindow.Load += HandleLoad;
        NativeWindow.Update += HandleUpdate;
        NativeWindow.Render += HandleRender;
        NativeWindow.FramebufferResize += HandleResize;
        NativeWindow.Closing += HandleClosing;
    }
    
    /// <summary>
    /// Returns the graphics API configuration for this window type.
    /// </summary>
    protected abstract GraphicsAPI GetGraphicsApi();
    
    /// <summary>
    /// Called when the window is loaded. Initialize graphics context here.
    /// </summary>
    protected abstract void OnWindowLoad();
    
    /// <summary>
    /// Starts the window and enters the main loop.
    /// This method blocks until the window is closed.
    /// </summary>
    public void Run() => NativeWindow.Run();
    
    /// <summary>
    /// Closes the window.
    /// </summary>
    public void Close() => NativeWindow.Close();
    
    /// <summary>
    /// Sets the window to fullscreen mode.
    /// </summary>
    public void SetFullscreen(bool fullscreen)
    {
        NativeWindow.WindowState = fullscreen ? WindowState.Fullscreen : WindowState.Normal;
    }
    
    /// <summary>
    /// Sets the window size.
    /// </summary>
    public void SetSize(int width, int height)
    {
        NativeWindow.Size = new Vector2D<int>(width, height);
    }
    
    /// <summary>
    /// Centers the window on the screen.
    /// </summary>
    public void Center()
    {
        var monitor = NativeWindow.Monitor;
        if (monitor != null)
        {
            var bounds = monitor.Bounds;
            var x = bounds.Origin.X + (bounds.Size.X - NativeWindow.Size.X) / 2;
            var y = bounds.Origin.Y + (bounds.Size.Y - NativeWindow.Size.Y) / 2;
            NativeWindow.Position = new Vector2D<int>(x, y);
        }
    }
    
    private void HandleLoad()
    {
        InputContext = NativeWindow.CreateInput();
        _inputState = new InputState(InputContext);
        OnWindowLoad();
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
        OnWindowResize(size.X, size.Y);
        OnResize?.Invoke(size.X, size.Y);
    }
    
    /// <summary>
    /// Called when the window is resized. Update viewport here.
    /// </summary>
    protected abstract void OnWindowResize(int width, int height);
    
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
    
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;
        
        if (disposing)
        {
            _inputState?.Dispose();
            InputContext?.Dispose();
            NativeWindow.Dispose();
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

