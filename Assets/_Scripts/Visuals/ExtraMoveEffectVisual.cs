using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class ExtraMoveEffectVisual : BoosterButtonEffectVisual
    {
        [SerializeField] private ParticleSystem _particle;

        private float _maxDuration = 1f;

        public override Sequence DoEffectAnim(Image target)
        {
            return DOTween.Sequence().AppendCallback(() => 
                                    {
                                        _particle.gameObject.SetActive(true);
                                        _particle.transform.position = target.transform.position;
                                        _particle.Play();
                                    })
                                    .InsertCallback(_particle.main.duration, () =>
                                    {
                                        _particle.gameObject.SetActive(false);
                                    });
        }

        public override float GetTotalDuration()
        {
            return _maxDuration;
        }
    }
}