using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ConfirmationSelector : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;

        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        private Action<bool> onComplete;

        private void Awake()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);

            yesButton.onClick.AddListener(() => Select(true));
            noButton.onClick.AddListener(() => Select(false));
        }

        public void Show(Action<bool> callback)
        {
            onComplete = callback;

            if (panelRoot != null)
                panelRoot.SetActive(true);
        }

        public void Hide()
        {
            onComplete = null;

            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        private void Select(bool result)
        {
            Action<bool> callback = onComplete;

            Hide();

            callback?.Invoke(result);
        }
    }
}