using CMF;
using UnityEngine;


public class PlayerAnimations : MonoBehaviour
{
    [SerializeField]
    private SimpleWalkerController _playerController;
    [SerializeField]
    private TriggerInteractor _interactor;
    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private string IsGroundedBoolParameter;
    [SerializeField]
    private string IsMovingBoolParameter;
    [SerializeField]
    private string IsInteractingBoolParameter;



    private void Awake()
    {
        if (_animator == null)
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        _animator.SetBool(IsGroundedBoolParameter, _playerController.IsGrounded());
        _animator.SetBool(IsMovingBoolParameter, _playerController.GetMovementVelocity().sqrMagnitude > 0);
        _animator.SetBool(IsInteractingBoolParameter, _interactor.IsInteracting());

        if (_animator.GetBool("IsAsleep"))
        {
            _playerController.
        }
    }
}
