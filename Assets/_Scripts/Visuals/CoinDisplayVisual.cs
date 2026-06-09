using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
        [SerializeField] private Transform _startPoint;
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
                _coinCountText.text = FormatCoinCount(to);
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
            
            yield return DOTween.To(() => from, x => _coinCountText.text = FormatCoinCount(x), to, duration)
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
            _coinCountText.text = FormatCoinCount(_lastCount);
        }

        private static string FormatCoinCount(int amount)
        {
            if (amount >= 1_000_000_000)
                return FormatWithSuffix(amount, 1_000_000_000d, "B", true);

            if (amount >= 100_000_000)
                return FormatWithSuffix(amount, 1_000_000d, "M", false);

            if (amount >= 1_000_000)
                return FormatWithSuffix(amount, 1_000_000d, "M", true);

            if (amount >= 100_000)
                return FormatWithSuffix(amount, 1_000d, "K", false);

            if (amount >= 1_000)
                return FormatWithSuffix(amount, 1_000d, "K", true);

            return amount.ToString();
        }

        private static string FormatWithSuffix(int amount, double divisor, string suffix, bool showDecimals)
        {
            double shortenedValue = amount / divisor;
            if (!showDecimals)
                return $"{Math.Floor(shortenedValue)}{suffix}";

            double truncatedValue = Math.Floor(shortenedValue * 100d) / 100d;
            return $"{truncatedValue.ToString("0.00", CultureInfo.InvariantCulture)}{suffix}";
        }

        // private void OnDestroy()
        // {
        //     UserManager.OnCoinChanged.RemoveListener(UpdateVisual);
        // }
    }
}
