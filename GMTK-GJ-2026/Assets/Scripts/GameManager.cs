using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static readonly Vector3 ClockSpawnLocation = new Vector3(0, 1.5f, -10);
    public static readonly Vector3 HubSpawnLocation = new Vector3(100, 1.5f, 0);

    public event Action OnGameReset;
    public event Action OnGameStop;
    public event Action OnGameStart;
    public event Action OnTutorialStart;
    public event Action OnLoadSave;

    public static GameManager Instance { get; private set; }
    public bool GameActive => _gameActive;
    private bool _gameActive = false;
    public Clock.Condition ClockCondition => _clockCondition ??= new Clock.Condition();
    private Clock.Condition _clockCondition;

    public bool PlayerControllerEnabled => _playerControllerEnabled;
    private bool _playerControllerEnabled = true;

    [SerializeField]
    private bool _disableTutorial;

    private Save.Data SaveData => Save.Manager.Instance != null
        ? Save.Manager.Instance.CurrentSave
        : null;

    void Awake()
    {
        VerifySingleton();
    }

    private void Start()
    {
        if (_disableTutorial || SaveData?.completedTutorial == true)
        {
            LoadSave();
        }
        else
        {
            NewSave();
        }
        GameEvents.OnLose += StopGame;
        GameEvents.OnWin += StopGame;
    }

    void LoadSave()
    {
        // Load save stuff here I think
        OnLoadSave?.Invoke();
    }

    void NewSave()
    {
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
        ResetGame();
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

    public void SetPlayerActive(bool isActive)
    {
        _playerControllerEnabled = isActive;
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

    [ContextMenu("Trigger Lose")]
    public void TriggerLose()
    {
        GameEvents.TriggerLose();
    }

    [ContextMenu("Trigger Break")]
    public void TriggerBreak()
    {
        GameEvents.TriggerClockBreak();
    }

    [ContextMenu("Trigger Win")]
    public void TriggerWin()
    {
        GameEvents.TriggerWin();
    }
}
