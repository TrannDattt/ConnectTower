#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Assets._Scripts.Editor
{
    public class TMPToTextTool : EditorWindow
    {
        [MenuItem("Tools/TMP To Legacy Text Converter")]
        public static void ShowWindow()
        {
            GetWindow<TMPToTextTool>("TMP -> Text");
        }

        private Font _targetFont;
        private bool _useOutline = false;
        private Color _outlineColor = Color.black;
        private Vector2 _outlineDistance = new Vector2(1, -1);

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("Font Settings", EditorStyles.boldLabel);
            _targetFont = (Font)EditorGUILayout.ObjectField("Font to Apply", _targetFont, typeof(Font), false);
            
            if (_targetFont == null)
            {
                _targetFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Font/PoetsenOne-Regular.ttf");
            }

            GUILayout.Space(20);
            EditorGUILayout.HelpBox("Phần 1: Chuyển đổi TMP sang Text (Legacy UI)", MessageType.Info);
            
            if (GUILayout.Button("Chuyển đổi tất cả TMP UGUI trong Scene", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Xác nhận", "Bạn có chắc chắn muốn chuyển đổi tất cả component TextMeshProUGUI trong scene không?", "Có", "Không"))
                {
                    ConvertAllInScene();
                }
            }

            if (GUILayout.Button("Chuyển đổi các đối tượng đang chọn", GUILayout.Height(30)))
            {
                ConvertSelection();
            }

            GUILayout.Space(20);
            EditorGUILayout.HelpBox("Phần 2: Thay đổi Font của tất cả Component Text", MessageType.Info);

            if (GUILayout.Button("Đổi font của tất cả Component Text trong Scene", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Xác nhận", "Đổi font của tất cả component Text (Legacy) sang font đã chọn?", "Có", "Không"))
                {
                    ChangeAllFontsInScene();
                }
            }

            if (GUILayout.Button("Đổi font các đối tượng đang chọn", GUILayout.Height(30)))
            {
                ChangeSelectedFonts();
            }

            GUILayout.Space(20);
            EditorGUILayout.HelpBox("Phần 3: Hiệu ứng Outline (Legacy UI)", MessageType.Info);
            _useOutline = EditorGUILayout.Toggle("Sử dụng Outline khi convert", _useOutline);
            _outlineColor = EditorGUILayout.ColorField("Màu Outline", _outlineColor);
            _outlineDistance = EditorGUILayout.Vector2Field("Độ rộng (Distance)", _outlineDistance);

            GUILayout.Space(10);
            if (GUILayout.Button("Thêm/Cập nhật Outline cho tất cả Text trong Scene", GUILayout.Height(30)))
            {
                AddOutlineAll();
            }

            if (GUILayout.Button("Xóa Outline của tất cả Text trong Scene", GUILayout.Height(30)))
            {
                RemoveOutlineAll();
            }
        }

        private void AddOutlineAll()
        {
            var texts = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Undo.SetCurrentGroupName("Add/Update Outline");
            int group = Undo.GetCurrentGroup();

            foreach (var t in texts)
            {
                Outline outline = t.gameObject.GetComponent<Outline>();
                if (outline == null) outline = Undo.AddComponent<Outline>(t.gameObject);
                outline.effectColor = _outlineColor;
                outline.effectDistance = _outlineDistance;
                EditorUtility.SetDirty(outline);
            }

            Undo.CollapseUndoOperations(group);
            Debug.Log($"Đã thêm/cập nhật Outline cho {texts.Length} component.");
        }

        private void RemoveOutlineAll()
        {
            var outlines = FindObjectsByType<Outline>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Undo.SetCurrentGroupName("Remove Outline");
            int group = Undo.GetCurrentGroup();

            foreach (var o in outlines)
            {
                Undo.DestroyObjectImmediate(o);
            }

            Undo.CollapseUndoOperations(group);
            Debug.Log($"Đã xóa {outlines.Length} component Outline.");
        }

        private void ChangeAllFontsInScene()
        {
            if (_targetFont == null) return;
            var texts = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            ApplyFont(texts);
        }

        private void ChangeSelectedFonts()
        {
            if (_targetFont == null) return;
            List<Text> texts = new List<Text>();
            foreach (GameObject go in Selection.gameObjects)
            {
                texts.AddRange(go.GetComponentsInChildren<Text>(true));
            }
            ApplyFont(texts.ToArray());
        }

        private void ApplyFont(Text[] texts)
        {
            if (texts == null || texts.Length == 0) return;
            
            Undo.RecordObjects(texts, "Change Text Font");
            foreach (var t in texts)
            {
                t.font = _targetFont;
                EditorUtility.SetDirty(t);
            }
            Debug.Log($"Đã đổi font cho {texts.Length} component Text.");
        }

        private void ConvertAllInScene()
        {
            var tmpComponents = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            ConvertComponents(tmpComponents);
        }

        private void ConvertSelection()
        {
            List<TextMeshProUGUI> tmpComponents = new List<TextMeshProUGUI>();
            foreach (GameObject go in Selection.gameObjects)
            {
                var tmp = go.GetComponentsInChildren<TextMeshProUGUI>(true);
                tmpComponents.AddRange(tmp);
            }
            ConvertComponents(tmpComponents.ToArray());
        }

        private void ConvertComponents(TextMeshProUGUI[] components)
        {
            if (components == null || components.Length == 0)
            {
                Debug.Log("Không tìm thấy component TMP nào.");
                return;
            }

            int count = 0;
            foreach (var tmp in components)
            {
                if (tmp == null) continue;

                GameObject go = tmp.gameObject;

                // Cache values
                string text = tmp.text;
                float fontSize = tmp.fontSize;
                Color color = tmp.color;
                TextAlignmentOptions alignment = tmp.alignment;
                bool raycast = tmp.raycastTarget;
                bool isEnabled = tmp.enabled;
                FontStyle fontStyle = MapFontStyle(tmp.fontStyle);

                // Start Undo Group
                Undo.SetCurrentGroupName("Convert TMP to Text");
                int group = Undo.GetCurrentGroup();

                // Destroy TMP BEFORE adding Text to avoid "Multiple Graphic components" error
                Undo.DestroyObjectImmediate(tmp);

                // Add legacy Text component
                Text legacyText = Undo.AddComponent<Text>(go);
                
                // Copy values
                legacyText.text = text;
                legacyText.fontSize = Mathf.RoundToInt(fontSize);
                legacyText.color = color;
                legacyText.raycastTarget = raycast;
                legacyText.alignment = MapAlignment(alignment);
                legacyText.fontStyle = fontStyle;
                legacyText.enabled = isEnabled;
                if (_targetFont != null) legacyText.font = _targetFont;

                // Add outline if enabled
                if (_useOutline)
                {
                    Outline outline = Undo.AddComponent<Outline>(go);
                    outline.effectColor = _outlineColor;
                    outline.effectDistance = _outlineDistance;
                }
                
                // Default settings for text to avoid clipping issues common in legacy text
                legacyText.horizontalOverflow = HorizontalWrapMode.Wrap;
                legacyText.verticalOverflow = VerticalWrapMode.Overflow;

                Undo.CollapseUndoOperations(group);
                count++;
            }

            Debug.Log($"Đã chuyển đổi thành công {count} component TextMeshProUGUI.");
            EditorUtility.DisplayDialog("Hoàn tất", $"Đã chuyển đổi xong {count} component.", "OK");
        }

        private static TextAnchor MapAlignment(TextAlignmentOptions tmpAlignment)
        {
            switch (tmpAlignment)
            {
                case TextAlignmentOptions.TopLeft: return TextAnchor.UpperLeft;
                case TextAlignmentOptions.Top: return TextAnchor.UpperCenter;
                case TextAlignmentOptions.TopRight: return TextAnchor.UpperRight;
                case TextAlignmentOptions.Left: return TextAnchor.MiddleLeft;
                case TextAlignmentOptions.Center: return TextAnchor.MiddleCenter;
                case TextAlignmentOptions.Right: return TextAnchor.MiddleRight;
                case TextAlignmentOptions.BottomLeft: return TextAnchor.LowerLeft;
                case TextAlignmentOptions.Bottom: return TextAnchor.LowerCenter;
                case TextAlignmentOptions.BottomRight: return TextAnchor.LowerRight;
                
                case TextAlignmentOptions.BaselineLeft: return TextAnchor.MiddleLeft;
                case TextAlignmentOptions.Baseline: return TextAnchor.MiddleCenter;
                case TextAlignmentOptions.BaselineRight: return TextAnchor.MiddleRight;
                
                case TextAlignmentOptions.MidlineLeft: return TextAnchor.MiddleLeft;
                case TextAlignmentOptions.Midline: return TextAnchor.MiddleCenter;
                case TextAlignmentOptions.MidlineRight: return TextAnchor.MiddleRight;
                
                case TextAlignmentOptions.CaplineLeft: return TextAnchor.UpperLeft;
                case TextAlignmentOptions.Capline: return TextAnchor.UpperCenter;
                case TextAlignmentOptions.CaplineRight: return TextAnchor.UpperRight;

                default: return TextAnchor.MiddleCenter;
            }
        }

        private static FontStyle MapFontStyle(FontStyles tmpStyle)
        {
            bool bold = (tmpStyle & FontStyles.Bold) != 0;
            bool italic = (tmpStyle & FontStyles.Italic) != 0;

            if (bold && italic) return FontStyle.BoldAndItalic;
            if (bold) return FontStyle.Bold;
            if (italic) return FontStyle.Italic;
            return FontStyle.Normal;
        }
    }
}
#endif
