using Clock;
using UnityEngine;

public class ClockHandPieChart : MonoBehaviour
{
    [SerializeField]
    WorldSpacePieChart _pieChart;
    [SerializeField]
    Hand _clockHand;
    float percentage = 0f;

    private void Update()
    {
        percentage = (360 - _clockHand.GetSmoothAngle()) / 360;
        UpdatePercentage(percentage);
    }

    private void UpdatePercentage(float percentage)
    {
        _pieChart.SetPercentage(percentage);
    }

}
