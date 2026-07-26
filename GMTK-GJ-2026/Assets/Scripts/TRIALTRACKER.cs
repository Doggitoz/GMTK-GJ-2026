using UnityEngine;

public class TRIALTRACKER : MonoBehaviour
{
    private void Start()
    {
        GameEvents.OnWin += OnCompleteGame;
    }

    private void OnDestroy()
    {
        GameEvents.OnWin -= OnCompleteGame;
    }

    public void OnCompleteGame()
    {
        Save.Manager.Instance.CompleteGame();

        if (GameItems.Items.Contains("Lucky Break"))
        {
            // Do ending sequence
        }
    }
}