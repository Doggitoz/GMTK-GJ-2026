using UnityEngine;

public class WindUpTeethRotator : MonoBehaviour
{
    [SerializeField] private WindUpTask _task;

    [Tooltip("Degrees per second while winding.")]
    [SerializeField] private float rotationSpeed = 180f;

    [Tooltip("Local axis to spin around")]
    [SerializeField] private Vector3 axis = Vector3.up;


    private void Update()
    {
        if (_task != null && _task.IsWinding)
        {
            transform.Rotate(axis, rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}
