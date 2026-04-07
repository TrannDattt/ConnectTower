using System.Collections;
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

        public bool IsActive {get; private set;} = false;

        public virtual IEnumerator Show()
        {
            if (GameManager.Instance.CurState == EGameState.Playing) GameManager.Instance.PauseGame();
            // Debug.Log($"Show {name}");
            IsActive = true;
            gameObject.SetActive(true);
            yield return DoShowAnim();
        }

        protected virtual IEnumerator DoShowAnim()
        {
            if (_popupRt == null) yield break;
            
            _popupRt.DOKill();
            _popupRt.localScale = Vector3.zero;
            yield return _popupRt.DOScale(Vector3.one, 0.5f)
                                 .SetEase(Ease.OutBack)
                                 .SetUpdate(true);
        }


        public virtual IEnumerator Hide()
        {
            IsActive = false;
            void OnHide()
            {
                PopupManager.Instance.OnPopupHidden();
                gameObject.SetActive(false);
                if (GameManager.Instance.CurState == EGameState.Pause) GameManager.Instance.ResumeGame();
            }
            yield return DoHideAnim(OnHide);
        }

        protected virtual IEnumerator DoHideAnim(UnityAction onHide)
        {
            _popupRt.DOKill();
            yield return _popupRt.DOScale(Vector3.zero, 0.25f)
                                 .SetEase(Ease.InBack)
                                 .SetUpdate(true)
                                 .OnComplete(() =>
                                 {
                                    onHide?.Invoke();
                                 });
        }

        protected virtual void Start()
        {
            _closeButton?.OnClicked.AddListener(() => StartCoroutine(Hide()));

            // Hide();
        }

        protected virtual void OnDestroy()
        {
            _closeButton?.OnClicked.RemoveAllListeners();
        }
    }
}