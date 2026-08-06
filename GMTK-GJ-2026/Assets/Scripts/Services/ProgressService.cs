using System.Collections.Generic;

public class ProgressService
{
    private readonly HashSet<string> _flags = new();
    private readonly HashSet<string> _completedTrials = new();

    public bool CompletedTutorial => HasFlag(ProgressKeys.CompletedTutorial);
    public bool BeatGame => HasFlag(ProgressKeys.BeatGame);
    public int CompletedTrialCount => _completedTrials.Count;

    public void Reset()
    {
        _flags.Clear();
        _completedTrials.Clear();
    }

    public void LoadSaveData(SaveData saveData)
    {
        Reset();

        if (saveData == null)
        {
            return;
        }

        if (saveData.progressFlags != null)
        {
            foreach (var key in saveData.progressFlags)
            {
                if (!string.IsNullOrWhiteSpace(key))
                {
                    _flags.Add(key);
                }
            }
        }

        if (saveData.completedTrials == null)
        {
            return;
        }

        foreach (var item in saveData.completedTrials)
        {
            if (!string.IsNullOrWhiteSpace(item))
            {
                _completedTrials.Add(item);
            }
        }
    }

    public bool HasFlag(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && _flags.Contains(key);
    }

    public void SetFlag(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        _flags.Add(key);
    }

    public void ClearFlag(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        _flags.Remove(key);
    }

    public void CompleteTutorial()
    {
        SetFlag(ProgressKeys.CompletedTutorial);
    }

    public void CompleteGame()
    {
        SetFlag(ProgressKeys.BeatGame);
    }

    public void CompleteGame(IEnumerable<string> items)
    {
        CompleteGame();

        if (items == null)
        {
            return;
        }

        foreach (var item in items)
        {
            _completedTrials.Add(item);
        }
    }

    public List<string> GetProgressFlags()
    {
        return new List<string>(_flags);
    }

    public List<string> GetCompletedTrials()
    {
        return new List<string>(_completedTrials);
    }
}