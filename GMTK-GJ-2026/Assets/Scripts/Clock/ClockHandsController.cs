using UnityEngine;
using System.Collections.Generic;

public class ClockHandsController : MonoBehaviour
{
    [SerializeField]
    private List<ClockHand> _clockHands;

    private GameManager _gameManager => GameManager.Instance;

    private void FixedUpdate()
    {
        if (!_gameManager.GameActive) return;
        foreach (ClockHand hand in _clockHands)
        {
            hand.FixedUpdateListener();
        }
    }
}
