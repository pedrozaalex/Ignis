namespace Ignis.Core.Tests;

using Core;
using Ignis.Core.Assets;
using Ignis.Core.Events;
using Ignis.Core.IO;

public class EngineContextTests
{
    [Fact]
    public void DefaultConstructor_CreatesWithDefaults()
    {
        var context = new EngineContext();
        
        Assert.NotNull(context.FileSystem);
        Assert.NotNull(context.EventBus);
    }
    
    [Fact]
    public void Constructor_AcceptsCustomServices()
    {
        var fileSystem = new MemoryFileSystem();
        var eventBus = new EventBus();
        
        var context = new EngineContext(fileSystem, eventBus);
        
        Assert.Same(fileSystem, context.FileSystem);
        Assert.Same(eventBus, context.EventBus);
    }
    
    [Fact]
    public void FileSystem_IsAccessible()
    {
        var fs = new MemoryFileSystem();
        var context = new EngineContext(fs, new EventBus());
        
        context.FileSystem.Write("test.txt", new byte[] { 1, 2, 3 });
        
        Assert.True(context.FileSystem.Exists("test.txt"));
    }
    
    [Fact]
    public void EventBus_IsAccessible()
    {
        var context = new EngineContext();
        int received = 0;
        
        context.EventBus.Subscribe<TestEvent>(e => received = e.Value);
        context.EventBus.Publish(new TestEvent { Value = 42 });
        
        Assert.Equal(42, received);
    }
    
    private struct TestEvent
    {
        public int Value;
    }
}

