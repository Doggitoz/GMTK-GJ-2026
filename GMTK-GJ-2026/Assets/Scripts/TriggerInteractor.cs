using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class TriggerInteractor : MonoBehaviour
{
    [SerializeField]
    LayerMask _interactionMask;

    [SerializeField]
    QueryTriggerInteraction _triggerInteraction;

    InputAction _interactAction;

    private void Awake()
    {
        _interactAction = InputSystem.actions.FindAction("Interact");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<IInteractable>(out var interactable)) return;

        interactable.OnInteractorHover();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.TryGetComponent<IInteractable>(out var interactable)) return;

        if (_interactAction.WasPressedThisFrame())
        {
            interactable.OnInteractorDown(transform);
        }

        interactable.OnInteractorStay(transform);

        if (_interactAction.WasReleasedThisFrame())
        {
            interactable.OnInteractorUp(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<IInteractable>(out var interactable)) return;

        interactable.OnInteractorLeave();
    }
}
