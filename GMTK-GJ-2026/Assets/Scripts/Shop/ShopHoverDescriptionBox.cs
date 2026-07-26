using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopHoverDescriptionBox : MonoBehaviour
{
    [SerializeField]
    ShopItem[] _itemButtons;

    public TMP_Text _nameBox;
    public TMP_Text _descriptionBox;

    public string[] DescriptionBarks;

    public ShopItem _selectedItem;

    private void Awake()
    {
        foreach (var button in _itemButtons)
        {
            ShopItem item = button;
            item.GetComponent<Button>().onClick.AddListener(() => ShowDescription(item));
        }
    }

    private void OnEnable()
    {
        _selectedItem = null;
        _nameBox.text = "Shopkeeper";
        int index = Random.Range(0, DescriptionBarks.Length);
        _descriptionBox.text = DescriptionBarks[index];
    }

    public void ShowDescription(ShopItem item)
    {
        _selectedItem = item;
        if (!item.IsPurchased)
        {
            _nameBox.text = "???";
            _descriptionBox.text = "";
        } else
        {
            _nameBox.text = item.ItemName;
            _descriptionBox.text = item.ItemDescription;
        }
    }

    private void Update()
    {
        if (_selectedItem != null && _selectedItem.IsPurchased && _nameBox.text == "???")
        {
            ShowDescription(_selectedItem);
        }
    }
}
