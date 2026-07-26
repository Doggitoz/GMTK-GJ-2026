using UnityEngine;
using UnityEngine.UI;

public class DangerMeterUI : MonoBehaviour
{
    [SerializeField] private WindUpTask task; //the task to display
    [SerializeField] private Slider bar; // min 0, max 1
    [SerializeField] private Text percentLabel;

    // Update is called once per frame
    void Update()
    {
        if (task == null) return;

        float normalized = task.DangerNormalized;
        if (bar != null) bar.value = normalized;
        if (percentLabel != null)
            percentLabel.text = Mathf.RoundToInt(normalized * 100f) + "%";
    }
}
