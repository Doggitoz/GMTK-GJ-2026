using UnityEngine;

public class AnimationSoundEffects : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private FMODUnity.EventReference footstepEvent;
    [SerializeField] private FMODUnity.EventReference standUpEvent;
    [SerializeField] private FMODUnity.EventReference inhaleEvent;
    [SerializeField] private FMODUnity.EventReference exhaleEvent;

    public void OnFootstep()
    {
        FMODUnity.RuntimeManager.PlayOneShot(footstepEvent, transform.position);
    }

    public void OnStandUp()
    {
        FMODUnity.RuntimeManager.PlayOneShot(standUpEvent, transform.position);
    }

    public void OnInhale()
    {
        FMODUnity.RuntimeManager.PlayOneShot(inhaleEvent, transform.position);
        FMODUnity.RuntimeManager.PlayOneShot(standUpEvent, transform.position);
    }

    public void OnExhale()
    {
        FMODUnity.RuntimeManager.PlayOneShot(exhaleEvent, transform.position);
        FMODUnity.RuntimeManager.PlayOneShot(standUpEvent, transform.position);
    }
}
