using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TreeViewExamples;
using UnityEngine;
using System;

namespace AssetsTools.ReferenceFinder
{
    public class UnuseAssetTreeViewWindow : EditorWindow
    {
        static bool s_InitializedData = false;
        private static ReferenceFinderData m_Data = new ReferenceFinderData();

        [NonSerialized]
        bool m_Initialized = false;
        TreeViewState m_TreeViewState;
        UnuseAssetTreeView m_TreeView;

        void InitTreeViewIfNeed()
        {
            if (!m_Initialized)
            {
                if (m_TreeViewState == null)
                {
                    m_TreeViewState = new TreeViewState();
                }
                var headerstate = UnuseAssetTreeView.CreateDefaultHeaderState();
                MultiColumnHeader header = new MultiColumnHeader(headerstate);
                header.ResizeToFit();
                m_Initialized = true;

                TreeModel<UnAsTreeElement> treeModel = new TreeModel<UnAsTreeElement>(CreateUnuseAssetTree());
                m_TreeView = new UnuseAssetTreeView(m_TreeViewState, header, treeModel);
                m_Initialized = true;
            }
        }

        Rect multiColumnTreeViewRect
        {
            get { return new Rect(20, 30, position.width - 40, position.height - 60); }
        }

        Rect BottomBarViewRect
        {
            get
            {
                return new Rect(20, position.height - 20, position.width - 40, 20);
            }
        }


        void OnGUI()
        {
            InitTreeViewIfNeed();
            m_TreeView?.OnGUI(multiColumnTreeViewRect);
            DrawBottomToolBar(BottomBarViewRect);
        }


        void DrawBottomToolBar(Rect rect)
        {
            GUILayout.BeginArea(rect);
            using (new EditorGUILayout.HorizontalScope())
            {
                var style = "miniButton";
                if (GUILayout.Button("ExpandAll", style))
                {
                    m_TreeView.ExpandAll();
                }
                if (GUILayout.Button("CollapseAll", style))
                {
                    m_TreeView.CollapseAll();
                }
                if (GUILayout.Button("IgnoreSelectFile", style))
                {
                    var so = UnuseAssetSetting.GetSetting();
                    IList<int> list = m_TreeView.GetSelection();
                    foreach (var id in list)
                    {
                        var item = m_TreeView.treeModel.Find(id);
                        if (item != null)
                        {
                            if (item.isFold)
                            {
                                if (!so.whitePaths.Contains(item.assetPath))
                                {
                                    so.whitePaths.Add(item.assetPath);
                                }
                            }
                            else
                            {
                                if (!so.whiteFilePaths.Contains(item.assetPath))
                                {
                                    so.whiteFilePaths.Add(item.assetPath);
                                }
                            }
                        }
                    }
                    EditorUtility.SetDirty(so);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    m_TreeView.treeModel.RemoveElements(list);
                }
                if (GUILayout.Button("Refresh AssetRef", style))
                {
                    m_Data.CollectDependenciesInfo();
                    m_Initialized = false;
                    InitTreeViewIfNeed();
                    GC.Collect();
                }
            }
            GUILayout.EndArea();
        }

        //初始化数据
        static void InitDataIfNeeded()
        {
            //初始化数据
            if (!s_InitializedData)
            {
                //初始化数据
                if (!m_Data.ReadFromCache())
                {
                    m_Data.CollectDependenciesInfo();
                }
                s_InitializedData = true;
            }
        }

        [MenuItem("AssetsTools/AssetFinder/UnuseAssetWindow")]
        public static UnuseAssetTreeViewWindow GetWindow()
        {
            var window = GetWindow<UnuseAssetTreeViewWindow>();
            window.titleContent = new GUIContent("Unuse Asset Finder.");
            window.Focus();
            window.Repaint();
            return window;
        }


        /// <summary>
        /// 创建没有被使用的文件的树状结构。
        /// </summary>
        /// <returns></returns>
        IList<UnAsTreeElement> CreateUnuseAssetTree()
        {
            InitDataIfNeeded();
            var so = UnuseAssetSetting.GetSetting();
            //限定根目录文件夹
            string rootDirPath = so.baseRootPath;
            int id = 1;
            int depthGap = Regex.Matches(rootDirPath, "/").Count + 1;
            Dictionary<int, List<UnAsTreeElement>> depthToList = new Dictionary<int, List<UnAsTreeElement>>();
            var root = new UnAsTreeElement(false, rootDirPath, rootDirPath, id, -1);
            depthToList.Add(-1, new List<UnAsTreeElement>() { root });

            //收集资源文件夹。
            foreach (KeyValuePair<string, ReferenceFinderData.AssetDescription> kvp in m_Data.assetDict)
            {
                if (!kvp.Value.path.StartsWith(rootDirPath, System.StringComparison.Ordinal))
                    continue;
                if (kvp.Value.path.EndsWith(".prefab", System.StringComparison.Ordinal))
                    continue;
                if (kvp.Value.references.Count > 0)
                    continue;
                string path = kvp.Value.path.Replace("\\", "/");
                //处理白名单。
                if (so.whiteFilePaths.Contains(path))
                    continue;
                bool inWhite = false;
                for (int i = 0; i < so.whiteFileExtension.Count; i++)
                {
                    if (Path.GetExtension(path) == so.whiteFileExtension[i])
                    {
                        inWhite = true;
                    }
                }
                if (inWhite)
                    continue;
                inWhite = false;
                string dirName = Path.GetDirectoryName(path).Replace("\\", "/");
                for (int i = 0; i < so.whitePaths.Count; i++)
                {
                    if (dirName.StartsWith(so.whitePaths[i], System.StringComparison.Ordinal))
                    {
                        inWhite = true;
                        break; ;
                    }
                }
                if (inWhite)
                    continue;

                int depth = Regex.Matches(path, "/").Count - depthGap;
                PathTreeUtility.CreatePathNodeRecursive(ref id, path, depth, depthToList);
            }
            List<UnAsTreeElement> rs = new List<UnAsTreeElement>();
            TreeElementUtility.TreeToList(root, rs);
            return rs;
        }
    }
}
