using System.Collections.Generic;
using Assets._Scripts.Enums;
using Newtonsoft.Json;

namespace Assets._Scripts.Datas
{
    public class UserRuntimeData
    {
        public string Id;
        public string Name;
        public string AvatarURL;
        // CURRENCY
        public int CoinCount;
        public int HeartCount;
        // PROGRESS
        public int CurrentLevelIndex;
        // BOOSTER
        public int ExtraMoveCount;
        public int ShuffleCount;
        public int HintCount;
        
        // TUTORIAL
        [JsonProperty]
        private HashSet<ETutorial> _playedTutorials = new();

        public bool HasPlayedTutorial(ETutorial tutorial) => _playedTutorials.Contains(tutorial);

        public void MarkTutorialPlayed(ETutorial tutorial) => _playedTutorials.Add(tutorial);

        // Constructor for new users
        public UserRuntimeData()
        {
            Id = "Admin_0";
            Name = "Admin";
            AvatarURL = "*";

            CoinCount = 100000;

            CurrentLevelIndex = 1;
            HeartCount = 5;

            ExtraMoveCount = 3;
            ShuffleCount = 4;
            HintCount = 5;
        }

        // JsonConstructor for deserialization if needed, 
        // but default constructor works fine for public fields.
    }
}