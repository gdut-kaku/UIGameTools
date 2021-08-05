using UnityEngine;
using UnityEditor;

namespace KakuEditorTools
{
    [CustomEditor(typeof(RectTransform))]
    [CanEditMultipleObjects]
    internal class RectEditor : RectTransformEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // 获取窗口的宽度。
            // float width = EditorGUIUtility.currentViewWidth / 5;

            GUILayout.BeginVertical();
            GUILayout.Space(3);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("扩张"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "扩张");
                if (rect != null)
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.anchoredPosition3D = Vector3.zero;
                    rect.sizeDelta = Vector2.zero;
                }
            }
            if (GUILayout.Button("顶部"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "顶部");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 1);
                    rect.anchorMax = new Vector2(0.5f, 1);
                    rect.pivot = new Vector2(0.5f, 1);
                    rect.anchoredPosition3D = Vector3.zero;
                }
            }
            if (GUILayout.Button("底部"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "底部");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 0);
                    rect.anchorMax = new Vector2(0.5f, 0);
                    rect.pivot = new Vector2(0.5f, 0);
                    rect.anchoredPosition3D = Vector3.zero;
                }
            }
            if (GUILayout.Button("左侧"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "左侧");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0, 0.5f);
                    rect.anchorMax = new Vector2(0, 0.5f);
                    rect.pivot = new Vector2(0, 0.5f);
                    rect.anchoredPosition3D = Vector3.zero;
                }
            }
            if (GUILayout.Button("右侧"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "右侧");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(1, 0.5f);
                    rect.anchorMax = new Vector2(1, 0.5f);
                    rect.pivot = new Vector2(1, 0.5f);
                    rect.anchoredPosition3D = Vector3.zero;
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(3);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("居中"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "居中");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition3D = Vector3.zero;
                    rect.sizeDelta = Vector2.one * 100;
                }
            }

            if (GUILayout.Button("左上"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "左上");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0, 1);
                    rect.anchorMax = new Vector2(0, 1);
                    rect.pivot = new Vector2(0, 1);
                    rect.anchoredPosition3D = Vector3.zero;
                }
            }
            if (GUILayout.Button("右上"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "右上");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(1, 1);
                    rect.anchorMax = new Vector2(1, 1);
                    rect.pivot = new Vector2(1, 1);
                    rect.anchoredPosition3D = Vector3.zero;
                }
            }
            if (GUILayout.Button("左下"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "左下");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0, 0);
                    rect.anchorMax = new Vector2(0, 0);
                    rect.pivot = new Vector2(0, 0);
                    rect.anchoredPosition3D = Vector3.zero;
                }
            }
            if (GUILayout.Button("右下"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "右下");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(1, 0);
                    rect.anchorMax = new Vector2(1, 0);
                    rect.pivot = new Vector2(1, 0);
                    rect.anchoredPosition3D = Vector3.zero;
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}


