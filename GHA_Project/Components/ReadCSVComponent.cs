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
    /// Read CSV Component
    /// 读取CSV文件组件
    /// </summary>
    public class ReadCSVComponent : GH_Component
    {
        public ReadCSVComponent()
          : base("Read CSV", "ReadCSV",
              "读取CSV文件并返回数据、列名和形状",
              "SimpleML", "01 Input")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Filepath", "F", "CSV文件路径", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Header", "H", "表头行号，默认0（0表示第一行是表头，-1表示无表头）", GH_ParamAccess.item, 0);
            pManager.AddTextParameter("Separator", "S", "分隔符，默认','（逗号）", GH_ParamAccess.item, ",");
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "数据Tree结构，每行数据作为一个分支", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Labels", "L", "列名的列表（Tree结构，单个分支）", GH_ParamAccess.tree);
            pManager.AddTextParameter("Info", "I", "字符串，描述文件位置、文件名、行数、列数", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "组件使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string filepath = string.Empty;
            int header = 0;
            string separator = ",";

            if (!DA.GetData(0, ref filepath)) return;
            DA.GetData(1, ref header);
            DA.GetData(2, ref separator);

            if (string.IsNullOrEmpty(filepath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "文件路径不能为空");
                return;
            }

            if (!File.Exists(filepath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"文件不存在: {filepath}");
                return;
            }

            try
            {
                // 获取myML路径
                string mymlPath = GetMyMLPath();
                if (string.IsNullOrEmpty(mymlPath))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, 
                        "未找到myML文件夹。请设置SIMPLEML_PATH环境变量或确保myML在默认位置。");
                    return;
                }

                // 转义文件路径中的特殊字符
                string escapedFilepath = filepath.Replace("\\", "\\\\").Replace("'", "\\'");

                // 构建Python代码
                string headerValue = header >= 0 ? header.ToString() : "None";
                string pythonCode = $@"
# -*- coding: utf-8 -*-
import sys
import os
import json
import site
import io

# 设置标准输出编码为UTF-8，避免中文输出错误
if sys.stdout.encoding != 'utf-8':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
if sys.stderr.encoding != 'utf-8':
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

# 添加项目路径
sys.path.insert(0, r'{mymlPath}')

# 确保Rhino Python的site-packages在路径中
# Rhino Python使用虚拟环境，需要确保site-packages可用
try:
    # 获取site-packages路径
    site_packages = site.getsitepackages()
    for sp in site_packages:
        if sp not in sys.path:
            sys.path.insert(0, sp)
    
    # 尝试添加Rhino Python的site-envs路径
    # pandas安装在: C:\Users\Administrator\.rhinocode\py39-rh8\site-envs\default-sFQ4Ch2s\pandas
    rhino_site_envs = r'C:\Users\Administrator\.rhinocode\py39-rh8\site-envs'
    if os.path.exists(rhino_site_envs):
        # 查找所有虚拟环境
        for item in os.listdir(rhino_site_envs):
            env_path = os.path.join(rhino_site_envs, item)
            if os.path.isdir(env_path):
                # 添加虚拟环境根目录（pandas等库可能直接在这里）
                if env_path not in sys.path:
                    sys.path.insert(0, env_path)
                # 添加虚拟环境的Lib\site-packages（如果存在）
                site_pkg = os.path.join(env_path, 'Lib', 'site-packages')
                if os.path.exists(site_pkg) and site_pkg not in sys.path:
                    sys.path.insert(0, site_pkg)
except Exception as e:
    pass  # 如果添加路径失败，继续执行

import pandas as pd
from components.file_io_components import read_csv

# 读取CSV
data, columns, shape = read_csv(
    r'{escapedFilepath}',
    header={headerValue},
    sep=r'{separator}'
)

# 将DataFrame转换为Tree结构格式
# 每行数据作为一个分支，每列的值作为该分支的元素
data_tree = []
for idx, row in data.iterrows():
    row_data = []
    for col in columns:
        value = row[col]
        # 处理NaN值
        if pd.isna(value):
            row_data.append(None)
        else:
            row_data.append(str(value))
    data_tree.append(row_data)

# 输出为JSON格式，便于C#解析
data_tree_json = json.dumps(data_tree, ensure_ascii=False)
# 将columns转换为单个分支的Tree结构（list格式）
labels_tree = [columns]  # 单个分支，包含所有列名
labels_json = json.dumps(labels_tree, ensure_ascii=False)
filepath_str = r'{escapedFilepath}'
info_str = f'文件位置: {{filepath_str}}, 文件名: {{os.path.basename(filepath_str)}}, 行数: {{shape[0]}}, 列数: {{shape[1]}}'

print('DATA:' + data_tree_json)
print('LABELS:' + labels_json)
print('INFO:' + info_str)
";

                // 执行Python脚本
                string output = PythonScriptExecutor.ExecuteCode(pythonCode);

                // 解析输出
                string dataTreeJson = ExtractValue(output, "DATA:");
                string labelsJson = ExtractValue(output, "LABELS:");
                string infoStr = ExtractValue(output, "INFO:");

                // 将JSON数据转换为Tree结构
                GH_Structure<GH_String> dataTree = ConvertJsonToTree(dataTreeJson);
                GH_Structure<GH_String> labelsTree = ConvertJsonToTree(labelsJson);

                DA.SetDataTree(0, dataTree);
                DA.SetDataTree(1, labelsTree);
                DA.SetData(2, infoStr);
                
                // Readme输出
                string readme = @"组件名称: Read CSV
功能: 读取CSV文件并转换为Grasshopper Tree结构

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. Filepath (文件路径) - Text类型，必需
   • 数据类型: 文本字符串
   • 格式: 完整的文件路径，支持绝对路径和相对路径
   • 示例: 
     - ""C:\Users\Data\iris.csv""
     - ""D:\Projects\data.csv""
   • 文件要求: 必须是有效的CSV文件（.csv扩展名）
   • 注意事项: 路径中包含中文或特殊字符时需确保编码正确

2. Header (表头行号) - Integer类型，默认0
   • 取值范围: 
     - 0: 第一行是表头（最常用）
     - 1: 第二行是表头
     - -1: 无表头，自动生成列名（column_0, column_1...）
     - 其他正整数: 指定表头所在行号（从0开始计数）
   • 建议: 大多数CSV文件第一行是表头，使用默认值0即可

3. Separator (分隔符) - Text类型，默认','
   • 可选值: ',', ';', '\t', '|', 或其他字符
   • 说明:
     - ',': 逗号分隔（标准CSV格式，最常用）
     - ';': 分号分隔（欧洲常用格式）
     - '\t': 制表符分隔（TSV格式）
     - '|': 管道符分隔
   • 建议: 大多数CSV文件使用逗号分隔，使用默认值即可

═══════════════════════════════════════════════════════════════
输出参数详解:
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
     → Create Dataset的X输入（特征数据）
     → Calculate Statistics的Data输入（统计分析）

2. Labels (列名) - Tree结构，单个分支
   • 数据类型: Tree结构
   • 数据结构: 单个分支 {0}，包含所有列名
   • 示例: {0} → [""sepal_length"", ""sepal_width"", ""petal_length"", ""petal_width"", ""species""]
   • 用途: 了解数据集的列名结构，可用于数据验证

3. Info (信息) - Text类型
   • 内容: 文件位置、文件名、行数、列数等统计信息
   • 用途: 快速查看数据文件的基本信息

═══════════════════════════════════════════════════════════════
典型工作流程:
═══════════════════════════════════════════════════════════════

数据输入流程:
Read CSV → Create Dataset → Split Dataset → Train Classifier/Regressor

数据分析流程:
Read CSV → Calculate Statistics / Calculate Correlation

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. 文件路径必须是有效的CSV文件
2. 确保文件编码为UTF-8，避免中文乱码
3. 数据以Tree结构输出，每行数据作为一个分支
4. 如果CSV文件包含表头，Header应设置为0
5. 分隔符需与CSV文件实际使用的分隔符匹配";
                DA.SetData(3, readme);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"执行失败: {ex.Message}");
                RhinoApp.WriteLine($"SimpleML错误详情: {ex}");
            }
        }

        private string GetMyMLPath()
        {
            // 方法1: 环境变量
            string envPath = Environment.GetEnvironmentVariable("SIMPLEML_PATH");
            if (!string.IsNullOrEmpty(envPath) && Directory.Exists(envPath))
                return envPath;

            // 方法2: 默认位置
            string defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Grasshopper", "UserObjects", "SimpleML", "myML");
            if (Directory.Exists(defaultPath))
                return defaultPath;

            // 方法3: 相对于GHA文件的位置
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

        /// <summary>
        /// 将JSON格式的数据转换为Grasshopper Tree结构（使用TreeConverter工具类）
        /// </summary>
        private GH_Structure<GH_String> ConvertJsonToTree(string jsonData)
        {
            return TreeConverter.ConvertJsonToTree(jsonData);
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(ReadCSVComponent));

        public override Guid ComponentGuid => new Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567891");
    }
}
