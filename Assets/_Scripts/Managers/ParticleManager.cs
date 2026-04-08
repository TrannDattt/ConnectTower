using System;
using System.Collections;
using System.Collections.Generic;
using Assets._Scripts.Enums;
using Assets._Scripts.Patterns;
using Assets._Scripts.Visuals;
using UnityEngine;

namespace Assets._Scripts.Managers
{
    public class ParticleManager : Singleton<ParticleManager>
    {
        [Serializable]
        private struct GameParticle
        {
            public EParticle Key;
            public ParticleSystem ParticlePrefab;
        }

        [SerializeField] private List<GameParticle> _gameParticles;
        [SerializeField] private int _initialPool;

        private Dictionary<EParticle, Pooling<ParticleSystem>> _particleDict = new();

        public IEnumerator PlayParticle(EParticle key, Vector3 position, Transform parent = null)
        {
            if (_particleDict.TryGetValue(key, out var pool))
            {
                var particle = pool.GetItem((p) =>
                {
                    p.transform.SetParent(parent ? parent : transform);
                    p.transform.position = position;
                    p.transform.localScale = Vector3.one;
                });
                
                particle.Play();
                
                // Wait at least one frame to let the particle system initialize its state
                yield return null;
                
                // Wait while particles are still active or system is emitting
                while (particle != null && particle.IsAlive(true))
                {
                    yield return null;
                }

                if (particle != null) pool.ReturnItem(particle);
            }
        }

        private void InitDict()
        {
            var coinDisplays = FindObjectsByType<CoinDisplayVisual>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var particle in _gameParticles)
            {
                _particleDict[particle.Key] = new Pooling<ParticleSystem>(particle.ParticlePrefab, _initialPool, transform, (p) =>
                {
                    if (particle.Key == EParticle.CoinFly && coinDisplays.Length > 0)
                    {
                        foreach (var display in coinDisplays) display.AssignCoinParticle(p);
                    }
                });
            }
        }

        protected override void Awake()
        {
            base.Awake();

            InitDict();
        }
    }
}