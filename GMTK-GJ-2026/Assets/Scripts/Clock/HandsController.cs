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

        private Coroutine _breakRoutine;

        private void Start()
        {
            GameEvents.OnBreakClock += BreakClock;
            GameManager.Instance.OnGameReset += ResetHands;
        }


        private void OnDestroy()
        {
            GameEvents.OnBreakClock -= BreakClock;
            GameManager.Instance.OnGameReset -= ResetHands;
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

            _breakRoutine = StartCoroutine(BreakClockRoutine());
        }

        private void ResetHands()
        {
            if (_breakRoutine != null)
            {
                StopCoroutine(_breakRoutine);
                _breakRoutine = null;
            }

            _clockBreaking = false;

            foreach (Clock.Hand hand in _clockHands)
            {
                hand.SetSpeedMultiplier(1f);
            }
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