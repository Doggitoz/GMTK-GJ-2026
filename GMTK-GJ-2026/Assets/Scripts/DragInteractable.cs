using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class DragInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Vector3 planeNormal = Vector3.up;
    [SerializeField] private bool usePhysics = true;
    [SerializeField] private bool originalUseGravity;

    private Plane dragPlane;
    private Vector3 dragOffset;
    private bool isDragging;
    

    private Rigidbody rb;

    private void Awake()
    {
        targetCamera = Camera.main;

        rb = GetComponent<Rigidbody>();

        if (usePhysics && rb == null)
        {
            Debug.LogWarning($"{name}: Use Physics is enabled but no Rigidbody was found.");
            usePhysics = false;
        }
    }

    public void OnInteractorHover() { }

    public void OnInteractorLeave() { }

   

    public void OnInteractorDown(Transform interactor)
    {
        isDragging = true;

        dragPlane = new Plane(planeNormal, transform.position);
        UpdateDragOffset();

        if (usePhysics)
        {
            originalUseGravity = rb.useGravity;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    [SerializeField] private float dragSpeed = 15f;

private Vector3 targetPosition;

public void OnInteractorStay(Transform interactor)
{
    if (!isDragging)
        return;

    Vector2 pointerPos = InputSystem.actions.FindAction("Point").ReadValue<Vector2>();
    Ray ray = targetCamera.ScreenPointToRay(pointerPos);

    if (dragPlane.Raycast(ray, out float enter))
    {
        targetPosition = ray.GetPoint(enter) + dragOffset;
    }
}

private void FixedUpdate()
{
    if (!usePhysics || !isDragging)
        return;

    Vector3 newPosition = Vector3.MoveTowards(
        rb.position,
        targetPosition,
        dragSpeed * Time.fixedDeltaTime);

    rb.MovePosition(newPosition);
}

    public void OnInteractorUp(Transform interactor)
    {
        isDragging = false;

        if (usePhysics)
        {
            rb.useGravity = originalUseGravity;
        }
    }

    private void UpdateDragOffset()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = targetCamera.ScreenPointToRay(mousePos);

        if (dragPlane.Raycast(ray, out float enter))
        {
            dragOffset = transform.position - ray.GetPoint(enter);
        }
        else
        {
            dragOffset = Vector3.zero;
        }
    }
}