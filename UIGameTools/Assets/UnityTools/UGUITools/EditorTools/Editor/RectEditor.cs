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

            // ��ȡ���ڵĿ�ȡ�
            // float width = EditorGUIUtility.currentViewWidth / 5;

            GUILayout.BeginVertical();
            GUILayout.Space(3);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("����"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "����");
                if (rect != null)
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.anchoredPosition3D = Vector3.zero;
                    rect.sizeDelta = Vector2.zero;
                }
            }
            if (GUILayout.Button("����"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "����");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 1);
                    rect.anchorMax = new Vector2(0.5f, 1);
                    rect.pivot = new Vector2(0.5f, 1);
                    rect.anchoredPosition3D = Vector3.zero;
                }
            }
            if (GUILayout.Button("�ײ�"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "�ײ�");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 0);
                    rect.anchorMax = new Vector2(0.5f, 0);
                    rect.pivot = new Vector2(0.5f, 0);
                    rect.anchoredPosition3D = Vector3.zero;
                }
            }
            if (GUILayout.Button("���"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "���");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0, 0.5f);
                    rect.anchorMax = new Vector2(0, 0.5f);
                    rect.pivot = new Vector2(0, 0.5f);
                    rect.anchoredPosition3D = Vector3.zero;
                }
            }
            if (GUILayout.Button("�Ҳ�"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "�Ҳ�");
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
            if (GUILayout.Button("����"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "����");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition3D = Vector3.zero;
                    rect.sizeDelta = Vector2.one * 100;
                }
            }

            if (GUILayout.Button("����"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "����");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0, 1);
                    rect.anchorMax = new Vector2(0, 1);
                    rect.pivot = new Vector2(0, 1);
                    rect.anchoredPosition3D = Vector3.zero;
                }
            }
            if (GUILayout.Button("����"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "����");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(1, 1);
                    rect.anchorMax = new Vector2(1, 1);
                    rect.pivot = new Vector2(1, 1);
                    rect.anchoredPosition3D = Vector3.zero;
                }
            }
            if (GUILayout.Button("����"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "����");
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0, 0);
                    rect.anchorMax = new Vector2(0, 0);
                    rect.pivot = new Vector2(0, 0);
                    rect.anchoredPosition3D = Vector3.zero;
                }
            }
            if (GUILayout.Button("����"))
            {
                RectTransform rect = ((RectTransform)target);
                Undo.RecordObject(rect, "����");
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


