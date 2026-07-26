using UnityEngine;
using System;

public static class GameEvents
{
    public static event Action<Vector3> OnPlayerTeleportRequested;
    public static event Action OnPlayerTeleportCompleted;

    public static void RequestPlayerTeleport(Vector3 position)
    {
        OnPlayerTeleportRequested?.Invoke(position);
    }

    public static void CompletePlayerTeleport()
    {
        OnPlayerTeleportCompleted?.Invoke();
    }
}
