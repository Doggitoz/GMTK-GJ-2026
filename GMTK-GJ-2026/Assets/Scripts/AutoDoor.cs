using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class AutoDoor : MonoBehaviour, IInteractable
{
    [SerializeField]
    Animator _animator;
    [SerializeField]
    string _boolParameterName = "IsOpen";

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

    public void OnInteractorHover(Transform interactor)
    {
        if (!interactor.TryGetComponent<TriggerInteractor>(out var _)) return;
        OpenDoors();
    }

    public void OnInteractorLeave(Transform interactor)
    {
        if (!interactor.TryGetComponent<TriggerInteractor>(out var _)) return;
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

    }
    void CloseDoors()
    {
        _animator.SetBool(_boolParameterName, false);
    }

}
