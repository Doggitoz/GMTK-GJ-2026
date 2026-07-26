using UnityEngine;
using UnityEngine.Events;

public class GameManagerEvents : MonoBehaviour
{
    [SerializeField]
    UnityEvent OnGameStarted;
    [SerializeField]
    UnityEvent OnGameStopped;
    [SerializeField]
    UnityEvent OnGameReset;

    private void Awake()
    {
        var manager = GetComponent<GameManager>();
        manager.OnGameStart += () => OnGameStarted?.Invoke();
        manager.OnGameStop += () => OnGameStopped?.Invoke();
        manager.OnGameReset += () => OnGameReset?.Invoke();
    }
}
