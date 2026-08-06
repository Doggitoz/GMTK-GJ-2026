using System.Collections.Generic;
using UnityEngine;

public class GameServices : MonoBehaviour
{
    public static GameServices Instance { get; private set; }
    public SaveService Save { get; private set; }
    public InventoryService Inventory { get; private set; }
    public ProgressService Progress { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Save = new SaveService();
        Inventory = new InventoryService();
        Progress = new ProgressService();

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

    public Save.SaveData GetCurrentSaveSnapshot()
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

    public void InitializeCurrency(Economy.Currency.CurrencyManager currencyManager)
    {
        if (currencyManager == null)
        {
            return;
        }

        currencyManager.LoadSaveData(Save.CurrentSave?.money ?? 0);
    }

    private void ApplySaveData(Save.SaveData saveData)
    {
        if (saveData == null)
        {
            saveData = new Save.SaveData();
        }

        Inventory.LoadSaveData(saveData.unlockedItems ?? new List<string>());
        Progress.LoadSaveData(saveData);
        LoadCurrency(saveData.money);
    }

    private Save.SaveData CreateSaveSnapshot()
    {
        return new Save.SaveData
        {
            progressFlags = Progress.GetProgressFlags(),
            completedTrials = Progress.GetCompletedTrials(),
            unlockedItems = Inventory.GetSaveData(),
            money = GetMoney()
        };
    }

    private void LoadCurrency(int amount)
    {
        if (Economy.Currency.CurrencyManager.Instance == null)
        {
            return;
        }

        Economy.Currency.CurrencyManager.Instance.LoadSaveData(amount);
    }

    private int GetMoney()
    {
        if (Economy.Currency.CurrencyManager.Instance == null)
        {
            return 0;
        }

        return Economy.Currency.CurrencyManager.Instance.MONEY;
    }
}
