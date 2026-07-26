using UnityEngine;
using CMF;
public class StopMovementOnInteract : MonoBehaviour
{
    [SerializeField]
    SimpleWalkerController _controller;

    [SerializeField]
    private TriggerInteractor _interactor;

    bool isStopped = false;
    float previousMovementSpeed;
    float previousJumpSpeed;
    private void Update()
    {
        if (!isStopped && _interactor.IsInteracting())
        {
            StopMovement();
        } else if (!_interactor.IsInteracting() && isStopped)
        {
            ResumeMovement();
        }
    }
    public void StopMovement()
    {
        previousMovementSpeed = _controller.movementSpeed;
        previousJumpSpeed = _controller.jumpSpeed;

        _controller.movementSpeed = 0;
        _controller.jumpSpeed = 0;
        isStopped = true;
    }


    public void ResumeMovement()
    {
        _controller.movementSpeed = previousMovementSpeed;
        _controller.jumpSpeed = previousJumpSpeed;
        isStopped = false;
    }
}
