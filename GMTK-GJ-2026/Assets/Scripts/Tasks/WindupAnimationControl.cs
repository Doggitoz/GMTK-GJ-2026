using UnityEngine;

public class WindupAnimationControl : MonoBehaviour
{
    [SerializeField]
    private WindUpTask _task;

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private string _speedMultFloatParameter;
    private void Update()
    {
        if (GameManager.Instance.GameActive)
            _animator.SetFloat(_speedMultFloatParameter, 1f - _task.DangerNormalized);
        else
        {
            _animator.SetFloat(_speedMultFloatParameter, 0f);
        }
    }
}
