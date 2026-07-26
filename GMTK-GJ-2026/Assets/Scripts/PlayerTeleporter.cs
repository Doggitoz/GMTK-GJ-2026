using UnityEngine;
using System.Collections;

public class PlayerTeleporter : MonoBehaviour
{
    private Coroutine _teleportRoutine;
    private void Start()
    {
        GameEvents.OnLose += TeleportToHub;
        GameEvents.OnWin += TeleportToHub;
    }

    private void OnDestroy()
    {
        GameEvents.OnLose -= TeleportToHub;
        GameEvents.OnWin -= TeleportToHub;
    }

    private void TeleportToHub()
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

    private void TeleportToClock()
    {
        if (_teleportRoutine != null)
            StopCoroutine(_teleportRoutine);

        _teleportRoutine = StartCoroutine(TeleportToHubCoroutine());
    }

    private IEnumerator TeleportToClockCoroutine()
    {
        yield return new WaitForSeconds(2f);
        GameEvents.RequestPlayerTeleport(GameManager.ClockSpawnLocation);
    }
}