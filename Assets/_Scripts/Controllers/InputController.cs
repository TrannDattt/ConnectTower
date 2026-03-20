using System;
using System.Collections.Generic;
using Assets._Scripts.Patterns;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Assets._Scripts.Controllers
{
    public class InputController : Singleton<InputController>
    {
        private List<BlockController> _selectedBlocks = new();
        private Camera _mainCamera;

        [SerializeField] private float _pillarHeight;
        [SerializeField] private float _pickupHeight;
        [SerializeField] private float _blockHeight;

        public UnityEvent OnBlocksMoved = new();

        void Start()
        {
            _mainCamera = Camera.main;

            var pillars = FindObjectsByType<PillarController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var pillar in pillars)
            {
                pillar.OnPillarClicked.AddListener(() => 
                {
                    if (GameController.Instance.IsPaused) return;
                    OnPillarClicked(pillar);
                });
            }
        }

        private void PickUpBlocks(PillarController pillar)
        {
            _selectedBlocks = pillar.TryGetBlocks();
            PickUpBlocksAnim(_selectedBlocks, pillar);
            // Debug.Log("Selected " + _selectedBlocks.Count + " blocks from " + pillar.name);
        }

        private void PickUpBlocksAnim(List<BlockController> blocks, PillarController pillar)
        {
            if (blocks.Count == 0) return;

            float tweenDuration = 0.3f;
            var sequence = DOTween.Sequence();
            float blockOffset = _blockHeight + .1f;
            float firstOffset = _pickupHeight + pillar.GetBlockCount() * _blockHeight + blocks.Count * blockOffset - blockOffset / 2;
            sequence.Append(blocks[0].transform.DOMove(pillar.BlockContainer.transform.position + Vector3.up * firstOffset, tweenDuration).SetEase(Ease.OutQuad));

            for (int i = 1; i < blocks.Count; i++)
            {
                var blockPos = firstOffset - i * blockOffset;
                sequence.Join(blocks[i].transform.DOMove(pillar.BlockContainer.transform.position + Vector3.up * blockPos, tweenDuration).SetEase(Ease.OutQuad));
            }

            sequence.Play();
        }

        private void PutBackBlocks(List<BlockController> blocks, PillarController pillar)
        {
            if (blocks.Count == 0) return;

            var parentPillar = blocks[0].GetPillarParent();
            // Debug.Log("Returned " + blocks.Count + " blocks to " + parentPillar.name);
            blocks.Reverse();
            parentPillar.AddBlocks(blocks);
            PutBackBlocksAnim(blocks, pillar);
        }

        private void PutBackBlocksAnim(List<BlockController> blocks, PillarController pillar)
        {
            if (blocks.Count == 0) return;

            float tweenDuration = 0.3f;
            var sequence = DOTween.Sequence();
            var firstPos = pillar.BlockContainer.transform.position + _blockHeight * (pillar.GetBlockCount() - blocks.Count) * Vector3.up;
            sequence.Append(blocks[0].transform.DOMove(firstPos, tweenDuration).SetEase(Ease.InOutQuad));
            for (int i = 1; i < blocks.Count; i++)
            {
                var blockPos = firstPos + Vector3.up * (i * _blockHeight);
                sequence.Join(blocks[i].transform.DOMove(blockPos, tweenDuration).SetEase(Ease.InOutQuad));
            }
            sequence.Play();
        }

        private void MoveBlocks(List<BlockController> blocks, PillarController newPillar)
        {
            if (blocks.Count == 0) return;
            
            newPillar.AddBlocks(blocks);
            MoveBlocksAnim(blocks, newPillar);
        }

        private void MoveBlocksAnim(List<BlockController> blocks, PillarController newPillar)
        {
            // TODO: Add animation fly towards new pillar
            if (blocks.Count == 0) return;

            float tweenDuration = 0.3f;
            var sequence = DOTween.Sequence();
            var firstPos = newPillar.BlockContainer.transform.position + _blockHeight * (newPillar.GetBlockCount() - blocks.Count) * Vector3.up;
            sequence.Append(blocks[0].transform.DOMove(firstPos, tweenDuration).SetEase(Ease.InOutQuad));
            for (int i = 1; i < blocks.Count; i++)
            {
                var blockPos = firstPos + Vector3.up * (i * _blockHeight);
                sequence.Join(blocks[i].transform.DOMove(blockPos, tweenDuration).SetEase(Ease.InOutQuad));
            }
            sequence.Play();
        }

        private void OnPillarClicked(PillarController pillar)
        {
            if(pillar.IsLocked())
            {
                Debug.Log("Pillar is locked!");
                return;
            }

            if (_selectedBlocks.Count == 0)
            {
                PickUpBlocks(pillar);
            }
            else
            {
                var parentPillar = _selectedBlocks[0].GetPillarParent();
                if (parentPillar == pillar)
                {
                    PutBackBlocks(_selectedBlocks, pillar);
                    _selectedBlocks.Clear();
                    return;
                }
                else
                {
                    int availableSpace = pillar.GetAvailableSpace();
                    var toMove = _selectedBlocks.GetRange(0, Mathf.Min(availableSpace, _selectedBlocks.Count));
                    var toReturn = new List<BlockController>();

                    if (availableSpace < _selectedBlocks.Count)
                    {
                        toReturn = _selectedBlocks.GetRange(availableSpace, _selectedBlocks.Count - availableSpace);
                    }

                    if (toMove.Count > 0)
                    {
                        MoveBlocks(toMove, pillar);
                    }

                    if (toReturn.Count > 0)
                    {
                        PutBackBlocks(toReturn, parentPillar);
                    }
                    _selectedBlocks.Clear();
                    OnBlocksMoved?.Invoke();
                }
            }
        }

        /// <summary>
        /// Detects the click and returns the object that was clicked.
        /// Works for objects with Collider components.
        /// </summary>
        /// <returns>The clicked GameObject or null if nothing was hit.</returns>
        private GameObject GetClickedObject()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                return hit.collider.gameObject;
            }

            return null;
        }
    }
}