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
                return;
            }

            string json = File.ReadAllText(SavePath);

            CurrentSave = JsonUtility.FromJson<Save.Data>(json);

            Debug.Log("Game loaded");
        }


        public void DeleteSave()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }

            CurrentSave = new Save.Data();

            Debug.Log("Save deleted");
        }


        public bool HasSave()
        {
            return File.Exists(SavePath);
        }
    }
}