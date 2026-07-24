using UnityEngine;

/// <summary>
/// Rotates a single clock hand as a kinematic rigidbody, so it keeps perfect time yet still collides with and pushes player. 
/// </summary>

[RequireComponent(typeof(Rigidbody))]
public class ClockHand : MonoBehaviour
{
    [Header("Tick timing")]
    [Tooltip("Real seconds between each step. Second hand = 1, Minute hand = 60")]
    [SerializeField] private float secondsPerStep = 1f;

    [Tooltip("Degrees moved per step. A 60-position clock face = 6 degrees (360/60)")]
    [SerializeField] private float stepDegrees = 6f;

    [Header("Direction")]
    [Tooltip("Local axis the hand spins around (the clock-face normal). This clock lies flat so Up")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    [Tooltip("Untick this if the hand turns the wrong way when you press Play.")]
    [SerializeField] private bool counterClockwise = true;

    [Header("Motion style")]
    [Tooltip("On = smooth sweep. Off = snap one step at a time.")]
    [SerializeField] private bool smooth = true;

    [Header("Game time")]
    [Tooltip("Multiply speed by GameManager.TimeScale so the hand tracks game time. Ignored if none exists")]
    [SerializeField] private bool useGameTimeScale = true;

    private Rigidbody _rb;
    private float _accumulator;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        float scale = 1f;
        if (useGameTimeScale && GameManager.Instance != null)
            scale = GameManager.Instance.TimeScale;

        if (secondsPerStep <= 0f || scale == 0f)
            return;

        float dir = counterClockwise ? 1f : -1f;
        Vector3 axis = rotationAxis.sqrMagnitude > 0f ? rotationAxis.normalized : Vector3.up;

        if (smooth)
        {
            float degreesPerSecond = stepDegrees / secondsPerStep;
            Rotate(axis, dir * degreesPerSecond * scale * Time.fixedDeltaTime);
        }
        else
        {
            _accumulator += scale * Time.fixedDeltaTime;
            while (_accumulator >= secondsPerStep)
            {
                _accumulator -= secondsPerStep;
                Rotate(axis, dir * stepDegrees);
            }
        }
    }

    private void Rotate(Vector3 localAxis, float degrees)
    {
        // Right-multiply => spin around the hand's own axis, so it still works
        // if the whole clock is later tilted or wall-mounted
        _rb.MoveRotation(_rb.rotation * Quaternion.AngleAxis(degrees, localAxis));
    }

}
