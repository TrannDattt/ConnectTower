using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
#endif

namespace Assets._Scripts.Visuals
{
    public class AvatarSelectorVisual : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scroll;
        [SerializeField] private AvatarSelectorButton _buttonPrefab;
        [SerializeField] private RectTransform _content;
        [SerializeField, Min(0f)] private float _iconScaleDelay = 0.03f;
        [SerializeField] private AnimationCurve _iconScaleUpCurve;
        [SerializeField] private AnimationCurve _iconScaleDownCurve;

        private readonly List<AvatarSelectorButton> _spawnedButtons = new();
        private readonly List<Vector3> _spawnedButtonScales = new();
        
        public void Init(Sprite[] avatarPool, Action<Sprite> onAvatarSelected)
        {
            int index = 0;

            for (; index < avatarPool.Length; index++)
            {
                AvatarSelectorButton button;
                if (_spawnedButtons.Count <= index)
                {
                    button = Instantiate(_buttonPrefab, _content);
                    _spawnedButtons.Add(button);
                    _spawnedButtonScales.Add(button.transform.localScale);
                }
                else
                    button = _spawnedButtons[index];

                button.name = $"{_buttonPrefab.name}_{avatarPool[index].name}";
                button.gameObject.SetActive(true);
                button.Init(avatarPool[index], onAvatarSelected);
            }

            for (; index < _spawnedButtons.Count; index++)
            {
                _spawnedButtons[index].gameObject.SetActive(false);
            }
        }

        public Sequence SetVisible(float timeStamp, float dur)
        {
            var sequence = DOTween.Sequence();
            var orderedIndices = GetActiveButtonIndicesInRevealOrder();

            if (orderedIndices.Count == 0)
                return sequence;

            float delayStep = GetDelayStep(dur, orderedIndices.Count);
            float iconDur = Mathf.Max(0f, dur - (delayStep * (orderedIndices.Count - 1)));

            for (int revealIndex = 0; revealIndex < orderedIndices.Count; revealIndex++)
            {
                int buttonIndex = orderedIndices[revealIndex];
                var buttonRt = _spawnedButtons[buttonIndex].transform as RectTransform;
                if (buttonRt == null) continue;

                buttonRt.DOKill();
                buttonRt.localScale = Vector3.zero;

                sequence.Insert(timeStamp + revealIndex * delayStep,
                                buttonRt.DOScale(_spawnedButtonScales[buttonIndex], iconDur)
                                        .SetEase(_iconScaleUpCurve));
            }

            return sequence;
        }

        public Sequence SetInvisible(float timeStamp, float dur)
        {
            var sequence = DOTween.Sequence();
            var orderedIndices = GetActiveButtonIndicesInRevealOrder();

            if (orderedIndices.Count == 0)
            {
                sequence.OnComplete(ResetScrollToStart);
                return sequence;
            }

            float delayStep = GetDelayStep(dur, orderedIndices.Count);
            float iconDur = Mathf.Max(0f, dur - (delayStep * (orderedIndices.Count - 1)));

            for (int hideIndex = 0; hideIndex < orderedIndices.Count; hideIndex++)
            {
                int buttonIndex = orderedIndices[orderedIndices.Count - 1 - hideIndex];
                var buttonRt = _spawnedButtons[buttonIndex].transform as RectTransform;
                if (buttonRt == null) continue;

                buttonRt.DOKill();
                buttonRt.localScale = _spawnedButtonScales[buttonIndex];

                sequence.Insert(timeStamp + hideIndex * delayStep,
                                buttonRt.DOScale(Vector3.zero, iconDur)
                                        .SetEase(_iconScaleDownCurve));
            }

            sequence.OnComplete(ResetScrollToStart);
            return sequence;
        }

        private List<int> GetActiveButtonIndicesInRevealOrder()
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);

            var orderedIndices = new List<int>();
            for (int i = 0; i < _spawnedButtons.Count; i++)
            {
                if (_spawnedButtons[i].gameObject.activeSelf)
                    orderedIndices.Add(i);
            }

            orderedIndices.Sort(CompareButtonRevealOrder);
            return orderedIndices;
        }

        private int CompareButtonRevealOrder(int leftIndex, int rightIndex)
        {
            var leftRt = _spawnedButtons[leftIndex].transform as RectTransform;
            var rightRt = _spawnedButtons[rightIndex].transform as RectTransform;

            if (leftRt == null || rightRt == null)
                return leftIndex.CompareTo(rightIndex);

            Vector2 leftPos = leftRt.anchoredPosition;
            Vector2 rightPos = rightRt.anchoredPosition;

            float leftDistanceFromTopLeft = leftPos.x - leftPos.y;
            float rightDistanceFromTopLeft = rightPos.x - rightPos.y;
            int diagonalCompare = leftDistanceFromTopLeft.CompareTo(rightDistanceFromTopLeft);
            if (diagonalCompare != 0)
                return diagonalCompare;

            int rowCompare = rightPos.y.CompareTo(leftPos.y);
            if (rowCompare != 0)
                return rowCompare;

            return leftPos.x.CompareTo(rightPos.x);
        }

        private float GetDelayStep(float dur, int buttonCount)
        {
            if (buttonCount <= 1 || dur <= 0f)
                return 0f;

            return Mathf.Min(_iconScaleDelay, dur / buttonCount);
        }

        private void ResetScrollToStart()
        {
            if (_scroll == null)
                _scroll = GetComponentInParent<ScrollRect>();

            if (_scroll == null)
                return;

            _scroll.StopMovement();
            Canvas.ForceUpdateCanvases();
            _scroll.normalizedPosition = new Vector2(0f, 1f);
        }
    }
}
