using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace CMF
{
    //A very simplified controller script;
    //This script is an example of a very simple walker controller that covers only the basics of character movement;
    public class SimpleWalkerController : Controller
    {
        [SerializeField]
        private CinemachineCamera _playerCamera;

        [SerializeField]
        private CinemachineCamera _fadeCamera;

        [SerializeField]
        private Animator _playerAnimator;

        [SerializeField]
        private string _teleportBoolParameter = "IsAsleep";

        private Coroutine _teleportRoutine;

        private Mover mover;
        float currentVerticalSpeed = 0f;
        bool isGrounded;
        private bool _isTeleporting = false;
        public float movementSpeed = 7f;
        public float jumpSpeed = 10f;
        public float gravity = 10f;

        [SerializeField] private FMODUnity.EventReference jumpSoundEvent;
        [SerializeField] private FMODUnity.EventReference landSoundEvent;
        [SerializeField] private FMODUnity.ParamRef muteFootSounds;

        Vector3 lastVelocity = Vector3.zero;

        public Transform cameraTransform;
        InputAction moveAction;
        InputAction jumpAction;
        Transform tr;

        // Use this for initialization
        void Start()
        {
            moveAction = InputSystem.actions.FindAction("Move");
            jumpAction = InputSystem.actions.FindAction("Jump");

            GameEvents.OnPlayerTeleportRequested += Teleport;
            GameEvents.OnPlayerHardTeleportRequested += HardTeleport;
        }

        private void Awake()
        {
            tr = transform;
            mover = GetComponent<Mover>();
            GameManager.Instance.OnLoadSave += OnSaveLoaded;
        }

        void FixedUpdate()
        {
            mover.CheckForGround();

            if (!isGrounded && mover.IsGrounded())
                OnGroundContactRegained(lastVelocity);

            isGrounded = mover.IsGrounded();

            Vector3 velocity = Vector3.zero;

            // Only allow movement when not teleporting
            if (!_isTeleporting)
            {
                velocity += CalculateMovementDirection() * movementSpeed;

                if (isGrounded &&
                    GameManager.Instance.PlayerControllerEnabled &&
                    jumpAction.IsPressed() &&
                    !GameItems.HasItem("God’s Femur"))
                {
                    OnJumpStart();
                    currentVerticalSpeed = jumpSpeed;
                    isGrounded = false;
                }
            }

            // Gravity always runs
            if (!isGrounded)
            {
                currentVerticalSpeed -= gravity * Time.deltaTime;
            }
            else if (currentVerticalSpeed < 0f)
            {
                currentVerticalSpeed = 0f;
            }

            velocity += tr.up * currentVerticalSpeed;

            lastVelocity = velocity;

            mover.SetExtendSensorRange(isGrounded);
            mover.SetVelocity(velocity);
        }

        private void Teleport(Vector3 newPosition)
        {
            if (_teleportRoutine != null)
            {
                StopCoroutine(_teleportRoutine);
            }

            _teleportRoutine = StartCoroutine(TeleportRoutine(newPosition));
        }

        private IEnumerator TeleportRoutine(Vector3 newPosition)
        {
            _isTeleporting = true;

            if (_playerAnimator != null)
            {
                // Start Idle -> Stand Up -> Sleep sequence
                _playerAnimator.SetBool(_teleportBoolParameter, true);

                // Wait until the sleep animation is reached
                yield return new WaitUntil(() =>
                {
                    AnimatorStateInfo state = _playerAnimator.GetCurrentAnimatorStateInfo(0);

                    return state.IsName("Sleep") ||
                           state.IsName("Base Layer.Sleep");
                });

                // Stay Sleep before fading
                yield return new WaitForSeconds(2f);
            }

            GameManager.Instance.SetPlayerActive(false);

            // Fade to black
            if (_fadeCamera != null)
            {
                _fadeCamera.Priority = 2;
                _fadeCamera.Prioritize();
            }

            // Give fade time to complete
            yield return new WaitForSeconds(1.5f);


            // Actual teleport
            currentVerticalSpeed = 0f;
            lastVelocity = Vector3.zero;

            mover.Teleport(newPosition);

            yield return new WaitForFixedUpdate();


            // Reset camera position
            if (_playerCamera != null)
            {
                Vector3 targetPos = new Vector3(
                    0,
                    transform.position.y + 5,
                    transform.position.z - 10
                );

                _playerCamera.ForceCameraPosition(
                    targetPos,
                    _playerCamera.transform.rotation
                );
            }


            // Unfade
            if (_fadeCamera != null)
            {
                _fadeCamera.Priority = -1;
            }

            // Wait 2 seconds after fade finishes before waking up
            yield return new WaitForSeconds(2f);


            // Trigger Sleep -> Stand Up -> Idle
            if (_playerAnimator != null)
            {
                _playerAnimator.SetBool(_teleportBoolParameter, false);

                // Give animator a frame to process transition
                yield return null;

                // Wait until back at Idle
                yield return new WaitUntil(() =>
                {
                    AnimatorStateInfo state = _playerAnimator.GetCurrentAnimatorStateInfo(0);
                    return state.IsName("Idle");
                });
            }


            _isTeleporting = false;

            GameManager.Instance.SetPlayerActive(true);

            GameEvents.CompletePlayerTeleport();
        }

        public void OnSaveLoaded()
        {
            HardTeleport(GameManager.HubSpawnLocation);
        }

        public void HardTeleport(Vector3 newPosition)
        {
            _isTeleporting = true;

            currentVerticalSpeed = 0f;
            lastVelocity = Vector3.zero;

            mover.Teleport(newPosition);

            // Force camera to update immediately
            if (_playerCamera != null)
            {
                Vector3 targetPos = new Vector3(
                    0,
                    transform.position.y + 5,
                    transform.position.z - 10
                );

                _playerCamera.ForceCameraPosition(
                    targetPos,
                    _playerCamera.transform.rotation
                );
            }

            _isTeleporting = false;

            GameEvents.CompletePlayerTeleport();
        }
        private Vector3 CalculateMovementDirection()
        {
            if (!GameManager.Instance.PlayerControllerEnabled) return Vector3.zero;
            //If no character input script is attached to this object, return no input;
            //if (characterInput == null)
                //return Vector3.zero;

            Vector3 _direction = Vector3.zero;
            var input = moveAction.ReadValue<Vector2>();
            //If no camera transform has been assigned, use the character's transform axes to calculate the movement direction;
            if (cameraTransform == null)
            {
                _direction += tr.right * input.x;//characterInput.GetHorizontalMovementInput();
                _direction += tr.forward * input.y;
            }
            else
            {
                //If a camera transform has been assigned, use the assigned transform's axes for movement direction;
                //Project movement direction so movement stays parallel to the ground;
                _direction += Vector3.ProjectOnPlane(cameraTransform.right, tr.up).normalized * input.x; //characterInput.GetHorizontalMovementInput();
                _direction += Vector3.ProjectOnPlane(cameraTransform.forward, tr.up).normalized * input.y; //characterInput.GetVerticalMovementInput();
            }

            //If necessary, clamp movement vector to magnitude of 1f;
            if (_direction.magnitude > 1f)
                _direction.Normalize();

            return _direction;
        }

        //This function is called when the controller has landed on a surface after being in the air;
        void OnGroundContactRegained(Vector3 _collisionVelocity)
        {
            Debug.Log("on land");
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MuteFootsteps", 1, false);
            FMODUnity.RuntimeManager.PlayOneShot(landSoundEvent, transform.position);

            //Call 'OnLand' delegate function;
            if (OnLand != null)
                OnLand(_collisionVelocity);
                
        }

        //This function is called when the controller has started a jump;
        void OnJumpStart()
        {
            Debug.Log("on jump");
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MuteFootsteps", 0, false);
            FMODUnity.RuntimeManager.PlayOneShot(jumpSoundEvent, transform.position);
            //Call 'OnJump' delegate function;
            if (OnJump != null)
                OnJump(lastVelocity);
                
        }

        //Return the current velocity of the character;
        public override Vector3 GetVelocity()
        {
            return lastVelocity;
        }

        //Return only the current movement velocity (without any vertical velocity);
        public override Vector3 GetMovementVelocity()
        {
            return lastVelocity;
        }

        //Return whether the character is currently grounded;
        public override bool IsGrounded()
        {
            return isGrounded;
        }

        private void OnDestroy()
        {
            GameEvents.OnPlayerTeleportRequested -= Teleport;
        }

    }

}
