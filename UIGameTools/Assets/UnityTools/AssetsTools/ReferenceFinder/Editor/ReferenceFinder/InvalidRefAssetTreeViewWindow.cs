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
    public class InvalidRefAssetTreeViewWindow : EditorWindow
    {
        [MenuItem("AssetsTools/AssetFinder/InvalidRefAssetWindow")]
        public static InvalidRefAssetTreeViewWindow GetWindow()
        {
            var window = GetWindow<InvalidRefAssetTreeViewWindow>();
            window.titleContent = new GUIContent("InvalidRef Asset Finder.");
            window.Focus();
            window.Repaint();
            return window;
        }


        static bool s_InitializedData = false;
        private static Dictionary<string, List<string>> s_MainAssetRefDict = new Dictionary<string, List<string>>();

        [NonSerialized]
        bool m_Initialized = false;
        TreeViewState m_TreeViewState;
        InvalidRefAssetTreeView m_TreeView;

        private string m_RootPath = "";
        private string m_FilterPath = "";

        void InitTreeViewIfNeed()
        {
            if (!m_Initialized)
            {
                if (m_TreeViewState == null)
                {
                    m_TreeViewState = new TreeViewState();
                }
                var headerstate = InvalidRefAssetTreeView.CreateDefaultHeaderState();
                MultiColumnHeader header = new MultiColumnHeader(headerstate);
                header.ResizeToFit();
                m_Initialized = true;

                TreeModel<InvalidRefTreeElement> treeModel = new TreeModel<InvalidRefTreeElement>(CreateUnuseAssetTree());
                m_TreeView = new InvalidRefAssetTreeView(m_TreeViewState, header, treeModel);
                m_Initialized = true;
            }
        }

        Rect multiColumnTreeViewRect
        {
            get { return new Rect(20, 120, position.width - 40, position.height - 150); }
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
            EditorGUILayout.LabelField("填写主资源根路径[必要]", GUILayout.Height(20));
            m_RootPath = EditorGUILayout.TextField(m_RootPath, GUILayout.Height(20));
            EditorGUILayout.LabelField("填写副资源根路径", GUILayout.Height(20));
            m_FilterPath = EditorGUILayout.TextField(m_FilterPath, GUILayout.Height(20));
            if (GUILayout.Button("搜索", GUILayout.Height(20)))
            {
                if (string.IsNullOrEmpty(m_RootPath))
                {
                    return;
                }
                m_Initialized = false;
                InitTreeViewIfNeed();
            }
            if (string.IsNullOrEmpty(m_RootPath))
            {
                return;
            }
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
                    m_TreeView?.ExpandAll();
                }
                if (GUILayout.Button("CollapseAll", style))
                {
                    m_TreeView?.CollapseAll();
                }
                if (GUILayout.Button("Refresh AssetRef", style))
                {
                    s_InitializedData = false;
                    InitDataIfNeeded();
                    m_Initialized = false;
                    InitTreeViewIfNeed();
                }
            }
            GUILayout.EndArea();
        }

        //初始化数据
        void InitDataIfNeeded()
        {
            //初始化数据
            if (!s_InitializedData)
            {
                if (string.IsNullOrEmpty(m_RootPath))
                {
                    return;
                }
                s_MainAssetRefDict.Clear();
                AssetDatabase.Refresh();
                bool searchFilter = !string.IsNullOrEmpty(m_FilterPath);
                string[] guids = AssetDatabase.FindAssets("*", new string[] { m_RootPath });
                int len = guids.Length;
                EditorUtility.DisplayProgressBar("正在处理数据", "正在处理数据...", 0 / (float)len);
                for (int i = 0; i < guids.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("正在处理数据", "正在处理数据...", i / (float)len);
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    string[] dependencies = AssetDatabase.GetDependencies(assetPath);
                    //判断有没有非m_RootPath路径下的依赖资源。只找一层就够了。
                    for (int j = 0; j < dependencies.Length; j++)
                    {
                        string dpPath = dependencies[j];
                        if (!dpPath.StartsWith(m_RootPath, StringComparison.Ordinal) &&
                            (!searchFilter || dpPath.StartsWith(m_FilterPath, StringComparison.Ordinal)))
                        {
                            if (!s_MainAssetRefDict.TryGetValue(assetPath, out List<string> list))
                            {
                                list = new List<string>();
                                s_MainAssetRefDict.Add(assetPath, list);
                            }
                            list.Add(dpPath);
                        }
                    }
                }
                EditorUtility.ClearProgressBar();
                s_InitializedData = true;
            }
        }


        /// <summary>
        /// 创建没有被使用的文件的树状结构。
        /// </summary>
        /// <returns></returns>
        IList<InvalidRefTreeElement> CreateUnuseAssetTree()
        {
            InitDataIfNeeded();
            int id = 1;
            InvalidRefTreeElement root = new InvalidRefTreeElement(true, m_RootPath, m_RootPath, id, -1);
            root.children = new List<TreeElement>();

            foreach (KeyValuePair<string, List<string>> kvp in s_MainAssetRefDict)
            {
                id++;
                var son = new InvalidRefTreeElement(false, kvp.Key, kvp.Key.Substring(kvp.Key.LastIndexOf("/") + 1), id, 0);
                son.children = new List<TreeElement>();
                root.children.Add(son);
                foreach (string dpPath in kvp.Value)
                {
                    id++;
                    string dpGUID = AssetDatabase.AssetPathToGUID(dpPath);
                    var goFileId = FindGameObjectFileIdByGUID(kvp.Key, dpGUID);
                    son.targetFileIdList.AddRange(goFileId);
                    var sonChild = new InvalidRefTreeElement(false, dpPath, dpPath.Substring(dpPath.LastIndexOf("/") + 1), id, 1);
                    son.children.Add(sonChild);
                }
            }
            List<InvalidRefTreeElement> rs = new List<InvalidRefTreeElement>();
            TreeElementUtility.TreeToList(root, rs);
            return rs;
        }

        /// <summary>
        /// 查找引用了guid资源的GameObject.
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        static List<string> FindGameObjectFileIdByGUID(string assetPath, string guid)
        {
            if (!assetPath.EndsWith(".prefab"))
            {
                throw new Exception("不能搜索非预设体资源");
            }
            List<string> fileIdList = new List<string>();
            string[] prefabLine = File.ReadAllLines(assetPath);

            for (int i = 0; i < prefabLine.Length; i++)
            {
                if (prefabLine[i].Contains(guid))
                {
                    int index = i;
                    for (int j = index - 1; j >= 0; j--)
                    {
                        if (prefabLine[j].Contains("m_GameObject:"))
                        {
                            int index2 = prefabLine[j].IndexOf("fileID:");
                            int index3 = prefabLine[j].IndexOf("}");
                            index2 += 8;
                            if (index3 > index2)
                            {
                                string fileId = prefabLine[j].Substring(index2, index3 - index2);
                                fileIdList.Add(fileId);
                            }
                            break;
                        }
                    }
                }
            }
            return fileIdList;
        }
    }
}

