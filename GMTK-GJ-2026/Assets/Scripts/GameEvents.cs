using UnityEngine;
using System;

public static class GameEvents
{
    public static event Action<Vector3> OnPlayerTeleportRequested;
    public static event Action OnPlayerTeleportCompleted;
    public static event Action OnBreakClock;
    public static event Action OnLose;
    public static event Action OnWin;

    public static void RequestPlayerTeleport(Vector3 position)
    {
        OnPlayerTeleportRequested?.Invoke(position);
    }

    public static void CompletePlayerTeleport()
    {
        OnPlayerTeleportCompleted?.Invoke();
    }

    public static void TriggerClockBreak()
    {
        OnBreakClock?.Invoke();
    }

    public static void TriggerLose()
    {
        OnLose?.Invoke();
    }

    public static void TriggerWin()
    {
        OnWin?.Invoke();
    }
}
