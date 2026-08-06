using TMPro;
using UnityEngine;

namespace UI
{
    public class CurrencyHUD : MonoBehaviour
    {
        [SerializeField]
        TMP_Text _text;
        private void Update()
        {
            _text.text = (Services.Currency?.Money ?? 0).ToString();
        }
    }
}