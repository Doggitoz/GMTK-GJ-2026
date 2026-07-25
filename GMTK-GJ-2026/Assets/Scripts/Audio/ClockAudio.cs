using UnityEngine;

public class ClockAudio : MonoBehaviour
{

    [SerializeField]
    private FMODUnity.EventReference tickSound;
    [SerializeField]
    private FMODUnity.EventReference music;
    private bool firstTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        Clock.TimeManager.Instance.OnSecondChanged += CallTickSound;
        firstTime = true;
    }

    private void OnDestroy()
    {
        Clock.TimeManager.Instance.OnSecondChanged -= CallTickSound;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CallTickSound(int second)
    {
        FMODUnity.RuntimeManager.PlayOneShot(tickSound, transform.position);
        if (firstTime & second == 59)
        {
            FMODUnity.RuntimeManager.PlayOneShot(music, transform.position);
            firstTime = false;
        }

    }
}
