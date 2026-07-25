using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerTeleporter : MonoBehaviour
{
    [SerializeField]
    private Vector3 _position;

    [SerializeField]
    Transform _player;

    [SerializeField]
    CinemachineCamera _playerCamera;

    [SerializeField]
    CinemachineCamera _fadeCamera;

    IEnumerator _routine;

    // Fade Out
    // TeleportPlayer
    // FadeIn

    public void TriggerTeleport()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
        }

        _routine = TeleportRoutine();
        StartCoroutine(_routine);
    }

    IEnumerator TeleportRoutine()
    {
        _fadeCamera.Priority = 2;
        _fadeCamera.Prioritize();
        yield return new WaitForSeconds(1.5f);
        GameEvents.RequestTeleport(_position);
        Vector3 targetPos = new Vector3(0, _player.transform.position.y + 5, _player.transform.position.z - 10);
        _playerCamera.ForceCameraPosition(targetPos, _playerCamera.transform.rotation);

        yield return new WaitForSeconds(.5f);
        _fadeCamera.Priority = -1;
    }
}
