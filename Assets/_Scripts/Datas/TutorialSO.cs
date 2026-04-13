using Assets._Scripts.Enums;
using UnityEngine;

namespace Assets._Scripts.Datas
{
    [CreateAssetMenu(menuName = "ScriptableObjects/TutorialData")]
    public class TutorialSO : ScriptableObject
    {
        public ETutorial Type;
        public string Name => Type.ToString();
        public Sprite Image;
        public string Detail;
    }
}