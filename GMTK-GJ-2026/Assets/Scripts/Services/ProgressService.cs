using System.Collections.Generic;

public class ProgressService
{
    private readonly HashSet<string> _completedTrials = new();

    public bool CompletedTutorial { get; private set; }
    public bool BeatGame { get; private set; }
    public int CompletedTrialCount => _completedTrials.Count;

    public void Reset()
    {
        CompletedTutorial = false;
        BeatGame = false;
        _completedTrials.Clear();
    }

    public void LoadSaveData(Save.SaveData saveData)
    {
        Reset();

        if (saveData == null)
        {
            return;
        }

        CompletedTutorial = saveData.completedTutorial;
        BeatGame = saveData.beatGame;

        foreach (var item in saveData.completedTrial)
        {
            _completedTrials.Add(item);
        }
    }

    public void CompleteTutorial()
    {
        CompletedTutorial = true;
    }

    public void CompleteGame(IEnumerable<string> items)
    {
        BeatGame = true;

        foreach (var item in items)
        {
            _completedTrials.Add(item);
        }
    }

    public List<string> GetCompletedTrialsSaveData()
    {
        return new List<string>(_completedTrials);
    }
}