using System;
using System.IO;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SimpleML.Core;
using Rhino;

using SimpleML.Localization;
namespace SimpleML.Components.DataInput
{
    public class ReadExcelComponent : GH_Component
    {
        public ReadExcelComponent()
          : base(L.Name("ReadExcelComponent"), L.Nick("ReadExcelComponent"), L.Desc("ReadExcelComponent"),
              "SimpleML", "01 Input")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Filepath", "F", "Excel文件路径（.xlsx或.xls）", GH_ParamAccess.item);
            pManager.AddTextParameter("Sheet Name", "S", "工作表名称或索引，默认'0'（0表示第一个工作表，也可以使用工作表名称如'Sheet1'）", GH_ParamAccess.item, "0");
            pManager.AddIntegerParameter("Header", "H", "表头行号，默认0", GH_ParamAccess.item, 0);
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
            string sheetName = "0";  // 默认使用索引0（第一个工作表）
            int header = 0;

            if (!DA.GetData(0, ref filepath)) return;
            DA.GetData(1, ref sheetName);
            DA.GetData(2, ref header);
            
            // 如果sheetName为空，使用默认值0
            if (string.IsNullOrWhiteSpace(sheetName))
            {
                sheetName = "0";
            }

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
                string mymlPath = GetMyMLPath();
                if (string.IsNullOrEmpty(mymlPath))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, 
                        L.T("err.package_missing"));
                    return;
                }

                string escapedFilepath = filepath.Replace("\\", "\\\\").Replace("'", "\\'");
                string escapedSheetName = sheetName.Replace("'", "\\'");
                
                // 尝试将sheetName转换为整数，如果失败则作为字符串使用
                string sheetParam = int.TryParse(sheetName, out int sheetIndex) 
                    ? sheetIndex.ToString() 
                    : $"'{escapedSheetName}'";

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
    # pandas安装在: Rhino site-packages (auto-discovered)
    rhino_site_envs = str(next((p for root in [__import__('pathlib').Path.home()/'.rhinocode', __import__('pathlib').Path.home()/'Library'/'Application Support'/'McNeel'/'Rhinoceros'/'.rhinocode'] if root.exists() for p in root.glob('py*-rh*/site-envs') if p.is_dir()), __import__('pathlib').Path.home()/'.rhinocode'/'site-envs'))
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

# 在路径设置之后导入pandas
import pandas as pd
from components.file_io_components import read_excel

data, columns, shape = read_excel(
    r'{escapedFilepath}',
    sheet_name={sheetParam},
    header={header}
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
                string readme = @"组件名称: Read Excel
功能: 读取Excel文件并转换为Grasshopper Tree结构

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. Filepath (文件路径) - Text类型，必需
   • 数据类型: 文本字符串
   • 格式: 完整的文件路径，支持绝对路径和相对路径
   • 示例: 
     - ""C:\Users\Data\iris.xlsx""
     - ""D:\Projects\data.xlsx""
   • 文件要求: 必须是有效的Excel文件（.xlsx或.xls扩展名）
   • 注意事项: 
     - 路径中包含中文或特殊字符时需确保编码正确
     - .xlsx文件需要openpyxl库支持
     - .xls文件需要xlrd库支持

2. Sheet Name (工作表名称) - Text类型，默认'0'
   • 数据类型: 文本字符串或数字字符串
   • 取值范围: 
     - 数字字符串（如""0"", ""1"", ""2""）: 表示工作表索引，从0开始
     - 工作表名称（如""Sheet1"", ""数据表""）: 使用具体的工作表名称
   • 说明:
     - ""0"": 第一个工作表（默认，最常用）
     - ""1"": 第二个工作表
     - ""Sheet1"": 名为""Sheet1""的工作表
   • 建议: 
     - 大多数情况下使用默认值""0""即可
     - 如果指定名称不存在，会自动回退到第一个工作表（索引0）
     - 使用索引比名称更可靠，避免工作表重命名导致的问题

3. Header (表头行号) - Integer类型，默认0
   • 取值范围: 
     - 0: 第一行是表头（最常用）
     - 1: 第二行是表头
     - -1: 无表头，自动生成列名（column_0, column_1...）
     - 其他正整数: 指定表头所在行号（从0开始计数）
   • 建议: 大多数Excel文件第一行是表头，使用默认值0即可

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
     → Write CSV/Excel的Data输入（保存数据）

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
Read Excel → Create Dataset → Split Dataset → Train Classifier/Regressor

数据分析流程:
Read Excel → Calculate Statistics / Calculate Correlation

数据保存流程:
Read Excel → (处理) → Write Excel (保存处理后的数据)

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. 文件路径必须是有效的Excel文件（.xlsx或.xls）
2. 确保文件编码为UTF-8，避免中文乱码
3. 数据以Tree结构输出，每行数据作为一个分支
4. 如果Excel文件包含表头，Header应设置为0
5. 如果指定的工作表名称不存在，会自动使用第一个工作表（索引0）
6. 大数据文件（>10000行）可能需要较长的读取时间";
                DA.SetData(3, readme);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, L.T("err.exec_failed", ex.Message));
                RhinoApp.WriteLine($"SimpleML错误: {ex}");
            }
        }

        private string GetMyMLPath()
        {
            return PathResolver.GetMyMLPath();
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

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(ReadExcelComponent));
        public override Guid ComponentGuid => new Guid("B2C3D4E5-F6A7-8901-BCDE-F12345678901");
    }
}
