#if UNITY_EDITOR
using Assets._Scripts.Patterns;
using UnityEngine;

namespace Assets._Scripts.Editor
{
    public class DebugFlagToggle : Singleton<DebugFlagToggle>
    {
        [field: SerializeField] public bool AllowPlayLockedLevel {get; private set;}
        [field: SerializeField] public bool SkipFirstLevel {get; private set;}
        [field: SerializeField] public bool SkipSelectBoosters {get; private set;}
        [field: SerializeField] public bool IgnoreMilestone {get; private set;}
        [field: SerializeField] public bool ShowAllLevel {get; private set;} = true;
    }
}
#endif
