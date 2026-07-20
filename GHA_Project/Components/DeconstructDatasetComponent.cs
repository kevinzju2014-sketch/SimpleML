using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SimpleML.Core;
using Rhino;

using SimpleML.Localization;
namespace SimpleML.Components.DatasetManagement
{
    public class DeconstructDatasetComponent : GH_Component
    {
        public DeconstructDatasetComponent()
          : base(L.Name("DeconstructDatasetComponent"), L.Nick("DeconstructDatasetComponent"), L.Desc("DeconstructDatasetComponent"),
              "SimpleML", "03 Dataset")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Dataset", "DS", "Dataset对象", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("X", "X", "特征数据（Tree结构）", GH_ParamAccess.tree);
            pManager.AddGenericParameter("y", "y", "标签数据（Tree结构），可选", GH_ParamAccess.tree);
            pManager.AddTextParameter("X Names", "XN", "X列名列表（可选）", GH_ParamAccess.list);
            pManager.AddTextParameter("y Names", "yN", "y列名列表（可选）", GH_ParamAccess.list);
            pManager.AddTextParameter("Readme", "R", "组件使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object datasetObj = null;
            if (!DA.GetData(0, ref datasetObj)) return;

            try
            {
                string mymlPath = GetMyMLPath();
                if (string.IsNullOrEmpty(mymlPath))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, 
                        L.T("err.package_missing"));
                    return;
                }

                string datasetStr = datasetObj?.ToString() ?? "";

                string pythonCode = $@"
# -*- coding: utf-8 -*-
import sys
import os
import json
import site
import io
import pickle
import base64

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
except Exception as e:
    pass

# 在路径设置之后导入numpy
import numpy as np
from components.dataset_components import deconstruct_dataset

# 反序列化数据集
try:
    dataset_bytes = base64.b64decode(r'''{datasetStr}''')
    dataset = pickle.loads(dataset_bytes)
except Exception as e:
    print('ERROR: 无效的Dataset对象: ' + str(e), file=sys.stderr)
    sys.exit(1)

# 解构数据集
result = deconstruct_dataset(dataset)
X, y, has_labels = result[0], result[1], result[2]
all_column_names = result[3] if len(result) > 3 else None

# 获取X_names和y_names
X_names = getattr(dataset, 'X_names', None)
y_names = getattr(dataset, 'y_names', None)

# 构建X输出（特征数据，Tree结构）
X_array = X.tolist() if isinstance(X, np.ndarray) else X
if not isinstance(X_array, list) or len(X_array) == 0 or not isinstance(X_array[0], list):
    # 确保X_array是列表的列表
    X_array = [[x_val] for x_val in X_array] if isinstance(X_array, list) else [[X_array]]
X_tree = X_array

# 构建y输出（标签数据，Tree结构，可选）
y_tree = []
if y is not None and has_labels:
    y_array = y.tolist() if isinstance(y, np.ndarray) else y
    # 确保y_array是一维列表
    if isinstance(y_array, list) and len(y_array) > 0 and isinstance(y_array[0], list):
        y_flat = [item[0] if isinstance(item, list) and len(item) > 0 else item for item in y_array]
        y_array = y_flat
    # 转换为Tree结构：每个分支一个标签值
    y_tree = [[y_val] for y_val in y_array]

# 构建X Names输出（X列名列表，可选）
X_names_output = []
if X_names is not None:
    X_names_output = X_names if isinstance(X_names, list) else [X_names]
elif all_column_names is not None:
    # 如果没有X_names，但从all_column_names中提取（排除y_names）
    if y_names is not None and isinstance(y_names, list):
        X_names_output = [name for name in all_column_names if name not in y_names]
    else:
        # 如果y_names不在all_column_names中，假设最后一个是y
        if has_labels and len(all_column_names) > 0:
            X_names_output = all_column_names[:-1]
        else:
            X_names_output = all_column_names.copy()

# 构建y Names输出（y列名列表，可选）
y_names_output = []
if y_names is not None:
    y_names_output = y_names if isinstance(y_names, list) else [y_names]
elif has_labels and all_column_names is not None:
    # 如果没有y_names，但从all_column_names中提取
    if X_names is not None and isinstance(X_names, list):
        y_names_output = [name for name in all_column_names if name not in X_names]
    else:
        # 如果X_names不在all_column_names中，假设最后一个是y
        if len(all_column_names) > 0:
            y_names_output = [all_column_names[-1]]

# 转换为JSON格式
X_tree_json = json.dumps(X_tree, ensure_ascii=False)
y_tree_json = json.dumps(y_tree, ensure_ascii=False) if len(y_tree) > 0 else '[]'
X_names_json = json.dumps(X_names_output, ensure_ascii=False) if len(X_names_output) > 0 else '[]'
y_names_json = json.dumps(y_names_output, ensure_ascii=False) if len(y_names_output) > 0 else '[]'

print('OUTPUT_0:' + X_tree_json)
print('OUTPUT_1:' + y_tree_json)
print('OUTPUT_2:' + X_names_json)
print('OUTPUT_3:' + y_names_json)
print('OUTPUT_4:从Dataset对象中提取X、y、X Names和y Names')
";

                string output = PythonScriptExecutor.ExecuteCode(pythonCode);
                
                // 解析输出并转换为Tree结构
                string xTreeJson = ExtractValue(output, "OUTPUT_0:");
                string yTreeJson = ExtractValue(output, "OUTPUT_1:");
                string xNamesJson = ExtractValue(output, "OUTPUT_2:");
                string yNamesJson = ExtractValue(output, "OUTPUT_3:");
                
                GH_Structure<GH_String> xTree = TreeConverter.ConvertJsonToTree(xTreeJson);
                GH_Structure<GH_String> yTree = TreeConverter.ConvertJsonToTree(yTreeJson);
                
                // 解析X Names和y Names（JSON数组格式）
                var xNamesList = new List<string>();
                var yNamesList = new List<string>();
                
                if (!string.IsNullOrEmpty(xNamesJson) && xNamesJson != "[]")
                {
                    try
                    {
                        // 手动解析JSON数组
                        xNamesJson = xNamesJson.Trim();
                        if (xNamesJson.StartsWith("[") && xNamesJson.EndsWith("]"))
                        {
                            xNamesJson = xNamesJson.Substring(1, xNamesJson.Length - 2);
                            if (!string.IsNullOrEmpty(xNamesJson))
                            {
                                var names = xNamesJson.Split(',');
                                foreach (var name in names)
                                {
                                    var trimmed = name.Trim().Trim('"').Trim('\'');
                                    if (!string.IsNullOrEmpty(trimmed))
                                        xNamesList.Add(trimmed);
                                }
                            }
                        }
                    }
                    catch { }
                }
                
                if (!string.IsNullOrEmpty(yNamesJson) && yNamesJson != "[]")
                {
                    try
                    {
                        // 手动解析JSON数组
                        yNamesJson = yNamesJson.Trim();
                        if (yNamesJson.StartsWith("[") && yNamesJson.EndsWith("]"))
                        {
                            yNamesJson = yNamesJson.Substring(1, yNamesJson.Length - 2);
                            if (!string.IsNullOrEmpty(yNamesJson))
                            {
                                var names = yNamesJson.Split(',');
                                foreach (var name in names)
                                {
                                    var trimmed = name.Trim().Trim('"').Trim('\'');
                                    if (!string.IsNullOrEmpty(trimmed))
                                        yNamesList.Add(trimmed);
                                }
                            }
                        }
                    }
                    catch { }
                }
                
                DA.SetDataTree(0, xTree);
                DA.SetDataTree(1, yTree);
                DA.SetDataList(2, xNamesList.Select(n => new GH_String(n)).ToList());
                DA.SetDataList(3, yNamesList.Select(n => new GH_String(n)).ToList());
                
                // Readme输出
                string readme = @"组件名称: Deconstruct Dataset
功能: 解构Dataset对象，提取X、y、X Names和y Names（Create Dataset的逆过程）

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. Dataset (数据集) - Generic类型，必需
   • 数据类型: Dataset对象（Base64编码的pickle对象）
   • 数据结构: 包含特征数据X和标签数据y的Dataset对象
   • 连接建议:
     ← Create Dataset的Dataset输出
     ← Split Dataset的Train Dataset/Test Dataset输出
     ← Load Dataset的Dataset输出
   • 注意事项: 
     - 必须是有效的Dataset对象
     - 如果Dataset没有标签（如聚类任务），y输出将为空

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. X (特征数据) - Tree结构
   • 数据类型: Grasshopper Tree结构
   • 数据结构: 
     - 每个分支代表一个样本
     - 分支内的元素是该样本的特征值
     - 分支路径为 {0}, {1}, {2}...（样本索引）
   • 示例:
     {0} → [""5.1"", ""3.5"", ""1.4"", ""0.2""]  (特征1, 特征2, 特征3, 特征4)
     {1} → [""4.9"", ""3.0"", ""1.4"", ""0.2""]
   • 连接建议:
     → Create Dataset的X输入（重新创建Dataset）
     → Calculate Statistics的Data输入（统计分析）
     → Write CSV/Excel的Data输入（保存数据）
   • 注意: 
     - X输出只包含特征数据，不包含标签

2. y (标签数据) - Tree结构，可选
   • 数据类型: Grasshopper Tree结构
   • 数据结构: 
     - 每个分支代表一个样本
     - 分支内的元素是该样本的标签值（单元素分支）
     - 分支路径为 {0}, {1}, {2}...（样本索引）
   • 示例:
     {0} → [""setosa""]
     {1} → [""versicolor""]
     {2} → [""virginica""]
   • 连接建议:
     → Create Dataset的y输入（重新创建Dataset）
   • 注意: 
     - 如果Dataset没有标签，y输出为空
     - y输出只包含标签值，不包含特征

3. X Names (X列名列表) - List，可选
   • 数据类型: 字符串列表
   • 数据结构: X每列的列名列表
   • 示例:
     [""feature_0"", ""feature_1"", ""feature_2"", ""feature_3""]
   • 连接建议:
     → Create Dataset的X Names输入（重新创建Dataset）
   • 注意: 
     - 如果Dataset没有X_names信息，输出可能为空或使用默认列名

4. y Names (y列名列表) - List，可选
   • 数据类型: 字符串列表
   • 数据结构: y的列名列表
   • 示例:
     [""target""]
   • 连接建议:
     → Create Dataset的y Names输入（重新创建Dataset）
   • 注意: 
     - 如果Dataset没有y_names信息或没有标签，输出可能为空

═══════════════════════════════════════════════════════════════
典型工作流程:
═══════════════════════════════════════════════════════════════

可逆操作（Create Dataset ↔ Deconstruct Dataset）:
Create Dataset (X, y, X Names, y Names) → Dataset → Deconstruct Dataset → X, y, X Names, y Names

提取数据用于保存:
Create Dataset → Deconstruct Dataset (X输出) → Write CSV/Excel

提取数据用于分析:
Split Dataset (Test Dataset) → Deconstruct Dataset (X输出) → Calculate Statistics

重新创建Dataset:
Deconstruct Dataset (X, y, X Names, y Names) → Create Dataset → Dataset

重新组织数据（可逆操作）:
Create Dataset (Data, Labels) → Deconstruct Dataset → (Data输出, Labels输出) → Create Dataset (Data, Labels)
注意: 
- Deconstruct Dataset输出的Data包含所有列（特征+标签）
- Labels输出包含所有列的列名（特征列名+标签列名），格式与Create Dataset输入的Labels格式一致

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. 输入必须是有效的Dataset对象
2. Data输出是Tree结构，每行数据作为一个分支，包含所有列（特征+标签）
3. Labels输出是Tree结构，包含所有列的列名（特征列名+标签列名），格式是单个分支包含所有列名
4. 如果Dataset没有标签（如聚类任务），Data只包含特征列，Labels只包含特征列名
5. Create Dataset和Deconstruct Dataset是可逆操作：输出的X、y、X Names和y Names可以直接用于Create Dataset的输入
6. 提取的数据可以用于后续处理、保存或分析";
                
                DA.SetData(4, readme);
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

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(DeconstructDatasetComponent));
        public override Guid ComponentGuid => new Guid("F6A7B8C9-D0E1-2345-F012-345678901235");
    }
}
