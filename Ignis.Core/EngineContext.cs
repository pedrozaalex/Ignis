using Ignis.Core.Events;
using Ignis.Core.IO;

namespace Ignis.Core;

/// <summary>
/// The service container passed to Scenes and Systems.
/// Provides access to engine services without global singletons.
/// </summary>
public class EngineContext
{
    /// <summary>File system abstraction.</summary>
    public IFileSystem FileSystem { get; }
    
    /// <summary>Event bus for decoupled communication.</summary>
    public EventBus EventBus { get; }
    
    public EngineContext() : this(new MemoryFileSystem(), new EventBus())
    {
    }
    
    public EngineContext(IFileSystem fileSystem, EventBus eventBus)
    {
        FileSystem = fileSystem;
        EventBus = eventBus;
    }
}

