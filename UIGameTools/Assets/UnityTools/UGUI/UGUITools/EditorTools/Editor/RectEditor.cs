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
            GUILayout.BeginVertical();
            GUILayout.Space(3);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("ZeroExpand"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "ZeroExpand");
                if (rect != null)
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.anchoredPosition3D = Vector3.zero;
                    rect.sizeDelta = Vector2.zero;
                }
            }
            if (GUILayout.Button("ZeroCenter"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "ZeroCenter");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition3D = Vector3.zero;
                    rect.sizeDelta = Vector2.one * 100;
                }
            }
            if (GUILayout.Button("ZeroTop"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "ZeroTop");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 1);
                    rect.anchorMax = new Vector2(0.5f, 1);
                    rect.pivot = new Vector2(0.5f, 1);
                    rect.anchoredPosition3D = Vector3.zero;
                }
            }
            if (GUILayout.Button("ZeroDown"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "ZeroDown");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 0);
                    rect.anchorMax = new Vector2(0.5f, 0);
                    rect.pivot = new Vector2(0.5f, 0);
                    rect.anchoredPosition3D = Vector3.zero;
                }
            }
            if (GUILayout.Button("ZeroLeft"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "ZeroLeft");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0, 0.5f);
                    rect.anchorMax = new Vector2(0, 0.5f);
                    rect.pivot = new Vector2(0, 0.5f);
                    rect.anchoredPosition3D = Vector3.zero;
                }
            }
            if (GUILayout.Button("ZeroRight"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "ZeroRight");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(1, 0.5f);
                    rect.anchorMax = new Vector2(1, 0.5f);
                    rect.pivot = new Vector2(1, 0.5f);
                    rect.anchoredPosition3D = Vector3.zero;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}


