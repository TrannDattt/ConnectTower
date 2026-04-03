using System.Collections.Generic;
using Assets._Scripts.Enums;
using Assets._Scripts.Patterns;
using UnityEngine;

namespace Assets._Scripts.Controllers
{
    public class ParticleManager : Singleton<ParticleManager>
    {
        [SerializeField] private ParticleSystem _smokePS;
        [SerializeField] private ParticleSystem _sparklePS;
        [SerializeField] private ParticleSystem _confettiUpPS;
        [SerializeField] private ParticleSystem _confettiDiagonalPS;

        //TODO: Do pooling
        private Dictionary<EParticle,ParticleSystem> _particleDict = new();

        public ParticleSystem PlayParticle(EParticle key, Vector3 position)
        {
            if (!_particleDict.TryGetValue(key, out var prefab)) return null;

            var newPS = Instantiate(prefab, position, Quaternion.identity, transform);
            newPS.Play();

            return newPS;
        }

        private void InitDict()
        {
            _particleDict[EParticle.Smoke] = _smokePS;
            _particleDict[EParticle.Sparkle] = _sparklePS;
            _particleDict[EParticle.ConfettiUp] = _confettiUpPS;
            _particleDict[EParticle.ConfettiDiagonal] = _confettiDiagonalPS;
        }

        protected override void Awake()
        {
            base.Awake();

            InitDict();
        }
    }
}