using UnityEngine;
using System.Collections.Generic;

namespace Clock {
    public class HandsController : MonoBehaviour
    {
        [SerializeField]
        private List<Clock.Hand> _clockHands;

        private GameManager _gameManager => GameManager.Instance;

        private void FixedUpdate()
        {
            if (!_gameManager.GameActive) return;
            foreach (Clock.Hand hand in _clockHands)
            {
                hand.FixedUpdateListener();
            }
        }
    }
}
