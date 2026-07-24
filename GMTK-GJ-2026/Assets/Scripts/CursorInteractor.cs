using UnityEngine;
using UnityEngine.InputSystem;

public class CursorInteractor : MonoBehaviour
{
    [SerializeField]
    LayerMask _interactionMask;

    [SerializeField]
    QueryTriggerInteraction _triggerInteraction;

    IInteractable _currentInteractable;

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
            _currentInteractable?.OnMouseLeave();
            _currentInteractable = hoveredInteractable;
            _currentInteractable?.OnMouseHover();
            hoveredThisFrame = true;
        }

        if (_currentInteractable == null) return;

        if (!hoveredThisFrame)
            _currentInteractable.OnMouseStay(transform);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            _currentInteractable.OnMouseDown(transform);
        } else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            _currentInteractable.OnMouseUp(transform);
        }
    }

    public IInteractable RaycastForInteractable()
    {
        if (_camera != null)
        {
            RaycastHit hit;
            Ray rayOrigin = _camera.ScreenPointToRay(Pointer.current.position.value, Camera.MonoOrStereoscopicEye.Mono);
            if (Physics.Raycast(rayOrigin.origin, rayOrigin.direction, out hit, 10f, _interactionMask, _triggerInteraction)) {
                return hit.transform.GetComponent<IInteractable>();
            }
        }
        return null;
    }
}
