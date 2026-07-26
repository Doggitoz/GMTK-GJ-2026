using System;
using UnityEngine;

public class RandomlyEnableOnGameEnd : MonoBehaviour
{
    [SerializeField] GameObject[] _targetObjects;

    public void Start()
    {
        GameManager.Instance.OnGameStop += SelectRandom;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameStop -= SelectRandom;
    }

    private void SelectRandom()
    {
        int index = UnityEngine.Random.Range(0,_targetObjects.Length);
        foreach(var dialogue in _targetObjects)
        {
            dialogue.gameObject.SetActive(false);
        }

        _targetObjects[index].gameObject.SetActive(true);
    }
}
