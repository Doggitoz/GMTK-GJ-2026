using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string ItemName;
    [TextArea]
    public string ItemDescription;

    public RectTransform ItemIcon;

    public bool IsPurchased => Services.Inventory.HasItem(ItemName);
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
            ToggleItem(ItemName);
            return;

        }

        if (!Economy.Currency.CurrencyManager.Instance.CanAfford(Cost))
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
        if (!Economy.Currency.CurrencyManager.Instance.CanAfford(Cost))
            return;

        Economy.Currency.CurrencyManager.Instance.LoseMoney(Cost);
        Services.Inventory.AddItem(ItemName);
        Services.Game.SaveGame();

        var dialogYesButton = _purchaseDialogObject
            .GetChild(0)
            .GetComponent<Button>();

        dialogYesButton.onClick.RemoveAllListeners();
        _purchaseDialogObject?.gameObject.SetActive(false);

        UpdateVisual();
    }

    private void ToggleItem(string item)
    {
        if (Services.Inventory.HasItem(item))
        {
            Services.Inventory.RemoveItem(item);
        }
        else
        {
            Services.Inventory.AddItem(item);
        }

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (!IsPurchased)
        {
            ItemIcon.GetComponent<Image>().color = Color.black;
            return;
        }

        if (Services.Inventory.HasItem(ItemName))
        {
            ItemIcon.GetComponent<Image>().color = Color.white;
        }
        else
        {
            ItemIcon.GetComponent<Image>().color = Color.gray;
        }
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
