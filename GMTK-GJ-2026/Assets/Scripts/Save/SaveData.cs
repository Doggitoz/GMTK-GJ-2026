using System;
using System.Collections.Generic;

namespace Save
{
    [Serializable]
    public class SaveData
    {
        public bool completedTutorial;
        public bool beatGame;
        public List<string> completedTrial = new();
        public List<string> unlockedItems = new();
        public int money = 0;
    }
}