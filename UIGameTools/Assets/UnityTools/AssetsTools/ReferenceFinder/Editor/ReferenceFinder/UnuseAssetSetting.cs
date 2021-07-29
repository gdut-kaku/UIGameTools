using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AssetsTools.ReferenceFinder
{
    public class UnuseAssetSetting : ScriptableObject
    {
        [SerializeField]
        [Header("搜索根目录")]
        public string baseRootPath = "Assets";
        [SerializeField]
        [Header("白名单目录")]
        public List<string> whitePaths = new List<string>();
        [SerializeField]
        [Header("白名单文件")]
        public List<string> whiteFilePaths = new List<string>();
        [SerializeField]
        [Header("白名单文件后缀名")]
        public List<string> whiteFileExtension = new List<string>();

        public static UnuseAssetSetting GetSetting()
        {
            if (!File.Exists("Assets/Editor/AssetsFinderUnuseWhitePath.asset"))
            {
                if (!Directory.Exists("Assets/Editor"))
                {
                    Directory.CreateDirectory("Assets/Editor");
                }
                var so2 = ScriptableObject.CreateInstance(typeof(UnuseAssetSetting)) as UnuseAssetSetting;
                AssetDatabase.CreateAsset(so2, "Assets/Editor/AssetsFinderUnuseWhitePath.asset");
                return so2;
            }
            var so = AssetDatabase.LoadAssetAtPath<UnuseAssetSetting>("Assets/Editor/AssetsFinderUnuseWhitePath.asset");
            return so;
        }

        [MenuItem("AssetsTools/AssetFinder/CreateUnuseAssetSetting")]
        public static void CreateSetting()
        {
            GetSetting();
        }
    }
}