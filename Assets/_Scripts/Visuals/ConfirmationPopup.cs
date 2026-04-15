using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class ConfirmationPopup : GamePopupVisual
    {
        [SerializeField] private Text _content;
        [SerializeField] private GameButtonVisual _confirmButton;
        [SerializeField] private GameButtonVisual _declineButton;

        public void SetContent(string content, string confirmContent = "", string declineContent = "")
        {
            _content.text = content;
            if (!string.IsNullOrEmpty(confirmContent)) _confirmButton.SetContent(confirmContent);
            if (!string.IsNullOrEmpty(declineContent)) _declineButton.SetContent(declineContent);
        }

        public void SetActions(UnityAction onConfirmed, UnityAction onDeclined)
        {
            _confirmButton.OnClicked.RemoveAllListeners();
            _declineButton.OnClicked.RemoveAllListeners();
            _confirmButton.OnClicked.AddListener(() =>
            {
                onConfirmed?.Invoke();
                StartCoroutine(Hide());
            });
            _declineButton.OnClicked.AddListener(() =>
            {
                onDeclined?.Invoke();
                StartCoroutine(Hide());
            });
        }
    }
}