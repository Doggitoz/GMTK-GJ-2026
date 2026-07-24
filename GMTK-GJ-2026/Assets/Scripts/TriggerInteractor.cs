using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class TriggerInteractor : MonoBehaviour
{
    InputAction _interactAction;

    HashSet<IInteractable> _currentInteractables = new();

    private void Awake()
    {
        _interactAction = InputSystem.actions.FindAction("Interact");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<IInteractable>(out var interactable)) return;

        interactable.OnInteractorHover();
        _currentInteractables.Add(interactable);
    }

    private void Update()
    {
        bool wasPressedThisFrame = _interactAction.WasPressedThisFrame();
        bool wasReleasedThisFrame = _interactAction.WasReleasedThisFrame();

        foreach (var interactable in _currentInteractables)
        {
            if (wasPressedThisFrame)
            {
                interactable.OnInteractorDown(transform);

            }

            interactable.OnInteractorStay(transform);

            if (wasReleasedThisFrame)
            {
                interactable.OnInteractorUp(transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<IInteractable>(out var interactable)) return;

        interactable.OnInteractorLeave();
        _currentInteractables.Remove(interactable);
    }
}
