using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;
using Assets._Scripts.Interfaces;
using Assets._Scripts.Managers;
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

        private Sprite _baseIcon;
        
        public EMechanic ActiveMechanic {get; set;} = EMechanic.None;
        public MechanicVisualControl MechanicVisual { get; set; }

        public void Init(BlockData data, string tag)
        {
            Id = data.Id;
            _tag = tag;
            _baseIcon = BlockGroupMapper.GetIcon(tag, data.IconId);
            _icon.sprite = _baseIcon;
            ActiveMechanic = EMechanic.None;
        }

        public void ChangeIcon(Sprite icon) => _icon.sprite = icon;

        public void ChangeToBaseIcon() => ChangeIcon(_baseIcon);

        public bool IsSameTag(string tag)
        {
            return _tag == tag;
        }

        public bool IsSameTag(BlockController other, bool ignoreMechanic = false)
        {
            return other != null
                   && !string.IsNullOrEmpty(other.tag)
                   && !string.IsNullOrEmpty(_tag)
                   && ((!(other as IMechanicHandler).IsHidden()
                   && !(this as IMechanicHandler).IsHidden()) || ignoreMechanic)
                   && _tag == other._tag;
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