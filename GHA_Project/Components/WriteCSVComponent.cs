using System;
using System.IO;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.DataInput
{
    /// <summary>
    /// Write CSV Component
    /// 写入CSV文件组件
    /// </summary>
    public class WriteCSVComponent : GH_Component
    {
        public WriteCSVComponent()
          : base("Write CSV", "WriteCSV",
              "将数据写入CSV文件",
              "SimpleML", "01 Input")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "要写入的数据（Tree结构）", GH_ParamAccess.tree);
            pManager.AddTextParameter("Filepath", "F", "保存路径（CSV文件路径）", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Index", "I", "是否写入行索引，默认False", GH_ParamAccess.item, false);
            pManager.AddTextParameter("Encoding", "E", "文件编码，默认'utf-8'", GH_ParamAccess.item, "utf-8");
            pManager.AddTextParameter("Separator", "S", "分隔符，默认','（逗号）", GH_ParamAccess.item, ",");
            pManager.AddBooleanParameter("Write", "W", "选择True时执行写入命令", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Saved Path", "SP", "保存的文件路径", GH_ParamAccess.item);
            pManager.AddTextParameter("Info", "I", "保存信息（行数、列数等）", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "组件使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> dataTree = new Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo>();
            string filepath = string.Empty;
            bool index = false;
            string encoding = "utf-8";
            string separator = ",";
            bool write = false;

            if (!DA.GetDataTree(0, out dataTree)) return;
            if (!DA.GetData(1, ref filepath)) return;
            DA.GetData(2, ref index);
            DA.GetData(3, ref encoding);
            DA.GetData(4, ref separator);
            DA.GetData(5, ref write);

            if (!write)
            {
                DA.SetData(0, "");
                DA.SetData(1, "请将Write设置为True以执行写入");
                DA.SetData(2, "组件使用说明：将Write设置为True以保存文件");
                return;
            }

            if (string.IsNullOrEmpty(filepath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "文件路径不能为空");
                return;
            }

            try
            {
                string mymlPath = GetMyMLPath();
                if (string.IsNullOrEmpty(mymlPath))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, 
                        "未找到myML文件夹。请设置SIMPLEML_PATH环境变量。");
                    return;
                }

                string dataList = ConvertTreeToPythonList(dataTree);
                string escapedFilepath = filepath.Replace("\\", "\\\\").Replace("'", "\\'");
                string escapedEncoding = encoding.Replace("'", "\\'").Replace("\"", "\\\"");
                string escapedSeparator = separator.Replace("'", "\\'").Replace("\"", "\\\"");

                string pythonCode = $@"
# -*- coding: utf-8 -*-
import sys
import os
import json
import site
import io

# 设置标准输出编码为UTF-8
if sys.stdout.encoding != 'utf-8':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
if sys.stderr.encoding != 'utf-8':
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

# 添加项目路径
sys.path.insert(0, r'{mymlPath}')

# 确保Rhino Python的site-packages在路径中
try:
    site_packages = site.getsitepackages()
    for sp in site_packages:
        if sp not in sys.path:
            sys.path.insert(0, sp)
    
    rhino_site_envs = r'C:\Users\Administrator\.rhinocode\py39-rh8\site-envs'
    if os.path.exists(rhino_site_envs):
        for item in os.listdir(rhino_site_envs):
            env_path = os.path.join(rhino_site_envs, item)
            if os.path.isdir(env_path):
                if env_path not in sys.path:
                    sys.path.insert(0, env_path)
                site_pkg = os.path.join(env_path, 'Lib', 'site-packages')
                if os.path.exists(site_pkg) and site_pkg not in sys.path:
                    sys.path.insert(0, site_pkg)
except Exception:
    pass

import pandas as pd
from components.file_io_components import write_csv

# 转换输入数据
data_list = {dataList}

# 写入CSV文件
saved_path, info = write_csv(
    data_list,
    r'{escapedFilepath}',
    index={index.ToString().ToLower()},
    encoding=r'{escapedEncoding}',
    sep=r'{escapedSeparator}'
)

print('OUTPUT_0:' + saved_path)
print('OUTPUT_1:' + info)
print('OUTPUT_2:将数据写入CSV文件')
";

                string output = PythonScriptExecutor.ExecuteCode(pythonCode);
                string savedPath = ExtractValue(output, "OUTPUT_0:");
                string info = ExtractValue(output, "OUTPUT_1:");

                DA.SetData(0, savedPath);
                DA.SetData(1, info);
                
                // Readme输出
                string readme = @"组件名称: Write CSV
功能: 将数据写入CSV文件

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. Data (数据) - Tree结构，必需
   • 数据类型: Grasshopper Tree结构
   • 数据结构: 
     - 每个分支代表一行数据
     - 分支内的元素是该行的列值
     - 分支路径为 {0}, {1}, {2}...（行索引）
   • 示例:
     {0} → [""5.1"", ""3.5"", ""1.4"", ""0.2"", ""setosa""]
     {1} → [""4.9"", ""3.0"", ""1.4"", ""0.2"", ""setosa""]
   • 连接建议:
     ← Read CSV/Excel的Data输出
     ← Create Dataset后Deconstruct Dataset的X输出
     ← Predict Classifier/Regressor的Predictions输出
     ← Calculate Statistics的结果数据

2. Filepath (文件路径) - Text类型，必需
   • 数据类型: 文本字符串
   • 格式: 完整的文件路径，支持绝对路径和相对路径
   • 示例: 
     - ""C:\Users\Data\output.csv""
     - ""D:\Projects\results.csv""
   • 文件要求: 建议使用.csv扩展名
   • 注意事项: 
     - 如果文件已存在，将被覆盖
     - 确保目录存在，否则会报错
     - 路径中包含中文或特殊字符时需确保编码正确

3. Index (是否写入行索引) - Boolean类型，默认False
   • 取值范围: True / False
   • 说明:
     - False: 不写入行索引（默认，最常用）
     - True: 在第一列写入行索引（0, 1, 2...）
   • 建议: 大多数情况下使用False，除非需要保留行索引信息

4. Encoding (文件编码) - Text类型，默认'utf-8'
   • 可选值: 'utf-8', 'gbk', 'gb2312', 'latin-1', 'ascii'等
   • 说明:
     - 'utf-8': 通用编码，支持中文和特殊字符（推荐）
     - 'gbk'/'gb2312': 中文编码，兼容性较好
     - 'latin-1': 西欧字符编码
   • 建议: 包含中文时使用'utf-8'，纯英文数据可使用'ascii'

5. Separator (分隔符) - Text类型，默认','
   • 可选值: ',', ';', '\t', '|', 或其他字符
   • 说明:
     - ',': 逗号分隔（标准CSV格式，最常用）
     - ';': 分号分隔（欧洲常用格式）
     - '\t': 制表符分隔（TSV格式）
     - '|': 管道符分隔
   • 建议: 大多数情况下使用默认值','即可

6. Write (执行写入) - Boolean类型，默认False
   • 取值范围: True / False
   • 说明:
     - False: 不执行写入操作（默认）
     - True: 执行写入操作，保存文件
   • 建议: 
     - 设置好所有参数后，将Write设置为True以保存文件
     - 这样可以避免意外覆盖文件

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Saved Path (保存路径) - Text类型
   • 内容: 实际保存的文件完整路径
   • 用途: 确认文件保存位置，可用于后续读取

2. Info (信息) - Text类型
   • 内容: 保存信息（行数、列数、文件大小等）
   • 用途: 验证数据是否正确保存

═══════════════════════════════════════════════════════════════
典型工作流程:
═══════════════════════════════════════════════════════════════

保存预测结果:
Predict Classifier/Regressor → Write CSV (保存预测结果)

保存处理后的数据:
Read CSV/Excel → Create Dataset → Deconstruct Dataset → Write CSV

保存分析结果:
Calculate Statistics → (格式化) → Write CSV

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. 必须将Write设置为True才会执行保存操作
2. 文件路径必须有效，确保目录存在
3. 如果文件已存在，将被覆盖，请谨慎操作
4. 数据必须是Tree结构，每行数据作为一个分支
5. 编码设置需与后续读取时的编码一致
6. 分隔符需与后续读取时的分隔符一致";
                
                DA.SetData(2, readme);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"执行失败: {ex.Message}");
                RhinoApp.WriteLine($"SimpleML错误: {ex}");
            }
        }

        private string ConvertTreeToPythonList(Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> tree)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("[");
            bool first = true;
            foreach (var path in tree.Paths)
            {
                if (!first) sb.Append(", ");
                sb.Append("[");
                var branch = tree[path];
                bool firstItem = true;
                foreach (var item in branch)
                {
                    if (!firstItem) sb.Append(", ");
                    string value = item.ToString();
                    if (double.TryParse(value, out double num))
                        sb.Append(num);
                    else
                        sb.Append("\"").Append(value.Replace("\"", "\\\"")).Append("\"");
                    firstItem = false;
                }
                sb.Append("]");
                first = false;
            }
            sb.Append("]");
            return sb.ToString();
        }

        private string GetMyMLPath()
        {
            string envPath = Environment.GetEnvironmentVariable("SIMPLEML_PATH");
            if (!string.IsNullOrEmpty(envPath) && Directory.Exists(envPath))
                return envPath;

            string defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Grasshopper", "UserObjects", "SimpleML", "myML");
            if (Directory.Exists(defaultPath))
                return defaultPath;

            string ghaPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string ghaDir = Path.GetDirectoryName(ghaPath);
            string relativePath = Path.Combine(ghaDir, "myML");
            if (Directory.Exists(relativePath))
                return relativePath;

            return null;
        }

        private string ExtractValue(string output, string prefix)
        {
            int startIndex = output.IndexOf(prefix);
            if (startIndex == -1) return string.Empty;
            startIndex += prefix.Length;
            int endIndex = output.IndexOf('\n', startIndex);
            if (endIndex == -1) endIndex = output.Length;
            return output.Substring(startIndex, endIndex - startIndex).Trim();
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(WriteCSVComponent));
        public override Guid ComponentGuid => new Guid("C1D2E3F4-A5B6-7890-CDEF-123456789012");
    }
}
