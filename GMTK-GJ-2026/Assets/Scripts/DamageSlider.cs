using UnityEngine;
using UnityEngine.UI;

public class DamageSlider : MonoBehaviour
{
    [SerializeField]
    Slider _slider;

    private void Update()
    {
        _slider.value =   GameManager.Instance.ClockCondition.DamagePercentage / 100f;
    }
}
