using UnityEngine;

public interface IInteractable
{
    void OnInteractorHover();
    void OnInteractorLeave();
    void OnInteractorDown(Transform interactor);
    void OnInteractorUp(Transform interactor);
    void OnInteractorStay(Transform interactor);
}
