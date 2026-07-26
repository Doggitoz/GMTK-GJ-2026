using System.Text;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ItemHudDisplay : MonoBehaviour
{
    [System.Serializable]
    public class ItemVisual
    {
        public string itemKey;
        public string displayName;
        public Sprite icon;
    }

    [Header("Catalog (one entry per item)")]
    [SerializeField] private ItemVisual[] _catalog;

    [Header("Icon row")]
    [SerializeField] private Transform _iconBar;
    [SerializeField] private Image _iconPrefab;

    [Header("Details panel (Tab)")]
    [SerializeField] private GameObject _detailsPanel;
    [SerializeField] private Transform _detailsList;
    [SerializeField] private TMP_Text _detailEntryPrefab;
    [SerializeField] private bool holdToShow = true;



    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart += Rebuild;
        }
    }
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart -= Rebuild;
        }
    }

    private void Update()
    {
        if (_detailsPanel == null || Keyboard.current == null) return;

        if (holdToShow)
        {
            _detailsPanel.SetActive(Keyboard.current.tabKey.isPressed);
        }
        else if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            _detailsPanel.SetActive(!_detailsPanel.activeSelf);
        }
    }

    private void Rebuild()
    {
        Debug.Log("Calling rebuild");
        Debug.Log(JsonUtility.ToJson(GameItems.Items));
        ClearChildren(_iconBar);
        foreach (var entry in _catalog)
        {
            Debug.Log($"{entry.displayName}: {GameItems.HasItem(entry.itemKey)}");
            if (!GameItems.HasItem(entry.itemKey)) continue;
            var icon = Instantiate(_iconPrefab, _iconBar);
            icon.sprite = entry.icon;
        }

        if (_detailsList == null || _detailEntryPrefab == null) return;
        ClearChildren(_detailsList);
        foreach (var entry in _catalog)
        {
            if (!GameItems.HasItem(entry.itemKey)) continue;
            var line = Instantiate(_detailEntryPrefab, _detailsList);
            line.text = $"<b>{entry.displayName}</b>\n{DescribeEffects(entry.itemKey)}";
        }
    }

    private static string DescribeEffects(string key)
    {
        var effects = GameItems.GetEffects(key);
        if (effects == null) return "No modifiers";

        var sb = new StringBuilder();
        foreach (var kv in effects)
        {
            sb.Append($"{Pretty(kv.Key)} \u00D7{kv.Value:0,##}  ");
        }

        return sb.ToString().TrimEnd();
    }

    private static string Pretty(ItemStat stat) => stat switch
    {
        ItemStat.ClockSpeed => "Clock Speed",
        ItemStat.Deterioration => "Deterioration",
        ItemStat.Repair => "Repair",
        _ => stat.ToString()
    };

    private static void ClearChildren(Transform t)
    {
        if (t == null) return;
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            Destroy(t.GetChild(i).gameObject);
        }
    }
}
