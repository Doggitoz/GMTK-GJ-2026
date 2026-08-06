using System.Collections.Generic;

public static class ItemDatabase
{
    private static readonly Dictionary<string, Dictionary<Enums.ItemStat, float>> Effects = new()
    {
        ["Gambler’s Bet"] = new()
        {
            [Enums.ItemStat.ClockSpeed] = 2f,
            [Enums.ItemStat.Repair] = 2f
        },

        ["Lucky Break"] = new()
        {
            [Enums.ItemStat.Deterioration] = 2f
        }
    };


    public static IReadOnlyDictionary<Enums.ItemStat, float> GetEffects(string name)
    {
        return Effects.TryGetValue(name, out var effects)
            ? effects
            : null;
    }
}