using System;
using System.Collections.Generic;
using Assets._Scripts.Helpers;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Assets._Scripts.Controllers
{
    public class BlockMovementController : Singleton<BlockMovementController>
    {
        private List<BlockController> _selectedBlocks = new();

        [SerializeField] private float _pickupHeight;
        public UnityEvent OnBlocksMoved = new();

        private float _blockHeight => GameObjectDataHelper.BlockHeight;

        //TODO: Handle logic when user do multiple interact at once

        public Vector3 GetBlockPosition(PillarController pillar, int index)
        {
            return pillar.BlockContainer.transform.position + index * _blockHeight * Vector3.up;
        }

        // void Start()
        // {
        //     var pillars = FindObjectsByType<PillarController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        //     foreach (var pillar in pillars)
        //     {
        //         pillar.OnPillarClicked.AddListener(() => 
        //         {
        //             if (GameManager.Instance.CurState == Enums.EGameState.Pause) return;
        //             OnPillarClicked(pillar);
        //         });
        //     }
        // }

        void OnDestroy()
        {
            OnBlocksMoved.RemoveAllListeners();
        }

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

            float tweenDuration = 0.3f;
            var sequence = DOTween.Sequence();
            float blockOffset = _blockHeight + .1f;
            float firstOffset = _pickupHeight + pillar.GetBlockCount() * _blockHeight + blocks.Count * blockOffset - blockOffset / 2;
            for (int i = 0; i < blocks.Count; i++)
            {
                blocks[i].transform.DOKill();
                var blockPos = firstOffset - i * blockOffset;
                if (i == 0)
                    sequence.Append(blocks[i].transform.DOMove(pillar.BlockContainer.transform.position + Vector3.up * firstOffset, tweenDuration).SetEase(Ease.OutQuad));
                else
                    sequence.Join(blocks[i].transform.DOMove(pillar.BlockContainer.transform.position + Vector3.up * blockPos, tweenDuration).SetEase(Ease.OutQuad));
            }

            sequence.OnComplete(() =>
            {
                foreach (var block in blocks) DoFloatAnim(block.gameObject);
            });
            sequence.Play();
        }

        private void DoFloatAnim(GameObject gameObject)
        {
            var moveOffset = .12f;
            var rotateOffset = 5f;

            // Di chuyen len xuong
            gameObject.transform.DOMoveY(moveOffset, 0.6f).SetRelative().SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            // Xoay trai phai nhe
            gameObject.transform.DORotate(new Vector3(0, 0, rotateOffset), 0.8f).SetRelative().SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }

        private void StopFloatAnim(GameObject obj)
        {
            obj.transform.DOKill();
            obj.transform.DORotate(Vector3.zero, 0.2f);
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

            float tweenDuration = 0.3f;
            var sequence = DOTween.Sequence();
            var firstPos = pillar.BlockContainer.transform.position + _blockHeight * (pillar.GetBlockCount() - blocks.Count) * Vector3.up;
            StopFloatAnim(blocks[0].gameObject);
            sequence.Append(blocks[0].transform.DOMove(firstPos, tweenDuration).SetEase(Ease.InOutQuad));
            for (int i = 1; i < blocks.Count; i++)
            {
                var blockPos = firstPos + Vector3.up * (i * _blockHeight);
                StopFloatAnim(blocks[i].gameObject);
                sequence.Join(blocks[i].transform.DOMove(blockPos, tweenDuration).SetEase(Ease.InOutQuad));
            }
            sequence.Play();
        }

        private void MoveBlocks(List<BlockController> blocks, PillarController fromPillar, PillarController toPillar)
        {
            if (blocks.Count == 0) return;
            
            toPillar.AddBlocksToTop(blocks);
            //TODO: Check match blocks
            DoMoveBlocksAnim(blocks, fromPillar, toPillar);
        }

        private void DoMoveBlocksAnim(List<BlockController> blocks, PillarController fromPillar, PillarController toPillar)
        {
            if (blocks.Count == 0 || fromPillar == null || toPillar == null) return;

            float duration = 0.7f; 
            float staggeredDelay = 0.05f;
            float jumpPower = 1.5f;
            
            var sequence = DOTween.Sequence();
            int groupStartIndex = toPillar.GetBlockCount() - blocks.Count;

            for (int i = 0; i < blocks.Count; i++)
            {
                var targetSlot = groupStartIndex + i;
                var targetPos = toPillar.BlockContainer.transform.position + Vector3.up * (targetSlot * _blockHeight);
                
                Vector3 fromTop = fromPillar.TopPillar.position;
                Vector3 toTop = toPillar.TopPillar.position;

                // Xac dinh diem cao nhat giua 2 cọc de di chuyen cung
                Vector3 midJump = (fromTop + toTop) / 2f + Vector3.up * jumpPower;
                
                // Quy dao: Tu vi tri hien tai (dang hover) -> Qua dinh coc cu -> Qua diem giua -> Qua dinh coc moi -> Roi xuong
                Vector3[] path = new Vector3[] { fromTop, midJump, toTop, targetPos };
                
                StopFloatAnim(blocks[i].gameObject);
                sequence.Insert(i * staggeredDelay, 
                    blocks[i].transform.DOPath(path, duration, PathType.CatmullRom)
                    .SetEase(Ease.OutQuad));
            }
            sequence.Play();
        }

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
                // Debug.Log($"Pillar {pillar.name} is clicked!");
                var parentPillar = _selectedBlocks[0].GetPillarParent();
                if (parentPillar == pillar)
                {
                    PutBackBlocks(_selectedBlocks, pillar);
                    _selectedBlocks.Clear();
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

                    if (toMove.Count > 0)
                    {
                        MoveBlocks(toMove, parentPillar, pillar);
                    }

                    if (toReturn.Count > 0)
                    {
                        // Debug.Log(1);
                        PutBackBlocks(toReturn, parentPillar);
                    }
                    _selectedBlocks.Clear();
                    OnBlocksMoved?.Invoke();
                }
            }
        }
    }
}