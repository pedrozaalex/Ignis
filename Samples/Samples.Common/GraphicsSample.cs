using Ignis.Core;
using Ignis.Core.Scenery;
using Ignis.Core.Timing;
using Ignis.Graphics;

namespace Samples.Common;

/// <summary>
/// Base class for graphics samples. Extends Scene to integrate with the engine.
/// </summary>
public abstract class GraphicsSample : Scene
{
    /// <summary>Sample display name.</summary>
    public abstract string Name { get; }
    
    /// <summary>The rendering server, set during OnEnter.</summary>
    protected IRenderingServer RenderingServer { get; private set; } = null!;
    
    /// <summary>The sample context providing access to input and other services.</summary>
    protected SampleContext? Context { get; private set; }
    
    /// <summary>Window width in pixels.</summary>
    protected int Width { get; private set; }
    
    /// <summary>Window height in pixels.</summary>
    protected int Height { get; private set; }
    
    public override void OnEnter(EngineContext context)
    {
        if (context is SampleContext sampleContext)
        {
            Context = sampleContext;
            RenderingServer = sampleContext.RenderingServer;
            Width = sampleContext.Width;
            Height = sampleContext.Height;
        }
        
        Load();
    }
    
    public override void OnExit()
    {
        Unload();
    }
    
    public override void Update(GameTime time)
    {
        OnUpdate(time.DeltaTime);
    }
    
    /// <summary>Called once when the sample is loaded.</summary>
    protected abstract void Load();
    
    /// <summary>Called when the sample is unloaded.</summary>
    protected virtual void Unload() { }
    
    /// <summary>Called every fixed update.</summary>
    protected virtual void OnUpdate(float deltaTime) { }
    
    /// <summary>Called every frame to render.</summary>
    public abstract void Render(float alpha);
    
    /// <summary>Called when the window is resized.</summary>
    public virtual void OnResize(int width, int height)
    {
        Width = width;
        Height = height;
    }
}

/// <summary>
/// Extended EngineContext that includes graphics-specific services.
/// </summary>
public class SampleContext : EngineContext
{
    public IRenderingServer RenderingServer { get; }
    public int Width { get; set; }
    public int Height { get; set; }
    
    /// <summary>Input state provider - set by the sample runner.</summary>
    public Func<InputState?>? InputProvider { get; set; }
    
    /// <summary>Get the current input state.</summary>
    public InputState? GetInput() => InputProvider?.Invoke();
    
    public SampleContext(IRenderingServer server, int width, int height)
    {
        RenderingServer = server;
        Width = width;
        Height = height;
    }
}

