using Assets._Scripts.Helpers;
using Assets._Scripts.Managers;
using Assets._Scripts.Visuals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class HeartDisplayVisual : MonoBehaviour
    {
        [SerializeField] private GameButtonVisual _buyHeartButton;
        [SerializeField] private Text _heartRecoverTimeText;
        [SerializeField] private Text _heartCountText;

        private int _lastCount = UserLifeHelper.MAX_LIFE;

        public void UpdateVisual() => UpdateVisual(UserManager.CurUser.HeartCount - _lastCount);

        public void UpdateVisual(int amount)
        {
            _lastCount = UserManager.CurUser.HeartCount;
            _heartCountText.text = _lastCount.ToString();
            
            if (_lastCount == UserLifeHelper.MAX_LIFE)
            {
                CancelInvoke(nameof(UpdateTimeCounter));
                _heartRecoverTimeText.text = "MAX";
            }
            else if (!IsInvoking(nameof(UpdateTimeCounter)))
            {
                InvokeRepeating(nameof(UpdateTimeCounter), 0, 1);
            }
        }

        private void UpdateTimeCounter()
        {
            float timeToRecover = UserLifeHelper.TimeToRecorver;

            if (timeToRecover <= 0 && UserManager.CurUser.HeartCount < UserLifeHelper.MAX_LIFE)
            {
                UserManager.RecoverHeart();
                if (UserManager.CurUser.HeartCount == UserLifeHelper.MAX_LIFE) return;
                timeToRecover = UserLifeHelper.TimeToRecorver;
            }

            int minutes = (int)timeToRecover / 60;
            int seconds = (int)timeToRecover % 60;
            _heartRecoverTimeText.text = $"{minutes}:{seconds:D2}";
        }

        void Start()
        {
            UpdateVisual();
            UserManager.OnHeartChanged.AddListener(UpdateVisual);
        }

        void OnDestroy()
        {
            UserManager.OnHeartChanged.RemoveListener(UpdateVisual);
        }
    }
}