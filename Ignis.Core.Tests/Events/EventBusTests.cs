namespace Ignis.Core.Tests.Events;

using Ignis.Core.Events;

public class EventBusTests
{
    private struct TestEvent
    {
        public int Value;
    }
    
    private struct OtherEvent
    {
        public string Message;
    }
    
    [Fact]
    public void Publish_WithNoSubscribers_DoesNotThrow()
    {
        var bus = new EventBus();
        
        var exception = Record.Exception(() => bus.Publish(new TestEvent { Value = 42 }));
        
        Assert.Null(exception);
    }
    
    [Fact]
    public void Subscribe_ReceivesPublishedEvents()
    {
        var bus = new EventBus();
        int receivedValue = 0;
        
        bus.Subscribe<TestEvent>(e => receivedValue = e.Value);
        bus.Publish(new TestEvent { Value = 42 });
        
        Assert.Equal(42, receivedValue);
    }
    
    [Fact]
    public void Subscribe_MultipleSubscribers_AllReceiveEvents()
    {
        var bus = new EventBus();
        int count = 0;
        
        bus.Subscribe<TestEvent>(_ => count++);
        bus.Subscribe<TestEvent>(_ => count++);
        bus.Subscribe<TestEvent>(_ => count++);
        bus.Publish(new TestEvent { Value = 1 });
        
        Assert.Equal(3, count);
    }
    
    [Fact]
    public void Subscribe_DifferentEventTypes_OnlyReceivesMatchingType()
    {
        var bus = new EventBus();
        int testEventCount = 0;
        int otherEventCount = 0;
        
        bus.Subscribe<TestEvent>(_ => testEventCount++);
        bus.Subscribe<OtherEvent>(_ => otherEventCount++);
        
        bus.Publish(new TestEvent { Value = 1 });
        bus.Publish(new TestEvent { Value = 2 });
        bus.Publish(new OtherEvent { Message = "hello" });
        
        Assert.Equal(2, testEventCount);
        Assert.Equal(1, otherEventCount);
    }
    
    [Fact]
    public void Unsubscribe_StopsReceivingEvents()
    {
        var bus = new EventBus();
        int count = 0;
        void Handler(TestEvent e) => count++;
        
        bus.Subscribe<TestEvent>(Handler);
        bus.Publish(new TestEvent { Value = 1 });
        Assert.Equal(1, count);
        
        bus.Unsubscribe<TestEvent>(Handler);
        bus.Publish(new TestEvent { Value = 2 });
        Assert.Equal(1, count); // Should not have increased
    }
}

