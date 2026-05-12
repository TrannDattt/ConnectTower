using System.Collections.Generic;
using Assets._Scripts.Controllers;
using Assets._Scripts.Helpers;
using Assets._Scripts.Patterns;
using UnityEngine;

namespace Assets._Scripts.Managers
{
    public class SlotLayoutManager : Singleton<SlotLayoutManager>
    {
        [SerializeField] private int _maxPerRow = 5;
        [SerializeField] private float _maxSpaceX = 2.5f;
        [SerializeField] private float _minSpaceX = .94f;
        [SerializeField] private float _rowSpacing = 4.2f;
        [SerializeField] private int _referencePillarCount = 10;
        [SerializeField] private float _referenceCameraSize = 5.3f;
        [SerializeField] private float _cameraPaddingX = .75f;
        [SerializeField] private float _cameraMinSize = 4f;

        public List<Vector3> GetPillarPositions(int amount, Transform board)
        {
            List<Vector3> positions = new();
            Camera mainCam = Camera.main;
            if (mainCam == null || amount <= 0 || board == null) return positions;

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

            float availableWidth = Mathf.Max(0f, screenWidthWorld - 2f * _cameraPaddingX);

            int rowCount = Mathf.Max(1, Mathf.CeilToInt((float)amount / _maxPerRow));
            int minPerRow = amount / rowCount;
            int extraPillars = amount % rowCount;

            float totalHeight = (rowCount - 1) * _rowSpacing;
            float startY = board.position.y - totalHeight / 2f;
            float widestRowWidth = 0f;

            for (int r = 0; r < rowCount; r++)
            {
                int currentPillarsInRow = minPerRow + (r < extraPillars ? 1 : 0);
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
                widestRowWidth = Mathf.Max(widestRowWidth, rowWidth);
                float startX = board.position.x - rowWidth / 2f;

                for (int i = 0; i < currentPillarsInRow; i++)
                {
                    positions.Add(new Vector3(startX + i * spacing, yPos, board.position.z));
                }
            }

            // if (mainCam.orthographic)
            // {
            //     int referenceRowCount = Mathf.Max(1, Mathf.CeilToInt((float)_referencePillarCount / _maxPerRow));
            //     float referenceHeight = (referenceRowCount - 1) * _rowSpacing;
            //     float verticalSize = _referenceCameraSize + (totalHeight - referenceHeight) * .5f;
            //     float widthSize = ((widestRowWidth * .5f) + _cameraPaddingX) / mainCam.aspect;
            //     mainCam.orthographicSize = Mathf.Max(_cameraMinSize, verticalSize, widthSize);
            // }

            return positions;
        }
    }
}
