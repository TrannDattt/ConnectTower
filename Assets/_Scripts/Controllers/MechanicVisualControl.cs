using System.Linq;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;
using Assets._Scripts.Interfaces;
using Assets._Scripts.Managers;
using Assets._Scripts.Visuals;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Controllers
{
    public class MechanicVisualControl : MonoBehaviour
    {
        //HiddenBlock
        [Header("Hidden Block")]
        [SerializeField] private BlockEffectVisual _blockVisual;
        [SerializeField] private Texture2D _hiddenTexture;

        //FrozenBlock
        [Header("Frozen Block")]

        [SerializeField] private Image _frozenIcon;
        [SerializeField] private Text _frozenMoveCountText;

        //CoveredPillar
        [Header("Covered Pillar")]
        [SerializeField] private float _offsetY = 3f;
        [SerializeField] private Image _clothImage;
        [SerializeField] private Image _clothIcon;
        private Vector2 _clothInitialAnchoredPos;
        private RectTransform _clothRectTransform;

        public void ApplyVisual(MechanicRuntimeData mechanicData)
        {
            var type = mechanicData.Key;
            if (type == EMechanic.None) return;

            switch (type)
            {
                case EMechanic.HiddenBlock:
                    _blockVisual?.ChangeIconDisplay(false);
                    _blockVisual?.ChangeTexture(_hiddenTexture);
                    break;
                case EMechanic.FrozenBlock:
                    if (_frozenIcon != null) _frozenIcon.gameObject.SetActive(true);
                    break;
                case EMechanic.CoveredPillar:
                    var coveredPillarData = mechanicData as CoveredPillarMechanic;
                    if (_clothIcon != null) 
                    {
                        var curLevel = LevelManager.PlayingLevel;
                        Debug.Log($"Finding block group for tag: {coveredPillarData.TagToOpen}");
                        Debug.Log($"{curLevel.BlockGroups.Count}");
                        var blockGroup = curLevel.BlockGroups.FirstOrDefault(g => g.Tag == coveredPillarData.TagToOpen);
                        Debug.Log($"{blockGroup.BlockDatas[0].IconId}");
                            _clothIcon.sprite = BlockIconMapper.GetIcon(blockGroup.BlockDatas[0].IconId);
                    }

                    if (_clothImage != null) 
                    {
                        _clothRectTransform.anchoredPosition = _clothInitialAnchoredPos;
                        _clothImage.gameObject.SetActive(true);
                    }
                    break;
                default:
                    break;
            }
        }

        public void RemoveVisual(EMechanic type, bool doEffect = true)
        {
            if (type == EMechanic.None) return;
            switch (type)
            {
                case EMechanic.HiddenBlock:
                    _blockVisual?.ChangeIconDisplay(true);
                    _blockVisual?.ChangeTexture(null);
                    if (!doEffect) break;
                    StartCoroutine(ParticleManager.Instance.PlayParticle(EParticle.Smoke, transform.position));
                    break;
                case EMechanic.FrozenBlock:
                    if (_frozenIcon != null) _frozenIcon.gameObject.SetActive(false);
                    break;
                case EMechanic.CoveredPillar:
                    if (_clothImage != null && doEffect) 
                    {
                        var animDuration = .5f;
                        var seqence = DOTween.Sequence().SetLink(_clothRectTransform.gameObject, LinkBehaviour.KillOnDisable);
                        seqence.Append(_clothRectTransform.DOAnchorPosY(_clothInitialAnchoredPos.y + _offsetY, animDuration).SetEase(Ease.InSine));
                        seqence.Insert(.5f, _clothImage.DOFade(0, animDuration * (1 - 0.5f)).SetEase(Ease.InSine));
                        seqence.OnComplete(() => 
                        {
                            _clothImage.gameObject.SetActive(false);
                            var color = _clothImage.color;
                            color.a = 1f;
                            _clothImage.color = color;
                        });

                        //TODO: Make pillar rotate around
                        seqence.Play();
                    }
                    break;
                default:
                    break;
            }
        }

        void Awake()
        {
            if (_clothImage != null)
            {
                _clothRectTransform = _clothImage.rectTransform;
                _clothInitialAnchoredPos = _clothRectTransform.anchoredPosition;
            }
        }
    }
}