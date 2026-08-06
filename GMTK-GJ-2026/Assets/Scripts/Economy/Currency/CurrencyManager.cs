using UnityEngine;

namespace Economy.Currency
{
    public class CurrencyManager : MonoBehaviour
    {
        public int MONEY => Services.Currency?.Money ?? 0;
        public static CurrencyManager Instance;

        [SerializeField]
        GameObject _balance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                QuestionDialogueTrigger.OnCorrectAnswer += AddMoney;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnGameStart += HideBalance;
                    GameManager.Instance.OnGameStop += ShowBalance;
                    GameManager.Instance.OnTutorialEnd += ShowBalance;
                }

            }
            else
            {
                Destroy(this);
                return;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                QuestionDialogueTrigger.OnCorrectAnswer -= AddMoney;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnGameStart -= HideBalance;
                    GameManager.Instance.OnGameStop -= ShowBalance;
                    GameManager.Instance.OnTutorialEnd -= ShowBalance;
                }

                Instance = null;
            }
        }

        public bool CanAfford(int price)
        {
            return Services.Currency != null && Services.Currency.CanAfford(price);
        }

        public void LoadSaveData(int amount)
        {
            Services.Currency?.LoadSaveData(amount);
        }

        public void LoseMoney(int amount)
        {
            Services.Currency?.SubtractMoney(amount);
        }

        public void AddMoney(int amount)
        {
            Services.Currency?.AddMoney(amount);
        }

        [ContextMenu("Add $100")]
        public void AddHundred()
        {
            AddMoney(100);
        }

        private void HideBalance()
        {
            _balance?.SetActive(false);
        }

        private void ShowBalance()
        {
            _balance?.SetActive(true);
        }
    }
}