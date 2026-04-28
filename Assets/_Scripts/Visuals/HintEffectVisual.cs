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
        [SerializeField] private float _moveDur = 1f;
        [SerializeField] private float _offsetX;
        [SerializeField] private AnimationCurve _iconMoveXCurve;
        [SerializeField] private float _offsetY;
        [SerializeField] private AnimationCurve _iconMoveYCurve;

        [SerializeField] private ParticleSystem _questionParticle;
        [SerializeField] private float _scaleDur = .5f;
        [SerializeField] private float _scaleFactor = 1.3f;
        [SerializeField] private AnimationCurve _blockScaleCurve;

        public override Sequence DoBoosterAnim(BoosterRuntimeData data, Image target)
        {
            var hintData = data as HintBoosterRuntimeData;
            var block1 = hintData?.RandomBlock;
            var block2 = hintData?.SameBlock;

            var sequence = DOTween.Sequence().SetTarget(this);

            sequence.Append(DoMoveImageAnim(target));

            sequence.AppendCallback(() =>
            {
                for (int i = 0; i < 2; i++)
                {
                    var it = ParticleManager.Instance.PlayParticle(EParticle.Hint, target.transform.position, target.transform.parent);
                    BoosterController.Instance.StartCoroutine(it);
                    var hintImg = it.Current;
                    var hintTarget = i == 0 ? block1 : block2;
                    var attractor = hintTarget.gameObject.AddComponent<UIParticleAttractor>();
                    attractor.AddParticleSystem(hintImg);
                    attractor.movement = UIParticleAttractor.Movement.Sphere;
                    attractor.maxSpeed = .3f;
                }
            })
            .AppendInterval(ParticleManager.Instance.GetParticleDuration(EParticle.Hint) + .3f);

            sequence.Append(block1.transform.DOScale(block1.transform.localScale * _scaleFactor, _scaleDur).SetEase(_blockScaleCurve))
                    .Join(block2.transform.DOScale(block2.transform.localScale * _scaleFactor, _scaleDur).SetEase(_blockScaleCurve))
                    .AppendCallback(() =>
                    {
                        var preBlockColor = BoardController.Instance.GetAllPillars()
                                                                     .Where(p => !p.IsLocked() && ((IMechanicHandler)p).IsInteractable())
                                                                     .SelectMany(p => p.GetAllBlocks())
                                                                     .Where(b => ((IMechanicHandler)b).IsInteractable() && b.IsSameTag(block1))
                                                                     .Select(b => b.GetComponent<BlockEffectVisual>().GetCurrentColor())
                                                                     .FirstOrDefault(c => c != EColor.None);

                        var block1Visual = block1.GetComponent<BlockEffectVisual>();
                        var block2Visual = block2.GetComponent<BlockEffectVisual>();
                        var toChange = preBlockColor != EColor.None ? preBlockColor : block1Visual.GetCurrentColor() != EColor.None ?
                                        block1Visual.GetCurrentColor() : block2Visual.GetCurrentColor() != EColor.None ?
                                        block2Visual.GetCurrentColor() : GetRandomUnusedColor();

                        block1Visual.ChangeColor(toChange);
                        block2Visual.ChangeColor(toChange);
                    });

            sequence.OnComplete(() =>
            {
                if (block1 != null && block1.TryGetComponent<UIParticleAttractor>(out var a1)) Destroy(a1);
                if (block2 != null && block2.TryGetComponent<UIParticleAttractor>(out var a2)) Destroy(a2);
            })
            .OnKill(() =>
            {
                if (block1 != null && block1.TryGetComponent<UIParticleAttractor>(out var a1)) Destroy(a1);
                if (block2 != null && block2.TryGetComponent<UIParticleAttractor>(out var a2)) Destroy(a2);
            });

            return sequence;
        }

        private Sequence DoMoveImageAnim(Image target)
        {
            var sequence = DOTween.Sequence();

            sequence.AppendCallback(() =>
            {
                _questionParticle.gameObject.SetActive(true);
                _questionParticle.transform.position = target.transform.position;
                _questionParticle.Play();
            })
            .Append(target.rectTransform.DOAnchorPosX(_offsetX, _moveDur).SetRelative().SetEase(_iconMoveXCurve))
            .Join(target.rectTransform.DOAnchorPosY(_offsetY, _moveDur).SetRelative().SetEase(_iconMoveYCurve))
            .AppendCallback(() =>
            {
                _questionParticle.Stop();
                _questionParticle.gameObject.SetActive(false);
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
