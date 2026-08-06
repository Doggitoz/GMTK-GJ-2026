using UnityEngine;

public static class Services
{
    public static GameServices Game => GameServices.Instance;
    public static SaveService Save => Game.Save;
    public static InventoryService Inventory => Game.Inventory;
    public static ProgressService Progress => Game.Progress;
    public static CurrencyService Currency => Game.Currency;
}
