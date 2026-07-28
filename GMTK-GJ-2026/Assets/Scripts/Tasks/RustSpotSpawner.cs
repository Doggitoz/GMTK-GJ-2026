using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class RustSpotSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private Vector2 spawnFrequencyRange = new(2f, 5f);

    [Header("Pool Settings")]
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxPoolSize = 100;

    [SerializeField] private Transform _parentObject;

    private ObjectPool<GameObject> _pool;
    private float _currentSpawnFrequency;
    private float _timer;
    private readonly List<GameObject> _activeObjects = new();

    private float CurrentSpawnFrequency =>
        _currentSpawnFrequency / SpawnRateMultiplier;

    private float SpawnRateMultiplier =>
    Services.Inventory.Contains("Eye of Horus") ? 2f : 1f;
    private GameManager _gameManager => GameManager.Instance;

    private void Awake()
    {
        _pool = new ObjectPool<GameObject>(
            CreateObject,
            OnGetFromPool,
            OnReleaseToPool,
            OnDestroyPoolObject,
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: maxPoolSize);

        _currentSpawnFrequency = Random.Range(
            spawnFrequencyRange.x,
            spawnFrequencyRange.y);
    }

    private void Start()
    {
        _gameManager.OnGameReset += ResetPool;
    }

    private void Update()
    {
        if (!_gameManager.GameActive) return;
        _timer += Time.deltaTime;

        if (_timer >= CurrentSpawnFrequency)
        {
            Spawn();
            _timer = 0f;
            _currentSpawnFrequency = Random.Range(
                spawnFrequencyRange.x,
                spawnFrequencyRange.y);
        }
    }

    private void Spawn()
    {
        Vector2 randomPoint = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = transform.position +
                                new Vector3(randomPoint.x, 0f, randomPoint.y);

        GameObject rustSpot = _pool.Get();
        rustSpot.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
    }

    private GameObject CreateObject()
    {
        GameObject obj = Instantiate(prefab, _parentObject);

        // Allows the object to return itself to the pool.
        var pooled = obj.GetComponent<RustSpot>();
        if (pooled == null)
            pooled = obj.AddComponent<RustSpot>();

        pooled.Initialize(_pool);

        return obj;
    }

    private void OnGetFromPool(GameObject obj)
    {
        obj.SetActive(true);
        _activeObjects.Add(obj);
    }

    private void OnReleaseToPool(GameObject obj)
    {
        obj.SetActive(false);
        _activeObjects.Remove(obj);
    }

    private void OnDestroyPoolObject(GameObject obj)
    {
        Destroy(obj);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }

    [ContextMenu("Reset Pool")]
    private void ResetPool()
    {
        foreach (GameObject obj in _activeObjects.ToArray())
        {
            _pool.Release(obj);
        }

        _activeObjects.Clear();

        _timer = 0f;
    }
}