using System;
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

        public void DoLockAnim(string tag)
        {
            SoundManager.Instance.PlayRandomSFX(ESfx.FullMatched);

            float animDuration = .5f;
            Vector3 initialScale = _border.transform.localScale;
            var color = GetRandomColor();

            ChangeLockColor(color);
            StartCoroutine(ParticleManager.Instance.PlayParticle(EParticle.Confetti, transform.position, _canvas.transform));
            _lockHolder.gameObject.SetActive(true);
            _tag.text = tag;
            _tag.gameObject.SetActive(false);
            _border.transform.localScale = Vector3.zero;

            _border.transform.DOScale(initialScale, animDuration)
                            .SetEase(Ease.OutBack, overshoot: 2f)
                            .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                            .OnComplete(() =>
                            {
                                _tag.gameObject.SetActive(true);
                            });
        }

        private Color GetRandomColor()
        {
            int index = UnityEngine.Random.Range(0, _unusedColor.Count);
            var key = _unusedColor.ElementAt(index);
            _unusedColor.Remove(key);
            return ColorMapper.GetColor(key);
        }

        private void ChangeLockColor(Color color)
        {
            _border.color = color;
            var blocks = _pillar?.GetAllBlocks();

            if (blocks == null) return;
            foreach(var block in blocks)
            {
                if (block.TryGetComponent<BlockEffectVisual>(out var visual))
                {
                    visual.ChangeColor(color);
                }
            }
        }

        public void ResetVisual()
        {
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