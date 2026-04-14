using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using DG.Tweening;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class LevelFinishedVisual : GamePopupVisual
    {

        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private GameButtonVisual _continueButton;
        [SerializeField] private GameButtonVisual _normalRewardButton;
        [SerializeField] private Text _normalRewardText;
        [SerializeField] private GameButtonVisual _adsRewardButton;
        [SerializeField] private Text _adsRewardText;

        [SerializeField] private List<GameObject> _blockImages;
        [SerializeField] private Transform _startPoint;

        [SerializeField] private ParticleSystem _topConfetti1;
        [SerializeField] private ParticleSystem _topConfetti2;
        [SerializeField] private ParticleSystem _bottomConfetti1;
        [SerializeField] private ParticleSystem _bottomConfetti2;

        private LevelRuntimeData _curLevelData => LevelManager.PlayingLevel;

        public override IEnumerator Show()
        {
            var clearedState = _curLevelData.IsCleared;
            _continueButton.gameObject.SetActive(clearedState);
            _normalRewardText.text = _curLevelData.CoinReward.ToString();
            _normalRewardButton.gameObject.SetActive(!clearedState);
            _adsRewardButton.gameObject.SetActive(!clearedState);
            _adsRewardText.text = (_curLevelData.CoinReward * 2).ToString();

            PrepareBlockImage();

            yield return base.Show();

            SoundManager.Instance.PlayRandomSFX(ESfx.Win);
            yield return DoTextAnim().WaitForCompletion();

            DoBlockImageAnim();
            yield return PlayParticles();
        }

        private Sequence DoTextAnim()
        {
            if (!_titleText.gameObject.TryGetComponent<WarpTextExample>(out var wrapper)) return null;

            var sequence = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDisable);

            var targetPos = _titleText.transform.localPosition;
            var targetColor = _titleText.color;
            var targetScale = Vector3.one;
            var targetCurveScale = 23f;

            float overshoot = .2f;

            var startPos = targetPos - Vector3.up * 250f;
            var startScale = targetScale * 0.4f;
            var startColor = new Color(targetColor.r, targetColor.g, targetColor.b, 0);
            var startCurveScale = 150f;

            // Manual Overshoot Values
            var overshootPos = targetPos - (startPos - targetPos) * overshoot;
            var overshootScale = targetScale - (startScale - targetScale) * overshoot;
            var overshootCurve = targetCurveScale - (startCurveScale - targetCurveScale) * overshoot;

            float popDuration = 0.45f;
            float settleDuration = 0.25f;

            // Initial State setup
            _titleText.transform.localPosition = startPos;
            _titleText.transform.localScale = startScale;
            _titleText.color = startColor;
            wrapper.CurveScale = startCurveScale;

            // --- Pop Sequence (Moves to overshoot position/scale) ---
            var popSeq = DOTween.Sequence();
            popSeq.Append(_titleText.transform.DOLocalMoveY(overshootPos.y, popDuration).SetEase(Ease.OutQuad));
            popSeq.Join(_titleText.transform.DOScale(overshootScale, popDuration).SetEase(Ease.OutQuad));
            popSeq.Join(_titleText.DOFade(1f, popDuration * 0.3f).SetEase(Ease.OutCubic));
            popSeq.Join(DOTween.To(() => wrapper.CurveScale, x => wrapper.CurveScale = x, overshootCurve, popDuration).SetEase(Ease.OutQuad));

            // --- Settle Sequence (Returns to target position/scale) ---
            var settleSeq = DOTween.Sequence();
            settleSeq.Append(_titleText.transform.DOLocalMoveY(targetPos.y, settleDuration).SetEase(Ease.OutSine));
            settleSeq.Join(_titleText.transform.DOScale(targetScale, settleDuration).SetEase(Ease.OutSine));
            settleSeq.Join(DOTween.To(() => wrapper.CurveScale, x => wrapper.CurveScale = x, targetCurveScale, settleDuration).SetEase(Ease.OutSine));

            // Assemble Main Sequence
            sequence.Append(popSeq);
            sequence.Append(settleSeq);
            sequence.Append(_titleText.transform.DOPunchScale(Vector3.one * 0.08f, 0.3f, 10, 0.5f));

            void reset()
            {
                _titleText.transform.localPosition = targetPos;
                _titleText.transform.localScale = targetScale;
                _titleText.color = targetColor;
                wrapper.CurveScale = targetCurveScale;
            }

            sequence.OnComplete(reset).OnKill(reset);

            return sequence.Play();
        }

        private Vector3[] _targetPositions, _targetScales;

        private void PrepareBlockImage()
        {
            _targetPositions = _blockImages.Select(b => b.transform.localPosition).ToArray();
            _targetScales = _blockImages.Select(b => b.transform.localScale).ToArray();

            for(int i = 0; i < _blockImages.Count; i++)
            {
                _blockImages[i].transform.localPosition = _startPoint.localPosition;
                _blockImages[i].transform.localScale = Vector3.one * .5f;
            }
        }

        private Sequence DoBlockImageAnim()
        {
            float popDuration = 0.45f;
            float settleDuration = 0.25f;
            float overshootFactor = 0.2f;

            var sequence = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDisable);
            
            var popSeq = DOTween.Sequence();
            var settleSeq = DOTween.Sequence();

            var startPos = _startPoint.localPosition;
            var startScale = Vector3.one * 0.5f;

            for (int i = 0; i < _blockImages.Count; i++)
            {
                var block = _blockImages[i];
                var tPos = _targetPositions[i];
                var tScale = _targetScales[i];

                var oPos = tPos - (startPos - tPos) * overshootFactor;
                var oScale = tScale - (startScale - tScale) * overshootFactor;

                popSeq.Join(block.transform.DOLocalMove(oPos, popDuration).SetEase(Ease.OutQuad));
                popSeq.Join(block.transform.DOScale(oScale, popDuration).SetEase(Ease.OutQuad));

                settleSeq.Join(block.transform.DOLocalMove(tPos, settleDuration).SetEase(Ease.OutSine));
                settleSeq.Join(block.transform.DOScale(tScale, settleDuration).SetEase(Ease.OutSine));
            }

            sequence.Append(popSeq);
            sequence.Append(settleSeq);
            sequence.OnComplete(() =>
            {
                foreach (var block in _blockImages)
                {
                    float randomDelay = UnityEngine.Random.Range(0f, 0.8f);
                    
                    // Hiệu ứng phập phồng (float) lên xuống
                    block.transform.DOLocalMoveY(block.transform.localPosition.y + 15f, 1.5f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetDelay(randomDelay)
                        .SetLink(block);

                    // Hiệu ứng xoay nhẹ quanh trục Z
                    block.transform.DOLocalRotate(new Vector3(0, 0, 15f), 2f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetDelay(randomDelay)
                        .SetLink(block);
                }
            });

            return sequence.Play();
        }

        private IEnumerator PlayParticles()
        {
            _topConfetti1.Play();
            _topConfetti2.Play();
            yield return new WaitForSeconds(.5f);
            _bottomConfetti1.Play();
            _bottomConfetti2.Play();
        }

        protected override void Start()
        {
            _continueButton.OnClicked.AddListener(() => 
            {
                Debug.Log("Continue next level");
                StartCoroutine(Hide());
                GameManager.Instance.GoToMenu();
            });
            _normalRewardButton.OnClicked.AddListener(() => 
            {
                Debug.Log("Gained level reward");
                StartCoroutine(Hide());
                GameManager.Instance.GoToMenu(() => UserManager.GainCoin(_curLevelData.CoinReward));
            });
            _adsRewardButton.OnClicked.AddListener(() => 
            {
                //TODO: Ads Service
                Debug.Log("Gained double reward via ads");
                StartCoroutine(Hide());
                GameManager.Instance.GoToMenu(() => UserManager.GainCoin(_curLevelData.CoinReward * 2));
            });

            base.Start();
        }
    }
}