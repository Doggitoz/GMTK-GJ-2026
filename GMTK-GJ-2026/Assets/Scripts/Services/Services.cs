using UnityEngine;

public static class Services
{
    public static InventoryService Inventory => GameServices.Instance.Inventory;
}
