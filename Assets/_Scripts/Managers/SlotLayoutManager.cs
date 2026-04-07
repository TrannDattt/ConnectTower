using System.Collections.Generic;
using Assets._Scripts.Controllers;
using Assets._Scripts.Helpers;
using Assets._Scripts.Patterns;
using UnityEngine;

namespace Assets._Scripts.Managers
{
    public class SlotLayoutManager : Singleton<SlotLayoutManager>
    {
        [SerializeField] private float _maxSpaceX = 2.5f;
        [SerializeField] private float _minSpaceX = .94f;
        [SerializeField] private float _rowSpacing = 4.2f;

        public List<Vector3> GetPillarPositions(int amount, Transform board)
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
                // For perspective, assume depth is where board is
                float distance = Mathf.Abs(board.position.z - mainCam.transform.position.z);
                screenWidthWorld = 2.0f * distance * Mathf.Tan(mainCam.fieldOfView * 0.5f * Mathf.Deg2Rad) * mainCam.aspect;
            }

            float margin = 1.0f; // Padding from screen edges
            float availableWidth = screenWidthWorld - 2 * margin;

            int rowCount = amount > 4 ? 2 : 1;
            int maxPerRow = Mathf.CeilToInt((float)amount / rowCount);
            int pillarsLeft = amount;

            float totalHeight = (rowCount - 1) * _rowSpacing;
            float startY = board.position.y - totalHeight / 2f;

            for (int r = 0; r < rowCount; r++)
            {
                int currentPillarsInRow = Mathf.Min(pillarsLeft, maxPerRow);
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
                float startX = board.position.x - rowWidth / 2f;

                for (int i = 0; i < currentPillarsInRow; i++)
                {
                    positions.Add(new Vector3(startX + i * spacing, yPos, board.position.z));
                }

                pillarsLeft -= currentPillarsInRow;
            }

            return positions;
        }
    }
}