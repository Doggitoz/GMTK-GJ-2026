using UnityEngine;
using System.Collections;

public class StartPortal : MonoBehaviour, IInteractable
{
    bool _debounce = false;
    public void OnInteractorDown(Transform interactor)
    {
        if (_debounce) return;
        if (!interactor.TryGetComponent<TriggerInteractor>(out var _))
        {
            return;
        }

        StartCoroutine(StartGameRoutine());
    }

    private bool _teleportFinished;

    private IEnumerator StartGameRoutine()
    {
        _debounce = true;

        _teleportFinished = false;

        void OnFinished() => _teleportFinished = true;

        GameEvents.OnPlayerTeleportCompleted += OnFinished;

        GameEvents.RequestPlayerTeleport(GameManager.ClockSpawnLocation);

        yield return new WaitUntil(() => _teleportFinished);

        yield return new WaitForSeconds(3f);

        GameEvents.OnPlayerTeleportCompleted -= OnFinished;

        GameManager.Instance.StartGame();
        _debounce = false;
    }
    public void OnInteractorHover(Transform interactor) { }
    public void OnInteractorLeave(Transform interactor) { }
    public void OnInteractorStay(Transform interactor) { }
    public void OnInteractorUp(Transform interactor) { }
}
