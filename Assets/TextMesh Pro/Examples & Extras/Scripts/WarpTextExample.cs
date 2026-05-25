using TMPro;
using UnityEngine;

namespace TMPro.Examples
{
    [ExecuteAlways]
    public class WarpTextExample : MonoBehaviour
    {
        private TMP_Text _textComponent;
        private string _lastText;
        private float _lastCurveScale;
        private int _lastCurveHash;
        private bool _needsWarp = true;

        public AnimationCurve VertexCurve = new AnimationCurve(
            new Keyframe(0, 0),
            new Keyframe(0.25f, 2.0f),
            new Keyframe(0.5f, 0),
            new Keyframe(0.75f, 2.0f),
            new Keyframe(1, 0f));

        public float CurveScale = 1.0f;

        void Awake()
        {
            CacheTextComponent();
        }

        void OnEnable()
        {
            CacheTextComponent();
            MarkDirty();
        }

        void OnValidate()
        {
            CacheTextComponent();
            MarkDirty();
            TryWarpText();
        }

        void LateUpdate()
        {
            TryWarpText();
        }

        private void CacheTextComponent()
        {
            if (_textComponent == null)
                _textComponent = GetComponent<TMP_Text>();
        }

        private void MarkDirty()
        {
            _needsWarp = true;
        }

        private void TryWarpText()
        {
            if (_textComponent == null)
                return;

            int curveHash = GetCurveHash(VertexCurve);
            bool curveScaleChanged = !Mathf.Approximately(_lastCurveScale, CurveScale);
            bool curveShapeChanged = _lastCurveHash != curveHash;
            bool textChanged = _lastText != _textComponent.text;

            if (!_needsWarp && !curveScaleChanged && !curveShapeChanged && !textChanged && !_textComponent.havePropertiesChanged)
                return;

            WarpText();
        }

        private void WarpText()
        {
            if (_textComponent == null)
                return;

            VertexCurve.preWrapMode = WrapMode.Clamp;
            VertexCurve.postWrapMode = WrapMode.Clamp;

            _textComponent.ForceMeshUpdate();

            TMP_TextInfo textInfo = _textComponent.textInfo;
            int characterCount = textInfo.characterCount;

            _lastText = _textComponent.text;
            _lastCurveScale = CurveScale;
            _lastCurveHash = GetCurveHash(VertexCurve);
            _needsWarp = false;

            if (characterCount == 0)
                return;

            float boundsMinX = _textComponent.bounds.min.x;
            float boundsMaxX = _textComponent.bounds.max.x;
            float boundsWidth = boundsMaxX - boundsMinX;

            if (Mathf.Approximately(boundsWidth, 0f))
                return;

            for (int i = 0; i < characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible)
                    continue;

                int vertexIndex = charInfo.vertexIndex;
                int materialIndex = charInfo.materialReferenceIndex;
                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                Vector3 offsetToMidBaseline = new Vector3(
                    (vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) * 0.5f,
                    charInfo.baseLine,
                    0f);

                vertices[vertexIndex + 0] -= offsetToMidBaseline;
                vertices[vertexIndex + 1] -= offsetToMidBaseline;
                vertices[vertexIndex + 2] -= offsetToMidBaseline;
                vertices[vertexIndex + 3] -= offsetToMidBaseline;

                float x0 = (offsetToMidBaseline.x - boundsMinX) / boundsWidth;
                float x1 = x0 + 0.0001f;
                float y0 = VertexCurve.Evaluate(x0) * CurveScale;
                float y1 = VertexCurve.Evaluate(x1) * CurveScale;

                Vector3 horizontal = Vector3.right;
                Vector3 tangent = new Vector3(x1 * boundsWidth + boundsMinX, y1, 0f) - new Vector3(offsetToMidBaseline.x, y0, 0f);

                float dot = Mathf.Acos(Mathf.Clamp(Vector3.Dot(horizontal, tangent.normalized), -1f, 1f)) * Mathf.Rad2Deg;
                Vector3 cross = Vector3.Cross(horizontal, tangent);
                float angle = cross.z > 0 ? dot : 360f - dot;

                Matrix4x4 matrix = Matrix4x4.TRS(new Vector3(0f, y0, 0f), Quaternion.Euler(0f, 0f, angle), Vector3.one);

                vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
                vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
                vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
                vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);

                vertices[vertexIndex + 0] += offsetToMidBaseline;
                vertices[vertexIndex + 1] += offsetToMidBaseline;
                vertices[vertexIndex + 2] += offsetToMidBaseline;
                vertices[vertexIndex + 3] += offsetToMidBaseline;
            }

            _textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
            _textComponent.havePropertiesChanged = false;
        }

        private static int GetCurveHash(AnimationCurve curve)
        {
            if (curve == null)
                return 0;

            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (int)curve.preWrapMode;
                hash = hash * 23 + (int)curve.postWrapMode;

                Keyframe[] keys = curve.keys;
                for (int i = 0; i < keys.Length; i++)
                {
                    Keyframe key = keys[i];
                    hash = hash * 23 + key.time.GetHashCode();
                    hash = hash * 23 + key.value.GetHashCode();
                    hash = hash * 23 + key.inTangent.GetHashCode();
                    hash = hash * 23 + key.outTangent.GetHashCode();
                    hash = hash * 23 + key.inWeight.GetHashCode();
                    hash = hash * 23 + key.outWeight.GetHashCode();
                    hash = hash * 23 + (int)key.weightedMode;
                }

                return hash;
            }
        }
    }
}
