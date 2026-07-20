using System;
using System.IO;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SimpleML.Core;
using Rhino;

using SimpleML.Localization;
namespace SimpleML.Components.DataAnalysis
{
    public class CalculateCorrelationComponent : GH_Component
    {
        public CalculateCorrelationComponent()
          : base(L.Name("CalculateCorrelationComponent"), L.Nick("CalculateCorrelationComponent"), L.Desc("CalculateCorrelationComponent"),
              "SimpleML", "02 Analysis")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "输入数据（Tree结构）", GH_ParamAccess.tree);
            pManager.AddTextParameter("Method", "M", "相关性计算方法，默认'pearson'（可选：pearson, spearman, kendall）", GH_ParamAccess.item, "pearson");
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Correlation Matrix", "CM", "相关性矩阵（Tree结构）", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Analysis", "A", "分析信息Tree结构：{{0}}=方法, {{1}}=维度, {{2+}}=强相关特征对", GH_ParamAccess.tree);
            pManager.AddTextParameter("Readme", "R", "组件使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> dataTree = new Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo>();
            string method = "pearson";

            if (!DA.GetDataTree(0, out dataTree)) return;
            DA.GetData(1, ref method);

            try
            {
                string mymlPath = GetMyMLPath();
                if (string.IsNullOrEmpty(mymlPath))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, 
                        L.T("err.package_missing"));
                    return;
                }

                string dataList = ConvertTreeToPythonList(dataTree);
                string escapedMethod = method.Replace("'", "\\'").Replace("\"", "\\\"");

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
    
    rhino_site_envs = str(next((p for root in [__import__('pathlib').Path.home()/'.rhinocode', __import__('pathlib').Path.home()/'Library'/'Application Support'/'McNeel'/'Rhinoceros'/'.rhinocode'] if root.exists() for p in root.glob('py*-rh*/site-envs') if p.is_dir()), __import__('pathlib').Path.home()/'.rhinocode'/'site-envs'))
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
import numpy as np
from components.statistics_components import calculate_correlation

# 转换输入数据
data_list = {dataList}
data = pd.DataFrame(data_list)

# 计算相关性
corr_matrix, corr_dict = calculate_correlation(data, method=r'''{escapedMethod}''')

# 转换为Tree结构格式（矩阵）
corr_matrix_tree = corr_matrix.values.tolist()

# 生成分析说明Tree结构
method_str = '{escapedMethod}'
analysis_tree = []

# {{0}}: 方法名称
analysis_tree.append([method_str])

# {{1}}: 矩阵维度 [行数, 列数]
analysis_tree.append([str(corr_matrix.shape[0]), str(corr_matrix.shape[1])])

# {{2+}}: 强相关特征对 [特征1名称, 特征2名称, 相关系数值]
strong_pairs = []
for i, col1 in enumerate(corr_matrix.columns):
    for j, col2 in enumerate(corr_matrix.columns):
        if i < j and abs(corr_matrix.iloc[i, j]) > 0.7:
            corr_value = corr_matrix.iloc[i, j]
            corr_str = '{{:.3f}}'.format(corr_value)
            strong_pairs.append([str(col1), str(col2), corr_str])

# 只保留前10对强相关特征对
for pair in strong_pairs[:10]:
    analysis_tree.append(pair)

# 如果没有强相关特征对，添加一个空分支
if len(strong_pairs) == 0:
    analysis_tree.append(['无强相关特征对'])

print('OUTPUT_0:' + json.dumps(corr_matrix_tree, ensure_ascii=False))
print('OUTPUT_1:' + json.dumps(analysis_tree, ensure_ascii=False))
print('OUTPUT_2:计算特征之间的相关性矩阵')
";

                string output = PythonScriptExecutor.ExecuteCode(pythonCode);
                
                string corrMatrixJson = ExtractValue(output, "OUTPUT_0:");
                string analysisJson = ExtractValue(output, "OUTPUT_1:");

                GH_Structure<GH_String> corrMatrixTree = TreeConverter.ConvertJsonToTree(corrMatrixJson);
                GH_Structure<GH_String> analysisTree = TreeConverter.ConvertJsonToTree(analysisJson);
                
                DA.SetDataTree(0, corrMatrixTree);
                DA.SetDataTree(1, analysisTree);
                
                // Readme输出
                string readme = GetReadmeText();
                
                DA.SetData(2, readme);
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

        private string GetReadmeText()
        {
            return @"组件名称: Calculate Correlation
功能: 计算特征之间的相关性矩阵

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

2. Method (相关性计算方法) - Text类型，默认'pearson'
   • 可选值: 'pearson', 'spearman', 'kendall'
   • 说明:
     - 'pearson': 皮尔逊相关系数（默认，最常用）
       * 测量线性相关性
       * 适用于连续数值数据
       * 取值范围: -1 到 1
     - 'spearman': 斯皮尔曼等级相关系数
       * 测量单调相关性
       * 适用于有序数据或非线性关系
       * 对异常值更鲁棒
     - 'kendall': 肯德尔等级相关系数
       * 测量排序相关性
       * 适用于小样本数据
       * 计算较慢但更稳健
   • 建议: 
     - 大多数情况下使用'pearson'即可
     - 如果数据有异常值或非线性关系，使用'spearman'
     - 小样本数据可以使用'kendall'

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Correlation Matrix (相关性矩阵) - Tree结构
   • 数据类型: Grasshopper Tree结构
   • 数据结构: 
     - 行和列都对应特征
     - 值表示两个特征之间的相关系数
     - 矩阵是对称的（行i列j = 行j列i）
     - 对角线值为1（特征与自身的相关性）
   • 示例: 4个特征的相关性矩阵
     {0} → [1.0, 0.87, 0.82, 0.79]  (特征0与其他特征的相关性)
     {1} → [0.87, 1.0, 0.96, 0.95]  (特征1与其他特征的相关性)
     {2} → [0.82, 0.96, 1.0, 0.97]  (特征2与其他特征的相关性)
     {3} → [0.79, 0.95, 0.97, 1.0]  (特征3与其他特征的相关性)
   • 相关系数解释:
     - 1.0: 完全正相关
     - 0.7-1.0: 强正相关
     - 0.3-0.7: 中等正相关
     - -0.3-0.3: 弱相关或无相关
     - -0.7--0.3: 中等负相关
     - -1.0--0.7: 强负相关
     - -1.0: 完全负相关
   • 连接建议:
     → 用于特征选择（移除高度相关的特征）
     → 用于数据探索（了解特征关系）

2. Analysis (分析说明) - Tree结构
   • 数据类型: Grasshopper Tree结构
   • 数据结构: 
     - {0}: [方法名称] - 使用的相关性计算方法（如 ""pearson"", ""spearman"", ""kendall""）
     - {1}: [行数, 列数] - 矩阵维度
     - {2+}: [特征1名称, 特征2名称, 相关系数值] - 强相关特征对（|r| > 0.7），最多10对
   • 示例:
     {0} → [""pearson""]  (使用方法)
     {1} → [""4"", ""4""]  (矩阵维度: 4x4)
     {2} → [""feature1"", ""feature2"", ""0.87""]  (强相关对1)
     {3} → [""feature2"", ""feature3"", ""0.92""]  (强相关对2)
   • 说明:
     - 如果没有强相关特征对，{2} 分支会包含 [""无强相关特征对""]
     - 只显示前10对强相关特征对
     - 相关系数值保留3位小数
   • 用途: 
     - 快速了解数据中的强相关特征对
     - 用于特征选择（识别需要移除的冗余特征）
     - 用于数据探索（了解特征关系）

═══════════════════════════════════════════════════════════════
典型工作流程:
═══════════════════════════════════════════════════════════════

数据探索流程:
Read CSV/Excel → Calculate Correlation → (查看特征相关性)

特征选择流程:
Read CSV/Excel → Calculate Correlation → (识别高度相关的特征) → Create Dataset (移除冗余特征)

数据预处理前检查:
Read CSV/Excel → Calculate Correlation → (检查多重共线性) → Create Dataset

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. 数据必须是Tree结构，每行数据作为一个分支
2. 只有数值型列会被计算相关性，非数值列会被忽略
3. 缺失值（NaN）会被自动排除在相关性计算之外
4. 相关性矩阵是对称的，对角线值为1
5. 强相关特征（|r| > 0.7）可能导致多重共线性问题
6. 建议在特征选择前先查看相关性矩阵";
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

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(CalculateCorrelationComponent));
        public override Guid ComponentGuid => new Guid("C3D4E5F6-A7B8-9012-CDEF-123456789013");
    }
}
