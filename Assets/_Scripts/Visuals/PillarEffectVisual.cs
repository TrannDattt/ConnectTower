using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Controllers;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;
using Assets._Scripts.Managers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    [RequireComponent(typeof(PillarController))]
    public class PillarEffectVisual : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private GameObject _lockHolder;
        [SerializeField] private Image _border;
        [SerializeField] private Text _tag;

        private PillarController _pillar;
        private static HashSet<EColor> _unusedColor = new();
        private EColor _currentColor = EColor.None;

        public IEnumerator DoLockAnim(string tag)
        {
            SoundManager.Instance.PlayRandomSFX(ESfx.FullMatched);

            float animDuration = .5f;
            Vector3 initialScale = _border.transform.localScale;
            var color = GetRandomColor();
            
            _lockHolder.gameObject.SetActive(true);
            _tag.text = tag;
            _tag.gameObject.SetActive(false);
            _border.transform.localScale = Vector3.zero;

            var sequence = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDisable);
            sequence.AppendInterval(ParticleManager.Instance.GetParticleDuration(EParticle.Confetti))
            .JoinCallback(() =>
            {
                ParticleManager.Instance.StartCoroutine(ParticleManager.Instance.PlayParticle(EParticle.Confetti, transform.position, _canvas.transform));
                ChangeLockColor(color);
            })
            .Join(_border.transform.DOScale(initialScale, animDuration).SetEase(Ease.OutBack, overshoot: 2f))
            .OnComplete(() =>
            {
                _tag.gameObject.SetActive(true);
            });

            yield return sequence.WaitForCompletion();
        }

        private EColor GetRandomColor()
        {
            int index = UnityEngine.Random.Range(0, _unusedColor.Count);
            var key = _unusedColor.ElementAt(index);
            _unusedColor.Remove(key);
            return key;
        }

        private void ChangeLockColor(EColor key)
        {
            _currentColor = key;
            var blockVisuals = _pillar?.GetAllBlocks().Select(b => b.GetComponent<BlockEffectVisual>());
            foreach(var blockVisual in blockVisuals)
            {
                var blockColor = blockVisual.GetCurrentColor();
                if (blockColor != EColor.None && blockColor != _currentColor) 
                {
                    _currentColor = blockColor;
                    break;
                }
            }

            if (blockVisuals == null) return;
            foreach(var blockVisual in blockVisuals)
            {
                blockVisual.ChangeColor(_currentColor);
            }

            _border.color = ColorMapper.GetColor(_currentColor);
        }

        public void ResetVisual()
        {
            _currentColor = EColor.None;
            _lockHolder.SetActive(false);
        }

        void Awake()
        {
            _pillar = GetComponent<PillarController>();
            
            foreach (EColor color in Enum.GetValues(typeof(EColor)))
            {
                if (color != EColor.None)
                    _unusedColor.Add(color);
            }

            // _pillar.OnFullMatched.AddListener(DoLockAnim);
        }

        void OnDisable()
        {
            if (_currentColor != EColor.None) _unusedColor.Add(_currentColor);
        }

        void OnDestroy()
        {
            // _pillar.OnFullMatched.RemoveListener(DoLockAnim);
        }
    }
}