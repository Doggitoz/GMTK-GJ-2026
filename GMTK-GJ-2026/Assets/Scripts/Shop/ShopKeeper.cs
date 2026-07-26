using UnityEngine;
using UnityEngine.Events;

public class ShopKeeper : MonoBehaviour, IInteractable
{
    public Animator _animator;
    public string _playerCloseBoolParameterName;

    bool FullSelect = false;

    public UnityEvent OpenShop;
    public UnityEvent CloseShop;

    [SerializeField] private FMODUnity.EventReference selectSound;
    public bool ShowInteractionIndicator => true;

    private void Awake()
    {
        CloseShop?.Invoke();    
    }

    public void OnInteractorDown(Transform interactor)
    {
        if (!interactor.TryGetComponent<TriggerInteractor>(out var _))
        {
            return;
        }
        FullSelect = true;
        FMODUnity.RuntimeManager.PlayOneShot(selectSound, transform.position);
    }

    public void OnInteractorHover(Transform interactor)
    {
        if (!interactor.TryGetComponent<TriggerInteractor>(out var _))
        {
            return;
        }
        if (_animator == null) return;
        _animator.SetBool(_playerCloseBoolParameterName, true);
    }

    public void OnInteractorLeave(Transform interactor)
    {
        if (!interactor.TryGetComponent<TriggerInteractor>(out var _))
        {
            return;
        }

        FullSelect = false;
        CloseShop?.Invoke();

        if (_animator == null) return;
        _animator.SetBool(_playerCloseBoolParameterName, false);

    }

    public void OnInteractorStay(Transform interactor)
    {
        
    }

    public void OnInteractorUp(Transform interactor)
    {
        if (!interactor.TryGetComponent<TriggerInteractor>(out var _))
        {
            return;
        }

        if (FullSelect)
        {
            OpenShop?.Invoke();
        }
    }
}
