using System;
using System.IO;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.DataAnalysis
{
    public class DescribeFeaturesComponent : GH_Component
    {
        public DescribeFeaturesComponent()
          : base("Describe Features", "Describe",
              "描述特征（count, mean, std, min, 25%%, 50%%, 75%%, max）",
              "SimpleML", "02 Analysis")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "输入数据（Tree结构）", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Feature Index", "FI", "要描述的特征列索引（从0开始），默认0。输入一个数字，代表研究第几列", GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Description", "Desc", "特征描述Tree结构，每个分支代表一个特征，包含该特征的统计指标", GH_ParamAccess.tree);
            pManager.AddTextParameter("Readme", "R", "组件使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> dataTree = new Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo>();
            int featureIndex = 0;

            if (!DA.GetDataTree(0, out dataTree)) return;
            DA.GetData(1, ref featureIndex);

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
                
                // 使用特征索引（如果未指定则默认为0）
                string featureIndicesParam = $"[{featureIndex}]";

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
except Exception as e:
    pass

# 在路径设置之后导入pandas
import pandas as pd
import numpy as np
from components.statistics_components import describe_features

# 转换输入数据
data_list = {dataList}
data = pd.DataFrame(data_list)

# 根据列索引选择要描述的特征
feature_indices = {featureIndicesParam}
# 根据索引选择列
numeric_cols = data.select_dtypes(include=[np.number]).columns.tolist()

# 确保只选择指定索引的特征
feature_names = []
if len(numeric_cols) > 0 and len(feature_indices) > 0:
    # 只选择有效的索引（在数值列范围内）
    for idx in feature_indices:
        if 0 <= idx < len(numeric_cols):
            feature_names.append(numeric_cols[idx])

# 只描述指定的特征列
if len(feature_names) > 0:
    # 只选择指定的列进行描述
    selected_data = data[feature_names]
    description = describe_features(selected_data, feature_names)
else:
    # 如果没有有效的特征，返回空字典
    description = {{}}

# 转换为Tree结构格式
# 每个特征作为一个分支，每个分支包含该特征的统计指标
# 统计指标顺序: [count, mean, std, min, 25%, 50%, 75%, max]
description_tree = []
feature_names_list = []

# description 是一个字典，键是特征名，值是该特征的统计信息字典
for feature_name, stats in description.items():
    feature_names_list.append(feature_name)
    # 按照顺序提取统计指标
    feature_stats = [
        str(stats.get('count', 'N/A')),
        str(stats.get('mean', 'N/A')),
        str(stats.get('std', 'N/A')),
        str(stats.get('min', 'N/A')),
        str(stats.get('25%', 'N/A')),
        str(stats.get('50%', 'N/A')),
        str(stats.get('75%', 'N/A')),
        str(stats.get('max', 'N/A'))
    ]
    description_tree.append(feature_stats)

# 输出为JSON格式，便于C#解析
description_tree_json = json.dumps(description_tree, ensure_ascii=False)
feature_names_json = json.dumps(feature_names_list, ensure_ascii=False)

print('OUTPUT_0:' + description_tree_json)
print('OUTPUT_1:' + feature_names_json)
print('OUTPUT_2:描述特征（count, mean, std, min, 25%, 50%, 75%, max）')
";

                string output = PythonScriptExecutor.ExecuteCode(pythonCode);
                string descriptionTreeJson = ExtractValue(output, "OUTPUT_0:");
                string featureNamesJson = ExtractValue(output, "OUTPUT_1:");
                
                // 将JSON数据转换为Tree结构
                GH_Structure<GH_String> descriptionTree = TreeConverter.ConvertJsonToTree(descriptionTreeJson);
                
                DA.SetDataTree(0, descriptionTree);
                
                // Readme输出
                string readme = @"组件名称: Describe Features
功能: 描述特征（count, mean, std, min, 25%, 50%, 75%, max）

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
     ← Load Dataset的Data输出
     ← Deconstruct Dataset的X输出
   • 注意事项: 
     - 数据应为数值型，非数值列会被自动忽略
     - 缺失值（NaN）会被自动处理

2. Feature Index (特征列索引) - 整数，可选，默认0
   • 数据类型: 整数（Integer）
   • 说明: 指定要描述的特征列的索引（从0开始），输入一个数字，代表研究第几列
   • 示例:
     - 不连接（默认）: 描述第1个数值列（索引0）
     - 输入 0: 描述第1个数值列
     - 输入 1: 描述第2个数值列
     - 输入 2: 描述第3个数值列
   • 注意事项:
     - 索引从0开始
     - 只计算数值型列，非数值列会被跳过
     - 如果索引超出范围，会被自动忽略
     - 如果未连接，则默认描述第1个数值列（索引0）

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Description (特征描述) - Tree结构
   • 数据类型: Grasshopper Tree结构
   • 数据结构: 
     - 单个分支代表指定的特征（数值型列）
     - 分支路径为 {0}
     - 分支包含8个统计指标，顺序为:
       [count, mean, std, min, 25%, 50%, 75%, max]
   • 示例（iris数据集，输入索引0，描述sepal_length列）:
     {0} → [""150.0"", ""5.843"", ""0.828"", ""4.3"", ""5.1"", ""5.8"", ""6.4"", ""7.9""]  (sepal_length列的统计)
   • 示例（iris数据集，输入索引1，描述sepal_width列）:
     {0} → [""150.0"", ""3.057"", ""0.436"", ""2.0"", ""2.8"", ""3.0"", ""3.3"", ""4.4""]  (sepal_width列的统计)
   • 统计指标说明:
     - count: 非空值数量
     - mean: 均值
     - std: 标准差
     - min: 最小值
     - 25%: 第一四分位数
     - 50%: 中位数（第二四分位数）
     - 75%: 第三四分位数
     - max: 最大值
   • 连接建议:
     → 用于数据探索（了解每个特征的分布情况）
     → 用于异常值检测（比较min/max与mean/std）
     → 用于数据质量检查（查看count了解缺失值）
   • 注意事项:
     - 只有数值型列会被包含在Tree中
     - 非数值列会被自动忽略
     - 分支的8个值对应8个统计指标
     - 输出只包含指定索引的特征

═══════════════════════════════════════════════════════════════
典型工作流程:
═══════════════════════════════════════════════════════════════

数据探索流程:
Read CSV/Excel → Describe Features → (查看所有特征的统计信息)

选择性特征分析:
Read CSV/Excel → Describe Features (指定索引) → (查看特定特征的统计信息)

数据预处理前检查:
Load Dataset → Describe Features → (了解特征分布) → Create Dataset

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. 数据必须是Tree结构，每行数据作为一个分支
2. 只描述数值型列，非数值列会被自动忽略
3. 特征索引从0开始，只计算数值型列
4. 如果索引超出范围或无效，会被自动忽略
5. 如果未指定索引，则默认描述第1个数值列（索引0）
6. 描述包括count、mean、std、min、25%、50%、75%、max等统计量
7. 建议在特征工程前先查看特征描述，了解数据分布";
                DA.SetData(1, readme);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"执行失败: {ex.Message}");
                RhinoApp.WriteLine($"SimpleML错误: {ex}");
            }
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

        private string ExtractValue(string output, string prefix)
        {
            int startIndex = output.IndexOf(prefix);
            if (startIndex == -1) return string.Empty;
            startIndex += prefix.Length;
            
            // 查找下一个 OUTPUT_ 标记或文件结尾
            int endIndex = output.Length;
            for (int i = startIndex; i < output.Length; i++)
            {
                if (i < output.Length - 7 && output.Substring(i, 7) == "OUTPUT_")
                {
                    endIndex = i;
                    break;
                }
            }
            
            return output.Substring(startIndex, endIndex - startIndex).Trim();
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(DescribeFeaturesComponent));
        public override Guid ComponentGuid => new Guid("E5F6A7B8-C9D0-1234-EF01-234567890124");
    }
}
