//---------------------------------------
// UGUI Toolset v0.55
// Author: Andrew Vasiliev [@AndrewChewie]
// https://github.com/andrew-chewie/UGUI-Toolset.git
//---------------------------------------

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine.UI;

namespace KakuEditorTools
{
    public class UGUIToolWindow : EditorWindow
    {
        static StringText s_Text = new ChineseText();

        [MenuItem("UGUITools/UGUIToolSet")]
        public static void ShowWindow()
        {
            GetWindow(typeof(UGUIToolWindow));
        }

        private class LayoutData
        {
            public UILayout LayoutType = UILayout.Vertical;
            public RectOffset padding = new RectOffset();
            public TextAnchor childAlignment = TextAnchor.UpperLeft;
            public bool childForceExpandHeight;
            public bool childForceExpandWidth;
            public float spacing;

            public LayoutData()
            {
            }

            public LayoutData(HorizontalOrVerticalLayoutGroup layoutGroup)
            {
                if (layoutGroup != null)
                {
                    if (layoutGroup.GetComponent<VerticalLayoutGroup>() != null)
                        LayoutType = UILayout.Vertical;

                    if (layoutGroup.GetComponent<HorizontalLayoutGroup>() != null)
                        LayoutType = UILayout.Horizontal;

                    spacing = layoutGroup.spacing;
                    padding = layoutGroup.padding;
                    childAlignment = layoutGroup.childAlignment;
                    childForceExpandHeight = layoutGroup.childForceExpandHeight;
                    childForceExpandWidth = layoutGroup.childForceExpandWidth;
                }
                else
                {
                    new LayoutData();
                }
            }
        }

        private enum UILayout
        {
            Vertical,
            Horizontal
        }

        private GameObject SelectedObject
        {
            get
            {
                if (_selectedObjects == null || _selectedObjects.Count == 0)
                    return null;

                return _selectedObjects[0].gameObject;
            }
        }

        private List<Transform> _selectedObjects;
        private List<Transform> SelectedObjects
        {
            get
            {
                if (IncludeChild)
                {
                    var result = new HashSet<Transform>();
                    foreach (Transform o in _selectedObjects)
                    {
                        result.Add(o);
                        if (IncludeRecursively)
                        {
                            foreach (Transform c in o.GetComponentsInChildren<Transform>(true))
                            {
                                result.Add(c);
                            }
                        }
                        else
                        {
                            foreach (Transform c in o.transform)
                            {
                                result.Add(c);
                            }
                        }
                    }

                    result.RemoveWhere(transform => transform.GetComponent<RectTransform>() == null);

                    return new List<Transform>(result);
                }

                return _selectedObjects;
            }
            set { _selectedObjects = value; }
        }

        private bool IncludeChild = false;
        private bool IncludeRecursively = false;

        void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGui;
        }

        private void OnSceneGui(SceneView sceneView)
        {
            Event e = Event.current;

            Repaint();
        }

        float buttonWidth = 100;
        float buttonHeight = 30;
        Vector2 scrollRect = Vector2.zero;

        private void OnGUI()
        {
            RefreshSelection();

            scrollRect = GUILayout.BeginScrollView(scrollRect);

            IncludeChild = GUILayout.Toggle(IncludeChild, "Include Child Objects", GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(20));

            if (IncludeChild)
                IncludeRecursively = GUILayout.Toggle(IncludeRecursively, "Include Recursively", GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(20));

            DrawUILine(Color.black);

            if (GUILayout.Button(s_Text.GroupSelected, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            {
                GroupSelected();
            }
            if (GUILayout.Button(s_Text.Add_Child, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            {
                AddEmptyChild();
            }

            GUI.color = Color.white;

            if (!WrongObject && SelectedObject.GetComponent<VerticalLayoutGroup>())
                GUI.color = Color.green;

            if (GUILayout.Button(s_Text.V_Group, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            {
                SetLayout(UILayout.Vertical);
            }

            GUI.color = Color.white;

            if (!WrongObject && SelectedObject.GetComponent<HorizontalLayoutGroup>())
                GUI.color = Color.green;

            if (GUILayout.Button(s_Text.H_Group, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            {
                SetLayout(UILayout.Horizontal);
            }

            GUI.color = Color.white;

            if (GUILayout.Button(s_Text.Flip_layout, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            {
                FlipLayout();
            }

            GUI.color = Color.white;

            if (!WrongObject && SelectedObject.GetComponent<ContentSizeFitter>())
                GUI.color = Color.green;

            if (GUILayout.Button(s_Text.Content_Size, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            {
                AddContentSizeFitter();
            }

            GUI.color = Color.white;

            if (!WrongObject && SelectedObject.GetComponent<LayoutElement>())
                GUI.color = Color.green;

            if (GUILayout.Button(s_Text.Layout_Element, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            {
                AddUIComponent(typeof(LayoutElement));
            }

            GUI.color = Color.white;

            if (!WrongObject && SelectedObject.GetComponent<Image>())
                GUI.color = Color.green;

            if (GUILayout.Button(s_Text.Add_Image, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            {
                AddUIComponent(typeof(Image));
            }

            GUI.color = Color.white;

            if (!WrongObject && SelectedObject.GetComponent<Button>())
                GUI.color = Color.green;

            if (!WrongObject && SelectedObject.GetComponent<Button>())
                GUI.color = Color.green;

            if (GUILayout.Button(s_Text.Add_Button, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            {
                AddUIComponent(typeof(Button));
            }

            DrawUILine(Color.black);

            GUI.color = Color.white;

            // ???????????????????????????
            //if (GUILayout.Button(s_Text.Duplicate, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            //{
            //    Duplicate();
            //}

            if (GUILayout.Button(s_Text.ApplyPrefab, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            {
                ApplySelectPrefabInstance();
            }

            if (GUILayout.Button(s_Text.V_Align, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            {
                AlignV();
            }

            if (GUILayout.Button(s_Text.H_Align, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            {
                AlignH();
            }

            if (GUILayout.Button(s_Text.V_Spacing, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            {
                SpacingV();
            }

            if (GUILayout.Button(s_Text.H_Spacing, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            {
                SpacingH();
            }

            if (GUILayout.Button(s_Text.TXT2TMP, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            {
                SwitchToTMP();
            }

            if (GUILayout.Button(s_Text.RemoveRaycast, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            {
                RemoveRaycast();
            }

            DrawUILine(Color.black);

            if (GUILayout.Button(s_Text.RoundRectPos, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            {
                RectPosRoundToWholeNumber();
            }

            if (GUILayout.Button(s_Text.FixCopyGoName, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            {
                FixCopyGameObjectName();
            }

            if (GUILayout.Button(s_Text.TrimGoName, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            {
                TrimGameObjectName();
            }

            if (GUILayout.Button(s_Text.ExpandRect, GUILayout.MinWidth(buttonWidth), GUILayout.MinHeight(buttonHeight)))
            {
                ExpandRectTransform();
            }
            
            DrawUILine(Color.black);

            GUILayout.EndScrollView();
        }

        private void RefreshSelection()
        {
            SelectedObjects = new List<Transform>(Selection.transforms);
        }

        public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }

        [MenuItem("Window/UGUI Toolset/Group objects %g")]
        static void GroupObjects()
        {
            var toolset = GetWindow(typeof(UGUIToolWindow)) as UGUIToolWindow;
            toolset?.GroupSelected();
        }

        private void GroupSelected()
        {
            RefreshSelection();

            if (SelectedObjects == null || SelectedObjects.Count == 0)
                return;

            var parent = SelectedObjects[0].parent;

            GameObject groupObject = new GameObject("Group");
            Undo.RegisterCreatedObjectUndo(groupObject, "Set new parent");

            if (parent != null)
            {
                groupObject.transform.SetParent(parent.transform);
            }

            if (isUIMode())
            {
                groupObject.AddComponent<RectTransform>();
            }

            groupObject.transform.position = CenterOfSelection();
            groupObject.transform.localScale = Vector3.one;

            foreach (Transform o in SelectedObjects)
            {
                Undo.SetTransformParent(o.transform, groupObject.transform, "Set new parent");
            }

            Selection.activeGameObject = groupObject;
            EditorUtility.SetDirty(groupObject);
        }

        private Vector3 CenterOfSelection()
        {
            if (_selectedObjects == null || _selectedObjects.Count == 0)
                return Vector3.zero;
            if (_selectedObjects.Count == 1)
                return _selectedObjects[0].position;

            float minX = Mathf.Infinity;
            float minY = Mathf.Infinity;
            float minZ = Mathf.Infinity;

            float maxX = -Mathf.Infinity;
            float maxY = -Mathf.Infinity;
            float maxZ = -Mathf.Infinity;

            foreach (Transform tr in _selectedObjects)
            {
                if (tr.position.x < minX)
                    minX = tr.position.x;
                if (tr.position.y < minY)
                    minY = tr.position.y;
                if (tr.position.z < minZ)
                    minZ = tr.position.z;

                if (tr.position.x > maxX)
                    maxX = tr.position.x;
                if (tr.position.y > maxY)
                    maxY = tr.position.y;
                if (tr.position.z > maxZ)
                    maxZ = tr.position.z;
            }

            return new Vector3((minX + maxX) / 2.0f, (minY + maxY) / 2.0f, (minZ + maxZ) / 2.0f);
        }

        private bool isUIMode()
        {
            return _selectedObjects.All(transform => transform.GetComponent<RectTransform>());
        }

        private void RemoveRaycast()
        {
            List<Graphic> cacheList = new List<Graphic>();
            foreach (Transform item in SelectedObjects)
            {
                item.GetComponents<Graphic>(cacheList);
                foreach (Graphic graphic in cacheList)
                {
                    graphic.raycastTarget = false;
                }
                var tmpro = item.GetComponent<TextMeshProUGUI>();
                if (tmpro != null)
                {
                    tmpro.raycastTarget = false;
                    EditorUtility.SetDirty(tmpro.gameObject);
                }
            }
        }

        private List<RectTransform> _selectedRectTransforms => _selectedObjects?.Select(transform => transform.GetComponent<RectTransform>()).ToList();

        private void SwitchToTMP()
        {
            foreach (Transform o in SelectedObjects)
            {
                Utility.ChangeTextToTMProText(o.gameObject);
            }
        }



        private void SpacingH()
        {
            if (SelectedObjects.Count < 2)
                return;

            int count = SelectedObjects.Count;
            float spacing;
            float minPos = Single.MaxValue;
            float maxPos = -Single.MaxValue;

            foreach (Transform o in SelectedObjects)
            {
                Vector2 anchoredPosition = o.GetComponent<RectTransform>().anchoredPosition;
                if (anchoredPosition.x > maxPos)
                {
                    maxPos = anchoredPosition.x;
                }

                if (anchoredPosition.x < minPos)
                {
                    minPos = anchoredPosition.x;
                }
            }

            spacing = (maxPos - minPos) / (count - 1);
            Transform parent = Selection.activeTransform.parent;
            foreach (var item in SelectedObjects)
            {
                if (item.parent != parent)
                {
                    Debug.Log("????????????????????????????????????????????????????????????");
                    return;
                }
            }
            List<Transform> tf = new List<Transform>(SelectedObjects);
            tf.Sort((x, y) => { return x.GetSiblingIndex().CompareTo(y.GetSiblingIndex()); });


            for (int i = 0; i < count; i++)
            {
                Vector2 anchoredPosition = tf[i].GetComponent<RectTransform>().anchoredPosition;
                tf[i].GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(minPos + i * spacing, anchoredPosition.y);
                EditorUtility.SetDirty(tf[i].gameObject);
            }
        }

        private void SpacingV()
        {
            if (SelectedObjects.Count < 2)
                return;

            int count = SelectedObjects.Count;
            float spacing;
            float minPos = Single.MaxValue;
            float maxPos = -Single.MaxValue;

            foreach (Transform o in SelectedObjects)
            {
                Vector2 anchoredPosition = o.GetComponent<RectTransform>().anchoredPosition;
                if (anchoredPosition.y > maxPos)
                {
                    maxPos = anchoredPosition.y;
                }

                if (anchoredPosition.y < minPos)
                {
                    minPos = anchoredPosition.y;
                }
            }

            spacing = (maxPos - minPos) / (count - 1);
            Transform parent = Selection.activeTransform.parent;
            foreach (var item in SelectedObjects)
            {
                if (item.parent != parent)
                {
                    Debug.Log("????????????????????????????????????????????????????????????");
                    return;
                }
            }
            List<Transform> tf = new List<Transform>(SelectedObjects);
            tf.Sort((x, y) => { return x.GetSiblingIndex().CompareTo(y.GetSiblingIndex()); });
            for (int i = 0; i < tf.Count; i++)
            {
                Vector2 anchoredPosition = tf[i].GetComponent<RectTransform>().anchoredPosition;
                tf[i].GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(anchoredPosition.x, minPos + i * spacing);
                EditorUtility.SetDirty(tf[i].gameObject);
            }
        }

        private void AlignH()
        {
            if (SelectedObjects.Count == 0)
                return;

            Transform first = Selection.activeTransform;

            foreach (Transform o in SelectedObjects)
            {
                o.transform.position = new Vector3(o.transform.position.x, first.transform.position.y, o.transform.position.z);
                EditorUtility.SetDirty(o.gameObject);
            }
        }

        private void AlignV()
        {
            if (SelectedObjects.Count == 0)
                return;

            Transform first = Selection.activeTransform;

            foreach (Transform o in SelectedObjects)
            {
                o.transform.position = new Vector3(first.transform.position.x, o.transform.position.y, o.transform.position.z);
                EditorUtility.SetDirty(o.gameObject);
            }
        }

        private void FlipLayout()
        {
            foreach (Transform t in SelectedObjects)
            {
                FlipLayout(t);
                EditorUtility.SetDirty(t.gameObject);
            }
        }

        private void FlipLayout(Transform t)
        {
            if (t.GetComponent<HorizontalLayoutGroup>())
            {
                SetLayout(t, UILayout.Vertical);
            }
            else if (t.GetComponent<VerticalLayoutGroup>())
            {
                SetLayout(t, UILayout.Horizontal);
            }
        }

        private void Duplicate()
        {
            if (WrongObject)
                return;

            GameObject go = Instantiate(SelectedObject);
            go.transform.SetParent(SelectedObject.transform.parent);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;

            RectTransform rectTransform = go.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector2.zero;
            go.name = SelectedObject.name;
            Selection.activeGameObject = go;
            EditorUtility.SetDirty(go);
        }

        private void AddUIComponent(Type type)
        {
            foreach (Transform o in SelectedObjects)
            {
                AddUIComponent(o, type);
                EditorUtility.SetDirty(o.gameObject);
            }
        }

        private Component AddUIComponent(Transform transform, Type type)
        {
            var component = transform.gameObject.GetComponent(type);
            if (component == null)
            {
                component = transform.gameObject.AddComponent(type);
            }
            else
            {
                DestroyImmediate(component);
            }
            return component;
        }

        private void AddEmptyChild()
        {
            foreach (var o in SelectedObjects)
            {
                Type windowType = Utility.GetType("UnityEditor", "UnityEditor.SceneHierarchyWindow");
                EditorWindow.FocusWindowIfItsOpen(windowType);
                GameObject go = new GameObject("Empty Child", typeof(RectTransform));
                go.transform.SetParent(o.transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                RectTransform rectTransform = go.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = Vector2.zero;
                EditorUtility.SetDirty(o.gameObject);
                Selection.activeGameObject = go;
            }
        }

        private void AddContentSizeFitter()
        {
            foreach (var o in SelectedObjects)
            {
                AddUIComponent(typeof(ContentSizeFitter));
                ContentSizeFitter contentSizeFitter = o.GetComponent<ContentSizeFitter>();
                if (contentSizeFitter != null)
                {
                    contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                }
            }
        }

        private void SetLayout(UILayout layoutType)
        {
            foreach (var t in SelectedObjects)
            {
                SetLayout(t, layoutType);
                EditorUtility.SetDirty(t.gameObject);
            }
        }

        private void SetLayout(Transform t, UILayout layoutType)
        {
            var layoutGroup = t.GetComponent<HorizontalOrVerticalLayoutGroup>();
            LayoutData newLayoutData = new LayoutData(layoutGroup);

            switch (layoutType)
            {
                case UILayout.Vertical:
                    newLayoutData.LayoutType = UILayout.Vertical;
                    break;
                case UILayout.Horizontal:
                    newLayoutData.LayoutType = UILayout.Horizontal;
                    break;
            }

            CreateLayout(t, newLayoutData);
        }

        private void CreateLayout(Transform t, LayoutData layoutData)
        {
            var layoutGroup = t.GetComponent<HorizontalOrVerticalLayoutGroup>();

            if (layoutGroup != null)
                DestroyImmediate(layoutGroup);

            if (layoutData.LayoutType == UILayout.Vertical && !(layoutGroup is VerticalLayoutGroup))
            {
                layoutGroup = (HorizontalOrVerticalLayoutGroup)AddUIComponent(t, typeof(VerticalLayoutGroup));
            }
            else if (layoutData.LayoutType == UILayout.Horizontal && !(layoutGroup is HorizontalLayoutGroup))
            {
                layoutGroup = (HorizontalOrVerticalLayoutGroup)AddUIComponent(t, typeof(HorizontalLayoutGroup));
            }

            SetLayout(layoutGroup, layoutData);
        }

        private void SetLayout(HorizontalOrVerticalLayoutGroup layoutGroup, LayoutData layoutData)
        {
            if (layoutGroup == null)
                return;

            layoutGroup.childForceExpandHeight = layoutData.childForceExpandHeight;
            layoutGroup.childForceExpandWidth = layoutData.childForceExpandWidth;
            layoutGroup.padding = layoutData.padding;
            layoutGroup.childAlignment = layoutData.childAlignment;
            layoutGroup.spacing = layoutData.spacing;
        }

        public void RectPosRoundToWholeNumber()
        {
            foreach (Transform tf in SelectedObjects)
            {
                RectTransform rect = tf as RectTransform;
                if (rect != null)
                {
                    Vector2 vector = rect.anchoredPosition;
                    vector.x = Mathf.RoundToInt(vector.x);
                    vector.y = Mathf.RoundToInt(vector.y);
                    rect.anchoredPosition = vector;
                    vector = rect.sizeDelta;
                    vector.x = Mathf.RoundToInt(vector.x);
                    vector.y = Mathf.RoundToInt(vector.y);
                    rect.sizeDelta = vector;
                }
            }
        }


        public void ApplySelectPrefabInstance()
        {
            GameObject root = PrefabUtility.GetNearestPrefabInstanceRoot(Selection.activeGameObject);
            if (null != root)
            {
                PrefabUtility.ApplyPrefabInstance(root, InteractionMode.UserAction);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// ???????????????????????????
        /// </summary>
        public void FixCopyGameObjectName()
        {
            foreach (Transform tf in SelectedObjects)
            {
                if (tf.name.Length > 3 && tf.name.EndsWith(")") && tf.name[tf.name.Length - 3] == '(')
                {
                    tf.name = tf.name.Substring(0, tf.name.Length - 3);
                    tf.name = tf.name.Trim();
                    EditorUtility.SetDirty(tf.gameObject);
                }
            }
        }

        /// <summary>
        /// ?????????????????????????????????????????????
        /// </summary>
        public void TrimGameObjectName()
        {
            foreach (Transform tf in SelectedObjects)
            {
                tf.name = tf.name.Trim();
                EditorUtility.SetDirty(tf.gameObject);
            }
        }

        /// <summary>
        /// ???Rect?????????????????????????????????
        /// </summary>
        public void ExpandRectTransform()
        {
            RectTransform rect = Selection.activeGameObject.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.anchoredPosition3D = Vector3.zero;
                rect.sizeDelta = Vector2.zero;
            }
        }

        public bool WrongObject
        {
            get
            {
                if (SelectedObject == null)
                    return true;
                if (SelectedObject.GetComponent<RectTransform>() == null)
                    return true;
                return false;
            }
        }
    }

    internal class StringText
    {
        internal string GroupSelected = "Group selected";
        internal string Add_Child = "Add Child";
        internal string V_Group = "V Group";
        internal string H_Group = "H Group";
        internal string Flip_layout = "Flip layout";
        internal string Content_Size = "Content Size";
        internal string Layout_Element = "Layout Element";
        internal string Add_Image = "Add Image";
        internal string Add_Button = "Add Button";
        internal string Duplicate = "Duplicate";
        internal string ApplyPrefab = "ApplyPrefab";
        internal string V_Align = "V-Align";
        internal string H_Align = "H-Align";
        internal string V_Spacing = "V-Spacing";
        internal string H_Spacing = "H-Spacing";
        internal string TXT2TMP = "TXT -> TMP";
        internal string RemoveRaycast = "Remove UI Raycast";
        internal string RoundRectPos = "Round Rect Pos";
        internal string FixCopyGoName = "Fix CopyGo Name";
        internal string TrimGoName = "Trim Go Name";
        internal string ExpandRect = "Expand Rect";

    }

    internal class ChineseText : StringText
    {
        public ChineseText()
        {
            GroupSelected = "???????????????";
            Add_Child = "???????????????";
            V_Group = "?????????????????????";
            H_Group = "?????????????????????";
            Flip_layout = "?????????????????????";
            Content_Size = "?????? Content Size";
            Layout_Element = "?????? Layout Element";
            Add_Image = "?????? Image";
            Add_Button = "?????? Button";
            Duplicate = "??????????????????";
            V_Align = "??????????????????????????????X?????????";
            H_Align = "??????????????????????????????Y?????????";
            V_Spacing = "Y???????????????";
            H_Spacing = "X???????????????";
            TXT2TMP = "TXT -> TMP";
            RemoveRaycast = "??????UI???Raycast";
            RoundRectPos = "??????Rect??????????????????";
            ApplyPrefab = "?????????????????????";
            FixCopyGoName = "????????????????????????";
            TrimGoName = "???????????????????????????";
            ExpandRect = "????????????????????????????????????";
        }
    }
}
