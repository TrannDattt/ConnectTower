using System.Collections.Generic;
using UnityEngine;

namespace Assets._Scripts.Datas
{
    [CreateAssetMenu(fileName = "BlockGroupData", menuName = "ScriptableObjects/BlockGroupData", order = 1)]
    public class BlockGroupSO : ScriptableObject
    {
        public string Name;
        public List<Sprite> Icons;
    }
}