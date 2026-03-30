using Assets._Scripts.Controllers;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using UnityEngine;

namespace Assets._Scripts.Visuals
{
    public class GamePopupVisual : MonoBehaviour
    {
        [SerializeField] protected RectTransform _popupRt;
        [SerializeField] protected GameButtonVisual _closeButton;

        public virtual void Show()
        {
            if (GameManager.Instance.CurState == EGameState.Playing) GameManager.Instance.PauseGame();
            Debug.Log($"Show {name}");
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
            if (GameManager.Instance.CurState == EGameState.Pause) GameManager.Instance.ResumeGame();
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