using UnityEngine;

public class MONEYMANAGER : MonoBehaviour
{
    public int MONEY = 0;
    public static MONEYMANAGER Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            QuestionDialogueTrigger.OnCorrectAnswer += AddMoney;

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
    }

    public void AddMoney(int amount)
    {
        MONEY += amount;
    }
}
