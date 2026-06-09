using Assets._Scripts.Controllers;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    [RequireComponent(typeof(BlockController))]
    public class BlockEffectVisual : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _blockRenderer;
        [SerializeField] private TrailRenderer _trailRenderer;
        [SerializeField] private SpriteRenderer _blockIcon;

        private BlockController _block;
        private Color _initialColor;
        private EColor _curColor = EColor.None;
        private int _baseLayer;

        private MaterialPropertyBlock _propertyBlock;
        private MaterialPropertyBlock PropertyBlock
        {
            get
            {
                if (_propertyBlock == null)
                    _propertyBlock = new MaterialPropertyBlock();
                return _propertyBlock;
            }
        }

        public void SetTrailEnable(bool state) => _trailRenderer.enabled = state;

        public void ChangeIconDisplay(bool isVisible) => _blockIcon.gameObject.SetActive(isVisible);

        public void ChangeTexture(Texture2D texture)
        {
            if (texture == null)
            {
                _blockRenderer.SetPropertyBlock(null);
                return;
            }

            var mb = PropertyBlock;
            _blockRenderer.GetPropertyBlock(mb);
            mb.SetTexture("_BaseMap", texture);
            _blockRenderer.SetPropertyBlock(mb);
        }

        public EColor GetCurrentColor() => _curColor;

        public void ChangeColor(EColor key)
        {
            _curColor = key;
            var color = ColorMapper.GetColor(key);
            ChangeColor(color);
        }
        
        public void SetTrailColor(Color color)
        {
            if (_trailRenderer == null) return;

            var gradient = new Gradient();

            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(color, 1f),
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f),
                }
            );

            _trailRenderer.colorGradient = gradient;
        }

        private void ChangeColor(Color color)
        {
            var mb = PropertyBlock;
            _blockRenderer.GetPropertyBlock(mb);
            mb.SetColor("_BaseColor", color);
            _blockRenderer.SetPropertyBlock(mb);
        }

        public void ResetVisual()
        {
            _curColor = EColor.None;
            ChangeIconDisplay(true);
            ChangeColor(_initialColor);
            ChangeTexture(null);
            SetTrailColor(Color.white);
        }

        public void ChangeLayer(int layer)
        {
            if (layer < 0 || layer > 31)
            {
                Debug.LogWarning($"Invalid layer index: {layer}. Expected value in range [0..31].", this);
                return;
            }

            SetLayerRecursively(transform, layer);
        }

        public void ResetLayer()
        {
            ChangeLayer(_baseLayer);
        }

        private static void SetLayerRecursively(Transform target, int layer)
        {
            target.gameObject.layer = layer;
            foreach (Transform child in target)
            {
                SetLayerRecursively(child, layer);
            }
        }

        void Awake()
        {
            _block = GetComponent<BlockController>();
            _initialColor = _blockRenderer.material.color;
            _baseLayer = gameObject.layer;
        }
    }
}
