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
        [SerializeField] private Image _border;
        [SerializeField] private Text _tag;
        [SerializeField] private AnimationCurve _borderScaleXCurve;
        [SerializeField] private AnimationCurve _borderScaleYCurve;
        [SerializeField] private GameObject _lockHolder;

        private PillarController _pillar;
        private static HashSet<EColor> _unusedColor = new();
        private EColor _currentColor = EColor.None;

        private static void InitializePool(bool force = false)
        {
            if (!force && _unusedColor.Count > 0) return;

            _unusedColor.Clear();
            foreach (EColor color in Enum.GetValues(typeof(EColor)))
            {
                if (color != EColor.None)
                    _unusedColor.Add(color);
            }
        }

        public Sequence DoLockAnim(string tag)
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
            sequence.AppendCallback(() => ChangeLockColor(color))
            // .Join(_border.transform.DOScale(initialScale, animDuration).SetEase(Ease.OutBack, overshoot: 2f))
            .Join(_border.transform.DOScaleX(initialScale.x, animDuration).SetEase(_borderScaleXCurve))
            .Join(_border.transform.DOScaleY(initialScale.y, animDuration).SetEase(_borderScaleYCurve))
            .InsertCallback(animDuration * .95f, () =>
            {
                ParticleManager.Instance.StartCoroutine(ParticleManager.Instance.PlayParticle(EParticle.Confetti, transform.position, _canvas.transform));
            })
            .OnComplete(() =>
            {
                _tag.gameObject.SetActive(true);
            });

            return sequence;
        }

        private EColor GetRandomColor()
        {
            if (_unusedColor.Count == 0) InitializePool(true);

            int index = UnityEngine.Random.Range(0, _unusedColor.Count);
            var key = _unusedColor.ElementAt(index);
            _unusedColor.Remove(key);
            return key;
        }

        private void ChangeLockColor(EColor key)
        {
            _currentColor = key;
            var blockVisuals = _pillar?.GetAllBlocks().Select(b => b.GetComponent<BlockEffectVisual>()).ToList();
            if (blockVisuals != null)
            {
                foreach(var blockVisual in blockVisuals)
                {
                    var blockColor = blockVisual.GetCurrentColor();
                    if (blockColor != EColor.None && blockColor != _currentColor) 
                    {
                        // Return the randomly picked key to the pool because we are using the block's color instead
                        if (key != EColor.None) _unusedColor.Add(key);
                        _currentColor = blockColor;
                        break;
                    }
                }

                foreach(var blockVisual in blockVisuals)
                {
                    blockVisual.ChangeColor(_currentColor);
                }
            }

            _border.color = ColorMapper.GetColor(_currentColor);
        }

        public void ResetVisual()
        {
            ReturnColorToPool();
            _currentColor = EColor.None;
            _lockHolder.SetActive(false);
        }

        private void ReturnColorToPool()
        {
            if (_currentColor != EColor.None)
            {
                _unusedColor.Add(_currentColor);
                _currentColor = EColor.None;
            }
        }

        void Awake()
        {
            _pillar = GetComponent<PillarController>();
            InitializePool();
        }

        void OnEnable()
        {
            if (_canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                _canvas.worldCamera = Camera.main;
            }
        }

        void OnDisable()
        {
            ReturnColorToPool();
        }

        void OnDestroy()
        {
            // _pillar.OnFullMatched.RemoveListener(DoLockAnim);
        }

        public void ChangeLayer(LayerMask layerMask) => ChangeLayer(LayerMaskToLayerIndex(layerMask));

        public void ChangeLayer(int layer)
        {
            if (layer < 0 || layer > 31)
            {
                Debug.LogWarning($"Invalid layer index: {layer}. Expected value in range [0..31].", this);
                return;
            }

            SetLayerRecursively(transform, layer);
        }

        private static void SetLayerRecursively(Transform target, int layer)
        {
            target.gameObject.layer = layer;
            foreach (Transform child in target)
            {
                SetLayerRecursively(child, layer);
            }
        }

        private static int LayerMaskToLayerIndex(LayerMask mask)
        {
            var value = mask.value;
            if (value <= 0 || (value & (value - 1)) != 0) return -1;

            var index = 0;
            while (value > 1)
            {
                value >>= 1;
                index++;
            }

            return index;
        }
    }
}
