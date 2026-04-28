using System.Collections;
using System.Collections.Generic;
using Assets._Scripts.Controllers;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns.EventBus;
using Assets._Scripts.Visuals;
using Coffee.UIExtensions;
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
        [SerializeField] private UIParticleAttractor _coinAttractor;

        private List<Image> _coinImages = new();
        private EventBinding<CurrencyChangedEvent> _currencyChangedBinding;

        private static int _lastCount;
        private static bool _isFirstAnim = false;

        public void UpdateVisual() => UpdateVisual(UserManager.CurUser.CoinCount - _lastCount);

        public void UpdateVisual(int amount)
        {
            Debug.Log($"Update coin visual {amount}");
            int to = UserManager.CurUser.CoinCount;
            int from = to - amount;
            bool doAnim = (amount != 0) && _isFirstAnim && gameObject.activeInHierarchy;
            if (doAnim)
            {
                StartCoroutine(DoGainCoinAnim(from, to, 1f));
            }
            else
            {
                _coinCountText.text = to.ToString();
            }
        }

        public void AssignCoinParticle(ParticleSystem coinParticle)
        {
            _coinAttractor.AddParticleSystem(coinParticle);
        }

        private IEnumerator DoGainCoinAnim(int from, int to, float duration)
        {
            Debug.Log("Do coin anim");
            float textDelayTime = 0;
            SoundManager.Instance.PlayRandomSFX(ESfx.CoinGained);

            if (to > from)
            {
                ParticleManager.Instance.StartCoroutine(ParticleManager.Instance.PlayParticle(EParticle.CoinFly, _startPoint.position, transform.parent));
                textDelayTime = ParticleManager.Instance.GetParticleDuration(EParticle.CoinFly) * 0.7f;
            }

            if (textDelayTime > 0)
            {
                yield return new WaitForSeconds(textDelayTime);
            }
            
            yield return DOTween.To(() => from, x => _coinCountText.text = x.ToString(), to, duration)
                                .SetTarget(_coinCountText)
                                .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                                .OnKill(() =>
                                {
                                    _isFirstAnim = false;
                                })
                                .WaitForCompletion();
        }

        private void OnEnable()
        {
            _currencyChangedBinding ??= new((evt) =>
            {
                _isFirstAnim = true;
                UpdateVisual(evt.CoinChanged);
            });
            EventBus<CurrencyChangedEvent>.Subscribe(_currencyChangedBinding);

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
                    StartCoroutine(PopupManager.Instance.ShowPopup(EPopup.Shop));
            });
        }

        void OnDisable()
        {
            EventBus<CurrencyChangedEvent>.Unsubscribe(_currencyChangedBinding);
            DOTween.Kill(_coinCountText, true);
            _lastCount = UserManager.CurUser.CoinCount;
            _coinCountText.text = _lastCount.ToString();
        }

        // private void OnDestroy()
        // {
        //     UserManager.OnCoinChanged.RemoveListener(UpdateVisual);
        // }
    }
}