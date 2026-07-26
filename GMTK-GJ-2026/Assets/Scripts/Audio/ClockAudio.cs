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

        GameEvents.OnBreakClock += clockBreak;

        GameManager.Instance.OnGameStart += gameplayParameterCall;
        GameManager.Instance.OnGameStop += hubParameterCall;

    }

    private void OnDestroy()
    {
        GameEvents.OnBreakClock -= clockBreak;

        GameManager.Instance.OnGameStart -= gameplayParameterCall;
        GameManager.Instance.OnGameStop -= hubParameterCall;
    }

    private void teleportToHub(Vector3 position)
    {
        Debug.Log("Teleport to Hub");
        FMODUnity.RuntimeManager.PlayOneShot(teleportSound, transform.position);
    }

    private void teleportToClock()
    {
        Debug.Log("Teleport to clock");
        FMODUnity.RuntimeManager.PlayOneShot(teleportSound, transform.position);
    }

    private void clockBreak()
    {
        Debug.Log("Clockbreak");
        FMODUnity.RuntimeManager.PlayOneShot(clockBreakSound, transform.position);
    }

    private void gameplayParameterCall()
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("parameter:/Music_Region", 4, false);
    }

    private void hubParameterCall()
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("parameter:/Music_Region", 3, false);
    }

}
