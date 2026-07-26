using UnityEngine;
using System.Collections;

public class StartPortal : MonoBehaviour, IInteractable
{
    [SerializeField] private ConfirmationUI confirmationUI;

    private bool _debounce = false;
    private bool _teleportFinished;

    public void OnInteractorDown(Transform interactor)
    {
        if (_debounce) return;

        if (!interactor.TryGetComponent<TriggerInteractor>(out var _))
            return;

        confirmationUI.Show(confirmed =>
        {
            if (confirmed)
            {
                EnterPortal();
            }
        });
    }

    public void EnterPortal()
    {
        if (_debounce) return;

        StartCoroutine(EnterPortalRoutine());
    }

    private IEnumerator EnterPortalRoutine()
    {
        _debounce = true;

        _teleportFinished = false;

        void OnFinished() => _teleportFinished = true;

        GameEvents.OnPlayerTeleportCompleted += OnFinished;

        GameEvents.RequestPlayerTeleport(GameManager.ClockSpawnLocation);

        yield return new WaitUntil(() => _teleportFinished);

        GameEvents.OnPlayerTeleportCompleted -= OnFinished;

        yield return new WaitForSeconds(3f);

        GameManager.Instance.StartGame();

        _debounce = false;
    }

    public void OnInteractorHover(Transform interactor) { }
    public void OnInteractorLeave(Transform interactor) { }
    public void OnInteractorStay(Transform interactor) { }
    public void OnInteractorUp(Transform interactor) { }
}