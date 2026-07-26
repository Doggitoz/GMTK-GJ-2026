using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static readonly Vector3 ClockSpawnLocation = new Vector3(0, 1.5f, -10);
    public static readonly Vector3 HubSpawnLocation = new Vector3(100, 1.5f, 0);

    // Remove this later
    [SerializeField]
    private bool _startOnRuntime;

    public event Action OnGameReset;
    public event Action OnGameStop;
    public event Action OnGameStart;
    public event Action OnTutorialStart;

    public static GameManager Instance { get; private set; }
    public bool GameActive => _gameActive;
    private bool _gameActive;
    public Clock.Condition ClockCondition => _clockCondition ??= new Clock.Condition();
    private Clock.Condition _clockCondition;

    public bool PlayerControllerEnabled => _playerControllerEnabled;
    private bool _playerControllerEnabled = true;

    void Awake()
    {
        VerifySingleton();
    }

    private void Start()
    {
        _gameActive = _startOnRuntime;
        if (_startOnRuntime) return;
        OnTutorialStart?.Invoke();
    }

    [ContextMenu("Start Tutorial")]
    public void StartTutorial()
    {
        OnTutorialStart?.Invoke();
    }

    [ContextMenu("Start Game")]
    public void StartGame()
    {
        _gameActive = true;
        OnGameStart?.Invoke();
    }

    [ContextMenu("Stop Game")]
    public void StopGame()
    {
        _gameActive = false;
        OnGameStop?.Invoke();
    }

    [ContextMenu("Reset Game")]
    public void ResetGame()
    {
        _clockCondition = null;
        OnGameReset?.Invoke();
    }

    [ContextMenu("Restart Game")]
    public void RestartGame()
    {
        StopGame();
        ResetGame();
        StartGame();
    }

    public void SetPlayerActive(bool isActive)
    {
        _playerControllerEnabled = isActive;
    }

    public void TriggerLoseGame()
    {
        StopGame();
    }

    private void VerifySingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public float _damagePercent;
    private void Update()
    {
        _damagePercent = ClockCondition.DamagePercentage;
    }
}
