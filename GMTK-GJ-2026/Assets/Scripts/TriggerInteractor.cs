using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class TriggerInteractor : MonoBehaviour
{
    [SerializeField]
    Transform _interactionVisual;

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
        PruneInteractables();

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
            if (interactable != null)
                interactable.OnInteractorStay(transform);
        }

        if (wasReleasedThisFrame)
        {
            foreach (var interactable in _selectedInteractables)
            {
                if (interactable != null)
                    interactable.OnInteractorUp(transform);
            }

            _selectedInteractables.Clear();
        }

        _interactionVisual.gameObject.SetActive(_currentInteractables != null && _currentInteractables.Count > 0);
    }
    private readonly List<IInteractable> _toRemove = new();

    private void PruneInteractables()
    {
        _toRemove.Clear();

        foreach (var interactable in _currentInteractables)
        {
            // Handles destroyed Unity objects
            if (interactable == null)
            {
                _toRemove.Add(interactable);
                continue;
            }

            // Handles disabled GameObjects or disabled parents
            if (!interactable.transform.gameObject.activeInHierarchy)
            {
                if (_selectedInteractables.Remove(interactable))
                    interactable.OnInteractorUp(transform);

                interactable.OnInteractorLeave(transform);
                _toRemove.Add(interactable);
            }
        }

        foreach (var interactable in _toRemove)
            _currentInteractables.Remove(interactable);

        _toRemove.Clear();

        // Clean up selected objects that were destroyed without being in _currentInteractables.
        foreach (var interactable in _selectedInteractables)
        {
            if (interactable == null)
                _toRemove.Add(interactable);
        }

        foreach (var interactable in _toRemove)
            _selectedInteractables.Remove(interactable);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<IInteractable>(out var interactable)) return;

        interactable.OnInteractorLeave(transform);
        _currentInteractables.Remove(interactable);
    }

    public bool IsInteracting()
    {
        return _selectedInteractables.Count > 0;
    }
}
