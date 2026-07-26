using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class WindUpTask : MonoBehaviour, IInteractable
{
    [Header("Danger")]
    [Tooltip("Current danger, 0-100. Rises on its own; wind it down to survive.")]
    [SerializeField] private float danger = 0f;
    [Tooltip("Danger gained per second while not being wound down.")]
    [SerializeField] private float dangerPerSecond = 2f; // 100 / 2 = 50s to destruction
    [Tooltip("Danger removed per second while winding down.")]
    [SerializeField] private float windDownPerSecond = 12f;

    [Header("Wind-down conditions")]
    [Tooltip("Player counts as 'still' when their move input is below this.")]
    [SerializeField] private float standStillThreshold = 0.1f;
    [Tooltip("Scale the danger rise by GameManager.TimeScale.")]
    [SerializeField] private bool useGameTimeScale = true;

    [SerializeField] private float damageOnFail = 100f;

    public float Danger => danger;
    public float DangerNormalized => danger / 100f; // 0..1 for the UI
    public bool IsWinding => _windupStarted;

    private bool _holdingInteract;
    private bool _windupStarted;
    private InputAction _moveAction;
    private GameManager _gameManager => GameManager.Instance;
    private Clock.Condition _clockCondition => _gameManager.ClockCondition;

    public bool ShowInteractionIndicator => true;

    [SerializeField] private FMODUnity.EventReference windupSoundEvent;
    private FMOD.Studio.EventInstance windupSound;

    private void Awake()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
    }

    private void Start()
    {
        windupSound = FMODUnity.RuntimeManager.CreateInstance(windupSoundEvent);
        _windupStarted = false;
    }
    bool _hasDamaged;

    private void Update()
    {
        if (!_gameManager.GameActive)
        {
            _windupStarted = false;
            windupSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            return;
        };
        float deteriorationScale = (useGameTimeScale) ? _clockCondition.DeteriorationTimeScale : 1f;
        bool standingStill = _moveAction == null || _moveAction.ReadValue<Vector2>().magnitude <= standStillThreshold;
        if (_holdingInteract && standingStill)
        {
            danger -= windDownPerSecond * Time.deltaTime * _clockCondition.RepairTimeScale; //winding down pauses the danger rise
            if (!_windupStarted)
            {
                if (_hasDamaged)
                {
                    _clockCondition.AddDamagePercentage(-damageOnFail);
                    _hasDamaged = false;
                }
                windupSound.start();
                _windupStarted = true;
            }
        }
        else
        {
            danger += dangerPerSecond * deteriorationScale * Time.deltaTime;
            windupSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _windupStarted = false;
        }
        danger = Mathf.Clamp(danger, 0f, 100f);

        if (danger >= 100f && !_hasDamaged)
        {
            _clockCondition.AddDamagePercentage(damageOnFail);
            _hasDamaged = true;
        }
            

    }

    public void OnInteractorDown(Transform interactor) => _holdingInteract = true;
    public void OnInteractorUp(Transform interactor) => _holdingInteract = false;
    public void OnInteractorLeave(Transform interactor) => _holdingInteract = false;
    public void OnInteractorStay(Transform interactor) { }
    public void OnInteractorHover(Transform interactor) { }
}
