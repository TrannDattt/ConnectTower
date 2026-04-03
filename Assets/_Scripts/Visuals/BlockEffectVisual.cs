using Assets._Scripts.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    [RequireComponent(typeof(BlockController))]
    public class BlockEffectVisual : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _blockRenderer;
        [SerializeField] private Image _blockIcon;

        private BlockController _block;


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

        public void ChangeColor(Color color)
        {
            var mb = PropertyBlock;
            _blockRenderer.GetPropertyBlock(mb);
            mb.SetColor("_BaseColor", color);
            _blockRenderer.SetPropertyBlock(mb);
        }

        void Awake()
        {
            _block = GetComponent<BlockController>();
        }
    }
}