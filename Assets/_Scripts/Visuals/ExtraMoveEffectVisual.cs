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
        [SerializeField] private float _offsetX;
        [SerializeField] private AnimationCurve _moveXCurve;
        [SerializeField] private float _offsetY;
        [SerializeField] private ParticleSystem _shakeParticle;
        [SerializeField] private AnimationCurve _moveYCurve;

        [SerializeField] private EParticle _absorbParticle = EParticle.Firefly;
        [SerializeField] private float _particleFlyTime = .4f;

        //TODO: Change animation

        public override Sequence DoBoosterAnim(BoosterRuntimeData data, Image target)
        {
            ParticleSystem particle = null;
            var attractor = FindFirstObjectByType<MoveCountVisual>().GetComponentInChildren<UIParticleAttractor>();
            float particleDelay = _duration - ParticleManager.Instance.GetParticleDuration(_absorbParticle) - _particleFlyTime;
            var sequence = DOTween.Sequence();

            RectTransform iconRt = target.rectTransform;
            Vector2 centerPivot = new Vector2(0.5f, 0.5f);
            if (iconRt.pivot != centerPivot) iconRt.pivot = centerPivot;

            sequence.Append(target.transform.DOMoveX(_gatherPoint.x + _offsetX, _duration).SetEase(_moveXCurve))
                    .Join(target.transform.DOMoveY(_gatherPoint.y + _offsetY, _duration).SetEase(_moveYCurve))
                    .Join(target.transform.DOLocalRotate(new Vector3(0, 0, _rotateAngle), _duration).SetEase(_rotateCurve))
                    .Join(target.transform.DOScale(target.transform.localScale * _scaleFactor, _duration).SetEase(_scaleCurve))
                    .JoinCallback(() =>
                    {
                        _shakeParticle.gameObject.SetActive(true);
                        _shakeParticle.transform.position = target.transform.position;
                        _shakeParticle.Play();
                    })
                    .Insert(particleDelay + _particleFlyTime, IngameVisualController.Instance.UpdateMoveCount(LevelManager.PlayingLevel.MoveCount, ParticleManager.Instance.GetParticleDuration(_absorbParticle)))
                    .InsertCallback(particleDelay, () => 
                    {
                        var it = ParticleManager.Instance.PlayParticle(_absorbParticle, target.transform.position, target.transform.parent);
                        BoosterController.Instance.StartCoroutine(it);
                        particle = it.Current;
                        particle.transform.SetAsFirstSibling();
                        
                        if (particle != null && attractor != null)
                            attractor.AddParticleSystem(particle);
                    })
                    .OnComplete(() =>
                    {
                        _shakeParticle.Stop();
                        _shakeParticle.gameObject.SetActive(false);
                        if (particle != null && attractor != null)
                            attractor.RemoveParticleSystem(particle);
                    })
                    .OnKill(() =>
                    {
                        if (particle != null && attractor != null)
                            attractor.RemoveParticleSystem(particle);
                    });

            return sequence;
        }
    }
}