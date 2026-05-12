using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;
using Assets._Scripts.Interfaces;
using Assets._Scripts.Managers;
using Coffee.UIExtensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static Assets._Scripts.Visuals.BoosterButtonVisual;

namespace Assets._Scripts.Visuals
{
    public class HintEffectVisual : BoosterButtonEffectVisual
    {
        [Header("Hint Curves")]
        [SerializeField] private float _blockScaleDur = .5f;
        [SerializeField] private float _blockScaleFactor = 1.3f;
        [SerializeField] private AnimationCurve _blockScaleCurve;

        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _lensImageHolder;
        [SerializeField] private Image[] _smallLensImages;
        [SerializeField] private EParticle _smokeParticle;
        [SerializeField] private float _lensMoveDur;
        [SerializeField] private Vector2 _moveOffset;
        [SerializeField] private AnimationCurve _moveXCurve;
        [SerializeField] private AnimationCurve _moveYCurve;
        [SerializeField] private float _lensScaleDur;
        [SerializeField] private AnimationCurve _lensScaleCurve;
        [SerializeField] private EParticle _hintParticle;

        public override Sequence DoBoosterAnim(BoosterRuntimeData data, Image target)
        {
            var hintData = data as HintBoosterRuntimeData;
            var groupBlock = hintData.GroupBlock;
            var camera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera != null ? _canvas.worldCamera : Camera.main;

            // TODO: Spawn lens with smoke effect, make lens move around block, then scale to 0 and do hint effect
            var sequence = DOTween.Sequence().SetTarget(this);

            sequence.AppendCallback(() =>
            {
                for (int i = 0; i < groupBlock.Length; i++)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(_lensImageHolder,
                                                                            Camera.main.WorldToScreenPoint(groupBlock[i].transform.position),
                                                                            camera,
                                                                            out Vector2 localPos);
                    _smallLensImages[i].rectTransform.anchoredPosition = localPos;
                    var it = ParticleManager.Instance.PlayParticle(_smokeParticle, _smallLensImages[i].transform.position, _lensImageHolder);
                    BoosterController.Instance.StartCoroutine(it);
                }
            })
            .AppendInterval(ParticleManager.Instance.GetParticleDuration(_smokeParticle) *.7f);

            var baseLenScale = _smallLensImages[0].transform.localScale;
            var baseBlockScale = groupBlock[0].transform.localScale;

            void reset(Image lensImage, BlockController block)
            {
                lensImage.transform.localScale = baseLenScale;
                block.transform.localScale = baseBlockScale;
            }

            for (int i = 0; i < groupBlock.Length; i++)
            {
                var image = _smallLensImages[i];
                var block = groupBlock[i];
                var hintSequence = DOTween.Sequence();
                hintSequence.AppendCallback(() => image.gameObject.SetActive(true))
                            .Append(image.rectTransform.DOAnchorPosX(_moveOffset.x, _lensMoveDur).SetEase(_moveXCurve).SetRelative())
                            .Join(image.rectTransform.DOAnchorPosY(_moveOffset.y, _lensMoveDur).SetEase(_moveYCurve).SetRelative())
                            .Append(image.transform.DOScale(Vector3.zero, _lensScaleDur).SetEase(_lensScaleCurve))
                            .AppendCallback(() =>
                            {
                                Debug.Log("DO hint effect");
                                BoosterController.Instance.StartCoroutine(ParticleManager.Instance.PlayParticle(_hintParticle, image.transform.position, _lensImageHolder));
                            })
                            .Append(block.transform.DOScale(_blockScaleFactor, _blockScaleDur).SetEase(_blockScaleCurve).SetRelative())
                            .AppendCallback(() => image.gameObject.SetActive(false))
                            .OnKill(() => reset(image, block))
                            .OnComplete(() => reset(image, block));

                if (i == 0)
                    sequence.Append(hintSequence);
                else
                    sequence.Join(hintSequence);
            }

            sequence.AppendCallback(() =>
            {
                var preColor = groupBlock[0].GetComponent<BlockEffectVisual>().GetCurrentColor();
                var toChange = preColor != EColor.None ? preColor : GetRandomUnusedColor();
                for (int i = 0; i < groupBlock.Length; i++)
                {
                    var mechanicHandler = groupBlock[i] as IMechanicHandler;
                    if (mechanicHandler.IsHidden()) mechanicHandler.ClearMechanic();
                    groupBlock[i].GetComponent<BlockEffectVisual>().ChangeColor(toChange);
                }
            });

            return sequence;
        }

        private EColor GetRandomUnusedColor()
        {
            var blockVisuals = BoardController.Instance.GetAllBlocks().Select(b => b.GetComponent<BlockEffectVisual>());
            HashSet<EColor> usedColors = new() { EColor.None };
            foreach(var visual in blockVisuals)
            {
                usedColors.Add(visual.GetCurrentColor());
            }

            var availableColors = ColorMapper.GetAllColors().Where(c => !usedColors.Contains(c)).ToArray();
            if (availableColors.Length == 0) return usedColors.First();
            return availableColors[Random.Range(0, availableColors.Length)];
        }
    }
}
