using UnityEngine;
using System;

public class ClockCondition
{
    public event Action<float> OnDamagePercentageChanged;
    public float DamagePercentage { get; private set; }
    public float DeteriorationTimeScale => _deteriorationTimeScale;
    private float _deteriorationTimeScale;

    public float RepairTimeScale => _repairTimeScale;
    private float _repairTimeScale;
    public ClockCondition()
    {
        _deteriorationTimeScale = 1f;
        _repairTimeScale = 1f;
        DamagePercentage = 0f;
    }

    public void AddDamagePercentage(float damagePercentage)
    {
        DamagePercentage = Mathf.Clamp(DamagePercentage + damagePercentage, 0, 100);
        OnDamagePercentageChanged?.Invoke(DamagePercentage);
    }

    public void SetDeteriorationTimeScale(float newScale)
    {
        _deteriorationTimeScale = newScale;
    }

    public void SetRepairTimeScale(float newScale)
    {
        _deteriorationTimeScale = newScale;
    }
}
