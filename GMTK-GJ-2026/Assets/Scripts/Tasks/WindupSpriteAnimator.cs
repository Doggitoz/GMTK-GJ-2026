using UnityEngine;
public class WindupSpriteAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] slices;

    [SerializeField]
    private WindUpTask task;
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    private int currentIndex = -1;

    private void Update()
    {
        if (task == null || slices == null || slices.Length == 0)
            return;

        float slicePercent = Mathf.Clamp01(task.DangerNormalized);

        int index = Mathf.RoundToInt(slicePercent * (slices.Length - 1));

        // Avoid assigning the same sprite every frame.
        if (index == currentIndex)
            return;

        currentIndex = index;
        spriteRenderer.sprite = slices[index];
    }

    public void SetTask(WindUpTask windUpTask)
    {
        task = windUpTask;
        currentIndex = -1; // Force refresh.
    }
}