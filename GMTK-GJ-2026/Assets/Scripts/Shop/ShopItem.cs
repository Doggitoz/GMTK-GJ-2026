using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler 
{
    public string ItemName;
    [TextArea]
    public string ItemDescription;
    
    public RectTransform ItemIcon;
    
    public bool IsPurchased = false;
    public float Cost;

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

        // Popup purchase confirmation
        var dialogYesButton = _purchaseDialogObject.GetChild(0).GetComponent<Button>();
        dialogYesButton.onClick.RemoveAllListeners();
        dialogYesButton.onClick.AddListener(() => PurchaseItem());
        _purchaseDialogObject?.gameObject.SetActive(true);
        UpdateVisual();
    }

    public void PurchaseItem()
    {
        if (IsPurchased) return;

        // Subtract Cost from Resource

        GameItems.AddItem(ItemName);

        IsPurchased = true;
        var dialogYesButton = _purchaseDialogObject.GetChild(0).GetComponent<Button>();
        dialogYesButton.onClick.RemoveAllListeners();
        _purchaseDialogObject?.gameObject.SetActive(false);

        UpdateVisual();
    }

    private void ToggleItem(string item)
    {
        if (GameItems.HasItem(item))
        {
            GameItems.RemoveItem(item);
        } else
        {
            GameItems.AddItem(item);
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

        if (GameItems.HasItem(ItemName))
        {
            ItemIcon.GetComponent<Image>().color = Color.white;
        } else
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
