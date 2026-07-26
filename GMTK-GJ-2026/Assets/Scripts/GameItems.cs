using System.Collections.Generic;
using UnityEngine;

public enum ItemStat { ClockSpeed, Deterioration, Repair }

public static class GameItems
{
    public static HashSet<string> Items = new();

    private static readonly Dictionary<string, Dictionary<ItemStat, float>> Effects = new()
    {
        ["Gambler’s Bet"] = new() { [ItemStat.ClockSpeed] = 2f, [ItemStat.Repair] = 2f },
        ["Lucky Break"] = new() { [ItemStat.Deterioration] = 2 }
    };

    public static IReadOnlyDictionary<ItemStat, float> GetEffects(string name) => Effects.TryGetValue(name, out var e) ? e : null;

    public static float GetMultiplier(ItemStat stat)
    {
        float m = 1f; // if not effect it will multiply by 1 (stay the same)
        foreach (var item in Items)
        {
            if (Effects.TryGetValue(item, out var effects) && effects.TryGetValue(stat, out var factor))
            {
                m *= factor;
            }
        }

        return m;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void Reset()
    {
        Items.Clear();
    }

    public static void AddItem(string name)
    {
        Items.Add(name);
    }

    public static void RemoveItem(string name)
    {
        if (Items.Contains(name))
        {
            Items.Remove(name);
        }

    }

    public static bool HasItem(string name)
    {
        return Items.Contains(name);
    }
}
