using UnityEngine;
using UnityEngine.Pool;

public class RustSpot : MonoBehaviour, IInteractable
{
    private ObjectPool<GameObject> _pool;

    public void Initialize(ObjectPool<GameObject> pool)
    {
        _pool = pool;
    }

    public void ReturnToPool()
    {
        _pool.Release(gameObject);
    }
    public static float growthRate = .02f;
    bool FullClick = false;

    float _currentDamageWorth = 0;

    private void Awake()
    {
        // Add itself to damage pool
        GameManager.Instance.AddDamagePercentage(1);
        _currentDamageWorth += 1;
    }

    public void OnInteractorDown(Transform interactor)
    {
        FullClick = true;
    }

    public void OnInteractorHover(Transform interactor)
    {
        
    }

    public void OnInteractorLeave(Transform interactor)
    {
        FullClick = false;
    }

    public void OnInteractorStay(Transform interactor)
    {
       
    }

    public void OnInteractorUp(Transform interactor)
    {
        if (FullClick)
        {
            StartClean();
        }
    }

    public void StartClean()
    {
        // Add logic here for minigame; have minigame call "CleanRust"
        CleanRust();
    }
    int damageIncreaseTarget = 2;

    public void Update()
    {
        transform.localScale = transform.localScale + (Vector3.one * (growthRate * Time.deltaTime * GameManager.Instance.TimeScale));
        if (transform.localScale.x > damageIncreaseTarget)
        {
            GameManager.Instance.AddDamagePercentage(1);
            _currentDamageWorth += 1;
            damageIncreaseTarget += 1;
        }

    }

    public void CleanRust()
    {
        // Reduce damage to watch
        GameManager.Instance.AddDamagePercentage(-_currentDamageWorth);
        ReturnToPool();
    }
}
