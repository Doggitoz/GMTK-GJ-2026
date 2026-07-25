using UnityEngine;
using UnityEngine.Pool;

public class RustSpot : MonoBehaviour, IInteractable
{
    private ObjectPool<GameObject> _pool;

    [SerializeField]
    private Transform _visuals;

    private ClockCondition ClockCondition =>
        GameManager.Instance.ClockCondition;

    [Header("Cleaning")]
    [SerializeField] private float cleanRate = 0.75f;
    [SerializeField] private float minScale = 0.05f;

    public void Initialize(ObjectPool<GameObject> pool)
    {
        _pool = pool;
    }

    public void ReturnToPool()
    {
        _pool.Release(gameObject);
    }

    public static float growthRate => GameItems.HasItem("RustSlow") ? 0.1f : 0.3f;

    private bool _isBeingCleaned;

    private float _currentDamageWorth = 0;
    private int damageIncreaseTarget = 2;

    private void OnEnable()
    {
        _visuals.localScale = Vector3.one;
        GetComponent<BoxCollider>().size = Vector3.one;
        _currentDamageWorth = 1;
        damageIncreaseTarget = 2;
        _isBeingCleaned = false;

        ClockCondition.AddDamagePercentage(1);
    }

    private void OnDisable()
    {
        ClockCondition.AddDamagePercentage(-_currentDamageWorth);

        _currentDamageWorth = 0;
        damageIncreaseTarget = 2;
        _isBeingCleaned = false;
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
    }

    public void OnInteractorLeave(Transform interactor)
    {
        _isBeingCleaned = false;
    }

    private void Update()
    {
        // Don't grow while actively cleaning.
        if (!_isBeingCleaned)
        {
            _visuals.localScale += Vector3.one *
                (growthRate * Time.deltaTime * ClockCondition.DeteriorationTimeScale);
            GetComponent<BoxCollider>().size = _visuals.localScale;

            if (_visuals.localScale.x > damageIncreaseTarget)
            {
                ClockCondition.AddDamagePercentage(1);
                _currentDamageWorth++;
                damageIncreaseTarget++;
            }
        }
    }

    private void CleanRust()
    {
        _isBeingCleaned = false;
        ReturnToPool();
    }
}