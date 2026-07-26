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
