namespace Ignis.Core.Tests.Scenery;

using Ignis.Core;
using Ignis.Core.Scenery;
using Ignis.Core.Timing;

public class SceneTests
{
    private class TestScene : Scene
    {
        public bool EnterCalled { get; private set; }
        public bool ExitCalled { get; private set; }
        public int UpdateCount { get; private set; }
        public EngineContext? ReceivedContext { get; private set; }
        
        public override void OnEnter(EngineContext context)
        {
            EnterCalled = true;
            ReceivedContext = context;
        }
        
        public override void OnExit()
        {
            ExitCalled = true;
        }
        
        public override void Update(GameTime time)
        {
            UpdateCount++;
        }
    }
    
    [Fact]
    public void OnEnter_ReceivesContext()
    {
        var scene = new TestScene();
        var context = new EngineContext();
        
        scene.OnEnter(context);
        
        Assert.True(scene.EnterCalled);
        Assert.Same(context, scene.ReceivedContext);
    }
    
    [Fact]
    public void Update_CanBeCalledMultipleTimes()
    {
        var scene = new TestScene();
        var time = new GameTime();
        
        scene.Update(time);
        scene.Update(time);
        scene.Update(time);
        
        Assert.Equal(3, scene.UpdateCount);
    }
}

public class SceneManagerTests
{
    private class TestScene : Scene
    {
        public bool EnterCalled { get; private set; }
        public bool ExitCalled { get; private set; }
        
        public override void OnEnter(EngineContext context) => EnterCalled = true;
        public override void OnExit() => ExitCalled = true;
        public override void Update(GameTime time) { }
    }
    
    [Fact]
    public void LoadScene_CallsOnEnter()
    {
        var manager = new SceneManager(new EngineContext());
        var scene = new TestScene();
        
        manager.LoadScene(scene);
        
        Assert.True(scene.EnterCalled);
    }
    
    [Fact]
    public void LoadScene_CallsOnExitOnPreviousScene()
    {
        var manager = new SceneManager(new EngineContext());
        var scene1 = new TestScene();
        var scene2 = new TestScene();
        
        manager.LoadScene(scene1);
        manager.LoadScene(scene2);
        
        Assert.True(scene1.ExitCalled);
        Assert.True(scene2.EnterCalled);
    }
    
    [Fact]
    public void CurrentScene_ReturnsActiveScene()
    {
        var manager = new SceneManager(new EngineContext());
        var scene = new TestScene();
        
        manager.LoadScene(scene);
        
        Assert.Same(scene, manager.CurrentScene);
    }
    
    [Fact]
    public void Update_DelegatesToCurrentScene()
    {
        var manager = new SceneManager(new EngineContext());
        var scene = new TestScene();
        manager.LoadScene(scene);
        
        var updateCount = 0;
        var countingScene = new CountingScene(() => updateCount++);
        manager.LoadScene(countingScene);
        
        manager.Update(new GameTime());
        manager.Update(new GameTime());
        
        Assert.Equal(2, updateCount);
    }
    
    private class CountingScene : Scene
    {
        private readonly Action _onUpdate;
        public CountingScene(Action onUpdate) => _onUpdate = onUpdate;
        public override void OnEnter(EngineContext context) { }
        public override void OnExit() { }
        public override void Update(GameTime time) => _onUpdate();
    }
}

