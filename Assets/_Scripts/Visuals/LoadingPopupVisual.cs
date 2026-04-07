using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Assets._Scripts.Visuals
{
    public class LoadingPopupVisual : GamePopupVisual
    {
        [SerializeField] private TextMeshProUGUI _loadingText;
        [SerializeField] private float _startOffsetY = 2376f;
        [SerializeField] private float _animDuration = 1f;

        [Header("Character Animation Settings")]
        [SerializeField] private float _bounceHeight = 10f;
        [SerializeField] private float _bounceDuration = 1f;
        [SerializeField] private float _delayBetweenChars = 0.2f;
        [SerializeField] private float _delayBetweenLoops = 0.5f;

        private Vector2 _targetAnchoredPos;
        private bool _isInitialized;
        private Tween _textTween;

        protected override IEnumerator DoShowAnim()
        {
            if (_popupRt == null) yield break;

            if (!_isInitialized)
            {
                _targetAnchoredPos = _popupRt.anchoredPosition;
                _isInitialized = true;
            }

            _popupRt.DOKill();
            _popupRt.anchoredPosition = new Vector2(_targetAnchoredPos.x, _targetAnchoredPos.y + _startOffsetY);

            StartBounceAnim();

            Sequence showSeq = DOTween.Sequence();
            
            float overshootAmount = 40f;
            float dropTime = _animDuration * 0.6f;
            float bounceTime = _animDuration * 0.4f;

            showSeq.Append(_popupRt.DOAnchorPos(_targetAnchoredPos, dropTime).SetEase(Ease.InQuad));
            showSeq.Append(_popupRt.DOAnchorPos(new Vector2(_targetAnchoredPos.x, _targetAnchoredPos.y + overshootAmount), bounceTime * 0.5f).SetEase(Ease.OutQuad));
            showSeq.Append(_popupRt.DOAnchorPos(_targetAnchoredPos, bounceTime * 0.5f).SetEase(Ease.InQuad));

            yield return showSeq.SetUpdate(true).WaitForCompletion();
        }

        private void StartBounceAnim()
        {
            _textTween?.Kill();
            _loadingText.ForceMeshUpdate();
            
            var textInfo = _loadingText.textInfo;
            int charCount = textInfo.characterCount;
            // Tính tổng thời gian để toàn bộ chuỗi ký tự hoàn thành một đợt nảy
            float totalActiveDuration = (charCount - 1) * _delayBetweenChars + _bounceDuration;
            
            float bounceValue = 0;
            Sequence seq = DOTween.Sequence();
            
            // Phần animation nảy
            seq.Append(DOTween.To(() => bounceValue, x => bounceValue = x, totalActiveDuration, totalActiveDuration)
                .SetEase(Ease.Linear)
                .OnUpdate(() =>
                {
                    _loadingText.ForceMeshUpdate();
                    var textInfoUpdate = _loadingText.textInfo;

                    for (int i = 0; i < textInfoUpdate.characterCount; i++)
                    {
                        var charInfo = textInfoUpdate.characterInfo[i];
                        if (!charInfo.isVisible) continue;

                        float charStartTime = i * _delayBetweenChars;
                        float localTime = bounceValue - charStartTime;
                        
                        float yOffset = 0;
                        // Chỉ nảy khi trong khoảng thời gian hiệu lực của ký tự đó
                        if (localTime >= 0 && localTime <= _bounceDuration)
                        {
                            // Sử dụng Sin để tạo hiệu ứng nảy (0 -> PI -> 0)
                            yOffset = Mathf.Sin(localTime * Mathf.PI / _bounceDuration) * _bounceHeight;
                        }

                        var meshInfo = textInfoUpdate.meshInfo[charInfo.materialReferenceIndex];
                        for (int j = 0; j < 4; j++)
                        {
                            var index = charInfo.vertexIndex + j;
                            meshInfo.vertices[index] += new Vector3(0, yOffset, 0);
                        }
                    }

                    for (int i = 0; i < textInfoUpdate.meshInfo.Length; i++)
                    {
                        textInfoUpdate.meshInfo[i].mesh.vertices = textInfoUpdate.meshInfo[i].vertices;
                        _loadingText.UpdateGeometry(textInfoUpdate.meshInfo[i].mesh, i);
                    }
                }));
            
            // Thêm delay giữa mỗi lần làm animation
            seq.AppendInterval(_delayBetweenLoops);
            seq.SetLoops(-1, LoopType.Restart);
            seq.SetUpdate(true);
            _textTween = seq;
        }

        protected override IEnumerator DoHideAnim(UnityAction onHide)
        {
            if (_popupRt == null)
            {
                onHide?.Invoke();
                yield break;
            }

            _textTween?.Kill();
            _popupRt.DOKill();
            
            yield return _popupRt.DOAnchorPos(new Vector2(_targetAnchoredPos.x, _targetAnchoredPos.y + _startOffsetY), _animDuration * 0.6f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .WaitForCompletion();
            
            onHide?.Invoke();
        }
    }
}
