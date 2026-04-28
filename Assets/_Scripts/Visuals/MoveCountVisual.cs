using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class MoveCountVisual : MonoBehaviour
    {
        [SerializeField] private Text _moveCountText;
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

        public Tween UpdateMoveCount(int count, float duration = 0)
        {
            if (_moveCountText == null) return null;

            if (duration <= 0)
            {
                _moveCountText.text = count.ToString();
                UpdateWarning(count);
                return null;
            }

            int startVal = 0;
            int.TryParse(_moveCountText.text, out startVal);

            Sequence sequence = DOTween.Sequence().SetTarget(gameObject).SetLink(gameObject, LinkBehaviour.CompleteAndKillOnDisable).SetUpdate(true);
            
            // 1. Phóng to lên
            sequence.Append(_moveCountText.transform.DOScale(_originalScale * 1.4f, duration * 0.3f).SetEase(Ease.OutBack));
            
            // 2. Chạy số (Join để chạy song song với các hiệu ứng khác nếu cần)
            sequence.Join(DOTween.To(() => startVal, x => 
            {
                startVal = x;
                _moveCountText.text = x.ToString();
            }, count, duration).SetEase(Ease.OutQuad));

            // 3. Thu nhỏ lại về ban đầu
            sequence.Append(_moveCountText.transform.DOScale(_originalScale, duration * 0.3f).SetEase(Ease.InBack));

            sequence.OnComplete(() => UpdateWarning(count));
            
            return sequence.Play();
        }

        private void UpdateWarning(int count)
        {
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
                DOTween.Kill(_moveCountText, true);
            }
        }

        private void WarnLowMoves()
        {
            // Kill existing to be safe
            DOTween.Kill(_moveCountText);

            _warnSequence = DOTween.Sequence().SetTarget(_moveCountText).SetLink(gameObject, LinkBehaviour.CompleteAndKillOnDisable);
            
            // Thiết lập hiệu ứng nhấp nháy màu đỏ và phóng to thu nhỏ
            _warnSequence.Append(_moveCountText.DOColor(Color.red, 0.5f).SetEase(Ease.InOutSine))
                        .Join(_moveCountText.transform.DOScale(_originalScale * 1.2f, 0.5f).SetEase(Ease.InOutSine))
                        .SetLoops(-1, LoopType.Yoyo)
                        .OnKill(StopWarningEffect);
            
            _warnSequence.Play();
        }

        private void StopWarningEffect()
        {
            _moveCountText.color = _originalColor;
            _moveCountText.transform.localScale = _originalScale;
        }
    }
}