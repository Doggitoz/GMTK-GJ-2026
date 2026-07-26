using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The three topic categories a player can pick after answering correctly.
/// Values also index into QuestionDialogueTrigger's topic choice buttons (0-2).
/// </summary>
public enum LoreTopic
{
    Yourself = 0,
    World = 1,
    ExtraDimensionalMetaSecrets = 2
}

/// <summary>One lore reveal - a block of dialogue lines (can be a single line or several).</summary>
[System.Serializable]
public class LoreEntry
{
    public DialogueLine[] lines;
}

/// <summary>
/// Holds the three shared, depletable pools of lore dialogue ("Yourself",
/// "The World", "Extra-dimensional Meta Secrets"). One of these lives once in
/// your scene. Fill each list in the Inspector with every possible reveal for
/// that topic; each is played once across the whole game, then discarded.
/// </summary>
public class LoreRevealManager : MonoBehaviour
{
    public static LoreRevealManager Instance { get; private set; }

    [Header("Yourself")]
    [SerializeField] private List<LoreEntry> yourselfEntries = new List<LoreEntry>();

    [Header("The World")]
    [SerializeField] private List<LoreEntry> worldEntries = new List<LoreEntry>();

    [Header("Extra-dimensional Meta Secrets")]
    [SerializeField] private List<LoreEntry> metaEntries = new List<LoreEntry>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Returns the next dialogue lines for the given topic and permanently
    /// removes that entry from the pool. Returns null if the pool is empty.
    /// </summary>
    public DialogueLine[] GetNextAndRemove(LoreTopic topic)
    {
        List<LoreEntry> pool = GetPool(topic);
        if (pool == null || pool.Count == 0)
            return null;

        DialogueLine[] lines = pool[0].lines;
        pool.RemoveAt(0);
        return lines;
    }

    public bool HasEntries(LoreTopic topic)
    {
        List<LoreEntry> pool = GetPool(topic);
        return pool != null && pool.Count > 0;
    }

    private List<LoreEntry> GetPool(LoreTopic topic)
    {
        switch (topic)
        {
            case LoreTopic.Yourself: return yourselfEntries;
            case LoreTopic.World: return worldEntries;
            case LoreTopic.ExtraDimensionalMetaSecrets: return metaEntries;
            default: return null;
        }
    }
}
