using UnityEngine;
using System.Collections;

public class PlayerTeleporter : MonoBehaviour
{
    private Coroutine _teleportRoutine;
    private void Start()
    {
        GameEvents.OnLose += TeleportToHub;
        GameEvents.OnWin += TeleportToHub;
        GameManager.Instance.OnTutorialStart += TeleportToClock;
    }

    private void OnDestroy()
    {
        GameEvents.OnLose -= TeleportToHub;
        GameEvents.OnWin -= TeleportToHub;
    }

    public void TeleportToHub()
    {
        if (_teleportRoutine != null)
            StopCoroutine(_teleportRoutine);

        _teleportRoutine = StartCoroutine(TeleportToHubCoroutine());
    }

    private IEnumerator TeleportToHubCoroutine()
    {
        yield return new WaitForSeconds(2f);
        GameEvents.RequestPlayerTeleport(GameManager.HubSpawnLocation);
    }

    public void TeleportToClock()
    {
        if (_teleportRoutine != null)
            StopCoroutine(_teleportRoutine);

        _teleportRoutine = StartCoroutine(TeleportToClockCoroutine());
    }

    private IEnumerator TeleportToClockCoroutine()
    {
        yield return new WaitForSeconds(2f);
        GameEvents.RequestPlayerTeleport(GameManager.ClockSpawnLocation);
    }
}