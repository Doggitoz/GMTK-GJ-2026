using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the multiple-choice answer panel. One of these lives in your scene
/// (e.g. as a sibling of the Dialogue Box). Wire up the panel root and the 3
/// buttons + their text labels in the Inspector.
/// </summary>
public class ChoiceUIManager : MonoBehaviour
{
    public static ChoiceUIManager Instance { get; private set; }

    [SerializeField] private GameObject choicePanelRoot;
    [SerializeField] private Button[] choiceButtons;        // size 3, in order
    [SerializeField] private TMP_Text[] choiceButtonLabels; // size 3, matches choiceButtons order

    private Action<int> onChoiceSelected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (choicePanelRoot != null)
            choicePanelRoot.SetActive(false);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i; // capture for the closure
            choiceButtons[i].onClick.AddListener(() => HandleButtonClicked(index));
        }
    }

    /// <summary>Shows the panel with the given choice strings and reports the
    /// selected index back through onSelected.</summary>
    public void ShowChoices(string[] choices, Action<int> onSelected)
    {
        onChoiceSelected = onSelected;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            bool hasChoice = i < choices.Length;
            choiceButtons[i].gameObject.SetActive(hasChoice);

            if (hasChoice && choiceButtonLabels != null && i < choiceButtonLabels.Length)
                choiceButtonLabels[i].text = choices[i];
        }

        if (choicePanelRoot != null)
            choicePanelRoot.SetActive(true);
    }

    public void HideChoices()
    {
        onChoiceSelected = null;

        if (choicePanelRoot != null)
            choicePanelRoot.SetActive(false);
    }

    private void HandleButtonClicked(int index)
    {
        Action<int> callback = onChoiceSelected;
        HideChoices();
        callback?.Invoke(index);
    }
}
