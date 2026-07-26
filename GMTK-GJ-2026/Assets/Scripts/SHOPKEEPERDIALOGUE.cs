using UnityEngine;

public class SHOPKEEPERDIALOGUE : MonoBehaviour
{
    public GameObject[] _shopkeepers;
    int highestDialogueSeen = 0;

    public void UpdateDialogue()
    {
        int trialCount = FindFirstObjectByType<TRIALTRACKER>().completedTrial.Count;
        if (highestDialogueSeen < trialCount)
        {
            highestDialogueSeen += 1;
            _shopkeepers[highestDialogueSeen].SetActive(true);
        } else
        {
            _shopkeepers[^1].SetActive(true);
        }
    }

}
