using UnityEngine;

public class ResetClock : MonoBehaviour
{
    
    public void TriggerResetClock()
    {
        GameManager.Instance.ResetGame();
    }
}
