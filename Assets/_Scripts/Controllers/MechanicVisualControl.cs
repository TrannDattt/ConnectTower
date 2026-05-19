using System.Linq;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;
using Assets._Scripts.Interfaces;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns.EventBus;
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
        [SerializeField] private MeshFilter _scratchMeshFilter;
        [SerializeField] private Mesh _scratchMesh1;
        [SerializeField] private Mesh _scratchMesh2;
        [SerializeField] private Mesh _scratchMesh3;
        [SerializeField] private ParticleSystem _scratchParticle;

        [Header("Trap Pillar")]
        [SerializeField] private SpriteRenderer _trapImage;
        [SerializeField] private Animator _trapAnimator;
        private string _applyTrapTriggerParam = "ApplyTrap";
        private string _removeTrapTriggerParam = "RemoveTrap";

        [Header("Sticky Block")]
        [SerializeField] private GameObject _slimeHolder;
        [SerializeField] private Image _topStrand;
        [field : SerializeField] public RectTransform TopStrandAnchor {get; private set;}
        [SerializeField] private Image _bottomStrand;
        [field : SerializeField] public RectTransform BottomStrandAnchor {get; private set;}
        [SerializeField] private float _strandOffset = .2f;
        private bool _isSticky;
        private bool _swappedStrands;
        private MechanicVisualControl _stickTargetTop;
        private MechanicVisualControl _stickTargetBottom;

        private BlockController _block;

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
                        _scratchMeshFilter.gameObject.SetActive(true);
                        _scratchMeshFilter.sharedMesh = _scratchMesh1;
                    } 
                    break;
                case EMechanic.StickyBlock:
                    _isSticky = true;
                    _swappedStrands = false;
                    _blocksMovedBinding = new((_) =>
                    {
                        // Debug.Log("Blocks moved, checking sticky strand swap");
                        _swappedStrands = false;
                    });
                    EventBus<BlocksMovedEvent>.Subscribe(_blocksMovedBinding);
                    if (_blockVisual != null && _block != null)
                    {
                        _slimeHolder.SetActive(true);
                        var pillar = _block.GetPillarParent();
                        var blockIndex = pillar.GetBlockIndex(_block);
                        pillar.TryGetBlockAt(blockIndex - 1, out var bottomBlock);
                        pillar.TryGetBlockAt(blockIndex + 1, out var topBlock);
                        SetStickyTarget(topBlock, bottomBlock);
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
                        _scratchMeshFilter.gameObject.SetActive(false);
                    }
                    if(!doEffect) break;
                    break;
                case EMechanic.StickyBlock:
                    _isSticky = false;
                    EventBus<BlocksMovedEvent>.Unsubscribe(_blocksMovedBinding);
                    if (_blockVisual != null)
                    {
                        _slimeHolder.SetActive(false);
                        _stickTargetTop = null;
                        _stickTargetBottom = null;
                    }
                    break;
                case EMechanic.TrapPillar:
                    if (_trapImage != null) _trapImage.gameObject.SetActive(false);
                    break;
                default:
                    break;
            }
        }

        public void UpdateVisual(MechanicRuntimeData data)
        {
            if (data == null)
            {
                Debug.LogError("Mechanic data is null! Cant update visual");
                return;
            }

            switch (data)
            {
                case ScratchedBlockMechanic scratchData:
                    Debug.Log($"Change scratch mesh from block {gameObject.name} to state {scratchData.ScratchState}");
                    var mesh = scratchData.ScratchState switch
                    {
                        1 => _scratchMesh1,
                        2 => _scratchMesh2,
                        3 => _scratchMesh3,
                        _ => null
                    };
                    float duration = 0.4f;
                    float strength = 0.05f;
                    int vibrato = 10;
                    transform.DOShakePosition(duration, new Vector3(strength, 0, 0), vibrato).SetTarget(gameObject);
                    _scratchMeshFilter.sharedMesh = mesh;
                    _scratchParticle.gameObject.SetActive(true);
                    _scratchParticle.Play();
                    break;
                default:
                    break;
            }
        }

        public void UpdateStickyTarget()
        {
            if (_block == null) return;
            var pillar = _block.GetPillarParent();
            var blockIndex = pillar.GetBlockIndex(_block);
            pillar.TryGetBlockAt(blockIndex - 1, out var bottomBlock);
            pillar.TryGetBlockAt(blockIndex + 1, out var topBlock);
            SetStickyTarget(topBlock, bottomBlock);
        }

        private void SetStickyTarget(IMechanicHandler top, IMechanicHandler bottom)
        {
            if (top != null && top is BlockController block)
                _stickTargetTop = block.MechanicVisual;
            else
                _stickTargetTop = null;

            if (bottom != null && bottom is BlockController block1)
                _stickTargetBottom = block1.MechanicVisual;
            else
                _stickTargetBottom = null;
            
            _topStrand.gameObject.SetActive(_stickTargetTop != null);
            _bottomStrand.gameObject.SetActive(_stickTargetBottom != null);
        }

        public void RemoveStickyTarget(bool top)
        {
            if (top)
            {
                _stickTargetTop = null;
                _topStrand.gameObject.SetActive(false);
            }
            else
            {
                _stickTargetBottom = null;
                _bottomStrand.gameObject.SetActive(false);
            }
        }

        public void ResetStickyStrand()
        {
            _topStrand.rectTransform.localScale = new Vector3(1, 0, 1);
            _bottomStrand.rectTransform.localScale = new Vector3(1, 0, 1);
            _topStrand.rectTransform.rotation = Quaternion.identity;
            _bottomStrand.rectTransform.rotation = Quaternion.identity;
        }

        private bool ShouldSwapStickyTargets(bool hasTopConnection, bool hasBottomConnection)
        {
            return ((hasTopConnection && IsStickyStrandAngleOutsideRange(true))
                || (hasBottomConnection && IsStickyStrandAngleOutsideRange(false)))
                && !_swappedStrands;
        }

        private bool IsStickyStrandAngleOutsideRange(bool topStrand)
        {
            if (!TryGetStickyStrandData(topStrand, out _, out var fromTarget, out var toTarget, out var parent))
                return false;

            var fromLocal = parent.InverseTransformPoint(fromTarget.position);
            var toLocal = parent.InverseTransformPoint(toTarget.position);
            var delta = toLocal - fromLocal;
            if (delta.sqrMagnitude <= Mathf.Epsilon) return false;

            var angle = GetStickyStrandAngle(delta, topStrand);
            return angle < -90f || angle > 90f;
        }

        private float GetStickyStrandAngle(Vector3 delta, bool topStrand)
        {
            var baseAngleOffset = topStrand ? -90f : 90f;
            return Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg + baseAngleOffset;
        }

        private bool TryGetStickyStrandData(
            bool topStrand,
            out RectTransform strandRt,
            out RectTransform fromTarget,
            out RectTransform toTarget,
            out Transform parent)
        {
            strandRt = (topStrand ? _topStrand : _bottomStrand)?.rectTransform;
            fromTarget = topStrand ? TopStrandAnchor : BottomStrandAnchor;
            toTarget = topStrand ? _stickTargetTop?.BottomStrandAnchor : _stickTargetBottom?.TopStrandAnchor;
            parent = strandRt?.parent;

            return strandRt != null && fromTarget != null && toTarget != null && parent != null;
        }

        private void SwapStickyTargets()
        {
            _swappedStrands = true;
            // Debug.Log("Swapping sticky strand targets");
            (_stickTargetTop, _stickTargetBottom) = (_stickTargetBottom, _stickTargetTop);
        }

        private void DoStickyStrandAnim(bool topStrand)
        {
            if (!TryGetStickyStrandData(topStrand, out var strandRt, out var fromTarget, out var toTarget, out var parent))
                return;

            var fromLocal = parent.InverseTransformPoint(fromTarget.position);
            var toLocal = parent.InverseTransformPoint(toTarget.position);
            var delta = toLocal - fromLocal;

            if (delta.sqrMagnitude <= Mathf.Epsilon)
            {
                strandRt.localPosition = fromLocal;
                strandRt.localRotation = Quaternion.identity;
                strandRt.localScale = new Vector3(1f, 0f, 1f);
                return;
            }

            var angle = GetStickyStrandAngle(delta, topStrand);
            var baseHeight = Mathf.Max(strandRt.rect.height, 0.0001f);
            var strandLength = delta.magnitude + _strandOffset;

            strandRt.localPosition = fromLocal;
            strandRt.localRotation = Quaternion.Euler(0f, 0f, angle);
            strandRt.localScale = new Vector3(1f, strandLength / baseHeight, 1f);
        }

        private EventBinding<BlocksMovedEvent> _blocksMovedBinding;

        void Awake()
        {
            _block = GetComponent<BlockController>();
        }

        void LateUpdate()
        {
            if (_isSticky)
            {
                var hasTopConnection = _stickTargetTop != null;
                var hasBottomConnection = _stickTargetBottom != null;
                if (ShouldSwapStickyTargets(hasTopConnection, hasBottomConnection))
                {
                    SwapStickyTargets();
                    hasTopConnection = _stickTargetTop != null;
                    hasBottomConnection = _stickTargetBottom != null;
                }

                _topStrand.gameObject.SetActive(hasTopConnection);
                _bottomStrand.gameObject.SetActive(hasBottomConnection);

                if (hasTopConnection)
                    DoStickyStrandAnim(true);

                if (hasBottomConnection)
                    DoStickyStrandAnim(false);
            }
        }
    }
}
