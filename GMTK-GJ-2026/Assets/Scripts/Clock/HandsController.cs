using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Clock
{
    public class HandsController : MonoBehaviour
    {
        [SerializeField]
        private List<Clock.Hand> _clockHands;

        [Header("Break Animation")]
        [SerializeField]
        private float breakSlowdownDuration = 5f;

        [SerializeField]
        private float finalSpeedMultiplier = 0.05f;


        private bool _clockBreaking;

        private void Start()
        {
            GameEvents.OnBreakClock += BreakClock;
        }


        private void OnDestroy()
        {
            GameEvents.OnBreakClock -= BreakClock;
        }


        private void FixedUpdate()
        {
            if (!GameManager.Instance.GameActive)
                return;

            foreach (Clock.Hand hand in _clockHands)
            {
                hand.FixedUpdateListener();
            }
        }


        private void BreakClock()
        {
            if (_clockBreaking)
                return;

            StartCoroutine(BreakClockRoutine());
        }


        private IEnumerator BreakClockRoutine()
        {
            _clockBreaking = true;

            float elapsed = 0f;

            while (elapsed < breakSlowdownDuration)
            {
                elapsed += Time.deltaTime;

                float percent = elapsed / breakSlowdownDuration;

                // Smoothly slow from normal speed to almost stopped
                float multiplier = Mathf.Lerp(
                    1f,
                    finalSpeedMultiplier,
                    percent
                );

                foreach (Clock.Hand hand in _clockHands)
                {
                    hand.SetSpeedMultiplier(multiplier);
                }

                yield return null;
            }


            foreach (Clock.Hand hand in _clockHands)
            {
                hand.SetSpeedMultiplier(finalSpeedMultiplier);
            }


            GameEvents.TriggerLose();
        }
    }
}