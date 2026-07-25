using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class DragInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Vector3 planeNormal = Vector3.up;
    [SerializeField] private bool usePhysics = true;

    [SerializeField] private float positionStrength = 250f;
    [SerializeField] private float positionDamping = 35f;
    [SerializeField] private float maxForce = 1000f;

    [SerializeField] private float angularDamping = 25f;

    private Vector3 targetPosition;
    private Vector3 dragOffset;
    private Plane dragPlane;
    
    private bool isDragging;
    private bool originalUseGravity;

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

    public void OnInteractorHover(Transform interactor) { }

    public void OnInteractorLeave(Transform interactor) { }

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

        if (!usePhysics)
        {
            transform.position = targetPosition; 
        }
    }

    private void FixedUpdate()
    {
        if (!usePhysics || !isDragging)
            return;

        Vector3 error = targetPosition - rb.position;

        // PD controller
        Vector3 force = error * positionStrength
                      - rb.linearVelocity * positionDamping;

        // Prevent ridiculous impulses
        if (force.sqrMagnitude > maxForce * maxForce)
            force = force.normalized * maxForce;

        rb.AddForce(force, ForceMode.Force);

        // Damp rotation so it doesn't spin uncontrollably.
        rb.AddTorque(-rb.angularVelocity * angularDamping, ForceMode.Acceleration);
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