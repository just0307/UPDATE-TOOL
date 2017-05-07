/********************************************************************/
/*  file name    : GlobalUtil.cs                                    */
/*  function     : 公共函数                                         */
/*  date/version : 2015/11/24/v1.0                                  */
/*  author       : wuchen                                           */
/********************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Input;
using InToolSet.Sys;

namespace InToolSet.Util
{
    /// <summary>
    /// 共通函数
    /// </summary>
    public class GlobalUtil
    {
        /// <summary>
        /// 文件是否存在的判定
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>true：存在/false:不存在</returns>
        public static Boolean FileExist(string path)
        {
            FileInfo file = new FileInfo(path);
            if (!file.Exists)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 文件夹是否存在的判定
        /// </summary>
        /// <param name="path">文件夹路径</param>
        /// <returns>true：存在/false:不存在</returns>
        public static Boolean DirectoryExist(string path)
        {
            if (!Directory.Exists(path))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 判断目标是否是文件夹
        /// </summary>
        /// <param name="filepath">文件名</param>
        /// <returns>true : 文件夹、false：文件</returns>
        public static Boolean IsDirectory(string filepath)
        {
            FileInfo fi = new FileInfo(filepath);
            if (0 != (fi.Attributes & FileAttributes.Directory))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断目标是否是文件
        /// </summary>
        /// <param name="filepath">文件名</param>
        /// <returns>true : 文件、false：文件夹</returns>
        public static Boolean IsFile(string filepath)
        {
            FileInfo fi = new FileInfo(filepath);
            if (0 == (fi.Attributes & FileAttributes.Directory))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 查找指定目录下后缀是：
        /// </summary>
        /// <param name="path">指定路径</param>
        /// <param name="extension">扩展名,如:xml</param>
        /// <returns>文件目录</returns>
        public static FileInfo[] SelectFile(string path, string extension)
        {
            if (!IsDirectory(path))
            {
                return null;
            }
            DirectoryInfo directory = new DirectoryInfo(path);
            FileInfo[] files = directory.GetFiles("*." + extension);

            return files;
        }

        /// <summary>
        /// 递归复制文件夹
        /// </summary>
        /// <param name="from">元文件夹</param>
        /// <param name="to">目标文件夹</param>
        public static void CopyFolder(string from, string to)
        {
            foreach (string sub in Directory.GetDirectories(from))
            {
                CopyFolder(sub + "\\", to + Path.GetFileName(sub) + "\\");        // 文件        
            }
            CopyFilesInFolder(from, to);
        }

        /// <summary>
        /// 复制文件夹下各文件
        /// </summary>
        /// <param name="from">元文件夹</param>
        /// <param name="to">目标文件夹</param>
        public static void CopyFilesInFolder(string from, string to)
        {
            try
            {
                if (!Directory.Exists(to))
                {
                    DirectoryInfo info = Directory.CreateDirectory(to);        // 子文件夹
                    info.Attributes = FileAttributes.Normal & FileAttributes.Directory;
                }
                foreach (string file in Directory.GetFiles(from))
                {
                    if (File.Exists(to + Path.GetFileName(file)))
                    {
                        System.IO.File.SetAttributes(to + Path.GetFileName(file), System.IO.FileAttributes.Normal);
                    }
                    try
                    {
                        // 复制
                        File.Copy(file, to + "\\" + Path.GetFileName(file), true);
                    }
                    catch (IOException e)
                    {
                        Logging.Log.Warning(e);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Log.Warning(e);
            }
        }

        /// <summary>
        /// 从文件读取信息
        /// </summary>
        /// <param name="fileName">文件名</param>
        public static string ReadFile(string strFileName)
        {
            try
            {
                using (StreamReader sw = new StreamReader(strFileName))
                {
                    string str = sw.ReadToEnd();
                    sw.Close();
                    return str;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 判断日期是否为空
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>空/非空</returns>
        public static bool DateTimeIsEmpty(DateTime date)
        {
            if (date.Year < 1900)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 是否是数字键
        /// </summary>
        /// <param name="key">按键</param>
        /// <returns>是/否</returns>
        public static bool IsDigit(Key key)
        {
            bool shiftKey = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            bool retVal;
            //按住shift键后，数字键并不是数字键
            if (key >= Key.D0 && key <= Key.D9 && !shiftKey)
            {
                retVal = true;
            }
            else
            {
                retVal = key >= Key.NumPad0 && key <= Key.NumPad9;
            }
            return retVal;
        }


        /// <summary>
        /// 空字符串转换为指定字符串
        /// </summary>
        /// <param name="metaData">元数据</param>o
        /// <param name="target">目标数据</param>
        /// <returns>目标数据</returns>
        public static string NVLstring(string metaData, string target)
        {
            if (string.IsNullOrEmpty(metaData))
            {
                return target;
            }
            return metaData;
        }

        /// <summary>
        /// 判断输入是否为整数
        /// </summary>
        /// <param name="_string">输入文字</param>
        /// <returns>判断结果</returns>
        public static bool IsInteger(string _string)
        {
            if (string.IsNullOrEmpty(_string))
                return false;
            foreach (char c in _string)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 判断输入是否为整数或小数
        /// </summary>
        /// <param name="_string">输入文字</param>
        /// <returns>判断结果</returns>
        public static bool IsNumberic(string _string)
        {
            if (string.IsNullOrEmpty(_string))
                return false;
            Regex re = new Regex("^\\d+(\\.\\d+)?$");
            return re.IsMatch(_string);
        }

        /// <summary>
        /// 判断IP是否已经设定
        /// </summary>
        /// <param name="strIP"></param>
        /// <returns></returns>
        public static bool IPIsEmpty(string strIP)
        {
            if (string.IsNullOrEmpty(strIP) || (strIP == GlobalDeviceInfo.EMPTY_IP))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 比较两个IP地址的大小
        /// </summary>
        /// <param name="strIPv4Left"></param>
        /// <param name="strIPv4Right"></param>
        /// <returns></returns>
        public static int CompareIPv4(string strIPv4Left, string strIPv4Right)
        {
            return CommonUtil.CompareIPv4(strIPv4Left, strIPv4Right);
        }

        public static int GetIntValue(string strKey, string strValue)
        {
            return CommonUtil.GetIntValue(strKey, strValue);
        }
    }
}
