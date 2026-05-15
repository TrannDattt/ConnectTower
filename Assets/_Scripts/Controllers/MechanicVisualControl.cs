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
        [Header("Hidden Block")]
        [SerializeField] private BlockEffectVisual _blockVisual;
        [SerializeField] private Texture2D _hiddenTexture;

        [Header("Frozen Block")]
        [SerializeField] private Image _frozenBlockIcon;
        [SerializeField] private GameObject _frozenPillarRod;
        [SerializeField] private GameObject _frozenPillarBase;
        // [SerializeField] private Text _frozenMoveCountText;

        [Header("Covered Pillar")]
        [SerializeField] private SpriteRenderer _clothImage;
        [SerializeField] private Animator _clothAnimator;
        [SerializeField] private Image _clothIcon;
        private string _clothTriggerParam = "Flip";

        [Header("Scratched Block")]
        [SerializeField] private Image _scratchImage;
        // [SerializeField] private Texture2D _hiddenTexture;

        [Header("Trap Pillar")]
        [SerializeField] private SpriteRenderer _trapImage;
        [SerializeField] private Animator _trapAnimator;
        private string _applyTrapTriggerParam = "ApplyTrap";
        private string _removeTrapTriggerParam = "RemoveTrap";

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
                case EMechanic.ScratchBlock:
                    if (_blockVisual != null)
                    {
                        _blockVisual.ChangeIconDisplay(false);
                        _scratchImage.gameObject.SetActive(true);
                        // _blockVisual.ChangeTexture(_hiddenTexture);    
                    } 
                    break;
                case EMechanic.StickyBlock:
                    if (_blockVisual != null)
                    {
                        _blockVisual.ChangeColor(EColor.Green);
                    }
                    break;
                case EMechanic.TrapPillar:
                    if (_trapImage != null) _trapImage.gameObject.SetActive(true);
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
                        seqence.OnComplete(() => 
                        {
                            var color = _clothImage.color;
                            color.a = 1f;
                            _clothImage.color = color;
                            _clothImage.gameObject.SetActive(false);
                        });

                        seqence.Play();
                    }
                    break;
                case EMechanic.ScratchBlock:
                    if (_blockVisual != null)
                    {
                        _blockVisual.ChangeIconDisplay(true);
                        _scratchImage.gameObject.SetActive(false);
                    }
                    if(!doEffect) break;
                    break;
                case EMechanic.StickyBlock:
                    if (_blockVisual != null)
                    {
                        _blockVisual.ChangeColor(EColor.None);
                    }
                    break;
                case EMechanic.TrapPillar:
                    if (_trapImage != null) _trapImage.gameObject.SetActive(false);
                    break;
                default:
                    break;
            }
        }
    }
}