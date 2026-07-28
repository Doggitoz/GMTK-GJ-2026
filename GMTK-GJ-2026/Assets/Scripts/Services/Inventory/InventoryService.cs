using System.Collections.Generic;

public class InventoryService
{
    private readonly InventoryState _state;

    public InventoryService()
    {
        _state = new InventoryState();
    }

    public IReadOnlyCollection<string> Items => _state.Items;

    public bool Contains(string item)
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
}