using System.Collections.Generic;
using UnityEngine;

public class TRIALTRACKER : MonoBehaviour
{
    public static HashSet<string> completedTrial = new();

    public void OnCompleteGame()
    {
        foreach (var item in GameItems.Items)
        {
            completedTrial.Add(item);
        }
    }
}
