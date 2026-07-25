using UnityEngine;

public class PlayerTeleporter : MonoBehaviour
{
    [SerializeField]
    private Vector3 _position;

    public void TriggerTeleport()
    {
        GameEvents.RequestTeleport(_position);
    }
}
