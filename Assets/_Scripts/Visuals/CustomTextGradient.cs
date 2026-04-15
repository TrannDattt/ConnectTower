using UnityEngine;
using UnityEngine.UI;
using UIGradients;

namespace Assets._Scripts.Visuals
{
    [AddComponentMenu("UI/Effects/Custom Text Gradient")]
    [RequireComponent(typeof(Text))]
    public class CustomTextGradient : BaseMeshEffect
    {
        public Color m_color1 = Color.white;
        [Range(-180f, 180f)] public float m_angle = 0f;
        [Range(0f, 1f)] public float m_blend = 1f;
        public bool m_isGlobal = true;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!enabled || graphic == null)
                return;

            var rect = m_isGlobal ? graphic.rectTransform.rect : new Rect(0f, 0f, 1f, 1f);
            var dir = UIGradientUtils.RotationDir(m_angle);
            var localPositionMatrix = UIGradientUtils.LocalPositionMatrix(rect, dir);

            var vertex = default(UIVertex);
            for (var i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                
                // Use vertex.position for global gradient, or per-character index for local gradient
                Vector2 position = m_isGlobal ? (Vector2)vertex.position : UIGradientUtils.VerticePositions[i % 4];
                var localPosition = localPositionMatrix * position;
                
                // The base color is the current vertex color (which starts as the Text component's color)
                // We interpolate from this base color towards m_color1
                Color targetColor = Color.Lerp(vertex.color, m_color1, localPosition.y);
                
                // Finally, blend between the original color and the calculated gradient result
                vertex.color = Color.Lerp(vertex.color, targetColor, m_blend);
                
                vh.SetUIVertex(vertex, i);
            }
        }
    }
}
