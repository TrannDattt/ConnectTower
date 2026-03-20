using UnityEngine;

namespace Assets._Scripts.Controllers
{
    public class BlockController : MonoBehaviour
    {
        [SerializeField] private string _tag;

        public void SetTag(string tag)
        {
            _tag = tag;
        }

        public bool IsSameTag(BlockController other)
        {
            return _tag == other._tag;
        }

        public PillarController GetPillarParent()
        {
            return GetComponentInParent<PillarController>();
        }
    }

    public class BoardController
    {
        
    }
}