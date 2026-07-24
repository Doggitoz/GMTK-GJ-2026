using UnityEngine;
using UnityEngine.Events;

public class BasicInteractable : MonoBehaviour, IInteractable
{
    const bool SHOULD_LOG = true;
    public UnityEvent MouseHover;
    public UnityEvent MouseLeave;
    public UnityEvent MouseDown;
    public UnityEvent MouseUp;

    public void OnMouseDown(Transform interactor)
    {
        if (SHOULD_LOG) Debug.Log("Mouse Down");
        MouseDown?.Invoke();
    }

    public void OnMouseHover()
    {
        if (SHOULD_LOG) Debug.Log("Mouse Hover");
        MouseHover?.Invoke();
    }

    public void OnMouseLeave()
    {
        if (SHOULD_LOG) Debug.Log("Mouse Leave");
        MouseLeave?.Invoke();
    }

    public void OnMouseStay(Transform interactor)
    {
        
    }

    public void OnMouseUp(Transform interactor)
    {
        if (SHOULD_LOG) Debug.Log("Mouse Up");
        MouseUp?.Invoke();
    }
}
