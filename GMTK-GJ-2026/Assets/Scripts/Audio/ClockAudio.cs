using UnityEngine;

public class ClockAudio : MonoBehaviour
{

    [SerializeField]
    private FMODUnity.EventReference teleportSound;
    [SerializeField]
    private FMODUnity.EventReference clockBreakSound;
    private bool firstTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        GameEvents.OnLose += teleport;
        GameEvents.OnWin += teleport;
        GameManager.Instance.OnTutorialStart += teleport;
        GameEvents.OnBreakClock += clockBreak;

    }

    private void OnDestroy()
    {
        GameEvents.OnLose -= teleport;
        GameEvents.OnWin -= teleport;
        GameManager.Instance.OnTutorialStart -= teleport;
    }

    private void teleport()
    {
        FMODUnity.RuntimeManager.PlayOneShot(teleportSound, transform.position);
    }

    private void clockBreak()
    {
        FMODUnity.RuntimeManager.PlayOneShot(clockBreakSound, transform.position);
    }


}
