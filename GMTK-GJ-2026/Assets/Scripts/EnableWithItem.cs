using UnityEngine;

public class EnableWithItem : MonoBehaviour
{
    [SerializeField]
    GameObject _targetObject;

    [SerializeField]
    string _itemName;
    void Update()
    {
        _targetObject.SetActive(GameItems.HasItem(_itemName));
    }
}
