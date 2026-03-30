using System.Collections.Generic;
using Assets._Scripts.Controllers;
using Assets._Scripts.Helpers;
using Assets._Scripts.Patterns;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
#endif

namespace Assets._Scripts.Managers
{
    public class SlotLayoutManager : Singleton<SlotLayoutManager>
    {
        [SerializeField] private Transform _pillarHolder;
        [SerializeField] private PillarController _pillarPrefab;
        [SerializeField] private BlockController _blockPrefab;
        [SerializeField] private int _maxPerRow = 5;
        [SerializeField] private float _maxSpaceX = 2.5f;
        [SerializeField] private float _minSpaceX = .94f;
        [SerializeField] private float _rowSpacing = 4.2f;
        
        public BlockController GetBlock(int index, PillarController pillar)
        {
            Vector3 blockPos = Vector3.zero;
            Transform parent = _pillarHolder;
            if (index >= 0 && pillar != null)
            {
                //TODO: Get block pos
                blockPos = pillar.BlockContainer.transform.position + GameObjectDataHelper.BlockHeight * index * Vector3.up;
                parent = pillar.BlockContainer.transform;
            }

            var newBlock = Instantiate(_blockPrefab, blockPos, Quaternion.identity, parent);
            return newBlock;
        }

        public void ReturnBlock(BlockController block)
        {
            if (block == null) return;
            Destroy(block.gameObject);
        }

        //TODO: Use Pooling
        public PillarController GetPillar(Vector3 pos)
        {
            var newPillar = Instantiate(_pillarPrefab, pos, Quaternion.identity, _pillarHolder);
            return newPillar;
        }

        //TODO: Do some camera work to resize 
        public List<PillarController> GetPillars(int amount)
        {
            List<PillarController> _pillars = new();
            var positions = GetPillarPositions(amount);
            foreach (var pos in positions)
            {
                var newPillar = GetPillar(pos);
                _pillars.Add(newPillar);
            }
            return _pillars;
        }

        public void ReturnPillar(PillarController pillar)
        {
            if (pillar == null) return;

            foreach(var block in pillar.GetAllBlocks())
            {
                ReturnBlock(block);
            }

            Destroy(pillar.gameObject);
        }

        public void ReturnAllPillar(List<PillarController> pillars)
        {
            if (pillars.Count == 0) return;

            while(pillars.Count > 0)
            {
                var pillar = pillars[^1];
                pillars.Remove(pillar);
                ReturnPillar(pillar);
            }
        }

        private List<Vector3> GetPillarPositions(int amount)
        {
            List<Vector3> positions = new();
            Camera mainCam = Camera.main;
            if (mainCam == null) return positions;

            float screenWidthWorld;
            if (mainCam.orthographic)
            {
                screenWidthWorld = mainCam.orthographicSize * 2 * mainCam.aspect;
            }
            else
            {
                // For perspective, assume depth is where _pillarHolder is
                float distance = Mathf.Abs(_pillarHolder.position.z - mainCam.transform.position.z);
                screenWidthWorld = 2.0f * distance * Mathf.Tan(mainCam.fieldOfView * 0.5f * Mathf.Deg2Rad) * mainCam.aspect;
            }

            float margin = 1.0f; // Padding from screen edges
            float availableWidth = screenWidthWorld - 2 * margin;

            int rowCount = Mathf.CeilToInt((float)amount / _maxPerRow);
            int pillarsLeft = amount;

            float totalHeight = (rowCount - 1) * _rowSpacing;
            float startY = _pillarHolder.position.y - totalHeight / 2f;

            for (int r = 0; r < rowCount; r++)
            {
                int currentPillarsInRow = Mathf.Min(pillarsLeft, _maxPerRow);
                float yPos = startY + r * _rowSpacing;

                float spacing;
                if (currentPillarsInRow > 1)
                {
                    spacing = Mathf.Clamp(availableWidth / (currentPillarsInRow - 1), _minSpaceX, _maxSpaceX);
                }
                else
                {
                    spacing = 0;
                }

                float rowWidth = (currentPillarsInRow - 1) * spacing;
                float startX = _pillarHolder.position.x - rowWidth / 2f;

                for (int i = 0; i < currentPillarsInRow; i++)
                {
                    positions.Add(new Vector3(startX + i * spacing, yPos, _pillarHolder.position.z));
                }

                pillarsLeft -= currentPillarsInRow;
            }

            return positions;
        }
    }
}