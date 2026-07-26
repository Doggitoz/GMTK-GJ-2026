using UnityEngine;
using System.Collections;

namespace Clock
{
    /// <summary>
    /// Rotates a single clock hand as a kinematic rigidbody, so it keeps perfect time
    /// yet still collides with and pushes the player.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Hand : MonoBehaviour
    {
        [Header("Tick timing")]
        [Tooltip("Game seconds between each step. Second hand = 1, Minute hand = 60, Hour hand = 3600")]
        [SerializeField] private float secondsPerStep = 1f;

        [Tooltip("Degrees moved per step. Second/Minute hand = 6 degrees. Hour hand = 30 degrees.")]
        [SerializeField] private float stepDegrees = 6f;

        [Header("Direction")]
        [Tooltip("Local axis the hand spins around (the clock-face normal). This clock lies flat so Up")]
        [SerializeField] private Vector3 rotationAxis = Vector3.up;

        [Tooltip("Untick this if the hand turns the wrong way when you press Play.")]
        [SerializeField] private bool counterClockwise = true;

        [Header("Motion style")]
        [Tooltip("On = smooth sweep. Off = snap one step at a time.")]
        [SerializeField] private bool smooth = true;

        private Rigidbody _rb;
        private Quaternion _startingRotation;

        private bool _tutorialMode;

        [SerializeField] private Collider collisionBox;

        GameManager _gameManager => GameManager.Instance;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.isKinematic = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            _startingRotation = _rb.rotation;
        }

        private void Start()
        {
            _gameManager.OnGameReset += ResetClock;
        }

        public void FixedUpdateListener()
        {
            if (_tutorialMode)
                return;

            if (Clock.TimeManager.Instance == null)
                return;

            if (secondsPerStep <= 0f)
                return;

            float angle = smooth ? GetSmoothAngle() : GetSteppedAngle();

            if (counterClockwise)
                angle *= -1f;

            Quaternion targetRotation =
                _startingRotation *
                Quaternion.AngleAxis(angle, rotationAxis.normalized);

            _rb.MoveRotation(targetRotation);
        }

        public float GetSmoothAngle()
        {
            float elapsedSeconds = Clock.TimeManager.Instance.TotalSecondsElapsed;
            float completedSteps = elapsedSeconds / secondsPerStep;

            return completedSteps * stepDegrees;
        }

        public float GetSteppedAngle()
        {
            float elapsedSeconds = Clock.TimeManager.Instance.TotalSecondsElapsed;
            int completedSteps = Mathf.FloorToInt(elapsedSeconds / secondsPerStep);

            return completedSteps * stepDegrees;
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
            _rb.MoveRotation(_startingRotation);
        }
    }
}