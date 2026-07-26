using UnityEngine;
using System.Collections;

namespace Clock
{
    /// <summary>
    /// Rotates a single clock hand as a kinematic rigidbody.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Hand : MonoBehaviour
    {
        [SerializeField] private bool isHourHand;

        [Header("Tick timing")]
        [SerializeField] private float secondsPerStep = 1f;

        [SerializeField] private float stepDegrees = 6f;

        [Header("Direction")]
        [SerializeField] private Vector3 rotationAxis = Vector3.up;

        [SerializeField] private bool counterClockwise = true;

/*        [Header("Motion style")]
        [SerializeField] private bool smooth = true;*/

        private float _currentAngle;

        private Rigidbody _rb;
        private Quaternion _startingRotation;

        private bool _tutorialMode;

        private float _speedMultiplier = 1f;

        [SerializeField] private Collider collisionBox;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.isKinematic = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            _startingRotation = _rb.rotation;
        }

        private void Start()
        {
            GameManager.Instance.OnGameReset += ResetClock;
        }

        public void FixedUpdateListener()
        {
            if (_tutorialMode)
                return;

            if (Clock.TimeManager.Instance == null)
                return;

            float targetAngle = GetTargetAngle();

            float speed = _speedMultiplier;

            _currentAngle = Mathf.MoveTowards(
                _currentAngle,
                targetAngle,
                Time.fixedDeltaTime * 360f * speed
            );

            float angle = _currentAngle;

            if (counterClockwise)
                angle *= -1f;

            Quaternion targetRotation =
                _startingRotation *
                Quaternion.AngleAxis(angle, rotationAxis.normalized);

            _rb.MoveRotation(targetRotation);
        }

        private float GetTargetAngle()
        {
            float elapsed = Clock.TimeManager.Instance.TotalSecondsElapsed;

            if (isHourHand)
            {
                // Full timer = one revolution
                return Clock.TimeManager.Instance.NormalizedTime * 360f;
            }

            return (elapsed / secondsPerStep) * stepDegrees;
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = multiplier;
        }


        public IEnumerator TutorialSpin(float duration = 4f)
        {
            _tutorialMode = true;

            if (collisionBox != null)
                collisionBox.enabled = false;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float percent = Mathf.Clamp01(elapsed / duration);
                float angle = percent * 360f;

                if (counterClockwise)
                    angle *= -1f;

                Quaternion targetRotation =
                    _startingRotation *
                    Quaternion.AngleAxis(angle, rotationAxis.normalized);

                _rb.MoveRotation(targetRotation);

                yield return null;
            }

            _rb.MoveRotation(_startingRotation);

            if (collisionBox != null)
                collisionBox.enabled = true;

            _tutorialMode = false;
        }


        private void ResetClock()
        {
            _speedMultiplier = 1f;
            _currentAngle = 0f;

            _rb.MoveRotation(_startingRotation);
        }
    }
}