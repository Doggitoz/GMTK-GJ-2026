using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class TriggerInteractor : MonoBehaviour
{
    InputAction _interactAction;

    HashSet<IInteractable> _currentInteractables = new();
    HashSet<IInteractable> _selectedInteractables = new();

    private void Awake()
    {
        _interactAction = InputSystem.actions.FindAction("Interact");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<IInteractable>(out var interactable)) return;

        interactable.OnInteractorHover(transform);
        _currentInteractables.Add(interactable);
    }

    private void Update()
    {
        bool wasPressedThisFrame = _interactAction.WasPressedThisFrame();
        bool wasReleasedThisFrame = _interactAction.WasReleasedThisFrame();

        if (wasPressedThisFrame)
        {
            foreach (var interactable in _currentInteractables)
            {
                interactable.OnInteractorDown(transform);
                _selectedInteractables.Add(interactable);
            }
        }

        foreach (var interactable in _selectedInteractables)
        {
            interactable.OnInteractorStay(transform);
        }

        if (wasReleasedThisFrame)
        {
            foreach (var interactable in _selectedInteractables)
            {
                if (interactable == null) continue;
                interactable.OnInteractorUp(transform);
            }
            _selectedInteractables.Clear();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<IInteractable>(out var interactable)) return;

        interactable.OnInteractorLeave(transform);
        _currentInteractables.Remove(interactable);
    }
}
