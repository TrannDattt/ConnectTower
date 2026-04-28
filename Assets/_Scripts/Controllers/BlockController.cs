using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;
using Assets._Scripts.Interfaces;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets._Scripts.Controllers
{
    public class BlockController : MonoBehaviour, IMechanicHandler
    {
        public int Id {get; private set;} = -1;
        [field: SerializeField] public GameObject Base {get; private set;}
        [SerializeField] private Image _icon;
        [SerializeField] private string _tag;
        public string Tag => _tag;
        
        public EMechanic ActiveMechanic {get; set;} = EMechanic.None;
        public MechanicVisualControl MechanicVisual { get; set; }

        public void Init(BlockData data, string tag)
        {
            Id = data.Id;
            _tag = tag;
            _icon.sprite = BlockIconMapper.GetIcon(data.IconId);
            ActiveMechanic = EMechanic.None;
        }

        public bool IsSameTag(string tag)
        {
            return _tag == tag;
        }

        public bool IsSameTag(BlockController other)
        {
            return other != null && _tag == other._tag;
        }

        public PillarController GetPillarParent()
        {
            return GetComponentInParent<PillarController>();
        }

        void Awake()
        {
            MechanicVisual = GetComponent<MechanicVisualControl>();
        }
    }
}