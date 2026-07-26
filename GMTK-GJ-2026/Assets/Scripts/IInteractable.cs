using UnityEngine;

public interface IInteractable
{
    Transform transform { get; }
    bool ShowInteractionIndicator { get; }
    void OnInteractorHover(Transform interactor);
    void OnInteractorLeave(Transform interactor);
    void OnInteractorDown(Transform interactor);
    void OnInteractorUp(Transform interactor);
    void OnInteractorStay(Transform interactor);
}
