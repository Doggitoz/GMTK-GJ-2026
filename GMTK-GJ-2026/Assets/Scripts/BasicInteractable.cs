using UnityEngine;
using UnityEngine.Events;

public class BasicInteractable : MonoBehaviour, IInteractable
{
    const bool SHOULD_LOG = true;
    public UnityEvent MouseHover;
    public UnityEvent MouseLeave;
    public UnityEvent MouseDown;
    public UnityEvent MouseUp;

    public bool ShowInteractionIndicator => true;

    public void OnInteractorDown(Transform interactor)
    {
        if (SHOULD_LOG) Debug.Log("Mouse Down");
        MouseDown?.Invoke();
    }

    public void OnInteractorHover(Transform interactor)
    {
        if (SHOULD_LOG) Debug.Log("Mouse Hover");
        MouseHover?.Invoke();
    }

    public void OnInteractorLeave(Transform interactor)
    {
        if (SHOULD_LOG) Debug.Log("Mouse Leave");
        MouseLeave?.Invoke();
    }

    public void OnInteractorStay(Transform interactor)
    {
        
    }

    public void OnInteractorUp(Transform interactor)
    {
        if (SHOULD_LOG) Debug.Log("Mouse Up");
        MouseUp?.Invoke();
    }
}
