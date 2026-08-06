using System.Collections.Generic;
using UnityEngine;

public class GameServices : MonoBehaviour
{
    public static GameServices Instance { get; private set; }
    public SaveService Save { get; private set; }
    public InventoryService Inventory { get; private set; }
    public ProgressService Progress { get; private set; }
    public CurrencyService Currency { get; private set; }

    private bool VerifySingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return false;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        return true;
    }

    private void Awake()
    {
        if (!VerifySingleton()) return;

        Save = new SaveService();
        Inventory = new InventoryService();
        Progress = new ProgressService();
        Currency = new CurrencyService();

        LoadServices();
    }

    public void LoadServices()
    {
        ApplySaveData(Save.Load());
    }

    public void SaveGame()
    {
        Save.SetCurrentSave(CreateSaveSnapshot());
        Save.Save();
    }

    public SaveData GetCurrentSaveSnapshot()
    {
        return CreateSaveSnapshot();
    }

    public void NewGame()
    {
        Save.Delete();
        ApplySaveData(Save.CurrentSave);
        Save.Save();
    }

    public void DeleteSave()
    {
        Save.Delete();
        ApplySaveData(Save.CurrentSave);
    }

    private void ApplySaveData(SaveData saveData)
    {
        if (saveData == null)
        {
            saveData = new SaveData();
        }

        Inventory.LoadSaveData(saveData.unlockedItems ?? new List<string>());
        Progress.LoadSaveData(saveData);
        Currency.LoadSaveData(saveData.money);
    }

    private SaveData CreateSaveSnapshot()
    {
        return new SaveData
        {
            progressFlags = Progress.GetProgressFlags(),
            completedTrials = Progress.GetCompletedTrials(),
            unlockedItems = Inventory.GetSaveData(),
            money = Currency.GetSaveData()
        };
    }
}
