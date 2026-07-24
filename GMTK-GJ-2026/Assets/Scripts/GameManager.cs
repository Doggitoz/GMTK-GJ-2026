using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action<float> OnTimeScaleChanged;
    public event Action<float> OnDamagePercentageChanged;
    public float TimeScale { get; private set; } = 1f;
    public float DamagePercentage { get; private set; } = 0f;
    public static GameManager Instance { get; private set; }

    private float _timer;

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

    void Update()
    {
        _timer = Mathf.Clamp(SubtractTime(), 0f, Mathf.Infinity); // This lowkey sucks but idk how to do this anymore LOL
    }

    float SubtractTime()
    {
        return _timer -= Time.deltaTime * TimeScale;
    }

    public void SetTimeScale(float newTimeScale)
    {
        TimeScale = newTimeScale;
        OnTimeScaleChanged?.Invoke(newTimeScale);
    }

    public void AddDamagePercentage(float damagePercentage)
    {
        DamagePercentage = Mathf.Clamp(DamagePercentage + damagePercentage, 0, 100);
        OnDamagePercentageChanged?.Invoke(DamagePercentage);
    }
}
