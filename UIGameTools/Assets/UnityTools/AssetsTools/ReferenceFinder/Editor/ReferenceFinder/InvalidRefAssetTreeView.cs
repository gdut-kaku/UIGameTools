using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TreeViewExamples;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetsTools.ReferenceFinder
{
    public class InvalidRefTreeElement : PathTreeElement
    {
        /// <summary>
        /// 记录错误引用资源的GameObject的FileId
        /// </summary>
        public List<string> targetFileIdList = new List<string>();
        public InvalidRefTreeElement()
        {
        }

        public InvalidRefTreeElement(bool isFold, string assetPath, string displayName, int id, int depth) : base(isFold, assetPath, displayName, id, depth)
        {
        }

        public void PingAsset()
        {
            if (!isFold)
            {
                EditorUtility.FocusProjectWindow();
                Object go = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                Selection.activeObject = go;
                EditorGUIUtility.PingObject(go);
            }
        }

        public Object GetObejct()
        {
            if (!isFold)
            {
                Object go = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                return go;
            }
            else
            {
                return null;
            }
        }

        public List<GameObject> GetTargetObject(GameObject go)
        {
            List<GameObject> gameObjects = new List<GameObject>();
            List<string> list = GetTargetName();
            if (go != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    gameObjects.Add(go.transform.Find(list[i]).gameObject);
                }
            }
            return gameObjects;
        }

        /// <summary>
        /// 由于实例化后的物体的fileid会被重置，因此将fileid转换成名字，这样实例化后也可以找到指定的物体。
        /// /// </summary>
        /// <returns></returns>
        public List<string> GetTargetName()
        {
            List<string> nameList = new List<string>();
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (go != null)
            {
                Transform[] tf = go.GetComponentsInChildren<Transform>();
                for (int i = 0; i < tf.Length; i++)
                {
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(tf[i].gameObject.GetInstanceID(), out string guid, out long localId);
                    if (targetFileIdList.Contains(localId.ToString()))
                    {
                        nameList.Add(GetName(tf[i].gameObject, go));
                    }
                }
            }
            return nameList;
        }

        static string GetName(GameObject son, GameObject root)
        {
            var rootTf = root.transform;
            string name = son.transform.name;
            var tempTf = son.transform.parent;
            while (tempTf != null && tempTf != rootTf)
            {
                name = tempTf.name + "/" + name;
            }
            return name;
        }

    }

    public class InvalidRefAssetTreeView : TreeViewWithTreeModel<InvalidRefTreeElement>
    {
        //图标宽度
        const float kIconWidth = 18f;
        //列表高度
        const float kRowHeights = 20f;

        //列信息
        enum MyColumns
        {
            Icon,
            Name,
            AssetPath,
            ObejctField,
            OpenFinder,
        }

        readonly Type m_SceneHierarchWindowType;

        public InvalidRefAssetTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<InvalidRefTreeElement> model) : base(state, multiColumnHeader, model)
        {
            m_SceneHierarchWindowType = KakuEditorTools.Utility.GetType("UnityEditor", "UnityEditor.SceneHierarchyWindow");

            rowHeight = kRowHeights;
            columnIndexForTreeFoldouts = 1;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
            extraSpaceBeforeIconAndLabel = kIconWidth;


            Reload();
        }


        protected override void RowGUI(RowGUIArgs args)
        {
            TreeViewItem<InvalidRefTreeElement> item = args.item as TreeViewItem<InvalidRefTreeElement>;
            for (int i = 0; i < args.GetNumVisibleColumns(); i++)
            {
                CellGUI(args.GetCellRect(i), item.data, (MyColumns)args.GetColumn(i), ref args);
            }
        }


        private void CellGUI(Rect cellRect, InvalidRefTreeElement element, MyColumns column, ref RowGUIArgs args)
        {
            // Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {
                case MyColumns.Icon:
                    GUI.DrawTexture(cellRect, GetIcon(element.assetPath), ScaleMode.ScaleToFit);
                    break;
                case MyColumns.Name:
                    args.rowRect = cellRect;
                    base.RowGUI(args);
                    break;
                case MyColumns.AssetPath:
                    GUI.Label(cellRect, element.assetPath);
                    break;
                case MyColumns.ObejctField:
                    if (!element.isFold)
                    {
                        Object @object = element.GetObejct();
                        EditorGUI.ObjectField(cellRect, GUIContent.none, @object, @object.GetType(), allowSceneObjects: false);
                    }
                    break;
                case MyColumns.OpenFinder:
                    if (!element.isFold)
                    {
                        if (GUI.Button(cellRect, element.targetFileIdList.Count > 0 ? "查看错误引用对象" : "查看资源引用"))
                        {
                            Selection.activeObject = element.GetObejct();
                            if (element.targetFileIdList.Count > 0)
                            {
                                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                                if (prefabStage == null || prefabStage.assetPath != element.assetPath)
                                {
                                    AssetDatabase.OpenAsset(Selection.activeObject.GetInstanceID());
                                    prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                                }
                                var list = element.GetTargetObject(prefabStage.prefabContentsRoot);
                                //设置 Selection.objects 的时候，需要聚焦到对应的窗口，否者选择项不会变成蓝色。
                                EditorWindow.FocusWindowIfItsOpen(m_SceneHierarchWindowType);
                                Selection.objects = list.ToArray();
                            }
                            else
                            {
                                element.PingAsset();
                                //ReferenceFinderWindow.FindRef();
                            }
                        }
                    }
                    break;
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            TreeViewItem<InvalidRefTreeElement> item = FindItem(id, rootItem) as TreeViewItem<InvalidRefTreeElement>;
            if (item != null)
            {
                item.data.PingAsset();
            }
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return false;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            base.RenameEnded(args);
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return false;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        //根据资源信息获取资源图标
        private Texture2D GetIcon(string path)
        {
            Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
            if (obj != null)
            {
                Texture2D icon = AssetPreview.GetMiniThumbnail(obj);
                if (icon == null)
                    icon = AssetPreview.GetMiniTypeThumbnail(obj.GetType());
                return icon;
            }
            return null;
        }

        public static MultiColumnHeaderState CreateDefaultHeaderState()
        {
            MultiColumnHeaderState.Column[] columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Icon"),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    width = 80,
                    minWidth = 40,
                    maxWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = false,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Name"),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    width = 200,
                    minWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = false,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("AssetPath"),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    width = 420,
                    minWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = false,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("ObjectField"),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    width = 200,
                    minWidth = 150,
                    autoResize = false,
                    allowToggleVisibility = false,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Function"),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    width = 200,
                    minWidth = 150,
                    autoResize = false,
                    allowToggleVisibility = false,
                    canSort = false
                }
            };
            MultiColumnHeaderState headerState = new MultiColumnHeaderState(columns);
            return headerState;
        }

    }
}
