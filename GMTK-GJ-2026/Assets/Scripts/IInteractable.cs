using UnityEngine;

public interface IInteractable
{
    void OnMouseHover();
    void OnMouseLeave();
    void OnMouseDown(Transform interactor);
    void OnMouseUp(Transform interactor);
    void OnMouseStay(Transform interactor);
}
