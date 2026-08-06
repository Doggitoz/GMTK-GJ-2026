using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string ItemName;
    [TextArea]
    public string ItemDescription;

    public RectTransform ItemIcon;

    public bool IsPurchased => Services.Inventory.IsUnlocked(ItemName);
    public int Cost;

    public RectTransform _purchaseDialogObject;

    private void Awake()
    {
        UpdateVisual();
    }

    public void OnItemClicked()
    {
        if (IsPurchased)
        {
            _purchaseDialogObject?.gameObject.SetActive(false);
            Services.Inventory.ToggleEquipped(ItemName);
            UpdateVisual();
            return;

        }

        if (!Services.Currency.CanAfford(Cost))
        {
            return;
        }

        // Popup purchase confirmation
        var dialogYesButton = _purchaseDialogObject.GetChild(0).GetComponent<Button>();
        dialogYesButton.onClick.RemoveAllListeners();
        dialogYesButton.onClick.AddListener(() => PurchaseItem());
        _purchaseDialogObject?.gameObject.SetActive(true);
        UpdateVisual();
    }

    public void PurchaseItem()
    {
        if (IsPurchased)
            return;
        if (!Services.Currency.CanAfford(Cost))
            return;

        Services.Currency.SubtractMoney(Cost);
        Services.Inventory.UnlockItem(ItemName);
        Services.Game.SaveGame();

        var dialogYesButton = _purchaseDialogObject
            .GetChild(0)
            .GetComponent<Button>();

        dialogYesButton.onClick.RemoveAllListeners();
        _purchaseDialogObject?.gameObject.SetActive(false);

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (!IsPurchased)
        {
            ItemIcon.GetComponent<Image>().color = Color.black;
            return;
        }

        ItemIcon.GetComponent<Image>().color = Services.Inventory.IsEquipped(ItemName)
            ? Color.white
            : Color.gray;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ItemIcon.localScale = Vector3.one * 1.25f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemIcon.localScale = Vector3.one;
    }
}
