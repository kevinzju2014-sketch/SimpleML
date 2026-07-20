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
    public class CalculateStatisticsComponent : GH_Component
    {
        public CalculateStatisticsComponent()
          : base(L.Name("CalculateStatisticsComponent"), L.Nick("CalculateStatisticsComponent"), L.Desc("CalculateStatisticsComponent"),
              "SimpleML", "02 Analysis")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "输入数据（Tree结构）", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Statistics", "S", "统计信息Tree结构，每个分支代表一列，包含该列的统计指标", GH_ParamAccess.tree);
            pManager.AddTextParameter("Readme", "R", "组件使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> dataTree = new Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo>();
            if (!DA.GetDataTree(0, out dataTree)) return;

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
from components.statistics_components import calculate_statistics

# 转换输入数据
data_list = {dataList}
data = pd.DataFrame(data_list)

# 计算统计信息
stats = calculate_statistics(data)

# 获取缺失值统计
missing_values = data.isnull().sum().to_dict()

# 获取数据类型
data_types = data.dtypes.to_dict()

# 转换为Tree结构格式
# 每个列作为一个分支，每个分支包含该列的所有统计指标
# 统计指标顺序: [mean, std, min, max, median, count, missing_count, data_type]
stats_tree = []
column_names = []

# 只处理数值型列
numeric_cols = data.select_dtypes(include=[np.number]).columns.tolist()

for col in numeric_cols:
    column_names.append(col)
    col_stats = [
        str(stats['mean'].get(col, 'N/A')),
        str(stats['std'].get(col, 'N/A')),
        str(stats['min'].get(col, 'N/A')),
        str(stats['max'].get(col, 'N/A')),
        str(stats['median'].get(col, 'N/A')),
        str(int(stats['count'].get(col, 0))),
        str(int(missing_values.get(col, 0))),
        str(data_types.get(col, 'N/A'))
    ]
    stats_tree.append(col_stats)

# 输出为JSON格式，便于C#解析
stats_tree_json = json.dumps(stats_tree, ensure_ascii=False)
column_names_json = json.dumps(column_names, ensure_ascii=False)

print('OUTPUT_0:' + stats_tree_json)
print('OUTPUT_1:' + column_names_json)
print('OUTPUT_2:计算数据的描述性统计（均值、标准差、最小值、最大值等），包含缺失值数量和数据类型')
";

                string output = PythonScriptExecutor.ExecuteCode(pythonCode);
                string statisticsJson = ExtractValue(output, "OUTPUT_0:");
                string columnNamesJson = ExtractValue(output, "OUTPUT_1:");
                
                // 将JSON数据转换为Tree结构
                GH_Structure<GH_String> statisticsTree = TreeConverter.ConvertJsonToTree(statisticsJson);
                
                DA.SetDataTree(0, statisticsTree);
                
                // Readme输出
                string readme = GetReadmeText();
                
                DA.SetData(1, readme);
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
            return @"组件名称: Calculate Statistics
功能: 计算数据的描述性统计信息（均值、标准差、最小值、最大值、中位数等）

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

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Statistics (统计信息) - Tree结构
   • 数据类型: Grasshopper Tree结构
   • 数据结构: 
     - 每个分支代表一列（数值型列）
     - 分支路径为 {0}, {1}, {2}...（列索引）
     - 每个分支包含8个统计指标，顺序为:
       [均值(mean), 标准差(std), 最小值(min), 最大值(max), 中位数(median), 数量(count), 缺失值数量(missing_count), 数据类型(data_type)]
   • 示例（iris数据集，4个数值列）:
     {0} → [""5.843"", ""0.828"", ""4.3"", ""7.9"", ""5.8"", ""150"", ""0"", ""float64""]  (sepal_length列的统计)
     {1} → [""3.057"", ""0.436"", ""2.0"", ""4.4"", ""3.0"", ""150"", ""0"", ""float64""]  (sepal_width列的统计)
     {2} → [""3.758"", ""1.765"", ""1.0"", ""6.9"", ""4.35"", ""150"", ""0"", ""float64""] (petal_length列的统计)
     {3} → [""1.199"", ""0.762"", ""0.1"", ""2.5"", ""1.3"", ""150"", ""0"", ""float64""]  (petal_width列的统计)
   • 统计指标说明:
     - 均值 (mean): 数据的平均值
     - 标准差 (std): 数据的离散程度
     - 最小值 (min): 数据的最小值
     - 最大值 (max): 数据的最大值
     - 中位数 (median): 数据的中间值
     - 数量 (count): 非空值的数量
     - 缺失值数量 (missing_count): 该列的缺失值（NaN）数量
     - 数据类型 (data_type): 该列的数据类型（如 float64, int64 等）
   • 连接建议:
     → 用于数据探索（了解每列的统计分布）
     → 用于异常值检测（比较min/max与mean）
     → 用于数据质量检查（查看count了解缺失值）
   • 注意事项:
     - 只有数值型列会被包含在Tree中
     - 非数值列会被自动忽略
     - 每个分支的8个值对应8个统计指标（包含缺失值数量和数据类型）
     - 缺失值数量可以帮助识别数据质量问题
     - 数据类型信息有助于理解数据的特征

═══════════════════════════════════════════════════════════════
典型工作流程:
═══════════════════════════════════════════════════════════════

数据分析流程:
Read CSV/Excel → Calculate Statistics → (查看统计信息)

数据探索流程:
Load Dataset → Calculate Statistics → (了解数据分布)

数据预处理前检查:
Read CSV/Excel → Calculate Statistics → (检查异常值) → Create Dataset

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. 数据必须是Tree结构，每行数据作为一个分支
2. 只有数值型列会被计算统计信息，非数值列会被忽略
3. 缺失值（NaN）会被自动排除在统计计算之外，但会单独统计缺失值数量
4. 统计信息以Tree结构输出，便于进一步处理
5. 建议在数据预处理前先查看统计信息，了解数据分布和缺失值情况
6. 数据类型信息有助于判断是否需要数据转换或特征工程";
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

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(CalculateStatisticsComponent));
        public override Guid ComponentGuid => new Guid("C3D4E5F6-A7B8-9012-CDEF-123456789012");
    }
}
