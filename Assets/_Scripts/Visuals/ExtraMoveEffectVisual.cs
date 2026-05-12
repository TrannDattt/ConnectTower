using System.Collections;
using System.Collections.Generic;
using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using Coffee.UIExtensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static Assets._Scripts.Visuals.BoosterButtonVisual;

namespace Assets._Scripts.Visuals
{
    public class ExtraMoveEffectVisual : BoosterButtonEffectVisual
    {
        [Header("Extra Move Curves")]
        [SerializeField] private float _duration;
        [SerializeField] private float _rotateAngle;
        [SerializeField] private AnimationCurve _rotateCurve;
        [SerializeField]private float _scaleFactor;
        [SerializeField] private AnimationCurve _scaleCurve;

        [SerializeField] private EParticle _absorbParticle = EParticle.Firefly;
        [SerializeField] private float _textUpdateDur;
        [SerializeField] private float[] _particleDelay;
        [SerializeField] private float _particleFlyTime = .4f;

        //TODO: Change animation

        public override Sequence DoBoosterAnim(BoosterRuntimeData data, Image target)
        {
            var sequence = DOTween.Sequence();

            RectTransform iconRt = target.rectTransform;
            Vector2 centerPivot = new Vector2(0.5f, 0.5f);
            if (iconRt.pivot != centerPivot) iconRt.pivot = centerPivot;

            sequence.Append(target.transform.DOLocalRotate(new Vector3(0, 0, _rotateAngle), _duration).SetEase(_rotateCurve))
                    .Join(target.transform.DOScale(target.transform.localScale * _scaleFactor, _duration).SetEase(_scaleCurve))
                    .Join(DoParticle(target));

            return sequence;
        }

        private List<ParticleSystem> _particles = new();
        private UIParticleAttractor _attractor;
        private Sequence DoParticle(Image target)
        {
            _attractor = FindFirstObjectByType<MoveCountVisual>().GetComponentInChildren<UIParticleAttractor>();
            var sequence = DOTween.Sequence();
            for (int i = 0; i < _particleDelay.Length; i++)
            {
                int index = i;
                sequence.InsertCallback(_particleDelay[index], () =>
                {
                    var it = ParticleManager.Instance.PlayParticle(_absorbParticle, target.transform.position, target.transform.parent);
                    BoosterController.Instance.StartCoroutine(it);
                    _particles.Add(it.Current);
                    _particles[^1].transform.SetAsFirstSibling();

                    if (_particles[^1] != null && _attractor != null)
                        _attractor.AddParticleSystem(_particles[^1]);
                });

                sequence.InsertCallback(_particleDelay[index] + _particleFlyTime, () =>
                {
                    IngameVisualController.Instance.UpdateMoveCount(LevelManager.PlayingLevel.MoveCount - _particleDelay.Length + 1 + index, _textUpdateDur);
                });
            }

            sequence.OnKill(() =>
            {
                foreach(var p in _particles)
                {
                    _attractor.RemoveParticleSystem(p);
                }
                IngameVisualController.Instance.UpdateMoveCount(LevelManager.PlayingLevel.MoveCount);
            });

            return sequence;
        }

        public override Sequence DoEndAnim()
        {
            return DOTween.Sequence().Append(base.DoEndAnim())
                                        .AppendCallback(() =>
                                        {
                                            foreach(var p in _particles)
                                            {
                                                _attractor.RemoveParticleSystem(p);
                                            }
                                        });
        }
    }
}