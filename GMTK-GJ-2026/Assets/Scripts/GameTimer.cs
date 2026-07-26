using Clock;
using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    [SerializeField]
    TMP_Text _timerText;

    private void Update()
    {
        UpdateTimer(TimeManager.Instance.Minutes, TimeManager.Instance.Seconds);
    }

    public void UpdateTimer(int minutes,int seconds)
    {
        Debug.Log("meow");
        int mtens = (minutes / 10) % 10;
        int mones = (minutes % 10);

        int stens = (seconds / 10) % 10;
        int sones = (seconds % 10);

        _timerText.text = $"{mtens} {mones}  {stens} {sones}";
    }
}
