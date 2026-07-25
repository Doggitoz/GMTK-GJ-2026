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

    private void Awake()
    {
        // Add itself to damage pool
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

    public void Update()
    {
        transform.localScale = transform.localScale + (Vector3.one * (growthRate * Time.deltaTime * GameManager.Instance.TimeScale));
    }

    public void CleanRust()
    {
        // Reduce damage to watch

        ReturnToPool();
    }
}
