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
        _targetObject.SetActive(_shouldInvert ? !GameItems.HasItem(_itemName) : GameItems.HasItem(_itemName));
    }
}
