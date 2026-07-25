using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class GameItems
{
    public static HashSet<string> Items = new();

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
