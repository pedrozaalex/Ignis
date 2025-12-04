using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Ignis.Gfx.Backends.OpenGL;

/// <summary>
/// OpenGL-specific window implementation.
/// </summary>
public sealed class OpenGLWindow : Window
{
    private readonly OpenGLRenderingServer _renderingServer;
    private GL? _gl;
    
    /// <summary>The OpenGL rendering server.</summary>
    public override IRenderingServer RenderingServer => _renderingServer;
    
    /// <summary>Direct access to the OpenGL API.</summary>
    public GL? GL => _gl;
    
    /// <summary>
    /// Creates a new OpenGL window with the specified options.
    /// </summary>
    public OpenGLWindow(WindowOptions options) : base(options)
    {
        _renderingServer = new OpenGLRenderingServer();
    }
    
    /// <summary>
    /// Creates a new OpenGL window with default options.
    /// </summary>
    public OpenGLWindow() : this(WindowOptions.Default) { }
    
    /// <summary>
    /// Creates a new OpenGL window with title and size.
    /// </summary>
    public OpenGLWindow(string title, int width, int height) : this(new WindowOptions
    {
        Title = title,
        Width = width,
        Height = height,
        VSync = true,
        Resizable = true,
        Fullscreen = false,
        Samples = 0,
        Backend = WindowBackend.Auto
    }) { }
    
    protected override GraphicsAPI GetGraphicsApi()
    {
        return new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(3, 3));
    }
    
    protected override void OnWindowLoad()
    {
        _gl = Silk.NET.OpenGL.GL.GetApi(NativeWindow);
        _renderingServer.InitializeWithContext(_gl, NativeWindow.Size.X, NativeWindow.Size.Y);
    }
    
    protected override void OnWindowResize(int width, int height)
    {
        _gl?.Viewport(0, 0, (uint)width, (uint)height);
        _renderingServer.Resize(width, height);
    }
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _renderingServer.Dispose();
        }
        base.Dispose(disposing);
    }
}

