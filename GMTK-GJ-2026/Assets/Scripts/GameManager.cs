using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action<float> OnDamagePercentageChanged;
    public float DamagePercentage { get; private set; } = 0f;
    public static GameManager Instance { get; private set; }
    public float DeteriorationTimeScale => _deteriorationTimeScale;
    private float _deteriorationTimeScale = 1f;

    public float RepairTimeScale => _repairTimeScale;
    private float _repairTimeScale = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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
