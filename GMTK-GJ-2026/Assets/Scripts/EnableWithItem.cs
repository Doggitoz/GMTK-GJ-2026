using UnityEngine;

public class EnableWithItem : MonoBehaviour
{
    [SerializeField]
    GameObject _targetObject;

    [SerializeField]
    string _itemName;

    [SerializeField]
    bool _shouldInvert;
    void Update()
    {
        _targetObject.SetActive(_shouldInvert ? !Services.Inventory.Contains(_itemName) : Services.Inventory.Contains(_itemName));
    }
}
