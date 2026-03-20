using Assets._Scripts.Controllers;
using UnityEngine;

namespace Assets._Scripts.Datas
{
    public abstract class BoosterRuntimeData
    {
        public int UseCount { get; private set; }
        public bool LockStatus {get; private set;}

        public BoosterRuntimeData(int initialCount, bool lockStatus)
        {
            UseCount = initialCount;
            LockStatus = lockStatus;
        }

        public void Do()
        {
            OnUsed();
            UseCount--;
        }

        public void Add(int count)
        {
            UseCount += count;
        }

        public abstract void OnUsed();
    }

#region ExtraMoveBooster
    public class ExtraMoveBoosterRuntimeData : BoosterRuntimeData
    {
        public int BonusAmount; 

        public ExtraMoveBoosterRuntimeData(int initialCount, bool lockStatus, int bonusAmount) : base(initialCount, lockStatus)
        {
            BonusAmount = bonusAmount;
        }

        public override void OnUsed()
        {
            GameController.Instance.AddMoves(BonusAmount);
            Debug.Log("Used Extra Move");
        }
    }
#endregion

#region ShuffleBooster
    public class ShuffleBoosterRuntimeData : BoosterRuntimeData
    {
        public ShuffleBoosterRuntimeData(int initialCount, bool lockStatus) : base(initialCount, lockStatus)
        {
        }

        public override void OnUsed()
        {
            Debug.Log("Used Shuffle");
        }
    }
#endregion

#region HintBooster
    public class HintBoosterRuntimeData : BoosterRuntimeData
    {
        public HintBoosterRuntimeData(int initialCount, bool lockStatus) : base(initialCount, lockStatus)
        {
        }

        public override void OnUsed()
        {
            Debug.Log("Used Hint");
        }
    }
#endregion
}