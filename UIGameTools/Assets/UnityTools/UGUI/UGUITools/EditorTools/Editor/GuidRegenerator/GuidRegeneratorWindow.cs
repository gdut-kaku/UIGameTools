using UnityEngine;
using UnityEditor;
using System.IO;

namespace KakuEditorTools
{
    [WindowBaseAttribute("拷贝文件夹并重定向GUID窗口", true)]
    public class GuidRegeneratorWindow : WindowEditorBase<GuidRegeneratorWindow>
    {
        [MenuItem("UGUITools/其他/拷贝工具")]
        static void OpenWindow()
        {
            Init();
        }

        string sourcePath = "";
        string targetPath = "";
        int CheckCount = 3;

        protected override void OnGUI()
        {
            base.OnGUI();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("选择源文件夹");
            EditorGUILayout.BeginHorizontal();
            sourcePath = EditorGUILayout.TextField(sourcePath);
            if (GUILayout.Button("选择文件夹", GUILayout.Width(150)))
            {
                string result = EditorUtility.OpenFolderPanel("Select assetfold to change guid.", "Assets", "");
                if (result.Contains(Application.dataPath))
                {
                    sourcePath = Utility.FixAssetPathSeparatorChar(result);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("选择目标文件夹");
            EditorGUILayout.BeginHorizontal();
            targetPath = EditorGUILayout.TextField(targetPath);
            if (GUILayout.Button("选择文件夹", GUILayout.Width(150)))
            {
                string result = EditorUtility.OpenFolderPanel("Select assetfold to change guid.", "Assets", "");
                if (result.Contains(Application.dataPath))
                {
                    targetPath = Utility.FixAssetPathSeparatorChar(result);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("开始拷贝"))
            {
                if (!string.IsNullOrEmpty(sourcePath) && !string.IsNullOrEmpty(targetPath))
                {
                    CheckCount--;
                    if (CheckCount <= 0)
                    {
                        CopyAndRegeneratorAsset(sourcePath, targetPath);
                        ResetState();
                    }
                }
            }
            EditorGUILayout.TextField($"请确认路径是否正确。仍需确认次数：{CheckCount.ToString()} 次。");
            EditorGUILayout.TextField($"请确认路径是否正确。仍需确认次数：{CheckCount.ToString()} 次。");
            EditorGUILayout.TextField($"请确认路径是否正确。仍需确认次数：{CheckCount.ToString()} 次。");
            EditorGUILayout.TextField($"请确认路径是否正确。仍需确认次数：{CheckCount.ToString()} 次。");
            EditorGUILayout.TextField($"请确认路径是否正确。仍需确认次数：{CheckCount.ToString()} 次。");
            EditorGUILayout.TextField($"请确认路径是否正确。仍需确认次数：{CheckCount.ToString()} 次。");
            EditorGUILayout.EndVertical();
        }

        private void ResetState()
        {
            sourcePath = "";
            targetPath = "";
            CheckCount = 0;
        }

        private void CopyAndRegeneratorAsset(string sourcePath, string targetPath)
        {
            string[] files = Directory.GetFiles(targetPath);
            if (files.Length > 0)
            {
                Debug.LogError("指向的文件夹不是空文件夹，请重新选择目标文件夹。");
                return;
            }
            string[] dirs = Directory.GetDirectories(targetPath);
            if (dirs.Length > 0)
            {
                Debug.LogError("指向的文件夹不是空文件夹，请重新选择目标文件夹。");
                return;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(Application.dataPath);
            DirectoryInfo rootInfo = directoryInfo.Parent.Parent;
            string tempPath = rootInfo.FullName + "\\zbyTemp";
            tempPath = Utility.FixAssetPathSeparatorChar(tempPath);
            Utility.CopyDirectory(sourcePath, tempPath);
            //
            UnityGuidRegeneratorMenu.RegenerateGuids(tempPath);
            Utility.MoveDirectory(tempPath, targetPath);
            AssetDatabase.Refresh();
        }

    }
}