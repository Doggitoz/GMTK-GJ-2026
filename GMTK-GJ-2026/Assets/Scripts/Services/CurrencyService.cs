public class CurrencyService
{
    private const int MinMoney = 0;
    private const int MaxMoney = 999;

    private int _money;

    public int Money => _money;

    public bool CanAfford(int price)
    {
        return _money >= price;
    }

    public void LoadSaveData(int amount)
    {
        _money = ClampMoney(amount);
    }

    public int GetSaveData()
    {
        return _money;
    }

    public void SubtractMoney(int amount)
    {
        _money = ClampMoney(_money - amount);
    }

    public void AddMoney(int amount)
    {
        _money = ClampMoney(_money + amount);
    }

    private static int ClampMoney(int amount)
    {
        if (amount < MinMoney)
        {
            return MinMoney;
        }

        if (amount > MaxMoney)
        {
            return MaxMoney;
        }

        return amount;
    }
}
