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

        //TODO: Use class UserJSON as a parameter
        public UserRuntimeData()
        {
            Id = "Admin_0";
            Name = "Admin";
            AvatarURL = "*";

            CoinCount = 100000;

            CurrentLevelIndex = 3;
            HeartCount = 5;

            ExtraMoveCount = 3;
            ShuffleCount = 4;
            HintCount = 5;
        }
    }
}