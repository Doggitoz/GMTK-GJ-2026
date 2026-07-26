using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Periodically spawns world-space "trigger" objects around itself (pooled,
/// same pattern as RustSpotSpawner). Each spawned object is an IInteractable
/// picked up by your existing TriggerInteractor: walking into range shows an
/// optional hover prompt, pressing Interact opens the shared GearPuzzleGame
/// popup. On a successful solve, that trigger is returned to the pool (i.e.
/// removed from the map) automatically.
///
/// SETUP
/// 1. Assign `prefab` — any GameObject with a visual (SpriteRenderer or
///    MeshRenderer/Renderer works). A Collider is added automatically if
///    the prefab doesn't already have one (needed for TriggerInteractor's
///    OnTriggerEnter/Exit to detect it), and a GearMinigameTrigger
///    component is added automatically too.
/// 2. Assign `minigame` — the single GearPuzzleGame instance in your scene
///    (its Canvas can live anywhere; it stays hidden until opened).
///
/// NOTE: per Unity's physics rules, trigger overlap events only fire if at
/// least one of the two colliders involved is a trigger AND at least one of
/// the two GameObjects has a Rigidbody — this should already be satisfied
/// by whatever your TriggerInteractor is attached to, since that script is
/// already working in your project.
/// </summary>
public class GearMinigameSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private Vector2 spawnFrequencyRange = new(2f, 5f);

    [Header("Minigame")]
    [SerializeField] private GearPuzzleGame minigame;
    // If true, the popup auto-closes itself a moment after a successful
    // solve. If false, the player closes it manually (clicking outside the
    // panel, or walking away — see GearMinigameTrigger), and the trigger is
    // still despawned either way.
    [SerializeField] private bool autoCloseOnWin = true;
    [SerializeField] private float autoCloseDelay = 1.5f;

    [Header("Pool Settings")]
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxPoolSize = 100;
    // Deactivating a pooled GameObject does NOT reliably fire OnTriggerExit
    // in Unity — that's exactly why TriggerInteractor's PruneInteractables
    // exists, checking activeInHierarchy as a fallback. But that fallback
    // only works if it gets a frame to run BEFORE this same instance is
    // reused (repositioned + reactivated) elsewhere — otherwise the
    // interactor is left holding a stale reference to a teleported object.
    // We hold released objects out of the pool's available set for this
    // many frames to guarantee that window exists. 1 is normally enough;
    // bumped to 2 for safety margin against script execution order.
    [SerializeField] private int framesToDelayReuse = 2;

    [SerializeField] private Transform _parentObject;

    private ObjectPool<GameObject> _pool;
    private float _currentSpawnFrequency;
    private float _timer;

    // The trigger currently "owning" an open popup, if any.
    private GearMinigameTrigger _activeTrigger;
    private float _autoCloseTimer;
    private bool _autoCloseCountingDown;

    // Objects that have been deactivated but are being held back from the
    // pool's available set for a few frames (see framesToDelayReuse above).
    private readonly List<GameObject> _pendingRelease = new();
    private readonly List<int> _pendingReleaseFrames = new();

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

        if (minigame == null)
        {
            Debug.LogError($"{nameof(GearMinigameSpawner)} on '{name}' has no {nameof(GearPuzzleGame)} assigned.");
        }
        else
        {
            minigame.onMinigameComplete.AddListener(OnMinigameComplete);
            minigame.onMinigameClosed.AddListener(OnMinigameClosed);
        }
    }
    private void Start()
    {
        GameManager.Instance.OnGameReset += ResetPool;
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


    private void OnDestroy()
    {
        if (minigame != null)
        {
            minigame.onMinigameComplete.RemoveListener(OnMinigameComplete);
            minigame.onMinigameClosed.RemoveListener(OnMinigameClosed);
        }
    }

    private void Update()
    {
        if (!GameManager.Instance.GameActive) return;
        _timer += Time.deltaTime;

        if (_timer >= _currentSpawnFrequency)
        {
            Spawn();
            _timer = 0f;
            _currentSpawnFrequency = Random.Range(
                spawnFrequencyRange.x,
                spawnFrequencyRange.y);
        }

        if (_autoCloseCountingDown)
        {
            _autoCloseTimer -= Time.deltaTime;
            if (_autoCloseTimer <= 0f)
            {
                _autoCloseCountingDown = false;
                if (minigame != null) minigame.CloseMinigame();
            }
        }

        TickPendingReleases();
    }

    private void TickPendingReleases()
    {
        for (int i = _pendingRelease.Count - 1; i >= 0; i--)
        {
            _pendingReleaseFrames[i]--;
            if (_pendingReleaseFrames[i] <= 0)
            {
                _pool.Release(_pendingRelease[i]);
                _pendingRelease.RemoveAt(i);
                _pendingReleaseFrames.RemoveAt(i);
            }
        }
    }

    private void Spawn()
    {
        Vector2 randomPoint = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = transform.position +
                                new Vector3(randomPoint.x, 0f, randomPoint.y);

        GameObject trigger = _pool.Get();
        trigger.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
    }

    private GameObject CreateObject()
    {
        GameObject obj = Instantiate(prefab, _parentObject);

        EnsureCollider(obj);

        // Allows the object to request that the minigame popup be
        // opened/closed via interaction events, and to hand itself back
        // for release once the minigame is won.
        var trigger = obj.GetComponent<GearMinigameTrigger>();
        if (trigger == null)
            trigger = obj.AddComponent<GearMinigameTrigger>();

        trigger.Initialize(this);

        return obj;
    }

    /// <summary>
    /// TriggerInteractor detects objects via OnTriggerEnter/Exit, which
    /// requires a Collider on this object. Sprite-based prefabs in
    /// particular often don't have one by default — so we add a reasonably
    /// fitted BoxCollider automatically if the prefab is missing one.
    /// </summary>
    private void EnsureCollider(GameObject obj)
    {
        if (obj.GetComponent<Collider>() != null) return;

        SpriteRenderer sr = obj.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            BoxCollider col = obj.AddComponent<BoxCollider>();
            Bounds b = sr.sprite != null ? sr.sprite.bounds : new Bounds(Vector3.zero, Vector3.one);
            col.center = b.center;
            col.size = new Vector3(b.size.x, b.size.y, Mathf.Max(0.1f, b.size.z));
            return;
        }

        Renderer renderer = obj.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            obj.AddComponent<BoxCollider>(); // auto-fits to the renderer's bounds by default
        }
    }

    private void OnGetFromPool(GameObject obj)
    {
        obj.SetActive(true);
        _activeObjects.Add(obj);
    }
    private readonly List<GameObject> _activeObjects = new();
    private void OnReleaseToPool(GameObject obj)
    {
        obj.SetActive(false);
        _activeObjects.Remove(obj);
    }

    private void OnDestroyPoolObject(GameObject obj)
    {
        Destroy(obj);
    }

    /// <summary>Called by a GearMinigameTrigger when the player interacts with it.</summary>
    public void RequestOpenMinigame(GearMinigameTrigger trigger)
    {
        if (minigame == null) return;

        // Only one popup at a time — ignore interactions with other
        // triggers while one is already open.
        if (minigame.IsOpen) return;

        _activeTrigger = trigger;
        _autoCloseCountingDown = false;
        minigame.OpenMinigame();
    }

    /// <summary>
    /// Called by a GearMinigameTrigger (e.g. on walking out of range) to
    /// close the popup — but only if that trigger is the one that opened it.
    /// </summary>
    public void RequestCloseIfActive(GearMinigameTrigger trigger)
    {
        if (minigame == null || _activeTrigger != trigger) return;

        minigame.CloseMinigame();
    }

    /// <summary>
    /// Called by a GearMinigameTrigger once it's ready to be despawned
    /// (i.e. the puzzle it opened was won). Deactivates it immediately —
    /// removed from the map right away — but doesn't let the pool hand this
    /// same instance back out again until framesToDelayReuse has elapsed,
    /// so TriggerInteractor's own cleanup has time to run first.
    /// </summary>
    public void ScheduleRelease(GameObject obj)
    {
        obj.SetActive(false);
        _pendingRelease.Add(obj);
        _pendingReleaseFrames.Add(Mathf.Max(1, framesToDelayReuse));
    }

    // The popup closing for ANY reason (backdrop click, walking away, or
    // auto-close after a win) always routes through here, so _activeTrigger
    // never goes stale regardless of which path closed it.
    private void OnMinigameClosed()
    {
        _activeTrigger = null;
    }

    private void OnMinigameComplete(bool success)
    {
        if (success && _activeTrigger != null)
        {
            _activeTrigger.ReturnToPool();
        }

        if (success && autoCloseOnWin)
        {
            _autoCloseTimer = autoCloseDelay;
            _autoCloseCountingDown = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}

/// <summary>
/// Attached (automatically, if missing) to each spawned trigger object.
/// Implements IInteractable so it's picked up by TriggerInteractor. Hands
/// itself back to the spawner (for delayed pool release) once the minigame
/// is won.
///
/// ASSUMED IInteractable SHAPE (inferred from TriggerInteractor's calls —
/// adjust the method signatures below if your actual interface differs):
///   void OnInteractorHover(Transform interactor);
///   void OnInteractorDown(Transform interactor);
///   void OnInteractorStay(Transform interactor);
///   void OnInteractorUp(Transform interactor);
///   void OnInteractorLeave(Transform interactor);
/// </summary>
public class GearMinigameTrigger : MonoBehaviour, IInteractable
{
    [Header("Optional")]
    // Enabled while the player is in range (before pressing Interact) —
    // a natural place for a "Press E" style prompt. Left unassigned = no-op.
    [SerializeField] private GameObject hoverIndicator;
    // OFF by default: if your TriggerInteractor is driven by camera
    // look direction, moving the mouse to click a gear can also rotate the
    // interactor away from this object, closing the popup unintentionally.
    // GearPuzzleGame already releases the cursor while open to prevent
    // that; only enable this if you specifically want "walking away closes
    // the popup" and have confirmed it doesn't fight with mouse-look.
    [SerializeField] private bool closeOnInteractorLeave = false;

    private GearMinigameSpawner _spawner;

    private void OnEnable()
    {
        GameManager.Instance.ClockCondition.AddDamagePercentage(2);
    }

    private void OnDisable()
    {
        GameManager.Instance.ClockCondition.AddDamagePercentage(2);
    }

    public void Initialize(GearMinigameSpawner spawner)
    {
        _spawner = spawner;
    }

    public void OnInteractorHover(Transform interactor)
    {
        if (hoverIndicator != null) hoverIndicator.SetActive(true);
    }

    public void OnInteractorDown(Transform interactor)
    {
        _spawner.RequestOpenMinigame(this);
    }

    public void OnInteractorStay(Transform interactor)
    {
        // No continuous "held" behavior needed — the popup is a toggle,
        // opened once on OnInteractorDown.
    }

    public void OnInteractorUp(Transform interactor)
    {
        // Releasing Interact doesn't need to do anything; the popup stays
        // open until the player closes it or wins.
    }

    public void OnInteractorLeave(Transform interactor)
    {
        if (hoverIndicator != null) hoverIndicator.SetActive(false);

        if (closeOnInteractorLeave)
            _spawner.RequestCloseIfActive(this);
    }

    public void ReturnToPool()
    {
        if (hoverIndicator != null) hoverIndicator.SetActive(false);
        _spawner.ScheduleRelease(gameObject);
    }
}