using UnityEngine;

public class AutoDoor : IInteractable
{
    [SerializeField]
    Animator _animator;
    [SerializeField]
    string _boolParameterName = "IsOpen";

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

    }
    void CloseDoors()
    {
        _animator.SetBool(_boolParameterName, false);
    }

}
