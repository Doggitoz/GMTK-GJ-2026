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
        Save.SaveManager.Instance.CompleteGame();

        if (Services.Inventory.Contains("Lucky Break"))
        {
            // Do ending sequence
        }
    }
}