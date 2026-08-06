using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Save
{
    [Serializable]
    public class SaveData
    {
        public List<string> progressFlags = new();
        [FormerlySerializedAs("completedTrial")]
        public List<string> completedTrials = new();
        public List<string> unlockedItems = new();
        public int money = 0;
    }
}