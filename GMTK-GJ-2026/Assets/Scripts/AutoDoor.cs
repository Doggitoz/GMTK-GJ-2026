using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class AutoDoor : MonoBehaviour, IInteractable
{
    [SerializeField]
    Animator _animator;
    [SerializeField]
    string _boolParameterName = "IsOpen";

    [SerializeField]
    private FMODUnity.EventReference doorMoveEvent;

    void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        if (_animator == null)
        {
            Debug.LogError("No Animator Assigned to Door");
            enabled = false;
        }
    }

    public void OnInteractorDown(Transform interactor)
    {   
    }

    public void OnInteractorHover()
    {
        OpenDoors();
    }

    public void OnInteractorLeave()
    {
        CloseDoors();
    }

    public void OnInteractorStay(Transform interactor)
    {
    }

    public void OnInteractorUp(Transform interactor)
    {
    }

    void OpenDoors()
    {
        _animator.SetBool(_boolParameterName, true);
        FMODUnity.RuntimeManager.PlayOneShot(doorMoveEvent, transform.position);
    }
    void CloseDoors()
    {
        _animator.SetBool(_boolParameterName, false);
        FMODUnity.RuntimeManager.PlayOneShot(doorMoveEvent, transform.position);
    }

}
