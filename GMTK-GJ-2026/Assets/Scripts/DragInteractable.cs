using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DragInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Vector3 planeNormal = Vector3.up;

    private bool isDragging;
    private Transform currentInteractor;

    private Plane dragPlane;
    private Vector3 dragOffset;

    public void OnInteractorHover()
    {
        // Optional: Highlight object
    }

    public void OnInteractorLeave()
    {
        // Optional: Remove highlight
    }

    public void OnInteractorDown(Transform interactor)
    {
        currentInteractor = interactor;
        isDragging = true;

        // Plane passing through the object's current position.
        dragPlane = new Plane(planeNormal, transform.position);

        Ray ray = new Ray(interactor.position, interactor.forward);

        if (dragPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            dragOffset = transform.position - hitPoint;
        }
        else
        {
            dragOffset = Vector3.zero;
        }
    }

    public void OnInteractorStay(Transform interactor)
    {
        Debug.Log("test");
        if (!isDragging)
            return;

        Ray ray = new Ray(interactor.position, interactor.forward);

        if (dragPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            transform.position = hitPoint + dragOffset;
        }
    }

    public void OnInteractorUp(Transform interactor)
    {
        ResetDrag();
    }

    private void ResetDrag()
    {
        isDragging = false;
        currentInteractor = null;
    }
}