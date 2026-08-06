using System.Collections.Generic;

public class InventoryService
{
    private readonly InventoryState _unlockedState;
    private readonly HashSet<string> _equippedItems;

    public InventoryService()
    {
        _unlockedState = new InventoryState();
        _equippedItems = new HashSet<string>();
    }

    public IReadOnlyCollection<string> Items => _equippedItems;
    public IReadOnlyCollection<string> UnlockedItems => _unlockedState.Items;

    public void LoadFromSaveData(List<string> items)
    {
        _unlockedState.Clear();
        _equippedItems.Clear();

        foreach (var item in items)
        {
            _unlockedState.Add(item);
        }
    }

    public bool HasItem(string item)
    {
        // Back-compat: "has item" means currently equipped/active.
        return _equippedItems.Contains(item);
    }

    public void AddItem(string name)
    {
        // Back-compat: treat AddItem as unlock, not auto-equip.
        _unlockedState.Add(name);
    }

    public void RemoveItem(string name)
    {
        // Back-compat: avoid removing ownership; this now unequips only.
        _equippedItems.Remove(name);
    }

    public bool IsUnlocked(string item)
    {
        return _unlockedState.Contains(item);
    }

    public bool IsEquipped(string item)
    {
        return _equippedItems.Contains(item);
    }

    public void UnlockItem(string name)
    {
        _unlockedState.Add(name);
    }

    public void ToggleEquipped(string name)
    {
        if (!_unlockedState.Contains(name))
        {
            return;
        }

        if (!_equippedItems.Add(name))
        {
            _equippedItems.Remove(name);
        }
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
        _unlockedState.Clear();
        _equippedItems.Clear();
    }

    // Persistance boundary

    public List<string> GetSaveData()
    {
        return new List<string>(_unlockedState.Items);
    }

    public void LoadSaveData(List<string> items)
    {
        _unlockedState.Clear();
        _equippedItems.Clear();

        foreach (string item in items)
        {
            _unlockedState.Add(item);
        }
    }
}