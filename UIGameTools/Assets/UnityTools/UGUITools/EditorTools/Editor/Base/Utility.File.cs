using System.IO;
using UnityEngine;

namespace KakuEditorTools
{
    public static partial class Utility
    {
        /// <summary>
        /// 修正路径的目录分隔符。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string FixAssetPathSeparatorChar(string path)
        {
            string tempPath = path.Replace('/', Path.DirectorySeparatorChar);
            tempPath = tempPath.Replace('\\', Path.DirectorySeparatorChar);
            return tempPath;
        }

        /// <summary>
        /// 绝对路径转相对路径。
        /// </summary>
        /// <param name="absPath">绝对路径。</param>
        /// <returns>返回相对路径。</returns>
        public static string AbsolutePathToRelativePath(string absPath)
        {
            return absPath.Replace(Application.dataPath, "Assets");
        }

        /// <summary>
        /// 资源路径转文件夹路径。
        /// </summary>
        /// <param name="assetPath">资源路径。</param>
        /// <returns>返回文件夹路径。</returns>
        public static string AssetPathToAssetFold(string assetPath)
        {
            return Path.GetDirectoryName(assetPath);
        }

        /// <summary>
        /// 复制文件夹下的所有内容。
        /// </summary>
        /// <param name="sourcePath">来源位置。</param>
        /// <param name="targetPath">目标位置。</param>
        public static void CopyDirectory(string sourcePath, string targetPath)
        {
            if (!Directory.Exists(sourcePath))
            {
                return;
            }
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            string[] filesPath = Directory.GetFiles(sourcePath);
            foreach (string p in filesPath)
            {
                File.Copy(p, p.Replace(sourcePath, targetPath), true);
            }
            string[] directorysPath = Directory.GetDirectories(sourcePath);
            foreach (string d in directorysPath)
            {
                CopyDirectory(d, d.Replace(sourcePath, targetPath));
            }
        }

        /// <summary>
        /// 移动文件夹下的所有内容。
        /// </summary>
        /// <param name="sourcePath">来源位置。</param>
        /// <param name="targetPath">目标位置。</param>
        public static void MoveDirectory(string sourcePath, string targetPath)
        {
            if (!Directory.Exists(sourcePath))
            {
                return;
            }
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            string[] filesPath = Directory.GetFiles(sourcePath);
            foreach (string p in filesPath)
            {
                File.Move(p, p.Replace(sourcePath, targetPath));
            }
            string[] directorysPath = Directory.GetDirectories(sourcePath);
            foreach (string d in directorysPath)
            {
                MoveDirectory(d, d.Replace(sourcePath, targetPath));
            }
            Directory.Delete(sourcePath);
        }
    }
}