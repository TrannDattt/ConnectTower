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

        [SerializeField] private Image _frozenBlockIcon;
        [SerializeField] private GameObject _frozenPillarRod;
        [SerializeField] private GameObject _frozenPillarBase;
        // [SerializeField] private Text _frozenMoveCountText;

        //CoveredPillar
        [Header("Covered Pillar")]
        [SerializeField] private SpriteRenderer _clothImage;
        [SerializeField] private Animator _clothAnimator;
        [SerializeField] private Image _clothIcon;
        [SerializeField] private float _fadeDelayFactor = .7f;
        private string _clothTriggerParam = "Flip";

        public void ApplyVisual(MechanicRuntimeData mechanicData)
        {
            var type = mechanicData.Key;
            if (type == EMechanic.None) return;

            switch (type)
            {
                case EMechanic.HiddenBlock:
                    if (_blockVisual != null)
                    {
                        _blockVisual.ChangeIconDisplay(false);
                        _blockVisual.ChangeTexture(_hiddenTexture);    
                    } 
                    break;
                case EMechanic.FrozenBlock:
                    if (_frozenBlockIcon != null) _frozenBlockIcon.gameObject.SetActive(true);
                    if (_frozenPillarRod != null) _frozenPillarRod.SetActive(true);
                    if (_frozenPillarBase != null) _frozenPillarBase.SetActive(true);
                    //TODO: Move rod up base on block count
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
                        _clothIcon.sprite = BlockGroupMapper.GetGroupIcons(coveredPillarData.TagToOpen)[0];
                    }

                    _clothImage.gameObject.SetActive(true);
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
                    if (_blockVisual != null)
                    {
                        _blockVisual.ChangeIconDisplay(true);
                        _blockVisual.ChangeTexture(null);
                    }
                    if (!doEffect) break;
                    StartCoroutine(ParticleManager.Instance.PlayParticle(EParticle.Smoke, transform.position));
                    break;
                case EMechanic.FrozenBlock:
                    if (_frozenBlockIcon != null) _frozenBlockIcon.gameObject.SetActive(false);
                    if (_frozenPillarRod != null) _frozenPillarRod.SetActive(false);
                    if (_frozenPillarBase != null) _frozenPillarBase.SetActive(false);
                    break;
                case EMechanic.CoveredPillar:
                    if (_clothImage != null && doEffect) 
                    {
                        _clothAnimator.SetTrigger(_clothTriggerParam);
                        var animDur = .98f;

                        var seqence = DOTween.Sequence().SetLink(_clothAnimator.gameObject, LinkBehaviour.KillOnDisable);
                        seqence.AppendInterval(animDur);
                        seqence.Insert(animDur * _fadeDelayFactor, _clothImage.DOFade(0, animDur * (1 - _fadeDelayFactor)).SetEase(Ease.OutQuad));
                        seqence.OnComplete(() => 
                        {
                            var color = _clothImage.color;
                            color.a = 1f;
                            _clothImage.color = color;
                            _clothImage.gameObject.SetActive(false);
                        });

                        //TODO: Make pillar rotate around
                        seqence.Play();
                    }
                    break;
                default:
                    break;
            }
        }
    }
}