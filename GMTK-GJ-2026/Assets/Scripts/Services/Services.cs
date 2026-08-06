using UnityEngine;

public static class Services
{
    public static GameServices Game => GameServices.Instance;
    public static SaveService Save => GameServices.Instance.Save;
    public static InventoryService Inventory => GameServices.Instance.Inventory;
    public static ProgressService Progress => GameServices.Instance.Progress;
    public static CurrencyService Currency => GameServices.Instance.Currency;
}
