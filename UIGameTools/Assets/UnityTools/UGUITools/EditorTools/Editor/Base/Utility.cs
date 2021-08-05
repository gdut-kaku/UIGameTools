using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System;
using TMPro;

namespace KakuEditorTools
{
    public static partial class Utility
    {
        /// <summary>
        /// 获取物体的Hierarchy深度。
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static int GetHierarchyDepth(Transform transform)
        {
            int count = 0;
            Transform tf = transform;
            while (tf.parent != null)
            {
                tf = tf.parent;
                count++;
            }
            return count;
        }

        /// <summary>
        /// 将Text组件切换成TMPro组件。
        /// </summary>
        /// <param name="o"></param>
        public static void ChangeTextToTMProText(GameObject o)
        {
            GameObject goObject = o;
            if (PrefabUtility.IsPartOfPrefabAsset(o))
            {
                goObject = GameObject.Instantiate(o);
            }
            var text = goObject.GetComponent<UnityEngine.UI.Text>();
            if (text == null)
                return;

            var textTmpText = text.text;
            var textTmpFontSize = text.fontSize;
            var textTmpSize = text.rectTransform.sizeDelta;
            var textTmpAlign = text.alignment;

            GameObject.DestroyImmediate(text);

            TextMeshProUGUI textTMP = goObject.gameObject.AddComponent<TextMeshProUGUI>();
            textTMP.text = textTmpText;
            textTMP.fontSize = textTmpFontSize;
            textTMP.rectTransform.sizeDelta = textTmpSize;

            switch (textTmpAlign)
            {
                case TextAnchor.UpperLeft:
                    textTMP.alignment = TextAlignmentOptions.MidlineLeft;
                    break;
                case TextAnchor.UpperCenter:
                    textTMP.alignment = TextAlignmentOptions.Top;
                    break;
                case TextAnchor.UpperRight:
                    textTMP.alignment = TextAlignmentOptions.TopRight;
                    break;
                case TextAnchor.MiddleLeft:
                    textTMP.alignment = TextAlignmentOptions.MidlineLeft;
                    break;
                case TextAnchor.MiddleCenter:
                    textTMP.alignment = TextAlignmentOptions.Center;
                    break;
                case TextAnchor.MiddleRight:
                    textTMP.alignment = TextAlignmentOptions.MidlineRight;
                    break;
                case TextAnchor.LowerLeft:
                    textTMP.alignment = TextAlignmentOptions.BottomLeft;
                    break;
                case TextAnchor.LowerCenter:
                    textTMP.alignment = TextAlignmentOptions.Bottom;
                    break;
                case TextAnchor.LowerRight:
                    textTMP.alignment = TextAlignmentOptions.BottomRight;
                    break;
            }
            if (PrefabUtility.IsPartOfPrefabAsset(o))
            {
                string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(o);
                PrefabUtility.SaveAsPrefabAsset(goObject, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                GameObject.DestroyImmediate(goObject);
            }
            else
            {
                EditorUtility.SetDirty(o.gameObject);
            }

        }


        static Dictionary<string, Assembly> s_AssemblyCache = new Dictionary<string, Assembly>();

        public static Assembly GetAssembly(string name)
        {
            if (!s_AssemblyCache.ContainsKey(name))
            {
                s_AssemblyCache.Add(name, Assembly.Load(name));
            }
            Assembly assembly = s_AssemblyCache[name];
            return assembly;
        }

        public static Type GetType(string assemblyName, string fullClassName)
        {
            Assembly assembly = GetAssembly(assemblyName);
            return assembly.GetType(fullClassName);
        }



    }
}