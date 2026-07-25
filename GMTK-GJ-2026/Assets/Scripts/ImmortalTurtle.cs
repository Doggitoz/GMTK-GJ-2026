using Clock;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.AI;

public class ImmortalTurtle : MonoBehaviour,IInteractable
{
    [SerializeField]
    private Transform _playerTransform;

    [SerializeField]
    private NavMeshAgent _agent;

    private void Update()
    {
        _agent.SetDestination(GetTargetPosition());
    }

    private Vector3 GetTargetPosition()
    {
        return _playerTransform.position;
    }

    public void OnInteractorHover(Transform interactor)
    {
        // Damage player
        GameManager.Instance.ClockCondition.AddDamagePercentage(100f);
    }

    public void OnInteractorLeave(Transform interactor)
    { 
    }

    public void OnInteractorDown(Transform interactor)
    { 
    }

    public void OnInteractorUp(Transform interactor)
    { 
    }

    public void OnInteractorStay(Transform interactor)
    {
    }
}
