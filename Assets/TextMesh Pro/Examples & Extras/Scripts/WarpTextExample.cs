using UnityEngine;
using System.Collections;
using TMPro;

namespace TMPro.Examples
{
    [ExecuteAlways]
    public class WarpTextExample : MonoBehaviour
    {
        private TMP_Text m_TextComponent;

        public AnimationCurve VertexCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.25f, 2.0f), new Keyframe(0.5f, 0), new Keyframe(0.75f, 2.0f), new Keyframe(1, 0f));
        public float CurveScale = 1.0f;
        private float _curCurveScale;

        void Awake()
        {
            m_TextComponent = gameObject.GetComponent<TMP_Text>();
        }

        void OnEnable()
        {
            if (m_TextComponent == null) m_TextComponent = GetComponent<TMP_Text>();

            _curCurveScale = CurveScale;

            if (Application.isPlaying && m_TextComponent != null)
                DoWarpText();
        }

        void OnValidate()
        {
            _curCurveScale = CurveScale;
        }

        void Start()
        {
            _curCurveScale = CurveScale;

            if (Application.isPlaying && m_TextComponent != null)
                DoWarpText();

            // StartCoroutine(WarpTextCoroutine());
        }

        IEnumerator WarpTextCoroutine()
        {
            while (true)
            {
                _curCurveScale = CurveScale;
                DoWarpText();
                yield return new WaitForSeconds(0.05f);
            }
        }

        void Update()
        {
            if (_curCurveScale != CurveScale)
            {
                _curCurveScale = CurveScale;
                DoWarpText();
            }
        }

        public void DoWarpText()
        {
            if (m_TextComponent == null) return;

            m_TextComponent.ForceMeshUpdate();

            TMP_TextInfo textInfo = m_TextComponent.textInfo;
            if (textInfo == null || textInfo.meshInfo == null) return;

            int characterCount = textInfo.characterCount;
            if (characterCount == 0) return;

            float boundsMinX = m_TextComponent.bounds.min.x;
            float boundsMaxX = m_TextComponent.bounds.max.x;
            float boundsWidth = boundsMaxX - boundsMinX;

            if (Mathf.Approximately(boundsWidth, 0f)) return;

            for (int i = 0; i < characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;

                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                if (materialIndex < 0 || materialIndex >= textInfo.meshInfo.Length) continue;

                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
                if (vertices == null || vertexIndex + 3 >= vertices.Length) continue;

                Vector2 charMidBaseline = new Vector2(
                    (vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2,
                    textInfo.characterInfo[i].baseLine);

                vertices[vertexIndex + 0] -= (Vector3)charMidBaseline;
                vertices[vertexIndex + 1] -= (Vector3)charMidBaseline;
                vertices[vertexIndex + 2] -= (Vector3)charMidBaseline;
                vertices[vertexIndex + 3] -= (Vector3)charMidBaseline;

                float x0 = (charMidBaseline.x - boundsMinX) / boundsWidth;
                float x1 = x0 + 0.0001f;
                float y0 = VertexCurve.Evaluate(x0) * _curCurveScale;
                float y1 = VertexCurve.Evaluate(x1) * _curCurveScale;

                float angle = Mathf.Atan2(y1 - y0, (x1 - x0) * boundsWidth) * Mathf.Rad2Deg;
                Matrix4x4 matrix = Matrix4x4.TRS(new Vector3(0, y0, 0), Quaternion.Euler(0, 0, angle), Vector3.one);

                vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
                vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
                vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
                vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);

                vertices[vertexIndex + 0] += (Vector3)charMidBaseline;
                vertices[vertexIndex + 1] += (Vector3)charMidBaseline;
                vertices[vertexIndex + 2] += (Vector3)charMidBaseline;
                vertices[vertexIndex + 3] += (Vector3)charMidBaseline;
            }

            m_TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }
    }
}
