using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class AutoDoor : MonoBehaviour
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
    public void OpenDoors()
    {
        _animator.SetBool(_boolParameterName, true);

    }
    public void CloseDoors()
    {
        _animator.SetBool(_boolParameterName, false);
    }

}
