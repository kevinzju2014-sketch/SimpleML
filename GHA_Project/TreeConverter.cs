using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace SimpleML.Core
{
    /// <summary>
    /// Tree转换工具类
    /// 用于将JSON格式的数据转换为Grasshopper Tree结构
    /// </summary>
    public static class TreeConverter
    {
        /// <summary>
        /// 将JSON格式的2D数组（列表的列表）转换为Tree结构
        /// 格式: [["value1", "value2"], ["value3", "value4"], ...]
        /// 每行数据作为一个分支
        /// </summary>
        public static GH_Structure<GH_String> ConvertJsonToTree(string jsonData)
        {
            GH_Structure<GH_String> tree = new GH_Structure<GH_String>();
            
            try
            {
                if (string.IsNullOrEmpty(jsonData))
                    return tree;
                
                // 移除首尾的方括号和空白
                string trimmed = jsonData.Trim();
                if (trimmed.StartsWith("["))
                    trimmed = trimmed.Substring(1);
                if (trimmed.EndsWith("]"))
                    trimmed = trimmed.Substring(0, trimmed.Length - 1);
                trimmed = trimmed.Trim();
                
                if (string.IsNullOrEmpty(trimmed))
                    return tree;
                
                // 分割行（每行是一个列表）
                List<string> rows = ParseJsonRows(trimmed);
                
                // 解析每一行
                for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                {
                    string rowStr = rows[rowIndex].Trim();
                    if (rowStr.StartsWith("["))
                        rowStr = rowStr.Substring(1);
                    if (rowStr.EndsWith("]"))
                        rowStr = rowStr.Substring(0, rowStr.Length - 1);
                    
                    GH_Path path = new GH_Path(rowIndex);
                    
                    // 分割列值
                    List<string> values = ParseJsonArray(rowStr);
                    foreach (string value in values)
                    {
                        // 移除引号和转义字符
                        string cleanValue = UnescapeJsonString(value);
                        tree.Append(new GH_String(cleanValue), path);
                    }
                }
            }
            catch
            {
                // 如果解析失败，返回空Tree
            }
            
            return tree;
        }

        /// <summary>
        /// 将JSON格式的1D数组转换为Tree结构（单分支）
        /// 格式: ["value1", "value2", "value3", ...]
        /// </summary>
        public static GH_Structure<GH_String> ConvertJsonArrayToTree(string jsonData)
        {
            GH_Structure<GH_String> tree = new GH_Structure<GH_String>();
            
            try
            {
                if (string.IsNullOrEmpty(jsonData))
                    return tree;
                
                string trimmed = jsonData.Trim();
                if (trimmed.StartsWith("["))
                    trimmed = trimmed.Substring(1);
                if (trimmed.EndsWith("]"))
                    trimmed = trimmed.Substring(0, trimmed.Length - 1);
                trimmed = trimmed.Trim();
                
                if (string.IsNullOrEmpty(trimmed))
                    return tree;
                
                GH_Path path = new GH_Path(0);
                List<string> values = ParseJsonArray(trimmed);
                
                foreach (string value in values)
                {
                    string cleanValue = UnescapeJsonString(value);
                    tree.Append(new GH_String(cleanValue), path);
                }
            }
            catch
            {
                // 如果解析失败，返回空Tree
            }
            
            return tree;
        }

        /// <summary>
        /// 解析JSON行数组
        /// </summary>
        private static List<string> ParseJsonRows(string jsonData)
        {
            List<string> rows = new List<string>();
            int bracketCount = 0;
            bool inString = false;
            bool escapeNext = false;
            int startIndex = 0;
            
            for (int i = 0; i < jsonData.Length; i++)
            {
                if (escapeNext)
                {
                    escapeNext = false;
                    continue;
                }
                
                char c = jsonData[i];
                
                if (c == '\\' && inString)
                {
                    escapeNext = true;
                    continue;
                }
                
                if (c == '"')
                {
                    inString = !inString;
                    continue;
                }
                
                if (!inString)
                {
                    if (c == '[')
                        bracketCount++;
                    else if (c == ']')
                        bracketCount--;
                    else if (bracketCount == 0 && c == ',')
                    {
                        string row = jsonData.Substring(startIndex, i - startIndex).Trim();
                        if (!string.IsNullOrEmpty(row))
                            rows.Add(row);
                        startIndex = i + 1;
                    }
                }
            }
            
            // 添加最后一行
            if (startIndex < jsonData.Length)
            {
                string row = jsonData.Substring(startIndex).Trim();
                if (!string.IsNullOrEmpty(row))
                    rows.Add(row);
            }
            
            return rows;
        }

        /// <summary>
        /// 解析JSON数组字符串
        /// </summary>
        private static List<string> ParseJsonArray(string jsonArray)
        {
            List<string> values = new List<string>();
            if (string.IsNullOrEmpty(jsonArray))
                return values;
            
            bool inString = false;
            bool escapeNext = false;
            int startIndex = 0;
            
            for (int i = 0; i < jsonArray.Length; i++)
            {
                if (escapeNext)
                {
                    escapeNext = false;
                    continue;
                }
                
                char c = jsonArray[i];
                
                if (c == '\\' && inString)
                {
                    escapeNext = true;
                    continue;
                }
                
                if (c == '"')
                {
                    inString = !inString;
                    continue;
                }
                
                if (!inString && c == ',')
                {
                    string value = jsonArray.Substring(startIndex, i - startIndex).Trim();
                    if (!string.IsNullOrEmpty(value))
                        values.Add(value);
                    startIndex = i + 1;
                }
            }
            
            // 添加最后一个值
            if (startIndex < jsonArray.Length)
            {
                string value = jsonArray.Substring(startIndex).Trim();
                if (!string.IsNullOrEmpty(value))
                    values.Add(value);
            }
            
            return values;
        }

        /// <summary>
        /// 解析JSON字符串值（移除引号和转义字符）
        /// </summary>
        private static string UnescapeJsonString(string jsonValue)
        {
            if (string.IsNullOrEmpty(jsonValue))
                return "";
            
            string trimmed = jsonValue.Trim();
            
            // 移除首尾引号
            if (trimmed.StartsWith("\""))
                trimmed = trimmed.Substring(1);
            if (trimmed.EndsWith("\""))
                trimmed = trimmed.Substring(0, trimmed.Length - 1);
            
            // 处理null值
            if (trimmed == "null" || trimmed == "None")
                return "";
            
            // 处理转义字符
            trimmed = trimmed.Replace("\\\"", "\"")
                            .Replace("\\n", "\n")
                            .Replace("\\r", "\r")
                            .Replace("\\t", "\t")
                            .Replace("\\\\", "\\");
            
            return trimmed;
        }
    }
}
