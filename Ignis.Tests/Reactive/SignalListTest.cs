using Ignis.Engine.Reactive;

namespace Ignis.Tests.Reactive;

/// <summary>
/// Tests for SignalList&lt;T&gt; - Observable collection with fine-grained change notifications
/// </summary>
public class SignalListTests
{
    [Fact]
    public void SignalList_Add_RaisesItemAddedEvent()
    {
        // Arrange
        var list = new SignalList<string>();
        var addedItems = new List<string>();

        list.ItemAdded += (item, index) => addedItems.Add(item);

        // Act
        list.Add("A");
        list.Add("B");
        list.Add("C");

        // Assert
        Assert.Equal(3, list.Count);
        Assert.Equal(new[] { "A", "B", "C" }, addedItems);
    }

    [Fact]
    public void SignalList_Remove_RaisesItemRemovedEvent()
    {
        // Arrange
        var list = new SignalList<string>();
        var removedItems = new List<string>();

        list.Add("A");
        list.Add("B");
        list.Add("C");

        list.ItemRemoved += (item, index) => removedItems.Add(item);

        // Act
        list.Remove("B");

        // Assert
        Assert.Equal(2, list.Count);
        Assert.Equal(new[] { "B" }, removedItems);
    }

    [Fact]
    public void SignalList_Move_RaisesItemMovedEvent()
    {
        // Arrange
        var list = new SignalList<string>();
        var moveEvents = new List<(string item, int oldIndex, int newIndex)>();

        list.Add("A");
        list.Add("B");
        list.Add("C");

        list.ItemMoved += (item, oldIndex, newIndex) => moveEvents.Add((item, oldIndex, newIndex));

        // Act
        list.Move(0, 2); // Move A to end

        // Assert
        Assert.Equal("B", list[0]);
        Assert.Equal("C", list[1]);
        Assert.Equal("A", list[2]);
        Assert.Single(moveEvents);
        Assert.Equal(("A", 0, 2), moveEvents[0]);
    }

    [Fact]
    public void SignalList_ComplexOperations_MaintainsConsistency()
    {
        // Arrange
        var list = new SignalList<int>();

        // Act & Assert
        list.Add(1);
        list.Add(2);
        list.Add(3);
        Assert.Equal(3, list.Count);

        list.Insert(1, 10);
        Assert.Equal([1, 10, 2, 3], list.Items);

        list.RemoveAt(2);
        Assert.Equal([1, 10, 3], list.Items);

        Assert.Contains(10, list.Items);
        Assert.Equal(1, list.IndexOf(10));
    }
}

