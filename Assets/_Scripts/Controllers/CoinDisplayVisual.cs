using System.Collections.Generic;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using Assets._Scripts.Visuals;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Controllers
{
    public class CoinDisplayVisual : MonoBehaviour
    {
        [SerializeField] private GameButtonVisual _buyCoinButton;
        [SerializeField] private TextMeshProUGUI _coinCountText;
        [SerializeField] private Transform _coinAppearPos;
        [SerializeField] private Image _coinImagePrefab;
        [SerializeField] private int _coinAppearAmount = 10;

        private List<Image> _coinImages = new();

        private static int _lastCount;
        private static bool _isFirstAnim = true;

        public void UpdateVisual(int amount)
        {
            Debug.Log($"Update coin visual");
            int to = UserManager.CurUser.CoinCount;
            int from = to - amount;
            bool doAnim = (amount != 0) && _isFirstAnim;
            Debug.Log($"Update coin visual {(doAnim ? "with" : "without")} anim");
            DoGainCoinAnim(from, to, doAnim ? .5f : 0);
        }

        public void DoGainCoinAnim(int from, int to, float duration)
        {
            foreach(var image in _coinImages)
            {
                image.transform.position = _coinAppearPos.position;
            }

            _coinCountText.DOKill();
            DOTween.To(() => from, x => _coinCountText.text = x.ToString(), to, duration).SetTarget(_coinCountText).OnComplete(() =>
            {
                _isFirstAnim = false;
            });
        }

        void OnEnable()
        {
            UpdateVisual(UserManager.CurUser.CoinCount - _lastCount);
        }

        void Start()
        {
            _buyCoinButton.OnClicked.AddListener(() =>
            {
                var activeScene = GameSceneManager.Instance.GetActiveScene();
                if (activeScene == EGameScene.Menu)
                    MainMenuVisualControl.Instance.OpenShop();
                else if (activeScene == EGameScene.Ingame)
                    IngameVisualController.Instance.OpenShop();
            });

            if (_coinImagePrefab != null)
            {
                for(int i = 0; i < _coinAppearAmount; i++)
                {
                    var newImage = Instantiate(_coinImagePrefab, _coinAppearPos);
                    _coinImages.Add(newImage);
                }
            }
            UserManager.OnCoinChanged.AddListener((amount) => 
            {
                _isFirstAnim = true;
                UpdateVisual(amount);
            });
        }

        void OnDisable()
        {
            int.TryParse(_coinCountText.text, out _lastCount);
        }

        // private void OnDestroy()
        // {
        //     UserManager.OnCoinChanged.RemoveListener(UpdateVisual);
        // }
    }
}