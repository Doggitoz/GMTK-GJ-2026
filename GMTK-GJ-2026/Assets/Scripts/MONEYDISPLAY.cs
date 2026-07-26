using TMPro;
using UnityEngine;

public class MONEYDISPLAY : MonoBehaviour
{
    [SerializeField]
    TMP_Text _text;
    private void Update()
    {
        _text.text = MONEYMANAGER.Instance.MONEY.ToString();
    }
}
