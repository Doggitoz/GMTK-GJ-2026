using System.IO;
using UnityEngine;

public class SaveService
{
    private const string SaveKey = "save.json";

    private string SavePath => Path.Combine(
        Application.persistentDataPath,
        SaveKey
    );

    public Save.SaveData CurrentSave { get; private set; }


    public bool HasSave()
    {
        return File.Exists(SavePath);
    }


    public Save.SaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            CurrentSave = new Save.SaveData();
            return CurrentSave;
        }


        string json = File.ReadAllText(SavePath);

        CurrentSave =
            JsonUtility.FromJson<Save.SaveData>(json);

        if (CurrentSave == null)
        {
            CurrentSave = new Save.SaveData();
        }

        return CurrentSave;
    }


    public void Save()
    {
        if (CurrentSave == null)
        {
            CurrentSave = new Save.SaveData();
        }

        string json =
            JsonUtility.ToJson(CurrentSave, true);


        File.WriteAllText(SavePath, json);
    }


    public void Delete()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }

        CurrentSave = new Save.SaveData();
    }


    public void SetCurrentSave(Save.SaveData saveData)
    {
        if (saveData == null)
        {
            CurrentSave = new Save.SaveData();
            return;
        }

        CurrentSave = saveData;
    }


    private void EnsureSave()
    {
        if (CurrentSave == null)
        {
            CurrentSave = new Save.SaveData();
        }
    }
}