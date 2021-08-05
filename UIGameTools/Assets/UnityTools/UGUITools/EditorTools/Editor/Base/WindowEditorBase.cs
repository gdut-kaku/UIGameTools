using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KakuEditorTools
{
    public abstract class WindowEditorBase<T> : EditorWindow where T : WindowEditorBase<T>
    {
        protected static string WindowTitle { get; set; }

        protected static bool IsUtility { get; set; }

        protected static T m_Window;

        protected static void Init()
        {
            Type type = typeof(T);
            WindowBaseAttribute info = null;
            foreach (var item in type.GetCustomAttributes(false))
            {
                info = item as WindowBaseAttribute;
                if (null != info)
                {
                    break;
                }
            }
            if (info != null)
            {
                m_Window = GetWindow<T>(info.IsUtility, info.Name, true);
            }
            else
            {
                m_Window = GetWindow<T>(false, typeof(T).Name, true);
            }
        }


        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }


        protected virtual void OnGUI()
        {
        }

        /// <summary>
        /// 绘制顶部Toggle标签。
        /// </summary>
        /// <param name="bars">可选标签名字数组。</param>
        /// <param name="lastIndex">当前选择的索引。</param>
        /// <param name="width">标签的宽度。</param>
        /// <returns>返回最后选择的索引。</returns>
        protected int DrawToolBar(string[] bars, int lastIndex, int width = 120)
        {
            int currentIndex = lastIndex;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            for (int i = 0; i < bars.Length; i++)
            {
                GUILayout.BeginVertical(EditorStyles.toolbar);
                if (GUILayout.Toggle(i == currentIndex, bars[i], EditorStyles.toolbarButton, GUILayout.Height(25), GUILayout.Width(width)))
                {
                    currentIndex = i;
                }
                GUILayout.EndVertical();
            }
            GUILayout.Toolbar(0, new[] { "" }, EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
            return currentIndex;
        }

        Vector2 m_DrawGameObjectSelector_ScrollPos = Vector2.zero;
        /// <summary>
        /// 绘制添加GameObject功能的基础图形。
        /// </summary>
        /// <param name="goList">存放GameObject的列表。</param>
        /// <param name="scrollPos">滚动列表的位置。</param>
        /// <param name="height">滚动列表的最小高度。</param>
        protected void DrawGameObjectSelector<AssetType>(List<AssetType> goList, bool allowSceneObject = true, int height = 120) where AssetType : UnityEngine.Object
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Label("手动添加的物体", EditorStyles.toolbarButton);
            if (goList.Count == 0 || goList[goList.Count - 1] != null)
            {
                goList.Add(null);
            }
            m_DrawGameObjectSelector_ScrollPos = EditorGUILayout.BeginScrollView(m_DrawGameObjectSelector_ScrollPos, false, false, GUILayout.Height(height));
            int length = goList.Count;
            for (int i = 0; i < goList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                AssetType obj = (AssetType)EditorGUILayout.ObjectField(goList[i], typeof(AssetType), true);
                if (goList[i] != obj)
                {
                    if (goList.Contains(obj))
                    {
                        goList.RemoveAt(i);
                        i--;
                        Debug.Log("该物体已经存在，不要添加重复物体。");
                        continue;
                    }
                    goList[i] = obj;
                }
                if ((goList[i] == null && i < length) || GUILayout.Button("Remove", GUILayout.Width(120)))
                {
                    goList.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        Vector2 m_DrawGameObjectSelectorArea_ScrollPos = Vector2.zero;
        /// <summary>
        /// 绘制检索文件夹物品功能的基础图形。
        /// </summary>
        /// <typeparam name="ObjectType">显示在GUI面板中的资源类型。</typeparam>
        /// <typeparam name="AssetType">从文件夹中加载的资源类型。</typeparam>
        /// <param name="btnName">按钮的名称。</param>
        /// <param name="filter">资源过滤条件。</param>
        /// <param name="goList">GameObject数组。</param>
        /// <param name="objectList">显示在GUI面板中的资源列表。</param>
        /// <param name="clickCallback">按钮点击回调。</param>
        protected void DrawGameObjectSelectorArea<ObjectType>(string btnName, string filter, List<GameObject> goList, List<ObjectType> objectList, Action<List<GameObject>> clickCallback) where ObjectType : UnityEngine.Object
        {
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button(btnName))
            {
                clickCallback?.Invoke(goList);
            }
            EditorGUILayout.Space(5);
            m_DrawGameObjectSelectorArea_ScrollPos = EditorGUILayout.BeginScrollView(m_DrawGameObjectSelectorArea_ScrollPos, false, false, GUILayout.Height(250));
            for (int i = 0; i < objectList.Count; i++)
            {
                EditorGUILayout.ObjectField(objectList[i], typeof(ObjectType), false);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        Vector2 m_DrawFoldSelector_ScrollPos = Vector2.zero;
        /// <summary>
        /// 绘制添加文件夹功能的基础图形。
        /// </summary>
        /// <param name="foldPathList">记录文件夹路径的列表</param>
        protected void DrawFoldSelector(List<string> foldPathList, int height = 120)
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Label("当前选中文件夹", EditorStyles.toolbarButton);
            m_DrawFoldSelector_ScrollPos = EditorGUILayout.BeginScrollView(m_DrawFoldSelector_ScrollPos, false, false, GUILayout.Height(height));
            for (int i = 0; i < foldPathList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(foldPathList[i], EditorStyles.textField);
                if (GUILayout.Button("Remove", GUILayout.Width(120)))
                {
                    foldPathList.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("添加文件夹"))
            {
                string foldPath = EditorUtility.OpenFolderPanel("选择一个文件夹", Application.dataPath, "");
                foldPath = Utility.AbsolutePathToRelativePath(foldPath);
                if (!string.IsNullOrEmpty(foldPath))
                {
                    foldPathList.Add(foldPath);
                }
            }
            EditorGUILayout.EndVertical();
        }

        Vector2 m_DrawFoldObjectArea_ScrollPos = Vector2.zero;
        /// <summary>
        /// 绘制检索文件夹物品功能的基础图形。
        /// </summary>
        /// <typeparam name="ObjectType">显示在GUI面板中的资源类型。</typeparam>
        /// <typeparam name="AssetType">从文件夹中加载的资源类型。</typeparam>
        /// <param name="btnName">按钮的名称。</param>
        /// <param name="filter">资源过滤条件。</param>
        /// <param name="foldArray">文件夹数组。</param>
        /// <param name="objectList">显示在GUI面板中的资源列表。</param>
        /// <param name="clickCallback">按钮点击回调。</param>
        protected void DrawFoldObjectArea<ObjectType, AssetType>(string btnName, string filter, string[] foldArray, List<ObjectType> objectList, Action<List<AssetType>> clickCallback) where AssetType : UnityEngine.Object where ObjectType : UnityEngine.Object
        {
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button(btnName))
            {
                List<AssetType> list = new List<AssetType>();
                string[] guidList = AssetDatabase.FindAssets(filter, foldArray);
                foreach (var guid in guidList)
                {
                    AssetType obejct = AssetDatabase.LoadAssetAtPath<AssetType>(AssetDatabase.GUIDToAssetPath(guid));
                    list.Add(obejct);
                }
                clickCallback?.Invoke(list);
            }
            EditorGUILayout.Space(5);
            m_DrawFoldObjectArea_ScrollPos = EditorGUILayout.BeginScrollView(m_DrawFoldObjectArea_ScrollPos, false, false, GUILayout.Height(250));
            for (int i = 0; i < objectList.Count; i++)
            {
                EditorGUILayout.ObjectField(objectList[i], typeof(ObjectType), false);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }


        /// <summary>
        /// 绘制水平线
        /// </summary>
        protected void DrawVerticalUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }

        /// <summary>
        /// 绘制垂直线
        /// </summary>
        protected void DrawHorizalUILine(Color color, int height = 100, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(padding + thickness));
            r.height = height;
            r.y -= 2;
            r.x += padding / 2;
            r.width = thickness;
            EditorGUI.DrawRect(r, color);
        }

    }

    public sealed class FoldObjectArea<ObjectType, AssetType> where AssetType : UnityEngine.Object where ObjectType : UnityEngine.Object
    {
        public string btnName;
        public string filter;
        public string[] foldArray;
        public List<ObjectType> objectList;
        public Action<List<AssetType>> clickCallback;

        public FoldObjectArea(string btnName, string filter, string[] foldArray, List<ObjectType> objectList, Action<List<AssetType>> clickCallback)
        {
            this.btnName = btnName;
            this.filter = filter;
            this.foldArray = foldArray;
            this.objectList = objectList;
            this.clickCallback = clickCallback;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class WindowBaseAttribute : Attribute
    {
        public WindowBaseAttribute(string name, bool isUtility)
        {
            Name = name;
            IsUtility = isUtility;
        }

        public string Name { get; set; }

        public bool IsUtility { get; set; }
    }
}

