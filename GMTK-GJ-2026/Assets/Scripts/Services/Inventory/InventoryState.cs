using System.Collections.Generic;

public class InventoryState
{
    private readonly HashSet<string> _items = new();

    public IReadOnlyCollection<string> Items => _items;

    public bool Contains(string item)
    {
        return _items.Contains(item);
    }

    public void Add(string item)
    {
        _items.Add(item);
    }

    public void Remove(string item)
    {
        _items.Remove(item);
    }

    public void Clear()
    {
        _items.Clear();
    }
}
