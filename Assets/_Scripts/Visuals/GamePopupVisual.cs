using Assets._Scripts.Controllers;
using UnityEngine;

namespace Assets._Scripts.Visuals
{
    public class GamePopupVisual : MonoBehaviour
    {
        [SerializeField] private GameObject _popupPanel;
        [SerializeField] private RectTransform _popupRt;
        [SerializeField] private GameButtonVisual _closeButton;

        public virtual void Show()
        {
            GameController.Instance.PauseGame();
            _popupPanel.SetActive(true);
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
            _popupPanel.SetActive(false);
            GameController.Instance.ResumeGame();
        }

        protected virtual void Start()
        {
            _closeButton.OnClicked.AddListener(Hide);

            Hide();
        }
    }
}