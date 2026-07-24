using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] 
    private float moveSpeed = 5f;

    [SerializeField]
    private float runSpeed = 10f;
    
    InputAction _moveAction;
    InputAction _sprintAction;
    CharacterController _controller;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        _moveAction = InputSystem.actions.FindAction("Move");
        _sprintAction = InputSystem.actions.FindAction("Sprint");
    }

    private void Update()
    {
        Vector2 input = _moveAction.ReadValue<Vector2>();

        float speedMultiplier = _sprintAction.IsPressed() ? runSpeed : moveSpeed;

        // Convert 2D input into 3D movement (X/Z)
        Vector3 movement = new Vector3(input.x, Physics.gravity.y, input.y);

        _controller.Move(movement * speedMultiplier * Time.deltaTime);
    }
}
