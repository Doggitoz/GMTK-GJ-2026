using UnityEngine;

namespace Economy.Currency
{
    public class CurrencyManager : MonoBehaviour
    {
        public int MONEY => _money;
        public static CurrencyManager Instance;

        private int _money;

        [SerializeField]
        GameObject _balance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                QuestionDialogueTrigger.OnCorrectAnswer += AddMoney;
                GameManager.Instance.OnGameStart += HideBalance;
                GameManager.Instance.OnGameStop += ShowBalance;
                GameManager.Instance.OnTutorialEnd += ShowBalance;

            }
            else
            {
                Destroy(this);
                return;
            }


        }

        private void Start()
        {
            Services.Game.InitializeCurrency(this);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Destroy(this);
                QuestionDialogueTrigger.OnCorrectAnswer -= AddMoney;
            }
        }

        public bool CanAfford(int price)
        {
            return MONEY >= price;
        }

        public void LoadSaveData(int amount)
        {
            _money = Mathf.Clamp(amount, 0, 999);
        }

        public void LoseMoney(int amount)
        {
            _money -= amount;
            _money = Mathf.Clamp(_money, 0, 999);

            Services.Game.SaveGame();
        }

        public void AddMoney(int amount)
        {
            _money += amount;
            _money = Mathf.Clamp(_money, 0, 999);

            Services.Game.SaveGame();
        }

        [ContextMenu("Add $100")]
        public void AddHundred()
        {
            AddMoney(100);
        }

        private void HideBalance()
        {
            _balance.SetActive(false);
        }

        private void ShowBalance()
        {
            _balance.SetActive(true);
        }
    }
}