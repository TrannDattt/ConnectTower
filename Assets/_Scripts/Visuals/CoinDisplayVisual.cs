using System.Collections.Generic;
using Assets._Scripts.Controllers;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using Assets._Scripts.Visuals;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class CoinDisplayVisual : MonoBehaviour
    {
        [SerializeField] private GameButtonVisual _buyCoinButton;
        [SerializeField] private Text _coinCountText;

        [SerializeField] private Image _coinImagePrefab;
        [SerializeField] private Transform _coinImageHolder;
        [SerializeField] private Transform _startPoint;
        [SerializeField] private Transform _endPoint;
        [SerializeField] private int _coinAppearAmount = 10;
        [SerializeField] private float _jumpPower = 100f;

        private List<Image> _coinImages = new();

        private static int _lastCount;
        private static bool _isFirstAnim = false;

        public void UpdateVisual() => UpdateVisual(UserManager.CurUser.CoinCount - _lastCount);

        public void UpdateVisual(int amount)
        {
            // Debug.Log($"Update coin visual");
            int to = UserManager.CurUser.CoinCount;
            int from = to - amount;
            bool doAnim = (amount != 0) && _isFirstAnim;
            // Debug.Log($"Update coin visual {(doAnim ? "with" : "without")} anim");
            DoGainCoinAnim(from, to, doAnim ? 1f : 0);
        }

        public void DoGainCoinAnim(int from, int to, float duration)
        {
            _coinCountText.DOKill();

            if (duration <= 0)
            {
                _coinCountText.text = to.ToString();
                return;
            }

            SoundManager.Instance.PlayRandomSFX(ESfx.CoinGained);

            float coinAnimDelay = .05f;
            float coinFlyDuration = duration * 0.7f;
            var coinFlySequence = DOTween.Sequence().SetTarget(this);

            if (to > from)
            {
                for (var i = 0; i < _coinImages.Count; i++)
                {
                    var coin = _coinImages[i];
                    coin.gameObject.SetActive(true);
                    coin.transform.position = _startPoint.position;
                    
                    float delay = i * coinAnimDelay;

                    Vector3 startPos = _startPoint.position;
                    Vector3 endPos = _endPoint.position;

                    float horizontalBias = 0.2f;
                    Vector3 midPos = Vector3.Lerp(startPos, endPos, horizontalBias) - Vector3.up * _jumpPower;

                    Vector3 p1 = Vector3.Lerp(startPos, midPos, 0.5f);
                    Vector3 p2 = Vector3.Lerp(midPos, endPos, 0.5f);

                    Vector3[] path = { p1, midPos, p2, endPos };

                    coinFlySequence.Insert(delay, coin.transform.DOPath(path, coinFlyDuration, PathType.CatmullRom)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() => coin.gameObject.SetActive(false)));

                    if (i == 0)
                    {
                        coinFlySequence.InsertCallback(coinFlyDuration, () =>
                        {
                            DOTween.To(() => from, x => _coinCountText.text = x.ToString(), to, duration).SetTarget(_coinCountText).OnComplete(() =>
                            {
                                _isFirstAnim = false;
                            });
                        });
                    }
                }
            }
            else
            {
                DOTween.To(() => from, x => _coinCountText.text = x.ToString(), to, duration).SetTarget(_coinCountText).OnComplete(() =>
                {
                    _isFirstAnim = false;
                });
            }
        }

        // void OnEnable()
        // {
        //     UpdateVisual(UserManager.CurUser.CoinCount - _lastCount);
        // }

        private void InitPool()
        {
            if (_coinImages.Count > 0) return;

            if (_coinImagePrefab != null)
            {
                for (int i = 0; i < _coinAppearAmount; i++)
                {
                    var newImage = Instantiate(_coinImagePrefab, _coinImageHolder);
                    _coinImages.Add(newImage);
                    newImage.gameObject.SetActive(false);
                }
            }
        }

        private void Awake()
        {
            InitPool();
        }

        void Start()
        {
            _buyCoinButton.OnClicked.AddListener(() =>
            {
                var activeScene = GameSceneManager.Instance.GetActiveScene();
                if (activeScene == EGameScene.Menu)
                    MainMenuVisualControl.Instance.OpenShop();
                else if (activeScene == EGameScene.Ingame)
                    StartCoroutine(PopupManager.Instance.ShowPopup(EPopup.Shop));
            });

            InitPool();

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