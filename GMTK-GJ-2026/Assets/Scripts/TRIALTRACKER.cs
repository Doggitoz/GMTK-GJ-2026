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
        Services.Progress.CompleteGame(Services.Inventory.Items);
        Services.Game.SaveGame();

        if (Services.Inventory.HasItem("Lucky Break"))
        {
            // Do ending sequence
        }
    }
}