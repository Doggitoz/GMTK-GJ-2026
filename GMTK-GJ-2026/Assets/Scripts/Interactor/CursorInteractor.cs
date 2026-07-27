using UnityEngine;
using UnityEngine.InputSystem;

public class CursorInteractor : MonoBehaviour
{
    [SerializeField]
    private float _interactionDistance = 10f;

    [SerializeField]
    LayerMask _interactionMask;

    [SerializeField]
    QueryTriggerInteraction _triggerInteraction;

    IInteractable _currentInteractable;
    IInteractable _selectedInteractable;

    private Camera _camera;
    
    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (_camera == null) return;

        bool hoveredThisFrame = false;

        var hoveredInteractable = RaycastForInteractable();

        if (hoveredInteractable != _currentInteractable)
        {
            _currentInteractable?.OnInteractorLeave(transform);
            _currentInteractable = hoveredInteractable;
            _currentInteractable?.OnInteractorHover(transform);
            hoveredThisFrame = true;
        }

        if (!hoveredThisFrame)
            _selectedInteractable?.OnInteractorStay(transform);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            _selectedInteractable = _currentInteractable;
            _selectedInteractable?.OnInteractorDown(transform);
            
           
        } else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            _selectedInteractable?.OnInteractorUp(transform);
            _selectedInteractable = null;
        }
    }

    public IInteractable RaycastForInteractable()
    {
        if (_camera != null)
        {
            RaycastHit hit;
            Ray rayOrigin = _camera.ScreenPointToRay(Pointer.current.position.value, Camera.MonoOrStereoscopicEye.Mono);
            if (Physics.Raycast(rayOrigin.origin, rayOrigin.direction, out hit, _interactionDistance, _interactionMask, _triggerInteraction)) {
                return hit.transform.GetComponent<IInteractable>();
            }
        }
        return null;
    }
}
