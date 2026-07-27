using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DamageSliderHUD : MonoBehaviour
    {
        [SerializeField]
        Slider _slider;

        private void Update()
        {
            _slider.value = GameManager.Instance.ClockCondition.DamagePercentage / 100f;
        }
    }
}