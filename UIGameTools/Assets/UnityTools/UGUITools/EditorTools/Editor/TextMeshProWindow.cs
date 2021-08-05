using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace KakuEditorTools
{
    [WindowBase("TMPro工具", false)]
    public class TextMeshProWindow : WindowEditorBase<TextMeshProWindow>
    {
        enum ToolsType
        {
            [InspectorName("批量修改TMPro组件的字体文件")]
            ChangeFont = 0,
            [InspectorName("批量修改TMPro组件的材质球")]
            ChangeMat = 1,
            [InspectorName("批量修改Text为TMProText")]
            ChangeTxtToTMPro = 2,
        }
        static readonly string[] ToolsTypeName = new string[]
        {
            "批量修改TMPro组件的字体文件",
            "批量修改TMPro组件的材质球",
            "批量修改Text为TMProText"
        };

        [MenuItem("UGUITools/TMPro/批量修改字体文件工具")]
        public static void OpenWindow()
        {
            Init();
        }

        List<string> m_FoldList = new List<string>();
        List<GameObject> m_AllDragObjectList = new List<GameObject>();
        //
        List<TextMeshProUGUI> m_AllFoldTMproTextList = new List<TextMeshProUGUI>();
        List<TextMeshProUGUI> m_AllDragTMproTextList = new List<TextMeshProUGUI>();
        //
        List<Text> m_AllFoldTextList = new List<Text>();
        List<Text> m_AllDragTextList = new List<Text>();

        int m_ToolsTypeIndex = 0;

        TMP_FontAsset m_OldFontAsset;
        string m_OldFontAssetPath = "";
        TMP_FontAsset m_NewFontAsset;
        string m_NewFontAssetPath = "";
        Material m_NewMaterial = null;

        void Clean()
        {
            m_FoldList.Clear();
            m_AllFoldTMproTextList.Clear();
            m_AllDragObjectList.Clear();
            m_AllDragTMproTextList.Clear();

            m_AllFoldTextList.Clear();
            m_AllDragTextList.Clear();
        }

        protected override void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            int oldIndex = m_ToolsTypeIndex;
            m_ToolsTypeIndex = DrawToolBar(ToolsTypeName, m_ToolsTypeIndex, 200);
            if (oldIndex != m_ToolsTypeIndex)
            {
                Clean();
            }
            if (m_ToolsTypeIndex == (int)ToolsType.ChangeFont)
            {
                GUILayout.Label("请先选择正确的字体文件", EditorStyles.toolbarButton);
                m_OldFontAsset = DrawFontAsset(m_OldFontAsset, "旧字体文件", ref m_OldFontAssetPath);
                m_NewFontAsset = DrawFontAsset(m_NewFontAsset, "新字体文件", ref m_NewFontAssetPath);
                if (m_NewFontAsset == null || m_OldFontAsset == null)
                {
                    EditorGUILayout.EndVertical();
                    return;
                }
                EditorGUILayout.Space(5);
                if (GUILayout.Button("替换列表中所有TMProText的字体文件"))
                {
                    List<TextMeshProUGUI> tempList = new List<TextMeshProUGUI>(64);
                    tempList.AddRange(m_AllDragTMproTextList);
                    tempList.AddRange(m_AllFoldTMproTextList);
                    ChangeTMProTextFont(tempList, m_OldFontAsset, m_NewFontAsset);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log("替换成功。替换个数为 : " + tempList.Count);
                }
                EditorGUILayout.Space(5);
            }
            else if (m_ToolsTypeIndex == (int)ToolsType.ChangeMat)
            {
                GUILayout.Label("请先选择新字体材质球", EditorStyles.toolbarButton);
                m_NewMaterial = (Material)EditorGUILayout.ObjectField("新字体材质球", m_NewMaterial, typeof(Material), false);
                if (m_NewMaterial == null)
                {
                    EditorGUILayout.EndVertical();
                    return;
                }
                EditorGUILayout.Space(5);
                if (GUILayout.Button("替换列表中所有TMProText的材质球"))
                {
                    List<TextMeshProUGUI> tempList = new List<TextMeshProUGUI>(64);
                    tempList.AddRange(m_AllDragTMproTextList);
                    tempList.AddRange(m_AllFoldTMproTextList);
                    ChangeTMProTextMat(tempList, m_NewMaterial);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            else if (m_ToolsTypeIndex == (int)ToolsType.ChangeTxtToTMPro)
            {
                if (GUILayout.Button("批量修改Text为TMProText"))
                {
                    List<Text> tempList = new List<Text>(64);
                    tempList.AddRange(m_AllDragTextList);
                    tempList.AddRange(m_AllFoldTextList);
                    foreach (var item in tempList)
                    {
                        Utility.ChangeTextToTMProText(item.gameObject);
                    }
                    Debug.Log("替换成功。替换个数为 : " + tempList.Count);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Clean();
                }
            }
            else
            {
                EditorGUILayout.EndVertical();
                return;
            }
            if (GUILayout.Button("清空当前数据"))
            {
                Clean();
            }
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            DrawFoldSelector(m_FoldList);
            EditorGUILayout.Space(5);
            switch (m_ToolsTypeIndex)
            {
                case (int)ToolsType.ChangeFont:
                    DrawFoldObjectArea<TextMeshProUGUI, GameObject>("检索文件夹中的TMProText组件", "t:prefab",
                        m_FoldList.ToArray(), m_AllFoldTMproTextList, (list) =>
                     {
                         m_AllFoldTMproTextList = CollectTextMeshProUGUI(list, TMProTextFontSelector);
                     });
                    break;
                case (int)ToolsType.ChangeMat:
                    DrawFoldObjectArea<TextMeshProUGUI, GameObject>("检索文件夹中的TMProText组件", "t:prefab",
                        m_FoldList.ToArray(), m_AllFoldTMproTextList, (list) =>
                     {
                         m_AllFoldTMproTextList = CollectTextMeshProUGUI(list, TMProTextMatSelector);
                     });
                    break;
                case (int)ToolsType.ChangeTxtToTMPro:
                    DrawFoldObjectArea<Text, GameObject>("检索文件夹中的Text组件", "t:prefab",
                        m_FoldList.ToArray(), m_AllFoldTextList, (list) =>
                        {
                            m_AllFoldTextList = CollectText(list, TextSelector);
                        });
                    break;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            DrawGameObjectSelector(m_AllDragObjectList);
            EditorGUILayout.Space(5);
            switch (m_ToolsTypeIndex)
            {
                case (int)ToolsType.ChangeFont:
                    DrawGameObjectSelectorArea<TextMeshProUGUI>("检索文件夹中的TMProText组件", "t:prefab",
                        m_AllDragObjectList, m_AllDragTMproTextList, (list) =>
                        {
                            m_AllDragTMproTextList = CollectTextMeshProUGUI(list, TMProTextFontSelector);
                        });
                    break;
                case (int)ToolsType.ChangeMat:
                    DrawGameObjectSelectorArea<TextMeshProUGUI>("检索文件夹中的TMProText组件", "t:prefab",
                        m_AllDragObjectList, m_AllDragTMproTextList, (list) =>
                        {
                            m_AllDragTMproTextList = CollectTextMeshProUGUI(list, TMProTextMatSelector);
                        });
                    break;
                case (int)ToolsType.ChangeTxtToTMPro:
                    DrawGameObjectSelectorArea<Text>("检索文件夹中的Text组件", "t:prefab",
                        m_AllDragObjectList, m_AllDragTextList, (list) =>
                        {
                            m_AllDragTextList = CollectText(list, TextSelector);
                        });
                    break;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }


        bool TMProTextFontSelector(TextMeshProUGUI text)
        {
            return text.font == m_OldFontAsset;
        }

        bool TMProTextMatSelector(TextMeshProUGUI text)
        {
            return text.font.material.mainTexture == m_NewMaterial.mainTexture && text.fontSharedMaterial != m_NewMaterial;
        }

        bool TextSelector(Text text)
        {
            return text.material.shader.name == "UI/Default";
        }

        List<TextMeshProUGUI> CollectTextMeshProUGUI(List<GameObject> objectList, Predicate<TextMeshProUGUI> predicate)
        {
            List<TextMeshProUGUI> tempList = new List<TextMeshProUGUI>(64);
            for (int i = 0; i < objectList.Count; i++)
            {
                var texts = objectList[i].GetComponentsInChildren<TextMeshProUGUI>();
                for (int j = 0; j < texts.Length; j++)
                {
                    if (texts[j].font == null || predicate == null || predicate(texts[j]))
                    {
                        tempList.Add(texts[j]);
                    }
                }
            }
            return tempList;
        }

        void CollectTextMeshProUGUI(GameObject gameObject, List<TextMeshProUGUI> resultList, Predicate<TextMeshProUGUI> predicate)
        {
            var texts = gameObject.GetComponentsInChildren<TextMeshProUGUI>();
            for (int j = 0; j < texts.Length; j++)
            {
                if (texts[j].font == null || predicate == null || predicate(texts[j]))
                {
                    resultList.Add(texts[j]);
                }
            }
        }

        List<Text> CollectText(List<GameObject> objectList, Predicate<Text> predicate)
        {
            List<Text> tempList = new List<Text>(64);
            for (int i = 0; i < objectList.Count; i++)
            {
                var texts = objectList[i].GetComponentsInChildren<Text>();
                for (int j = 0; j < texts.Length; j++)
                {
                    if (texts[j].font == null || predicate == null || predicate(texts[j]))
                    {
                        tempList.Add(texts[j]);
                    }
                }
            }
            return tempList;
        }

        /// <summary>
        /// 替换选中的列表中的TMProText组件的字体。
        /// </summary>
        /// <param name="TMProUGUIList"></param>
        /// <param name="oldFont"></param>
        /// <param name="newFont"></param>
        static void ChangeTMProTextFont(List<TextMeshProUGUI> TMProUGUIList, TMP_FontAsset oldFont, TMP_FontAsset newFont)
        {
            int count = 0;
            for (int i = 0; i < TMProUGUIList.Count; i++)
            {
                Material fontMat = TMProUGUIList[i].fontSharedMaterial;
                TMProUGUIList[i].font = newFont;
                if (fontMat != oldFont.material)
                {
                    fontMat = CopyAndCreateFontAssetMat(fontMat, oldFont, newFont);
                    if (fontMat != null)
                    {
                        TMProUGUIList[i].fontMaterial = fontMat;

                    }
                }
                count++;
                EditorUtility.SetDirty(TMProUGUIList[i].gameObject);
            }
            Debug.Log("替换成功。替换个数为 : " + count);
        }

        /// <summary>
        /// 批量替换TMProText的材质球。
        /// </summary>
        /// <param name="TMProUGUIList"></param>
        /// <param name="material"></param>
        static void ChangeTMProTextMat(List<TextMeshProUGUI> TMProUGUIList, Material material)
        {
            int count = 0;
            for (int i = 0; i < TMProUGUIList.Count; i++)
            {
                Material fontMat = TMProUGUIList[i].font.material;
                if (material.mainTexture == fontMat.mainTexture)
                {
                    TMProUGUIList[i].fontMaterial = material;
                    EditorUtility.SetDirty(TMProUGUIList[i].gameObject);
                    count++;
                }
            }
            Debug.Log("替换成功。替换个数为 : " + count);
        }

        /// <summary>
        /// 复制创建材质球。
        /// </summary>
        /// <param name="old"></param>
        /// <param name="oldFont"></param>
        /// <param name="newFont"></param>
        /// <returns></returns>
        static Material CopyAndCreateFontAssetMat(Material old, TMP_FontAsset oldFont, TMP_FontAsset newFont)
        {
            string name = old.name.Replace(oldFont.name, newFont.name);
            string path = Utility.AssetPathToAssetFold(AssetDatabase.GetAssetPath(newFont.instanceID)) + "/" + name + ".mat";
            if (File.Exists(path))
            {
                //如果已经存在，则使用存在的材质球。
                return AssetDatabase.LoadAssetAtPath<Material>(path);
            }
            bool isCreate = EditorUtility.DisplayDialog("是否创建新的材质球。", "新字体不存在与组件当前相同的材质球，是否选择拷贝并创建?", "确定", "取消");
            if (isCreate)
            {
                Material material = new Material(old);
                material.mainTexture = newFont.material.mainTexture;
                material.name = name;
                AssetDatabase.CreateAsset(material, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("创建了新的材质球，路径为 : " + path);
                return material;
            }
            return null;
        }

        public TMP_FontAsset DrawFontAsset(TMP_FontAsset fontAsset, string name, ref string fontAssetPath)
        {
            TMP_FontAsset old = fontAsset;
            fontAsset = (TMP_FontAsset)EditorGUILayout.ObjectField(name, fontAsset, typeof(TMP_FontAsset), false);
            if (fontAsset != null)
            {
                if (fontAsset != old)
                {
                    fontAssetPath = Utility.AssetPathToAssetFold(AssetDatabase.GetAssetPath(fontAsset.instanceID));
                }
            }
            else
            {
                fontAssetPath = null;
            }
            if (!string.IsNullOrEmpty(fontAssetPath))
            {
                EditorGUILayout.LabelField(fontAssetPath, EditorStyles.textField);
            }
            return fontAsset;
        }


    }
}
