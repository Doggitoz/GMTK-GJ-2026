using UnityEngine;

public interface IInteractable
{
    void OnInteractorHover(Transform interactor);
    void OnInteractorLeave(Transform interactor);
    void OnInteractorDown(Transform interactor);
    void OnInteractorUp(Transform interactor);
    void OnInteractorStay(Transform interactor);
}
