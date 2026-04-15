using System;
using System.Collections.Generic;
using Assets._Scripts.Helpers;
using Assets._Scripts.Visuals;
using Assets._Scripts.Patterns;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using Assets._Scripts.Managers;
using Assets._Scripts.Enums;

namespace Assets._Scripts.Controllers
{
    public class BlockMovementController : Singleton<BlockMovementController>
    {
        private List<BlockController> _selectedBlocks = new();

        [SerializeField] private float _pickupHeight;
        public Coroutine CompleteCoroutine {get; private set;}
        public UnityEvent<bool> OnBlocksMoved = new();

        private float _blockHeight => GameObjectDataHelper.BlockHeight;

        public void Init()
        {
            _selectedBlocks.Clear();
        }

        public Vector3 GetBlockPosition(PillarController pillar, int index)
        {
            return pillar.BlockContainer.transform.position + index * _blockHeight * Vector3.up;
        }

        void OnDestroy()
        {
            OnBlocksMoved.RemoveAllListeners();
        }

#region PICK UP
        private void PickUpBlocks(PillarController pillar)
        {
            if (pillar.TryRemoveTopBlocks(out _selectedBlocks))
            {
                // _selectedBlocks = blocks;
                DoPickUpBlocksAnim(_selectedBlocks, pillar);
            }
            // Debug.Log("Selected " + _selectedBlocks.Count + " blocks from " + pillar.name);
        }

        private void DoPickUpBlocksAnim(List<BlockController> blocks, PillarController pillar)
        {
            if (blocks.Count == 0) return;

            string tweenId = "Pick up";
            DOTween.Kill(pillar);

            var sequence = DOTween.Sequence().SetTarget(pillar).SetId(tweenId);
            float tweenDuration = 0.3f;
            float blockOffset = _blockHeight + .1f;
            float firstOffset = _pickupHeight + pillar.GetBlockCount() * _blockHeight + blocks.Count * blockOffset - blockOffset / 2;

            Vector3[] targetPos = new Vector3[blocks.Count];

            for (int i = 0; i < blocks.Count; i++)
            {
                targetPos[i] = pillar.BlockContainer.transform.position + Vector3.up * (firstOffset - i * blockOffset);
                sequence.Join(blocks[i].transform.DOMove(targetPos[i], tweenDuration).SetEase(Ease.OutQuad));
            }

            sequence.OnComplete(() =>
            {
                foreach (var block in blocks) DoFloatAnim(block);
            });
            sequence.Play();
        }

        private void DoFloatAnim(BlockController target)
        {
            var moveOffset = .12f;
            var rotateOffset = 5f;

            string tweenId = "Float";
            var sequence = DOTween.Sequence().SetRelative().SetTarget(target.GetPillarParent()).SetId(tweenId);

            // Di chuyen len xuong
            sequence.Append(target.transform.DOMoveY(moveOffset, 0.6f).SetEase(Ease.InOutSine).SetLoops(int.MaxValue, LoopType.Yoyo));
            // Xoay trai phai nhe
            sequence.Join(target.transform.DORotate(new Vector3(0, 0, rotateOffset), 0.8f).SetEase(Ease.InOutSine).SetLoops(int.MaxValue, LoopType.Yoyo));
            sequence.OnKill(() =>
            {
                target.transform.rotation = Quaternion.identity;
            });
        }
#endregion

#region PUT BACK
        public void PutBackSelectedBlocks()
        {
            if (_selectedBlocks.Count == 0) return;
            var toPutBack = _selectedBlocks.GetRange(0, _selectedBlocks.Count);
            _selectedBlocks.Clear();
            PutBackBlocks(toPutBack, toPutBack[0].GetPillarParent());
        }

        private void PutBackBlocks(List<BlockController> blocks, PillarController pillar)
        {
            if (blocks.Count == 0) return;

            var parentPillar = blocks[0].GetPillarParent();
            // Debug.Log("Returned " + blocks.Count + " blocks to " + parentPillar.name);
            blocks.Reverse();
            parentPillar.AddBlocksToTop(blocks);
            DoPutBackBlocksAnim(blocks, pillar);
        }

        private void DoPutBackBlocksAnim(List<BlockController> blocks, PillarController pillar)
        {
            if (blocks.Count == 0) return;

            DOTween.Kill(pillar, true);
            foreach (var block in blocks) block.transform.DOKill();
            var tweenId = "Put back";
            var sequence = DOTween.Sequence().SetTarget(pillar).SetId(tweenId);
            float tweenDuration = 0.3f;
            var firstPos = pillar.BlockContainer.transform.position + _blockHeight * (pillar.GetBlockCount() - blocks.Count) * Vector3.up;

            Vector3[] targetPos = new Vector3[blocks.Count];
            for (int i = 0; i < blocks.Count; i++)
            {
                targetPos[i] = firstPos + _blockHeight * i * Vector3.up;
                sequence.Join(blocks[i].transform.DOMove(targetPos[i], tweenDuration).SetEase(Ease.InOutQuad));
            }
            sequence.Play();
        }
#endregion

#region MOVE
        private void MoveBlocks(List<BlockController> blocks, PillarController fromPillar, PillarController toPillar)
        {
            if (blocks.Count == 0) return;
            
            toPillar.AddBlocksToTop(blocks);
            DoMoveBlocksAnim(blocks, fromPillar, toPillar);
        }

        private Sequence DoMoveBlocksAnim(List<BlockController> blocks, PillarController fromPillar, PillarController toPillar)
        {
            if (blocks.Count == 0 || fromPillar == null || toPillar == null) return null;

            DOTween.Kill(fromPillar, true);
            foreach (var block in blocks) block.transform.DOKill();
            string tweenId = "Move";
            var sequence = DOTween.Sequence().SetTarget(toPillar).SetId(tweenId);

            float duration = 0.7f; 
            float staggeredDelay = 0.05f;
            float jumpPower = 1.5f;
            
            int groupStartIndex = toPillar.GetBlockCount() - blocks.Count;
            bool isLockedThisMove = toPillar.IsLocked();
            toPillar.TryGetTopBlocks(out var matched, ignoreLock: true);

            SoundManager.Instance.PlayRandomSFX(ESfx.BlockMoved);

            Vector3[] targetPos = new Vector3[blocks.Count];

            for (int i = 0; i < blocks.Count; i++)
            {
                var targetSlot = groupStartIndex + i;
                targetPos[i] = toPillar.BlockContainer.transform.position + Vector3.up * (targetSlot * _blockHeight);
                
                Vector3 fromTop = fromPillar.TopPillar.position;
                Vector3 toTop = toPillar.TopPillar.position;

                // Xac dinh diem cao nhat giua 2 cọc de di chuyen cung
                Vector3 midJump = (fromTop + toTop) / 2f + Vector3.up * jumpPower;
                
                // Quy dao: Tu vi tri hien tai (dang hover) -> Qua dinh coc cu -> Qua diem giua -> Qua dinh coc moi -> Roi xuong
                Vector3[] path = new Vector3[] { fromTop, midJump, toTop, targetPos[i] };
                
                sequence.Insert(i * staggeredDelay, 
                    blocks[i].transform.DOPath(path, duration, PathType.CatmullRom)
                    .SetEase(Ease.OutQuad));
            }
            
            sequence.OnComplete(() =>
            {
                // Return if this tween is force-completed (e.g. blocks picked up again)
                if (blocks.Count > 0 && _selectedBlocks.Contains(blocks[0])) return;

                Sequence feedbackSequence = DOTween.Sequence();
                if (matched.Count > blocks.Count)
                {
                    SoundManager.Instance.PlayChainedSFXs(ESfx.BlockMatched, matched.Count);
                    feedbackSequence.Append(DoMatchAnim(matched));
                }
                else if (groupStartIndex > 0)
                {
                    SoundManager.Instance.PlayRandomSFX(ESfx.BlockNotMatched);
                    feedbackSequence.Append(DoNotMatchAnim(matched));
                }
                
                if (isLockedThisMove)
                    feedbackSequence.OnComplete(() =>
                    {
                        HapticManager.DoLightFeedback();
                        CompleteCoroutine = StartCoroutine(toPillar.gameObject.GetComponent<PillarEffectVisual>().DoLockAnim(blocks[0].Tag));
                    });
            });
            return sequence.Play();
        }

        private Sequence DoMatchAnim(List<BlockController> blocks)
        {
            if (blocks == null || blocks.Count == 0) return null;

            var pillar = blocks[0].GetPillarParent();
            string tweenId = "Match";
            var masterSequence = DOTween.Sequence().SetId(tweenId).SetTarget(pillar);

            float jumpHeight = .5f;
            float jumpDuration = 0.25f;
            float rotateAngle = 7f;
            float staggeredDelay = 0.03f;

            for (int i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                block.transform.DOKill();
                var sequence = DOTween.Sequence();
                float initialY = block.transform.position.y;
                Vector3 baseRotation = block.transform.localEulerAngles;

                // Move Sequence (0.5s total)
                sequence.Append(block.transform.DOMoveY(initialY + jumpHeight, jumpDuration).SetEase(Ease.OutQuad));
                sequence.Append(block.transform.DOMoveY(initialY, jumpDuration).SetEase(Ease.InQuad));

                // Connect 3 rotation movements to span the entire jump duration
                float rotPhase1 = jumpDuration * 0.5f; // 0.125s
                float rotPhase2 = jumpDuration;        // 0.250s
                float rotPhase3 = jumpDuration * 0.5f; // 0.125s

                sequence.Insert(0, block.transform.DOLocalRotate(baseRotation + new Vector3(0, 0, rotateAngle), rotPhase1).SetEase(Ease.OutSine));
                sequence.Insert(rotPhase1, block.transform.DOLocalRotate(baseRotation + new Vector3(0, 0, -rotateAngle), rotPhase2).SetEase(Ease.InOutSine));
                sequence.Insert(rotPhase1 + rotPhase2, block.transform.DOLocalRotate(baseRotation, rotPhase3).SetEase(Ease.InSine));

                masterSequence.Insert(i * staggeredDelay, sequence);
            }

            return masterSequence;
        }

        private Sequence DoNotMatchAnim(List<BlockController> blocks)
        {
            if (blocks == null || blocks.Count == 0) return null;

            var pillar = blocks[0].GetPillarParent();
            string tweenId = "Not match";
            var masterSequence = DOTween.Sequence().SetTarget(pillar).SetId(tweenId);

            float duration = 0.4f;
            float strength = 0.05f;
            int vibrato = 10;

            foreach (var block in blocks)
            {
                block.transform.DOKill();
                // Vibrate effect by shaking position specifically on the X axis
                masterSequence.Join(block.transform.DOShakePosition(duration, new Vector3(strength, 0, 0), vibrato));
            }

            masterSequence.JoinCallback(() =>
            {
                HapticManager.DoLightFeedback();
                StartCoroutine(ParticleManager.Instance.PlayParticle(EParticle.Sparkle, blocks[^1].transform.position));
            });

            return masterSequence;
        }
#endregion

#region ON PILLAR CLICK
        public void OnPillarClicked(PillarController pillar)
        {
            if(pillar.IsLocked())
            {
                Debug.Log("Pillar is locked!");
                return;
            }

            if (_selectedBlocks.Count == 0)
            {
                PickUpBlocks(pillar);
                // Debug.Log($"Selected {_selectedBlocks.Count} Blocks!");
            }
            else
            {
                var parentPillar = _selectedBlocks[0].GetPillarParent();
                if (parentPillar == pillar)
                {
                    var toPutBack = new List<BlockController>(_selectedBlocks);
                    _selectedBlocks.Clear();
                    PutBackBlocks(toPutBack, pillar);
                    return;
                }
                else
                {
                    int availableSpace = pillar.GetAvailableSlotCount();
                    var toMove = _selectedBlocks.GetRange(0, Mathf.Min(availableSpace, _selectedBlocks.Count));
                    var toReturn = new List<BlockController>();

                    if (availableSpace < _selectedBlocks.Count)
                    {
                        toReturn = _selectedBlocks.GetRange(availableSpace, _selectedBlocks.Count - availableSpace);
                    }

                    _selectedBlocks.Clear();

                    if (toMove.Count > 0)
                    {
                        MoveBlocks(toMove, parentPillar, pillar);
                    }

                    if (toReturn.Count > 0)
                    {
                        PutBackBlocks(toReturn, parentPillar);
                    }
                    
                    OnBlocksMoved?.Invoke(true);
                }
            }
        }
    }
#endregion
}