using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;
namespace AssetsTools
{
    /// <summary>
    /// 去除 Lua 等以 -- 和 --[[]] --[==[]==] 作为注释标签的文本的注释。
    /// </summary>
    public static class AutoRemoveLuaComments
    {
        [MenuItem("Tools/Lua")]
        public static void Test()
        {
            RemoveLuaComment("Assets/test_1", "Assets/test_2", (path) =>
            {
                return path.EndsWith(".lua");
            });
        }

        static bool KeepRNLine = false; //是否保留注释行中的的换行符。

        private static void RemoveLuaComment(string sourceDir, string targetDir, Func<string, bool> CheckFileName)
        {
            sourceDir = sourceDir.Replace("/", "\\").Replace('\\', Path.DirectorySeparatorChar);
            targetDir = targetDir.Replace("/", "\\").Replace('\\', Path.DirectorySeparatorChar); ;

            string[] files = Directory.GetFiles(sourceDir, "*", searchOption: SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                if (!CheckFileName(files[i]))
                {
                    continue;
                }
                string output = RemoveLuaStringComment(File.ReadAllText(files[i]));

                string fileName = files[i].Replace("/", "\\").Replace('\\', Path.DirectorySeparatorChar);
                string outputPath = fileName.Replace(sourceDir, targetDir);
                string dirPath = outputPath.Substring(0, outputPath.LastIndexOf(Path.DirectorySeparatorChar));
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                File.WriteAllText(outputPath, output);
            }
            Debug.Log("Finish.");
        }

        static readonly StringBuilder s_outputBuilder = new StringBuilder(); //存放一定不是字符串的
        static readonly StringBuilder s_tempBuilder = new StringBuilder();  //存放那些可能不是注释的字符串

        /// <summary>
        /// 移除Lua文件文本的注释。
        /// </summary>
        /// <param name="inputstr">Lua文件文本串。</param>
        /// <returns>返回移除了注释后的Lua文件文本串。</returns>
        public static string RemoveLuaStringComment(string inputstr)
        {
            s_outputBuilder.Clear(); //存放一定不是字符串的
            s_tempBuilder.Clear();//存放那些可能不是注释的字符串

            State state = State.NormalTextField;
            int blockType2Length = 0; // 记录 "--[=[ ]=]"组合的等号数量。
            bool keepRN = false; // 判断单行注释是否应该保留换行符。

            for (int i = 0; i < inputstr.Length; i++)
            {
                char c = inputstr[i];
                switch (state)
                {
                    case State.NormalTextField:
                        if (c == '"')
                        {
                            state = State.QuoteField;
                            s_outputBuilder.Append(c);
                        }
                        else if (c == '-')
                        {
                            state = State.Cm_CheckStep1;
                            s_tempBuilder.Append(c);
                        }
                        else
                        {
                            state = State.NormalTextField;
                            s_outputBuilder.Append(c);
                        }
                        break;
                    case State.Cm_CheckStep1:
                        if (c == '-')
                        {
                            //--必然是注释，进入下一步。
                            state = State.Cm_CheckStep2;
                        }
                        else
                        {
                            //非注释，转回NormalTextField
                            state = State.NormalTextField;
                            s_tempBuilder.Append(c);
                            s_outputBuilder.Append(s_tempBuilder);
                        }
                        s_tempBuilder.Clear();
                        break;
                    case State.Cm_CheckStep2:
                        int orginalI = i;
                        if (c == '[')
                        {
                            state = State.SingleCommentField;
                            //识别组合
                            if (i + 1 < inputstr.Length)
                            {
                                if (inputstr[i + 1] == '[')
                                {
                                    //已知下一个是[，和当前的[组合成为了一个嵌套[[注释块。直接跳过下一个字符判断。
                                    i++;
                                    state = State.BlockCommentField;
                                }
                                else
                                {
                                    //识别[==[模式
                                    int tempNum = 0;
                                    while (inputstr[i + tempNum + 1] == '=')
                                    {
                                        tempNum++;
                                    }
                                    if (inputstr[i + tempNum + 1] == '[')
                                    {
                                        //形成--[===[情况，进入块区域种类2
                                        state = State.BlockCommentField_2;
                                        i = i + tempNum;
                                        blockType2Length = tempNum;
                                    }
                                }
                            }
                        }
                        else
                        {
                            state = State.SingleCommentField;
                        }
                        //
                        int tempI = orginalI - 3;
                        bool tempB;
                        while (true)
                        {
                            if (tempI < 0) { tempB = true; break; }
                            char cc = inputstr[tempI];
                            if ((cc == '\n'))
                            {
                                tempB = true;
                                int num = (orginalI - 3 - tempI);
                                if (num > 0) //移除前方的空格+换行符
                                    s_outputBuilder.Remove(s_outputBuilder.Length - num, num);
                                break;
                            }
                            else if ((cc == ' ') || (cc == '\t')) { }
                            else { tempB = false; break; }
                            tempI--;
                        }
                        keepRN = !tempB;
                        break;
                    case State.SingleCommentField:
                        //这里进入了是单行注释区域。直接读取直到换行符出现。
                        if (c == '\r')
                        {
                            state = State.NormalTextField;
                            i++;
                            if (keepRN || KeepRNLine)
                            {
                                s_outputBuilder.Append(c);
                                s_outputBuilder.Append(inputstr[i]);
                            }
                            keepRN = false;
                        }
                        else if (c == '\n')
                        {
                            state = State.NormalTextField;
                            if (keepRN || KeepRNLine)
                            {
                                s_outputBuilder.Append(c);
                            }
                            keepRN = false;
                        }
                        else
                        {
                            state = State.SingleCommentField;
                        }
                        break;
                    case State.BlockCommentField:
                        //这里已经是注释块区域了。类型1 --[[ ]]
                        if (c == '\r')
                        {
                            i++;
                            if (KeepRNLine)
                            {
                                s_outputBuilder.Append(c);
                                s_outputBuilder.Append(inputstr[i]);
                            }
                        }
                        else if (c == '\n')
                        {
                            if (KeepRNLine)
                            {
                                s_outputBuilder.Append(c);
                            }
                        }
                        else if (c == ']')
                        {
                            if (i + 1 < inputstr.Length && inputstr[i + 1] == ']')
                            {
                                //已知下一个是]，和当前的]组合成为了一个嵌套]]注释块。直接跳过下一个字符判断。
                                state = State.EndBlockCommentTag;
                            }
                        }
                        break;
                    case State.BlockCommentField_2:
                        //这里已经是注释块区域了。类型2 --[==[]==]
                        if (c == '\r')
                        {
                            i++;
                            if (KeepRNLine)
                            {
                                s_outputBuilder.Append(c);
                                s_outputBuilder.Append(inputstr[i]);
                            }
                        }
                        else if (c == '\n')
                        {
                            if (KeepRNLine)
                            {
                                s_outputBuilder.Append(c);
                            }
                        }
                        else if (c == ']')
                        {
                            //识别[==[模式
                            int tempNum = 0; //记录等号的数量。
                            while ((i + tempNum + 1) < inputstr.Length && inputstr[i + tempNum + 1] == '=')
                            {
                                tempNum++;
                            }
                            if ((i + tempNum + 1) < inputstr.Length && inputstr[i + tempNum + 1] == ']')
                            {
                                if (blockType2Length == tempNum)
                                {
                                    //匹配成功。
                                    blockType2Length = 0;
                                    i = i + tempNum;
                                    state = State.EndBlockCommentTag;
                                }
                            }
                        }
                        break;
                    case State.EndBlockCommentTag:
                        if (c == ']')
                        {
                            if (i + 1 < inputstr.Length && inputstr[i + 1] != ' ' && inputstr[i + 1] != '\r' && inputstr[i + 1] != '\n')
                            {
                                if (s_outputBuilder.Length > 0 && s_outputBuilder[s_outputBuilder.Length - 1] == '\n')
                                {
                                    //如果上一个字符是换行符的话，可以不增加空格。
                                }
                                else
                                {
                                    //这里增加一个空格，避免块注释移除后，两个不想关的文本会链接起来。
                                    //比如 " local num = 0--[[]]local num = 1 " 这种情况。
                                    s_outputBuilder.Append(' ');
                                }
                            }
                            if (!keepRN && !KeepRNLine)
                            {
                                if (inputstr[i + 1] == '\r')
                                {
                                    i += 2;
                                }
                                if (inputstr[i + 1] == '\n')
                                {
                                    i += 1;
                                }
                            }
                            keepRN = false;
                            //切换回常规文本区域。
                            state = State.NormalTextField;
                        }
                        else
                        {
                            Debug.LogError("EndBlockCommentTag 识别错误。");
                            state = State.BlockCommentField;
                        }
                        break;
                    case State.QuoteField:
                        //进入双引号区域。
                        s_outputBuilder.Append(c);
                        if (c == '"')
                        {
                            state = State.NormalTextField;
                        }
                        else if (c == '\\')
                        {
                            state = State.BackslashChar;
                        }
                        else
                        {
                            state = State.QuoteField;
                        }
                        break;
                    case State.BackslashChar:
                        s_outputBuilder.Append(c);
                        state = State.QuoteField;
                        break;
                    default:
                        break;
                }
            }
            return s_outputBuilder.ToString();
        }
        internal enum State
        {
            /// <summary>
            /// 常规输入区域。
            /// </summary>
            NormalTextField = 0,
            /// <summary>
            /// 遇到'-',开始检测是否是单行注释或者块注释。
            /// </summary>
            Cm_CheckStep1,
            /// <summary>
            /// 第二次遇到'-',确定进入是注释区域，开始检测是单行注释还是块注释。
            /// </summary>
            Cm_CheckStep2,
            /// <summary>
            /// 进入单行注释区域。
            /// </summary>
            SingleCommentField,
            /// <summary>
            /// 进入块注释区域。--[[]] 模式
            /// </summary>
            BlockCommentField,
            /// <summary>
            /// 进入块注释区域。--[==[模式
            /// </summary>
            BlockCommentField_2,
            /// <summary>
            /// 跳出块注释区域。
            /// </summary>
            EndBlockCommentTag,
            /// <summary>
            /// 双引号文本输入区域。
            /// </summary>
            QuoteField,
            /// <summary>
            /// \ 转义字符处理。
            /// </summary>
            BackslashChar,
        }
    }
}