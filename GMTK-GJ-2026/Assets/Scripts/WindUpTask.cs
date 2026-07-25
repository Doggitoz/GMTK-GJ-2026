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

    public float Danger => danger;
    public float DangerNormalized => danger / 100f; // 0..1 for the UI
    public bool Triggered { get; private set; }

    private bool _holdingInteract;
    private InputAction _moveAction;

    private void Awake()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
    }

    private void Update()
    {
        if (Triggered) return;

        float scale = (useGameTimeScale && GameManager.Instance != null) ? GameManager.Instance.DeteriorationTimeScale: 1f;
        bool standingStill = _moveAction == null || _moveAction.ReadValue<Vector2>().magnitude <= standStillThreshold;
        if (_holdingInteract && standingStill)
            danger -= windDownPerSecond * Time.deltaTime; //winding down pauses the danger rise
        else
            danger += dangerPerSecond * scale * Time.deltaTime;

        danger = Mathf.Clamp(danger, 0f, 100f);

        if (danger >= 100f)
            Trigger();

    }

    private void Trigger()
    {
        Triggered = true;
        danger = 100f;
        if (GameManager.Instance != null)
            GameManager.Instance.AddDamagePercentage(100f);
    }


    public void OnInteractorDown(Transform interactor) => _holdingInteract = true;
    public void OnInteractorUp(Transform interactor) => _holdingInteract = false;
    public void OnInteractorLeave(Transform interactor) => _holdingInteract = false;
    public void OnInteractorStay(Transform interactor) { }
    public void OnInteractorHover(Transform interactor) { }
}
