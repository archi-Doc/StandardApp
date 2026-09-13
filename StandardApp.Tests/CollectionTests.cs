using System.Collections.ObjectModel;
using Arc.WPF;

namespace StandardApp.Tests;

/// <summary>
/// Tests collection sorting with duplicates and custom equality.
/// </summary>
public class CollectionTests
{
    [Fact]
    public void SortHandlesDuplicateValues()
    {
        var values = new ObservableCollection<int> { 2, 1, 2, 1, 3 };
        values.Sort((x, y) => x.CompareTo(y));
        Assert.Equal(new[] { 1, 1, 2, 2, 3 }, values);
    }

    [Fact]
    public void SortTracksDistinctInstancesEvenWhenTheyCompareEqualByEquality()
    {
        var first = new Item(3);
        var second = new Item(1);
        var third = new Item(2);
        var items = new ObservableCollection<Item> { first, second, third };
        items.Sort((x, y) => x.Order.CompareTo(y.Order));
        Assert.Same(second, items[0]);
        Assert.Same(third, items[1]);
        Assert.Same(first, items[2]);
    }

    [Fact]
    public void AlreadySortedCollectionDoesNotRaiseMoveEvents()
    {
        var items = new ObservableCollection<int> { 1, 1, 2 };
        var notifications = 0;
        items.CollectionChanged += (_, _) => notifications++;
        items.Sort((x, y) => x.CompareTo(y));
        Assert.Equal(0, notifications);
    }

    private sealed class Item(int order)
    {
        public int Order => order;

        public override bool Equals(object? obj) => obj is Item;

        public override int GetHashCode() => 0;
    }
}
