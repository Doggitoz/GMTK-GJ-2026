using UnityEngine;

namespace Save
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        public Save.SaveData CurrentSave => Services.Game.GetCurrentSaveSnapshot();


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }


        public void NewGame()
        {
            Services.Game.NewGame();
        }


        public void SaveGame()
        {
            Services.Game.SaveGame();
        }


        public void LoadGame()
        {
            Services.Game.LoadServices();
        }


        public void DeleteSave()
        {
            Services.Game.DeleteSave();
        }


        public bool HasSave()
        {
            return Services.Save.HasSave();
        }

        public void CompleteTutorial()
        {
            Services.Progress.CompleteTutorial();
            Services.Game.SaveGame();
        }

        public void CompleteGame()
        {
            Services.Progress.CompleteGame(Services.Inventory.Items);
            Services.Game.SaveGame();
        }

        public bool HasUnlockedItem(string item)
        {
            return Services.Inventory.HasItem(item);
        }

        public void UnlockItem(string item)
        {
            if (Services.Inventory.HasItem(item))
            {
                return;
            }

            Services.Inventory.AddItem(item);
            Services.Game.SaveGame();
        }
    }
}