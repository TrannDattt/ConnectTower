using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Assets._Scripts.Visuals
{
    public class GamePopupVisual : MonoBehaviour
    {
        [SerializeField] protected RectTransform _popupRt;
        // [SerializeField] protected RectTransform _baseRt;
        [SerializeField] protected GameButtonVisual _closeButton;

        public virtual void Show()
        {
            if (GameManager.Instance.CurState == EGameState.Playing) GameManager.Instance.PauseGame();
            Debug.Log($"Show {name}");
            gameObject.SetActive(true);
            DoShowAnim();
        }

        private void DoShowAnim()
        {
            if (_popupRt == null) return;
            
            _popupRt.DOKill();
            _popupRt.localScale = Vector3.zero;
            _popupRt.DOScale(Vector3.one, 0.5f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }


        public virtual void Hide()
        {
            void OnHide()
            {
                gameObject.SetActive(false);
                if (GameManager.Instance.CurState == EGameState.Pause) GameManager.Instance.ResumeGame();
            }
            DoHideAnim(OnHide);
        }

        private void DoHideAnim(UnityAction onHide)
        {
            _popupRt.DOKill();
            _popupRt.DOScale(Vector3.zero, 0.25f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                   onHide?.Invoke();
                });
        }

        protected virtual void Start()
        {
            _closeButton?.OnClicked.AddListener(Hide);

            // Hide();
        }

        protected virtual void OnDestroy()
        {
            _closeButton?.OnClicked.RemoveAllListeners();
        }
    }
}