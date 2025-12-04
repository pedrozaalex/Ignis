namespace Ignis.Gfx.Samples;

/// <summary>
/// Base interface for all samples.
/// </summary>
public interface ISample : IDisposable
{
    /// <summary>Sample display name.</summary>
    string Name { get; }
    
    /// <summary>Called once when the sample is loaded.</summary>
    void Load(IRenderingServer server);
    
    /// <summary>Called every frame to update logic.</summary>
    void Update(double deltaTime);
    
    /// <summary>Called every frame to render.</summary>
    void Render(IRenderingServer server, int width, int height);
}

