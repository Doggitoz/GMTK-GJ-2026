using System.Collections.Generic;
using System.Linq;

public class InventoryService
{
    private readonly InventoryState _state;

    public InventoryService()
    {
        _state = new InventoryState();
    }

    public IReadOnlyCollection<string> Items => _state.Items;

    public void LoadFromSaveData(List<string> items)
    {
        _state.Clear();

        foreach (var item in items)
        {
            _state.Add(item);
        }
    }

    public bool HasItem(string item)
    {
        return _state.Contains(item);
    }

    public void AddItem(string name)
    {
        _state.Add(name);
    }

    public void RemoveItem(string name)
    {
        _state.Remove(name);
    }

    public float CalculateModifier(Enums.ItemStat stat)
    {
        float multiplier = 1f;

        foreach (var item in Items)
        {
            var effects = ItemDatabase.GetEffects(item);

            if (effects != null &&
                effects.TryGetValue(stat, out var factor))
            {
                multiplier *= factor;
            }
        }

        return multiplier;
    }

    public void Clear()
    {
        _state.Clear();
    }

    // Persistance boundary

    public List<string> GetSaveData()
    {
        return new List<string>(_state.Items);
    }

    public void LoadSaveData(List<string> items)
    {
        _state.Clear();

        foreach (string item in items)
        {
            _state.Add(item);
        }
    }
}