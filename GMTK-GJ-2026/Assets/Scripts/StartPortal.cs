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

    private IEnumerator StartGameRoutine()
    {
        _debounce = true;
        GameEvents.RequestTeleport(GameManager.ClockSpawnLocation);

        yield return new WaitForSeconds(7f);

        GameManager.Instance.StartGame();
        _debounce = false;
    }
    public void OnInteractorHover(Transform interactor) { }
    public void OnInteractorLeave(Transform interactor) { }
    public void OnInteractorStay(Transform interactor) { }
    public void OnInteractorUp(Transform interactor) { }
}
