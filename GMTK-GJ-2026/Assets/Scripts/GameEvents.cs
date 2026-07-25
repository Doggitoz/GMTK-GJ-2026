using UnityEngine;
using System;

public static class GameEvents
{
    public static event Action<Vector3> OnTeleportRequested;

    public static void RequestTeleport(Vector3 position)
    {
        OnTeleportRequested?.Invoke(position);
    }
}
