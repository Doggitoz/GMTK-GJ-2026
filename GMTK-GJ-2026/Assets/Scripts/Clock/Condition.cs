using UnityEngine;
using System;

namespace Clock
{
    public class Condition
    {
        public event Action<float> OnDamagePercentageChanged;
        public float DamagePercentage { get; private set; }
        public float DeteriorationTimeScale => _deteriorationTimeScale * Services.Inventory.CalculateModifier(Enums.ItemStat.Deterioration);
        private float _deteriorationTimeScale;

        public float RepairTimeScale => _repairTimeScale * Services.Inventory.CalculateModifier(Enums.ItemStat.Repair);
        private float _repairTimeScale;
        public Condition()
        {
            _deteriorationTimeScale = 1f;
            _repairTimeScale = 1f;
            DamagePercentage = 0f;
        }

        public void AddDamagePercentage(float damagePercentage)
        {
            DamagePercentage = Mathf.Clamp(DamagePercentage + damagePercentage, 0, 100);
            OnDamagePercentageChanged?.Invoke(DamagePercentage);
            if (DamagePercentage == 100)
            {
                GameEvents.TriggerClockBreak();
            }
        }

        public void SetDeteriorationTimeScale(float newScale)
        {
            _deteriorationTimeScale = newScale;
        }

        public void SetRepairTimeScale(float newScale)
        {
            _repairTimeScale = newScale;
        }
    }
}
