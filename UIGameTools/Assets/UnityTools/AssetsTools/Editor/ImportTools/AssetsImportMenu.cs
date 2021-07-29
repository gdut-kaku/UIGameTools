using UnityEditor;
using UnityEngine;

namespace AssetsTools
{
    public static class AssetsImportMenu
    {

        [MenuItem("AssetsTools/Import/AudioReset")]
        public static void ResetAllAudio()
        {
            string[] audioGuid = AssetDatabase.FindAssets("t:audioclip");
            foreach (var guid in audioGuid)
            {
                AssetImporter assetImporter = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
                assetImporter.SaveAndReimport();
            }
        }

    }
}


