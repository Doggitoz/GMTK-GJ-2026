using UnityEngine;
using UnityEngine.Pool;

public class RustSpot : MonoBehaviour, IInteractable
{
    private ObjectPool<GameObject> _pool;

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
        transform.localScale = Vector3.one;
        _currentDamageWorth = 1;
        damageIncreaseTarget = 2;
        _isBeingCleaned = false;

        GameManager.Instance.AddDamagePercentage(1);
    }

    private void OnDisable()
    {
        GameManager.Instance.AddDamagePercentage(-_currentDamageWorth);

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

        float delta = cleanRate * Time.deltaTime * GameManager.Instance.RepairTimeScale;

        transform.localScale -= Vector3.one * delta;

        if (transform.localScale.x <= minScale)
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
            transform.localScale += Vector3.one *
                (growthRate * Time.deltaTime * GameManager.Instance.DeteriorationTimeScale);

            if (transform.localScale.x > damageIncreaseTarget)
            {
                GameManager.Instance.AddDamagePercentage(1);
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