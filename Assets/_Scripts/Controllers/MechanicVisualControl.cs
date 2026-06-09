using System.Linq;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;
using Assets._Scripts.Interfaces;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns.EventBus;
using Assets._Scripts.Visuals;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Controllers
{
    public class MechanicVisualControl : MonoBehaviour
    {
        [Header("Hidden Block")]
        [SerializeField] private BlockEffectVisual _blockVisual;
        [SerializeField] private Texture2D _hiddenTexture;
        [SerializeField] private AudioClip _fxHiddenRemove;
        [SerializeField] private ParticleSystem _hiddenRemoveParticle;

        [Header("Frozen Block")]
        [SerializeField] private GameObject _frozenBlockHolder;
        [SerializeField] private GameObject _frozenBlockIcon;
        [SerializeField] private float _frozenBlockRotateFrom;
        [SerializeField] private float _frozenBlockRotateTo;
        [SerializeField] private Vector3 _frozenBlockPositionFrom;
        [SerializeField] private Vector3 _frozenBlockPositionTo;
        [SerializeField] private float _frozenApplyDur;
        [SerializeField] private GameObject _frozenPillarRod;
        [SerializeField] private GameObject _frozenPillarBase;
        [SerializeField] private float _frozenPillarOffsetY;
        [SerializeField] private ParticleSystem _frozenEmissionParticle;
        [SerializeField] private ParticleSystem _iceRemoveParticle;
        [SerializeField] private float _pillarRemoveIceDelay;
        [SerializeField] private AudioClip _fxIceSpread;
        [SerializeField] private AudioClip _fxIceRemove;
        // [SerializeField] private Text _frozenMoveCountText;

        [Header("Covered Pillar")]
        [SerializeField] private SpriteRenderer _clothImage;
        [SerializeField] private Animator _clothAnimator;
        [SerializeField] private Image _clothIcon;
        private string _clothTriggerParam = "Flip";
        [SerializeField] private AudioClip _fxClotheRemove;

        [Header("Scratched Block")]
        [SerializeField] private MeshFilter _scratchMeshFilter;
        [SerializeField] private Mesh _scratchMesh1;
        [SerializeField] private Mesh _scratchMesh2;
        [SerializeField] private Mesh _scratchMesh3;
        [SerializeField] private ParticleSystem _scratchParticle;
        [SerializeField] private AudioClip _fxStoneBreak;

        [Header("Trap Pillar")]
        [SerializeField] private GameObject _trapHolder;
        [SerializeField] private Animator _trapAnimator;
        [SerializeField] private AnimationClip _trapDownAnim;
        [SerializeField] private AnimationClip _trapUpAnim;
        [SerializeField] private AudioClip _fxTrapOpen;
        [SerializeField] private AudioClip _fxTrapClose;

        [Header("Sticky Block")]
        [SerializeField] private GameObject _slimeHolder;
        [SerializeField] private SpriteRenderer _topStrand;
        [field : SerializeField] public Transform TopStrandAnchor {get; private set;}
        [SerializeField] private SpriteRenderer _bottomStrand;
        [field : SerializeField] public Transform BottomStrandAnchor {get; private set;}
        [SerializeField] private float _strandOffset = .2f;
        [SerializeField] private ParticleSystem _particle;
        [SerializeField] private AudioClip _fxStickyMove;
        [SerializeField] private AudioClip _fxStickyBreak;
        private bool _isSticky;
        private bool _swappedStrands;
        private MechanicVisualControl _stickTargetTop;
        private MechanicVisualControl _stickTargetBottom;
        private Vector3 _topStrandBaseLocalScale = Vector3.one;
        private Vector3 _bottomStrandBaseLocalScale = Vector3.one;
        private const string FrozenBlockTweenId = "FrozenBlock";
        private const string FrozenPillarRodTweenId = "FrozenPillarRod";
        private Vector3 _frozenPillarRodOriginalLocalPosition;
        private Transform _particleOriginalParent;
        private Vector3 _particleOriginalLocalPosition;
        private Quaternion _particleOriginalLocalRotation;
        private Vector3 _particleOriginalLocalScale;
        private Coroutine _stickyDetachParticleRoutine;
        private Coroutine _hiddenRemoveParticleRoutine;
        private Coroutine _trapAnimationRoutine;
        private readonly Queue<MechanicVisualRequest> _pendingMechanicVisualRequests = new();
        private Coroutine _mechanicVisualRequestRoutine;

        private BlockController _block;

        private struct MechanicVisualRequest
        {
            public bool IsApply;
            public MechanicRuntimeData MechanicData;
            public EMechanic MechanicType;
            public bool DoEffect;
        }

        public void ApplyVisual(MechanicRuntimeData mechanicData, bool doEffect = true)
        {
            if (mechanicData == null || mechanicData.Key == EMechanic.None) return;

            EnqueueMechanicVisualRequest(new MechanicVisualRequest
            {
                IsApply = true,
                MechanicData = mechanicData,
                DoEffect = doEffect
            });
        }

        public void ApplyVisualImmediate(MechanicRuntimeData mechanicData, bool doEffect = true)
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
                    ApplyFrozenVisual(doEffect);
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
                        _blockVisual.SetTrailColor(new Color(.53f, .55f, .56f));
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
                        _blockVisual.SetTrailColor(new Color(.42f, .67f, .21f));
                    }
                    break;
                case EMechanic.TrapPillar:
                    if (_trapHolder != null)
                    {
                        PlayTrapAnimation(_trapUpAnim, keepTrapVisible: doEffect, doEffect);
                        SoundManager.Instance.PlaySFX(_fxTrapClose);
                    } 
                    break;
                default:
                    break;
            }
        }

        public void RemoveVisual(EMechanic type, bool doEffect = true)
        {
            if (type == EMechanic.None) return;

            if (type == EMechanic.ScratchBlock)
            {
                RemoveVisualImmediate(type, doEffect);
                return;
            }

            EnqueueMechanicVisualRequest(new MechanicVisualRequest
            {
                IsApply = false,
                MechanicType = type,
                DoEffect = doEffect
            });
        }

        public void RemoveVisualImmediate(EMechanic type, bool doEffect = true)
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
                    SoundManager.Instance.PlaySFX(_fxHiddenRemove);
                    // StartCoroutine(ParticleManager.Instance.PlayParticle(EParticle.Smoke, transform.position));
                    if (_hiddenRemoveParticle != null)
                    {
                        PlayHiddenRemoveParticle();
                    }
                    break;
                case EMechanic.FrozenBlock:
                    RemoveFrozenVisual(doEffect);
                    break;
                case EMechanic.CoveredPillar:
                    if (_clothImage != null) 
                    {
                        void reset()
                        {
                            _clothImage.gameObject.SetActive(false);
                        }

                        if (!doEffect) 
                        {
                            reset();
                            break;
                        }

                        _clothAnimator.SetTrigger(_clothTriggerParam);
                        var animDur = .95f;

                        var seqence = DOTween.Sequence().SetLink(_clothAnimator.gameObject, LinkBehaviour.KillOnDisable);
                        seqence.AppendCallback(() => SoundManager.Instance.PlaySFX(_fxClotheRemove));
                        seqence.AppendInterval(animDur);
                        seqence.OnComplete(reset).OnKill(reset);

                        seqence.Play();
                    }
                    break;
                case EMechanic.ScratchBlock:
                    if (_blockVisual != null)
                    {
                        _blockVisual.ChangeIconDisplay(true);
                        _scratchMeshFilter.gameObject.SetActive(false);
                        _blockVisual.SetTrailColor(Color.white);
                        _scratchParticle.gameObject.SetActive(true);
                        _scratchParticle.Play();
                    }
                    if(!doEffect) break;
                    SoundManager.Instance.PlaySFX(_fxStoneBreak);
                    break;
                case EMechanic.StickyBlock:
                    _isSticky = false;
                    EventBus<BlocksMovedEvent>.Unsubscribe(_blocksMovedBinding);
                    if (_blockVisual != null)
                    {
                        _slimeHolder.SetActive(false);
                        _stickTargetTop = null;
                        _stickTargetBottom = null;
                        _blockVisual.SetTrailColor(Color.white);
                    }
                    break;
                case EMechanic.TrapPillar:
                    if (_trapHolder != null)
                    {
                        PlayTrapAnimation(_trapDownAnim, keepTrapVisible: doEffect, doEffect);
                        if (doEffect) SoundManager.Instance.PlaySFX(_fxTrapOpen);
                    } 
                    break;
                default:
                    break;
            }
        }

        public void ShowTrapInactiveImmediate()
        {
            if (_trapHolder == null)
                return;

            PlayTrapAnimation(_trapDownAnim, keepTrapVisible: true, doEffect: false);
        }

        private void EnqueueMechanicVisualRequest(MechanicVisualRequest request)
        {
            if (!gameObject.activeInHierarchy)
                return;

            _pendingMechanicVisualRequests.Enqueue(request);
            if (_mechanicVisualRequestRoutine == null && isActiveAndEnabled)
                _mechanicVisualRequestRoutine = StartCoroutine(ProcessMechanicVisualRequests());
        }

        private void ApplyFrozenVisual(bool doEffect)
        {
            if (_frozenEmissionParticle != null)
            {
                _frozenEmissionParticle.gameObject.SetActive(true);
                _frozenEmissionParticle.Play();
                SoundManager.Instance.PlaySFX(_fxIceSpread);
            }
            if (_block != null)
            {
                DOTween.Kill(this, FrozenBlockTweenId);

                if (_frozenBlockHolder != null)
                    _frozenBlockHolder.SetActive(true);

                if (_frozenBlockIcon != null)
                {
                    SetFrozenBlockIconPose(_frozenBlockPositionFrom, _frozenBlockRotateFrom);

                    if (doEffect)
                    {
                        var iconTransform = _frozenBlockIcon.transform;
                        var toPosition = GetFrozenBlockIconLocalPosition(_frozenBlockPositionTo);
                        var toRotation = iconTransform.localEulerAngles;
                        toRotation.x = _frozenBlockRotateTo;

                        DOTween.Sequence()
                            .SetEase(Ease.OutQuad)
                            .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                            .SetId(FrozenBlockTweenId)
                            .SetTarget(this)
                            .Join(iconTransform.DOLocalMove(toPosition, _frozenApplyDur))
                            .Join(iconTransform.DOLocalRotate(toRotation, _frozenApplyDur));
                    }
                    else
                    {
                        SetFrozenBlockIconPose(_frozenBlockPositionTo, _frozenBlockRotateTo);
                    }
                }

                var pillarVisual = _block.GetPillarParent()?.MechanicVisual;
                if (pillarVisual != null)
                    pillarVisual.RefreshFrozenPillarRod();

                return;
            }

            if (_frozenPillarRod != null) _frozenPillarRod.SetActive(true);
            if (_frozenPillarBase != null) _frozenPillarBase.SetActive(true);

            RefreshFrozenPillarRod();
        }

        private Sequence RemoveFrozenVisual(bool doEffect)
        {
            DOTween.Kill(this, FrozenBlockTweenId);

            if (_frozenEmissionParticle != null)
            {
                _frozenEmissionParticle.Stop();
                _frozenEmissionParticle.gameObject.SetActive(false);
            }
            if (_block != null)
            {
                if (doEffect && _iceRemoveParticle != null)
                {
                    _iceRemoveParticle.gameObject.SetActive(true);
                    _iceRemoveParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    _iceRemoveParticle.Play();
                }

                if (_frozenBlockHolder != null)
                    _frozenBlockHolder.SetActive(false);

                if (_frozenBlockIcon != null)
                    SetFrozenBlockIconPose(_frozenBlockPositionFrom, _frozenBlockRotateFrom);

                var pillarVisual = _block.GetPillarParent()?.MechanicVisual;
                if (pillarVisual != null)
                    pillarVisual.RefreshFrozenPillarRod();

                return DOTween.Sequence();
            }

            return DOTween.Sequence().InsertCallback(_pillarRemoveIceDelay, () =>
            {
                if (_frozenPillarRod != null) _frozenPillarRod.SetActive(false);
                if (_frozenPillarBase != null) _frozenPillarBase.SetActive(false);
                if (doEffect) SoundManager.Instance.PlaySFX(_fxIceRemove);
            });
        }

        private void SetFrozenBlockIconPose(Vector3 localPosition, float localXRotation)
        {
            if (_frozenBlockIcon == null) return;

            var iconTransform = _frozenBlockIcon.transform;
            iconTransform.localPosition = GetFrozenBlockIconLocalPosition(localPosition);

            var localEulerAngles = iconTransform.localEulerAngles;
            localEulerAngles.x = localXRotation;
            iconTransform.localEulerAngles = localEulerAngles;
        }

        private Vector3 GetFrozenBlockIconLocalPosition(Vector3 localPosition)
        {
            if (_frozenBlockIcon == null) return localPosition;

            var currentLocalPosition = _frozenBlockIcon.transform.localPosition;
            return new Vector3(localPosition.x, localPosition.y, currentLocalPosition.z);
        }

        private void RefreshFrozenPillarRod()
        {
            var pillar = _block != null ? _block.GetPillarParent() : GetComponent<PillarController>();
            if (pillar == null || _frozenPillarRod == null) return;

            var frozenBlockCount = pillar.GetAllBlocks()
                .Count(block => block != null && (block as IMechanicHandler).ActiveMechanic == EMechanic.FrozenBlock);

            var rodLocalPosition = _frozenPillarRodOriginalLocalPosition;
            rodLocalPosition.y += Mathf.Max(0, frozenBlockCount - .5f) * _frozenPillarOffsetY;
            DOTween.Kill(this, FrozenPillarRodTweenId);
            _frozenPillarRod.transform
                .DOLocalMove(rodLocalPosition, _frozenApplyDur)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                .SetId(FrozenPillarRodTweenId)
                .SetTarget(this);
        }

        private void PlayTrapAnimation(AnimationClip clip, bool keepTrapVisible, bool doEffect)
        {
            if (_trapHolder == null)
                return;

            var wasTrapHolderActive = _trapHolder.activeSelf;

            if (_trapAnimationRoutine != null)
            {
                StopCoroutine(_trapAnimationRoutine);
                _trapAnimationRoutine = null;
            }

            if (_trapAnimator == null || clip == null)
            {
                if (keepTrapVisible || !wasTrapHolderActive)
                    _trapHolder.SetActive(keepTrapVisible);
                return;
            }

            _trapHolder.SetActive(true);

            if (!doEffect)
            {
                _trapAnimator.Play(clip.name, 0, 1f);
                _trapAnimator.Update(0f);
                if (keepTrapVisible || !wasTrapHolderActive)
                    _trapHolder.SetActive(keepTrapVisible);
                return;
            }

            _trapAnimator.Play(clip.name, 0, 0f);
            _trapAnimator.Update(0f);
            _trapAnimationRoutine = StartCoroutine(TrapAnimationRoutine(clip.length, keepTrapVisible, wasTrapHolderActive));
        }

        private IEnumerator TrapAnimationRoutine(float duration, bool keepTrapVisible, bool wasTrapHolderActive)
        {
            yield return new WaitForSeconds(duration);

            if (_trapHolder != null && (keepTrapVisible || !wasTrapHolderActive))
                _trapHolder.SetActive(keepTrapVisible);

            _trapAnimationRoutine = null;
        }

        private IEnumerator ProcessMechanicVisualRequests()
        {
            while (_pendingMechanicVisualRequests.Count > 0)
            {
                yield return WaitForBlockingTweensToComplete();

                var request = _pendingMechanicVisualRequests.Dequeue();
                if (request.IsApply)
                {
                    // Ignore stale apply requests that were queued before the mechanic changed/cleared.
                    if (!ShouldProcessApplyRequest(request))
                        continue;

                    ApplyVisualImmediate(request.MechanicData, request.DoEffect);
                }
                else
                    RemoveVisualImmediate(request.MechanicType, request.DoEffect);
            }

            _mechanicVisualRequestRoutine = null;
        }

        private bool ShouldProcessApplyRequest(MechanicVisualRequest request)
        {
            if (request.MechanicData == null)
                return false;

            return request.MechanicData.Key == GetCurrentMechanicType();
        }

        private IEnumerator WaitForBlockingTweensToComplete()
        {
            while (HasBlockingTween())
                yield return null;
        }

        private bool HasBlockingTween()
        {
            var pillar = _block != null ? _block.GetPillarParent() : null;

            return HasBlockingTweenOnTarget(gameObject)
                || HasBlockingTweenOnTarget(pillar)
                || HasBlockingTweenOnTarget(pillar?.transform);
        }

        private bool HasBlockingTweenOnTarget(object target)
        {
            if (target == null) return false;

            var tweens = DOTween.TweensByTarget(target, true);
            if (tweens == null) return false;

            foreach (var tween in tweens)
            {
                if (tween == null || !tween.active || !tween.IsPlaying())
                    continue;

                if (Equals(tween.id, "Float"))
                    continue;

                return true;
            }

            return false;
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
                    SoundManager.Instance.PlaySFX(_fxStoneBreak);
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

        public void UpdateStickyTarget(BlockController topBlock, BlockController bottomBlock)
        {
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
            var targetAnchor = top ? _stickTargetTop?.BottomStrandAnchor : _stickTargetBottom?.TopStrandAnchor;

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

            PlayStickyDetachParticle(targetAnchor);
        }

        private void PlayStickyDetachParticle(Transform targetAnchor)
        {
            if (_particle == null || targetAnchor == null) return;

            if (_stickyDetachParticleRoutine != null)
            {
                StopCoroutine(_stickyDetachParticleRoutine);
                RestoreStickyDetachParticleParent();
            }

            SoundManager.Instance.PlaySFX(_fxStickyBreak);
            _stickyDetachParticleRoutine = StartCoroutine(PlayStickyDetachParticleRoutine(targetAnchor));
        }

        private IEnumerator PlayStickyDetachParticleRoutine(Transform targetAnchor)
        {
            var targetParent = GetActiveParticleParent(targetAnchor);
            _particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _particle.transform.SetParent(targetParent, true);
            _particle.transform.position = targetAnchor.position;
            _particle.transform.rotation = targetAnchor.rotation;
            _particle.transform.localScale = Vector3.one;
            _particle.Play();

            yield return null;
            yield return new WaitUntil(() => _particle == null || !_particle.IsAlive(true));

            RestoreStickyDetachParticleParent();
            _stickyDetachParticleRoutine = null;
        }

        private void PlayHiddenRemoveParticle()
        {
            if (_hiddenRemoveParticle == null) return;

            if (_hiddenRemoveParticleRoutine != null)
            {
                StopCoroutine(_hiddenRemoveParticleRoutine);
                _hiddenRemoveParticleRoutine = null;
            }

            _hiddenRemoveParticle.gameObject.SetActive(true);
            _hiddenRemoveParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _hiddenRemoveParticle.Play();
            _hiddenRemoveParticleRoutine = StartCoroutine(HiddenRemoveParticleRoutine());
        }

        private IEnumerator HiddenRemoveParticleRoutine()
        {
            yield return null;
            yield return new WaitUntil(() => _hiddenRemoveParticle == null || !_hiddenRemoveParticle.IsAlive(true));

            if (_hiddenRemoveParticle != null)
                _hiddenRemoveParticle.gameObject.SetActive(false);

            _hiddenRemoveParticleRoutine = null;
        }

        private Transform GetActiveParticleParent(Transform targetAnchor)
        {
            var current = targetAnchor;
            while (current != null)
            {
                if (current.gameObject.activeInHierarchy)
                    return current;

                current = current.parent;
            }

            return _particleOriginalParent != null ? _particleOriginalParent : transform;
        }

        private void RestoreStickyDetachParticleParent()
        {
            if (_particle == null) return;

            _particle.transform.SetParent(_particleOriginalParent, false);
            _particle.transform.localPosition = _particleOriginalLocalPosition;
            _particle.transform.localRotation = _particleOriginalLocalRotation;
            _particle.transform.localScale = _particleOriginalLocalScale;
        }

        public void ResetStickyStrand()
        {
            if (_topStrand != null)
            {
                var topBaseScale = GetStickyStrandBaseScale(true);
                _topStrand.transform.localScale = new Vector3(topBaseScale.x, 0, topBaseScale.z);
                _topStrand.transform.localRotation = Quaternion.identity;
            }

            if (_bottomStrand != null)
            {
                var bottomBaseScale = GetStickyStrandBaseScale(false);
                _bottomStrand.transform.localScale = new Vector3(bottomBaseScale.x, 0, bottomBaseScale.z);
                _bottomStrand.transform.localRotation = Quaternion.identity;
            }
        }

        private bool ShouldSwapStickyTargets(bool hasTopConnection, bool hasBottomConnection)
        {
            return ((hasTopConnection && IsStickyStrandAngleOutsideRange(true))
                || (hasBottomConnection && IsStickyStrandAngleOutsideRange(false)))
                && !_swappedStrands;
        }

        private bool IsStickyStrandAngleOutsideRange(bool topStrand)
        {
            if (!TryGetStickyStrandData(topStrand, out _, out _, out var fromTarget, out var toTarget, out var parent))
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
            out Transform strandTf,
            out SpriteRenderer strandRenderer,
            out Transform fromTarget,
            out Transform toTarget,
            out Transform parent)
        {
            strandRenderer = topStrand ? _topStrand : _bottomStrand;
            strandTf = strandRenderer?.transform;
            fromTarget = topStrand ? TopStrandAnchor : BottomStrandAnchor;
            toTarget = topStrand ? _stickTargetTop?.BottomStrandAnchor : _stickTargetBottom?.TopStrandAnchor;
            parent = strandTf?.parent;

            return strandTf != null && strandRenderer != null && fromTarget != null && toTarget != null && parent != null;
        }

        private void SwapStickyTargets()
        {
            _swappedStrands = true;
            // Debug.Log("Swapping sticky strand targets");
            (_stickTargetTop, _stickTargetBottom) = (_stickTargetBottom, _stickTargetTop);
        }

        private void DoStickyStrandAnim(bool topStrand)
        {
            if (!TryGetStickyStrandData(topStrand, out var strandTf, out var strandRenderer, out var fromTarget, out var toTarget, out var parent))
                return;

            var fromLocal = parent.InverseTransformPoint(fromTarget.position);
            var toLocal = parent.InverseTransformPoint(toTarget.position);
            var delta = toLocal - fromLocal;

            if (delta.sqrMagnitude <= Mathf.Epsilon)
            {
                var baseScale = GetStickyStrandBaseScale(topStrand);
                strandTf.localPosition = fromLocal;
                strandTf.localRotation = Quaternion.identity;
                strandTf.localScale = new Vector3(baseScale.x, 0f, baseScale.z);
                return;
            }

            var angle = GetStickyStrandAngle(delta, topStrand);
            var baseHeight = GetStickyStrandBaseHeight(strandRenderer);
            var strandLength = delta.magnitude + _strandOffset;
            var strandBaseScale = GetStickyStrandBaseScale(topStrand);
            var strandDirection = delta.normalized;
            var strandCenter = fromLocal + strandDirection * (strandLength * 0.5f);

            // SpriteRenderer strands use a centered pivot, so we place the transform
            // at the midpoint and scale along its local Y axis to keep the strand
            // anchored cleanly between the two blocks.
            strandTf.localPosition = strandCenter;
            strandTf.localRotation = Quaternion.Euler(0f, 0f, angle);
            strandTf.localScale = new Vector3(strandBaseScale.x, strandLength / baseHeight, strandBaseScale.z);
        }

        private float GetStickyStrandBaseHeight(SpriteRenderer strandRenderer)
        {
            if (strandRenderer == null || strandRenderer.sprite == null)
                return 0.0001f;

            return Mathf.Max(strandRenderer.sprite.bounds.size.y, 0.0001f);
        }

        private Vector3 GetStickyStrandBaseScale(bool topStrand)
        {
            return topStrand ? _topStrandBaseLocalScale : _bottomStrandBaseLocalScale;
        }

        public void DoStickySFX()
        {
            SoundManager.Instance.PlaySFX(_fxStickyMove);
        }

        private EventBinding<BlocksMovedEvent> _blocksMovedBinding;

        void Awake()
        {
            _block = GetComponent<BlockController>();
            if (_frozenPillarRod != null) _frozenPillarRodOriginalLocalPosition = _frozenPillarRod.transform.localPosition;
            if (_topStrand != null) _topStrandBaseLocalScale = _topStrand.transform.localScale;
            if (_bottomStrand != null) _bottomStrandBaseLocalScale = _bottomStrand.transform.localScale;
            if (_particle != null)
            {
                _particleOriginalParent = _particle.transform.parent;
                _particleOriginalLocalPosition = _particle.transform.localPosition;
                _particleOriginalLocalRotation = _particle.transform.localRotation;
                _particleOriginalLocalScale = _particle.transform.localScale;
            }
        }

        void OnDisable()
        {
            if (_mechanicVisualRequestRoutine != null)
            {
                StopCoroutine(_mechanicVisualRequestRoutine);
                _mechanicVisualRequestRoutine = null;
            }

            if (GetCurrentMechanicType() == EMechanic.None)
                _pendingMechanicVisualRequests.Clear();

            if (_stickyDetachParticleRoutine != null)
            {
                StopCoroutine(_stickyDetachParticleRoutine);
                _stickyDetachParticleRoutine = null;
            }

            if (_trapAnimationRoutine != null)
            {
                StopCoroutine(_trapAnimationRoutine);
                _trapAnimationRoutine = null;
            }

            if (_hiddenRemoveParticleRoutine != null)
            {
                StopCoroutine(_hiddenRemoveParticleRoutine);
                _hiddenRemoveParticleRoutine = null;
            }

            if (_trapHolder != null && _trapHolder.activeSelf)
                _trapHolder.SetActive(false);

            if (_hiddenRemoveParticle != null)
                _hiddenRemoveParticle.gameObject.SetActive(false);

            RestoreStickyDetachParticleParent();
        }

        void OnEnable()
        {
            if (_pendingMechanicVisualRequests.Count > 0 && _mechanicVisualRequestRoutine == null)
                _mechanicVisualRequestRoutine = StartCoroutine(ProcessMechanicVisualRequests());
        }

        private EMechanic GetCurrentMechanicType()
        {
            if (_block != null)
                return (_block as IMechanicHandler).ActiveMechanic;

            var pillar = GetComponent<PillarController>();
            if (pillar != null)
                return (pillar as IMechanicHandler).ActiveMechanic;

            return EMechanic.None;
        }

        void LateUpdate()
        {
            var pillar = GetComponent<PillarController>();
            if (pillar != null && pillar.IsFullMatch && _trapHolder != null && _trapHolder.activeSelf)
            {
                if (_trapAnimationRoutine != null)
                {
                    StopCoroutine(_trapAnimationRoutine);
                    _trapAnimationRoutine = null;
                }

                _trapHolder.SetActive(false);
            }

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
