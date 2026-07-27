using UnityEngine;
using UnityEngine.Events;

public class ZoneInteractable : MonoBehaviour, IInteractable
{
    public bool ShowInteractionIndicator => false;

    [SerializeField]
    private UnityEvent onEnter;

    [SerializeField]
    private UnityEvent onLeave;

    public void OnInteractorHover(Transform interactor)
    {
        if (!interactor.TryGetComponent<TriggerInteractor>(out var _)) return;
        onEnter?.Invoke();
    }

    public void OnInteractorLeave(Transform interactor)
    {
        if (!interactor.TryGetComponent<TriggerInteractor>(out var _)) return;
        onLeave?.Invoke();
    }

    public void OnInteractorStay(Transform interactor) { }
    public void OnInteractorUp(Transform interactor) { }
    public void OnInteractorDown(Transform interactor) { }
}
