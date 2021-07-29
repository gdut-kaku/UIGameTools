using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TreeViewExamples;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetsTools.ReferenceFinder
{
    public class UnAsTreeElement : PathTreeElement
    {
        public UnAsTreeElement() : base()
        {
        }

        public UnAsTreeElement(bool isFold, string assetPath, string displayName, int id, int depth) : base(isFold, assetPath, displayName, id, depth)
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
    }

    public class UnuseAssetTreeView : TreeViewWithTreeModel<UnAsTreeElement>
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
            ObejctField
        }


        public UnuseAssetTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<UnAsTreeElement> model) : base(state, multiColumnHeader, model)
        {
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
            TreeViewItem<UnAsTreeElement> item = args.item as TreeViewItem<UnAsTreeElement>;
            for (int i = 0; i < args.GetNumVisibleColumns(); i++)
            {
                CellGUI(args.GetCellRect(i), item.data, (MyColumns)args.GetColumn(i), ref args);
            }
        }


        private void CellGUI(Rect cellRect, UnAsTreeElement element, MyColumns column, ref RowGUIArgs args)
        {
            // Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {
                case MyColumns.Icon:
                    if (element.isFold)
                    {
                        GUI.DrawTexture(cellRect, EditorGUIUtility.FindTexture("Folder Icon"), ScaleMode.ScaleToFit);
                    }
                    else
                    {
                        GUI.DrawTexture(cellRect, GetIcon(element.assetPath), ScaleMode.ScaleToFit);
                    }
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
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            TreeViewItem<UnAsTreeElement> item = FindItem(id, rootItem) as TreeViewItem<UnAsTreeElement>;
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
                }
            };
            MultiColumnHeaderState headerState = new MultiColumnHeaderState(columns);
            return headerState;
        }

    }
}
