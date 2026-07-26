using System.IO;
using UnityEngine;

namespace Save
{
    public class Manager : MonoBehaviour
    {
        public static Manager Instance { get; private set; }

        private const string SaveFileName = "save.json";

        private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public Save.Data CurrentSave { get; private set; }


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadGame();
        }


        public void NewGame()
        {
            CurrentSave = new Save.Data();
            GameItems.Items.Clear();
            SaveGame();
        }


        public void SaveGame()
        {
            string json = JsonUtility.ToJson(CurrentSave, true);

            File.WriteAllText(SavePath, json);

            Debug.Log($"Game saved: {SavePath}");
        }


        public void LoadGame()
        {
            if (!File.Exists(SavePath))
            {
                CurrentSave = new Save.Data();
                GameItems.Items.Clear();
                return;
            }

            string json = File.ReadAllText(SavePath);

            CurrentSave = JsonUtility.FromJson<Save.Data>(json);

            GameItems.Items.Clear();
            foreach (var item in CurrentSave.unlockedItems)
            {
                GameItems.AddItem(item);
            }

            Debug.Log("Game loaded");
        }


        public void DeleteSave()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }

            CurrentSave = new Save.Data();
            GameItems.Items.Clear();

            Debug.Log("Save deleted");
        }


        public bool HasSave()
        {
            return File.Exists(SavePath);
        }

        public void CompleteTutorial()
        {
            CurrentSave.completedTutorial = true;
            SaveGame();
        }

        public void CompleteGame()
        {
            if (CurrentSave == null)
                CurrentSave = new Save.Data();

            CurrentSave.beatGame = true;

            foreach (var item in GameItems.Items)
            {
                if (!CurrentSave.completedTrial.Contains(item))
                {
                    CurrentSave.completedTrial.Add(item);
                }
            }

            SaveGame();
        }

        public bool HasUnlockedItem(string item)
        {
            return CurrentSave.unlockedItems.Contains(item);
        }

        public void UnlockItem(string item)
        {
            if (CurrentSave.unlockedItems.Contains(item))
                return;

            CurrentSave.unlockedItems.Add(item);
            SaveGame();
        }
    }
}