using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace KakuEditorTools
{
    public static partial class UIMenuReplace
    {
        static Dictionary<string, Assembly> s_AssemblyCache = new Dictionary<string, Assembly>();

        [MenuItem("GameObject/UI/NoRayImage", false, 2001)]
        public static void CreateImage(MenuCommand menuCommand)
        {
            Type menuOptionsType = Utility.GetType("UnityEditor.UI", "UnityEditor.UI.MenuOptions");
            menuOptionsType.InvokeMember("AddImage",
                BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public,
                null, null, new object[] { menuCommand });
            GameObject go = Selection.activeGameObject;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.GetComponent<Image>().raycastTarget = false;
        }

        /// <summary>
        /// Create a TextMeshPro object that works with the CanvasRenderer
        /// </summary>
        /// <param name="command"></param>
        [MenuItem("GameObject/UI/NoRayTextMeshPro", false, 2001)]
        public static void CreateTextMeshProGuiObjectPerform(MenuCommand menuCommand)
        {
            Type t = typeof(TMPro.EditorUtilities.TMPro_CreateObjectMenu);
            t.InvokeMember("CreateTextMeshProGuiObjectPerform",
                BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic,
                null, null, new object[] { menuCommand });
            TextMeshProUGUI text = Selection.activeGameObject.GetComponent<TextMeshProUGUI>();
            text.raycastTarget = false;
            text.horizontalAlignment = HorizontalAlignmentOptions.Center;
            text.verticalAlignment = VerticalAlignmentOptions.Middle;
        }
    }
}
