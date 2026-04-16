using UnityEngine;
using UnityEngine.UI;
using UIGradients;
using TMPro;

namespace Assets._Scripts.Visuals
{
    [AddComponentMenu("UI/Effects/Custom Text Gradient")]
    public class CustomTextGradient : BaseMeshEffect
    {
        public Color m_color1 = Color.white;
        [Range(-180f, 180f)] public float m_angle = 0f;
        [Range(0f, 1f)] public float m_blend = 1f;
        public bool m_isGlobal = true;

        private TMP_Text m_TextComponent;
        private bool m_isUpdating = false;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_TextComponent = GetComponent<TMP_Text>();
            if (m_TextComponent != null)
            {
                TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
                ApplyGradientToTMP();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (m_TextComponent != null)
            {
                TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
            }
        }

        private void OnTextChanged(Object obj)
        {
            if (m_TextComponent != null && obj == (Object)m_TextComponent)
            {
                ApplyGradientToTMP();
            }
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!enabled || graphic == null || m_TextComponent != null)
                return;

            var rect = m_isGlobal ? graphic.rectTransform.rect : new Rect(0f, 0f, 1f, 1f);
            var dir = UIGradientUtils.RotationDir(m_angle);
            var localPositionMatrix = UIGradientUtils.LocalPositionMatrix(rect, dir);

            var vertex = default(UIVertex);
            for (var i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                
                Vector2 position = m_isGlobal ? (Vector2)vertex.position : UIGradientUtils.VerticePositions[i % 4];
                var localPosition = localPositionMatrix * position;
                
                Color targetColor = Color.Lerp(vertex.color, m_color1, localPosition.y);
                vertex.color = Color.Lerp(vertex.color, targetColor, m_blend);
                
                vh.SetUIVertex(vertex, i);
            }
        }

        private void ApplyGradientToTMP()
        {
            if (m_TextComponent == null || m_isUpdating) return;

            m_isUpdating = true;
            try
            {
                m_TextComponent.ForceMeshUpdate();
                var textInfo = m_TextComponent.textInfo;
                if (textInfo == null || textInfo.meshInfo == null) return;

                var rect = m_isGlobal ? m_TextComponent.rectTransform.rect : new Rect(0f, 0f, 1f, 1f);
                var dir = UIGradientUtils.RotationDir(m_angle);
                var localPositionMatrix = UIGradientUtils.LocalPositionMatrix(rect, dir);

                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    var meshInfo = textInfo.meshInfo[i];
                    var vertices = meshInfo.vertices;
                    var colors = meshInfo.colors32;

                    if (vertices == null || colors == null) continue;

                    for (int j = 0; j < meshInfo.vertexCount; j++)
                    {
                        Vector2 position = m_isGlobal ? (Vector2)vertices[j] : UIGradientUtils.VerticePositions[j % 4];
                        var localPosition = localPositionMatrix * position;

                        Color vertexColor = colors[j];
                        Color targetColor = Color.Lerp(vertexColor, m_color1, localPosition.y);
                        colors[j] = Color.Lerp(vertexColor, targetColor, m_blend);
                    }
                }

                m_TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            }
            finally
            {
                m_isUpdating = false;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (m_TextComponent != null && gameObject.activeInHierarchy)
            {
                ApplyGradientToTMP();
            }
        }
#endif
    }
}
