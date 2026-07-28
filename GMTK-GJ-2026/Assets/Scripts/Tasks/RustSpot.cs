using UnityEngine;
using UnityEngine.Pool;

public class RustSpot : MonoBehaviour, IInteractable
{
    private ObjectPool<GameObject> _pool;

    [SerializeField]
    private Transform _visuals;

    private Clock.Condition ClockCondition =>
        GameManager.Instance.ClockCondition;
    private GameManager _gameManager => GameManager.Instance;

    [Header("Cleaning")]
    [SerializeField] private float cleanRate = 0.75f;
    [SerializeField] private float minScale = 0.05f;
    [SerializeField] private float maxScale = 3f;
    [SerializeField] private int damagePerRust = 5;

    [SerializeField] private FMODUnity.EventReference rustCleaningSoundEvent;
    private FMOD.Studio.EventInstance rustCleaningSound;

    public void Initialize(ObjectPool<GameObject> pool)
    {
        _pool = pool;
    }

    public void ReturnToPool()
    {
        _pool.Release(gameObject);
    }

    public static float growthRate => Services.Inventory.Contains("RustSlow") ? 0.1f : 0.3f;

    public bool ShowInteractionIndicator => true;

    private bool _isBeingCleaned;
    private bool _cleaningStarted;

    private float _currentDamageWorth = 0;
    private int damageIncreaseTarget = 2;

    private void OnEnable()
    {
        _visuals.localScale = Vector3.one;
        GetComponent<BoxCollider>().size = Vector3.one;
        _currentDamageWorth = damagePerRust;
        damageIncreaseTarget = 2;
        _isBeingCleaned = false;
        _cleaningStarted = false;

        ClockCondition.AddDamagePercentage(damagePerRust);
        rustCleaningSound = FMODUnity.RuntimeManager.CreateInstance(rustCleaningSoundEvent);
    }

    private void OnDisable()
    {
        ClockCondition.AddDamagePercentage(-_currentDamageWorth);
        rustCleaningSound.release();

        _currentDamageWorth = 0;
        damageIncreaseTarget = 2;
        _isBeingCleaned = false;
        _cleaningStarted = false;
    }

    public void OnInteractorHover(Transform interactor)
    {
    }

    public void OnInteractorDown(Transform interactor)
    {
        _isBeingCleaned = true;
    }

    public void OnInteractorStay(Transform interactor)
    {
        if (!_isBeingCleaned)
            return;
        if (!_cleaningStarted)
        {
            rustCleaningSound.start();
            _cleaningStarted = true;
        }

        float delta = cleanRate * Time.deltaTime * ClockCondition.RepairTimeScale;

        _visuals.localScale -= Vector3.one * delta;

        if (_visuals.localScale.x <= minScale)
        {
            CleanRust();
        }
    }

    public void OnInteractorUp(Transform interactor)
    {
        _isBeingCleaned = false;
        rustCleaningSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _cleaningStarted = false;
    }

    public void OnInteractorLeave(Transform interactor)
    {
        _isBeingCleaned = false;
        rustCleaningSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _cleaningStarted = false;
    }

    private void Update()
    {
        if (!_gameManager.GameActive) return;
        // Don't grow while actively cleaning.
        if (!_isBeingCleaned)
        {
            _visuals.localScale += Vector3.one *
                (growthRate * Time.deltaTime * ClockCondition.DeteriorationTimeScale);
            _visuals.localScale = Vector3.Min(_visuals.localScale, Vector3.one * maxScale);
            GetComponent<BoxCollider>().size = _visuals.localScale;

            if (_visuals.localScale.x > damageIncreaseTarget)
            {
                ClockCondition.AddDamagePercentage(damagePerRust);
                _currentDamageWorth += damagePerRust;
                damageIncreaseTarget++;
            }
        }
    }

    private void CleanRust()
    {
        _isBeingCleaned = false;
        rustCleaningSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _cleaningStarted = false;
        ReturnToPool();
    }
}