using UnityEngine;

public class MONEYMANAGER : MonoBehaviour
{
    public int MONEY = 0;
    public static MONEYMANAGER Instance;
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

        } else
        {
            Destroy(this);
            return;
        }

       
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

    public void LoseMoney(int amount)
    {
        MONEY -= amount;
        MONEY = Mathf.Clamp(MONEY, 0, 999);
    }

    public void AddMoney(int amount)
    {
        MONEY += amount;
        MONEY = Mathf.Clamp(MONEY, 0, 999);
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
