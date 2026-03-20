using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Assets._Scripts.Visuals
{
    public class MoveCountVisual : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _moveCountText;
        [Tooltip("Số lượt còn lại tối thiểu để bắt đầu hiệu ứng cảnh báo")]
        [SerializeField] private int _warnThreshold = 5;

        private Sequence _warnSequence;
        private Color _originalColor;
        private Vector3 _originalScale;

        private void Awake()
        {
            if (_moveCountText != null)
            {
                _originalColor = _moveCountText.color;
                _originalScale = _moveCountText.transform.localScale;
            }
        }

        public void UpdateMoveCount(int count)
        {
            if (_moveCountText == null) return;

            _moveCountText.text = count.ToString();

            if (count <= _warnThreshold)
            {
                // Chỉ bắt đầu hiệu ứng nếu nó chưa chạy
                if (_warnSequence == null || !_warnSequence.IsActive() || !_warnSequence.IsPlaying())
                {
                    WarnLowMoves();
                }
            }
            else
            {
                StopWarningEffect();
            }
        }

        private void WarnLowMoves()
        {
            // Kill existing to be safe
            _warnSequence?.Kill();

            _warnSequence = DOTween.Sequence();
            
            // Thiết lập hiệu ứng nhấp nháy màu đỏ và phóng to thu nhỏ
            _warnSequence.Append(_moveCountText.DOColor(Color.red, 0.5f).SetEase(Ease.InOutSine))
                        .Join(_moveCountText.transform.DOScale(_originalScale * 1.2f, 0.5f).SetEase(Ease.InOutSine))
                        .SetLoops(-1, LoopType.Yoyo);
            
            _warnSequence.Play();
        }

        private void StopWarningEffect()
        {
            if (_warnSequence != null && _warnSequence.IsActive())
            {
                _warnSequence.Kill();
                
                // Khôi phục lại trạng thái ban đầu
                _moveCountText.color = _originalColor;
                _moveCountText.transform.localScale = _originalScale;
            }
        }

        private void OnDestroy()
        {
            // Luôn kill tween khi object bị hủy để tránh lỗi rò rỉ bộ nhớ hoặc null reference
            _warnSequence?.Kill();
        }
    }
}