using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

using SimpleML.Localization;
namespace SimpleML.Components.DatasetManagement
{
    public class CreateDatasetComponent : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        public CreateDatasetComponent()
          : base(L.Name("CreateDatasetComponent"), L.Nick("CreateDatasetComponent"), L.Desc("CreateDatasetComponent"),
              "SimpleML", "03 Dataset")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // X参数 - 必需
            pManager.AddGenericParameter("X", "X", "特征数据（Tree结构）", GH_ParamAccess.tree);
            
            // y参数 - 可选（在RegisterInputParams之后设置Optional）
            pManager.AddGenericParameter("y", "y", "标签数据（Tree结构），可选", GH_ParamAccess.tree);
            
            // X Names参数 - 可选
            pManager.AddTextParameter("X Names", "XN", "X列名列表（可选），用于标识X每列的名称", GH_ParamAccess.list);
            
            // y Names参数 - 可选
            pManager.AddTextParameter("y Names", "yN", "y列名列表（可选），用于标识y的名称", GH_ParamAccess.list);
            
            // 其他参数
            pManager.AddBooleanParameter("Normalize", "N", "是否标准化，默认False", GH_ParamAccess.item, false);
            pManager.AddTextParameter("Normalize Method", "NM", "标准化方法，默认'standard'（可选：standard, minmax, robust）", GH_ParamAccess.item, "standard");
            pManager.AddBooleanParameter("Remove Outliers", "RO", "是否移除异常值，默认False", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Handle Missing", "HM", "是否处理缺失值，默认False", GH_ParamAccess.item, false);
            pManager.AddTextParameter("Missing Strategy", "MS", "缺失值处理策略，默认'mean'（可选：mean, median, mode, drop）", GH_ParamAccess.item, "mean");
        }
        
        public override void CreateAttributes()
        {
            base.CreateAttributes();
            
            // 在创建属性后立即设置可选参数
            if (Params.Input.Count >= 4)
            {
                Params.Input[1].Optional = true;  // y参数
                Params.Input[2].Optional = true;  // X Names参数
                Params.Input[3].Optional = true;  // y Names参数
            }
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Dataset", "DS", "数据集对象，能够连接后续的运算器，包含特征数据和标签数据", GH_ParamAccess.item);
            pManager.AddTextParameter("Info", "I", "描述当前数据处理的方式，数据集的shape", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "组件使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 初始化变量
            Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> xTree = null;
            Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> yTree = null;
            var xNamesList = new List<string>();
            var yNamesList = new List<string>();
            bool normalize = false;
            string normalizeMethod = "standard";
            bool removeOutliers = false;
            bool handleMissing = false;
            string missingStrategy = "mean";

            // 1. 获取X参数（必需）
            if (!DA.GetDataTree(0, out xTree) || xTree == null || xTree.PathCount == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "必须提供X参数（特征数据）");
                return;
            }

            // 2. 获取y参数（可选）- 完全安全的方式
            bool hasY = false;
            yTree = new Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo>();
            
            // 检查参数是否可选且已连接
            if (Params.Input[1].Optional)
            {
                // 参数是可选的，尝试获取但不强制
                try
                {
                    if (DA.GetDataTree(1, out yTree))
                    {
                        if (yTree != null && yTree.PathCount > 0)
                        {
                            hasY = true;
                        }
                    }
                }
                catch
                {
                    // 如果获取失败（参数未连接），hasY保持为false
                    hasY = false;
                    yTree = new Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo>();
                }
            }
            else
            {
                // 如果Optional还没设置，尝试设置并获取
                try
                {
                    Params.Input[1].Optional = true;
                    if (DA.GetDataTree(1, out yTree))
                    {
                        if (yTree != null && yTree.PathCount > 0)
                        {
                            hasY = true;
                        }
                    }
                }
                catch
                {
                    hasY = false;
                    yTree = new Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo>();
                }
            }

            // 3. 获取X Names参数（可选）- 完全安全的方式
            bool hasXNames = false;
            xNamesList.Clear();
            
            if (Params.Input[2].Optional)
            {
                try
                {
                    if (DA.GetDataList(2, xNamesList))
                    {
                        if (xNamesList != null && xNamesList.Count > 0)
                        {
                            hasXNames = true;
                        }
                    }
                }
                catch
                {
                    hasXNames = false;
                    xNamesList.Clear();
                }
            }
            else
            {
                try
                {
                    Params.Input[2].Optional = true;
                    if (DA.GetDataList(2, xNamesList))
                    {
                        if (xNamesList != null && xNamesList.Count > 0)
                        {
                            hasXNames = true;
                        }
                    }
                }
                catch
                {
                    hasXNames = false;
                    xNamesList.Clear();
                }
            }

            // 4. 获取y Names参数（可选）- 完全安全的方式
            bool hasYNames = false;
            yNamesList.Clear();
            
            if (Params.Input[3].Optional)
            {
                try
                {
                    if (DA.GetDataList(3, yNamesList))
                    {
                        if (yNamesList != null && yNamesList.Count > 0)
                        {
                            hasYNames = true;
                        }
                    }
                }
                catch
                {
                    hasYNames = false;
                    yNamesList.Clear();
                }
            }
            else
            {
                try
                {
                    Params.Input[3].Optional = true;
                    if (DA.GetDataList(3, yNamesList))
                    {
                        if (yNamesList != null && yNamesList.Count > 0)
                        {
                            hasYNames = true;
                        }
                    }
                }
                catch
                {
                    hasYNames = false;
                    yNamesList.Clear();
                }
            }

            // 5. 获取其他参数（都有默认值，安全）
            DA.GetData(4, ref normalize);
            DA.GetData(5, ref normalizeMethod);
            DA.GetData(6, ref removeOutliers);
            DA.GetData(7, ref handleMissing);
            DA.GetData(8, ref missingStrategy);

            try
            {
                string mymlPath = GetMyMLPath();
                if (string.IsNullOrEmpty(mymlPath))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, 
                        L.T("err.package_missing"));
                    return;
                }

                // 将Tree转换为Python列表
                string xList = ConvertTreeToPythonList(xTree);
                string yList = hasY ? ConvertTreeToPythonList(yTree) : "None";
                
                // 处理X Names和y Names
                string xNamesStr = "None";
                if (hasXNames && xNamesList.Count > 0)
                {
                    xNamesStr = "[" + string.Join(", ", xNamesList.Select(n => $"\"{n.Replace("\"", "\\\"")}\"")) + "]";
                }
                
                string yNamesStr = "None";
                if (hasYNames && yNamesList.Count > 0)
                {
                    yNamesStr = "[" + string.Join(", ", yNamesList.Select(n => $"\"{n.Replace("\"", "\\\"")}\"")) + "]";
                }
                
                string escapedNormalizeMethod = normalizeMethod.Replace("'", "\\'").Replace("\"", "\\\"");
                string escapedMissingStrategy = missingStrategy.Replace("'", "\\'").Replace("\"", "\\\"");
                
                // 将C#布尔值转换为Python布尔值
                string normalizePython = normalize ? "True" : "False";
                string removeOutliersPython = removeOutliers ? "True" : "False";
                string handleMissingPython = handleMissing ? "True" : "False";

                string pythonCode = $@"
# -*- coding: utf-8 -*-
import sys
import os
import site
import io
import pickle
import base64
import warnings

# 抑制所有警告
warnings.filterwarnings('ignore')

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
from components.dataset_components import create_dataset
from core.data_preprocessing import DataPreprocessor

# 转换输入数据
X = {xList}
y = {yList} if {yList} != 'None' else None

# 处理X Names和y Names - 确保正确处理None和空列表
X_names = {xNamesStr} if {xNamesStr} != 'None' else None
y_names = {yNamesStr} if {yNamesStr} != 'None' else None

# 如果X_names是空列表，设为None
if isinstance(X_names, list) and len(X_names) == 0:
    X_names = None

# 如果y_names是空列表，设为None
if isinstance(y_names, list) and len(y_names) == 0:
    y_names = None

# 数据预处理
preprocessor = DataPreprocessor()
normalize_bool = {normalizePython}
remove_outliers_bool = {removeOutliersPython}
handle_missing_bool = {handleMissingPython}

if handle_missing_bool or remove_outliers_bool or normalize_bool:
    result = preprocessor.prepare_data(
        X,
        y,
        normalize=normalize_bool,
        normalize_method=r'{escapedNormalizeMethod}',
        remove_outliers=remove_outliers_bool,
        handle_missing=handle_missing_bool,
        missing_strategy=r'{escapedMissingStrategy}'
    )
    if y is not None:
        X_processed, labels_processed = result
    else:
        X_processed = result
        labels_processed = None
else:
    X_processed = X
    labels_processed = y

# 创建数据集 - 确保正确处理None值
try:
    dataset = create_dataset(X_processed, labels_processed, X_names=X_names, y_names=y_names)
    # 将已拟合预处理器打包进 Dataset，供训练/预测复用
    if normalize_bool or handle_missing_bool:
        dataset.set_preprocessor(preprocessor, {{
            'normalize': normalize_bool,
            'normalize_method': r'{escapedNormalizeMethod}',
            'handle_missing': handle_missing_bool,
            'missing_strategy': r'{escapedMissingStrategy}',
            'remove_outliers': remove_outliers_bool,
        }})
    
    # 获取数据集信息
    normalize_str = '是' if normalize_bool else '否'
    missing_str = '是' if handle_missing_bool else '否'
    outlier_str = '是' if remove_outliers_bool else '否'
    has_labels = '有' if dataset.y is not None else '无'
    packed = '是' if getattr(dataset, 'preprocessor', None) is not None else '否'
    info = f'数据集shape: {{dataset.X.shape}}, 标签: {{has_labels}}, 标准化: {{normalize_str}}, 处理缺失值: {{missing_str}}, 移除异常值: {{outlier_str}}, 预处理已打包: {{packed}}'
    
    # 返回数据集对象（序列化）
    dataset_bytes = pickle.dumps(dataset)
    dataset_str = base64.b64encode(dataset_bytes).decode('utf-8')
    print('OUTPUT_0:' + dataset_str)
    print('OUTPUT_1:' + info)
except Exception as e:
    error_msg = f'创建Dataset失败: {{str(e)}}'
    print('ERROR:' + error_msg, file=sys.stderr)
    raise
";

                string output = PythonScriptExecutor.ExecuteCode(pythonCode);
                
                // 检查是否有错误
                if (output.Contains("ERROR:"))
                {
                    string errorMsg = ExtractValue(output, "ERROR:");
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Python执行错误: {errorMsg}");
                    return;
                }
                
                string datasetStr = ExtractValue(output, "OUTPUT_0:");
                string infoStr = ExtractValue(output, "OUTPUT_1:");
                
                if (string.IsNullOrEmpty(datasetStr))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "未能获取Dataset输出");
                    return;
                }
                
                DA.SetData(0, datasetStr);
                DA.SetData(1, infoStr);
                
                // Readme输出
                string readme = @"组件名称: Create Dataset
功能: 准备数据（标准化、处理缺失值、移除异常值），创建数据集对象

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. X (特征数据) - Tree结构，必需
   • 数据类型: Tree结构
   • 数据结构: 每行数据作为一个分支，每个分支内的元素是该行的特征值
   • 示例: 
     {0} → [1.5, 2.3, 3.1, 4.2]
     {1} → [2.1, 3.4, 1.9, 5.0]

2. y (标签数据) - Tree结构，可选
   • 可以不连接，用于预测场景（无标签）
   • 如果连接，每个分支包含一个标签值
   • 示例:
     {0} → [0]
     {1} → [1]

3. X Names (X列名) - List，可选
   • 可以不连接，用于标识X每列的名称
   • 示例: ['feature1', 'feature2', 'feature3']

4. y Names (y列名) - List，可选
   • 可以不连接，用于标识y的名称
   • 示例: ['target']

5. Normalize (是否标准化) - Boolean，默认False

6. Normalize Method (标准化方法) - Text，默认'standard'

7. Remove Outliers (是否移除异常值) - Boolean，默认False

8. Handle Missing (是否处理缺失值) - Boolean，默认False

9. Missing Strategy (缺失值处理策略) - Text，默认'mean'

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Dataset - Dataset对象
2. Info - 数据集信息
3. Readme - 组件使用说明";
                DA.SetData(2, readme);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, L.T("err.exec_failed", ex.Message));
                RhinoApp.WriteLine($"SimpleML错误: {ex}");
            }
        }

        private string ConvertTreeToPythonList(Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> tree)
        {
            if (tree == null || tree.PathCount == 0)
                return "[]";
                
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
                    if (item is Grasshopper.Kernel.Types.GH_Number num)
                        sb.Append(num.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    else if (item is Grasshopper.Kernel.Types.GH_String str)
                        sb.Append($"\"{str.Value.Replace("\"", "\\\"")}\"");
                    else
                        sb.Append(item.ToString());
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

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(CreateDatasetComponent));
        public override Guid ComponentGuid => new Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");
    }
}
