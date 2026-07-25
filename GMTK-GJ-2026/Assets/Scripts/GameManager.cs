using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Remove this later
    [SerializeField]
    private bool _startOnRuntime;

    public event Action OnGameReset;
    public event Action OnGameStop;
    public event Action OnGameStart;

    public static GameManager Instance { get; private set; }
    public bool GameActive => _gameActive;
    private bool _gameActive;
    public Clock.Condition ClockCondition => _clockCondition ??= new Clock.Condition();
    private Clock.Condition _clockCondition;

    void Awake()
    {
        VerifySingleton();
    }

    private void Start()
    {
        _gameActive = _startOnRuntime;
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

    public void TriggerLoseGame()
    {
        StopGame();
    }

    private void VerifySingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
