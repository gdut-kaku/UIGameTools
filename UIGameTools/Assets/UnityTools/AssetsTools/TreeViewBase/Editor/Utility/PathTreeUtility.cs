using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace UnityEditor.TreeViewExamples
{
    public class PathTreeElement : TreeElement
    {
        public bool isFold;
        public string assetPath;
        public string displayName;

        public PathTreeElement() : base()
        {

        }

        public PathTreeElement(bool isFold, string assetPath, string displayName, int id, int depth) : base(displayName, depth, id)
        {
            this.isFold = isFold;
            this.assetPath = assetPath;
            this.displayName = displayName;
        }
    }

    public class PathTreeUtility
    {
        public static IList<T> CreateAssetTreeByAssetPaths<T>(string rootPath, List<string> assetpaths, Predicate<string> predicate) where T : PathTreeElement, new()
        {
            //限定根目录文件夹
            string startwith = rootPath.Replace("\\", "/");
            int id = 1;
            int depthGap = Regex.Matches(startwith, "/").Count + 1;
            Dictionary<int, List<T>> depthToList = new Dictionary<int, List<T>>();
            var root = new T
            {
                isFold = true,
                assetPath = startwith,
                displayName = startwith,
                id = id,
                depth = -1
            };

            depthToList.Add(-1, new List<T>() { root });
            //收集资源文件夹。
            foreach (string path in assetpaths)
            {
                string validPath = path.Replace("\\", "/");
                if (predicate != null)
                {
                    if (!predicate(validPath))
                    {
                        continue;
                    }
                }
                int depth = Regex.Matches(validPath, "/").Count - depthGap;
                CreatePathNodeRecursive(ref id, validPath, depth, depthToList);
            }
            List<T> rs = new List<T>();
            TreeElementUtility.TreeToList(root, rs);
            return rs;
        }


        /// <summary>
        /// 尝试查找一个节点，如果不存在，则创建一个节点，并尝试查找创建其父节点。
        /// </summary>
        /// <returns></returns>
        public static T CreatePathNodeRecursive<T>(ref int id, string path, int depth, Dictionary<int, List<T>> depthToList) where T : PathTreeElement, new()
        {
            if (!depthToList.ContainsKey(-1))
            {
                throw new InvalidDataException("字典中没有包含 depth = -1 的节点.");
            }
            if (!depthToList.TryGetValue(depth, out List<T> list))
            {
                list = new List<T>();
                depthToList.Add(depth, list);
            }
            var parentList = list.Where(x => x.assetPath == path).ToList();
            int hasElemnent = parentList.Count;
            if (hasElemnent == 0)
            {
                id++;
                bool isFold = !Path.HasExtension(path);
                int tempI = path.LastIndexOf("/");
                string displayName = tempI >= 0 ? path.Substring(tempI + 1) : path;
                var item = new T
                {
                    isFold = isFold,
                    assetPath = path,
                    displayName = displayName,
                    id = id,
                    depth = depth
                };
                if (depth >= -1)
                {
                    //尝试查找父节点。
                    var parent = CreatePathNodeRecursive(ref id, path.Substring(0, path.LastIndexOf("/")), depth - 1, depthToList);
                    item.parent = parent;
                    if (parent.children == null)
                    {
                        parent.children = new List<TreeElement>();
                    }
                    parent.children.Add(item);
                }
                depthToList[depth].Add(item);
                return item;
            }
            else if (hasElemnent > 1)
            {
                throw new System.Exception(" error! ");
            }
            else
            {
                return parentList[0];
            }
        }

    }
}
