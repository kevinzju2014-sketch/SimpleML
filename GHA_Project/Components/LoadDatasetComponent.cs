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
    /// Load Dataset Component
    /// 加载scikit-learn内置数据集组件
    /// </summary>
    public class LoadDatasetComponent : GH_Component
    {
        public LoadDatasetComponent()
          : base("Load Dataset", "LoadDS",
              "加载scikit-learn内置数据集，供测试使用",
              "SimpleML", "01 Input")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Dataset Name", "DN", "数据集名称，默认'iris'（可选：iris, wine, breast_cancer, digits, diabetes, california_housing, linnerud, make_classification, make_regression, make_blobs）", GH_ParamAccess.item, "iris");
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "数据Tree结构，每行数据作为一个分支", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Labels", "L", "列名的列表（Tree结构，单个分支）", GH_ParamAccess.tree);
            pManager.AddTextParameter("Info", "I", "数据集信息（描述、样本数、特征数等）", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "组件使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string datasetName = "iris";

            if (!DA.GetData(0, ref datasetName)) return;

            if (string.IsNullOrWhiteSpace(datasetName))
            {
                datasetName = "iris";
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

                string escapedDatasetName = datasetName.Replace("'", "\\'").Replace("\"", "\\\"");

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
import numpy as np
from sklearn import datasets

# 定义load_sklearn_dataset函数（内联以避免导入问题）
def load_sklearn_dataset(dataset_name='iris', return_X_y=False):
    dataset_name = dataset_name.lower()
    
    if dataset_name == 'iris':
        data_obj = datasets.load_iris()
        info = ""鸢尾花数据集（分类）：150个样本，4个特征，3个类别""
    elif dataset_name == 'wine':
        data_obj = datasets.load_wine()
        info = ""葡萄酒数据集（分类）：178个样本，13个特征，3个类别""
    elif dataset_name == 'breast_cancer':
        data_obj = datasets.load_breast_cancer()
        info = ""乳腺癌数据集（分类）：569个样本，30个特征，2个类别""
    elif dataset_name == 'digits':
        data_obj = datasets.load_digits()
        info = ""手写数字数据集（分类）：1797个样本，64个特征，10个类别（0-9）""
    elif dataset_name == 'diabetes':
        data_obj = datasets.load_diabetes()
        info = ""糖尿病数据集（回归）：442个样本，10个特征""
    elif dataset_name == 'boston':
        try:
            data_obj = datasets.fetch_california_housing()
            info = ""注意：波士顿房价数据集已弃用，已替换为加州房价数据集（回归）：20640个样本，8个特征""
        except:
            data_obj = datasets.load_diabetes()
            info = ""波士顿数据集不可用，已使用糖尿病数据集（回归）：442个样本，10个特征""
    elif dataset_name == 'california_housing':
        data_obj = datasets.fetch_california_housing()
        info = ""加州房价数据集（回归）：20640个样本，8个特征""
    elif dataset_name == 'linnerud':
        data_obj = datasets.load_linnerud()
        info = ""Linnerud数据集（多输出回归）：20个样本，3个特征，3个目标""
    elif dataset_name == 'make_classification':
        X, y = datasets.make_classification(n_samples=100, n_features=4, n_informative=2, 
                                           n_redundant=0, n_classes=2, random_state=42)
        feature_names = [f'feature_{{i}}' for i in range(X.shape[1])]
        data_obj = type('obj', (object,), {{
            'data': X,
            'target': y,
            'feature_names': feature_names,
            'target_names': ['class_0', 'class_1'],
            'DESCR': '生成的分类数据集'
        }})()
        info = ""生成的分类数据集：100个样本，4个特征，2个类别""
    elif dataset_name == 'make_regression':
        X, y = datasets.make_regression(n_samples=100, n_features=4, n_informative=2, 
                                       noise=10, random_state=42)
        feature_names = [f'feature_{{i}}' for i in range(X.shape[1])]
        data_obj = type('obj', (object,), {{
            'data': X,
            'target': y,
            'feature_names': feature_names,
            'DESCR': '生成的回归数据集'
        }})()
        info = ""生成的回归数据集：100个样本，4个特征""
    elif dataset_name == 'make_blobs':
        X, y = datasets.make_blobs(n_samples=100, n_features=2, centers=3, 
                                   random_state=42)
        feature_names = [f'feature_{{i}}' for i in range(X.shape[1])]
        data_obj = type('obj', (object,), {{
            'data': X,
            'target': y,
            'feature_names': feature_names,
            'DESCR': '生成的聚类数据集'
        }})()
        info = ""生成的聚类数据集：100个样本，2个特征，3个聚类中心""
    else:
        raise ValueError(f""未知的数据集名称: {{dataset_name}}。可用数据集: iris, wine, breast_cancer, digits, diabetes, california_housing, linnerud, make_classification, make_regression, make_blobs"")
    
    if return_X_y:
        X = data_obj.data
        y = data_obj.target
        feature_names = data_obj.feature_names if hasattr(data_obj, 'feature_names') else [f'feature_{{i}}' for i in range(X.shape[1])]
        target_names = data_obj.target_names if hasattr(data_obj, 'target_names') else None
        return X, y, feature_names, target_names, info
    else:
        X = data_obj.data
        y = data_obj.target
        if hasattr(data_obj, 'feature_names'):
            feature_names = data_obj.feature_names
        else:
            feature_names = [f'feature_{{i}}' for i in range(X.shape[1])]
        data_dict = {{}}
        for i, name in enumerate(feature_names):
            data_dict[name] = X[:, i]
        data_dict['target'] = y
        data = pd.DataFrame(data_dict)
        columns = data.columns.tolist()
        shape = data.shape
        return data, columns, shape, info

# 加载数据集
data, columns, shape, info = load_sklearn_dataset(dataset_name=r'{escapedDatasetName}', return_X_y=False)

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
info_str = f'{{info}}, 行数: {{shape[0]}}, 列数: {{shape[1]}}'

print('OUTPUT_0:' + data_tree_json)
print('OUTPUT_1:' + labels_json)
print('OUTPUT_2:' + info_str)
";

                string output = PythonScriptExecutor.ExecuteCode(pythonCode);
                string dataTreeJson = ExtractValue(output, "OUTPUT_0:");
                string labelsJson = ExtractValue(output, "OUTPUT_1:");
                string infoStr = ExtractValue(output, "OUTPUT_2:");
                
                // Readme输出（在C#中直接定义，避免Python代码字符串中的三引号问题）
                string readme = GetReadmeText();

                GH_Structure<GH_String> dataTree = TreeConverter.ConvertJsonToTree(dataTreeJson);
                GH_Structure<GH_String> labelsTree = TreeConverter.ConvertJsonToTree(labelsJson);

                DA.SetDataTree(0, dataTree);
                DA.SetDataTree(1, labelsTree);
                DA.SetData(2, infoStr);
                DA.SetData(3, readme);
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

        private string GetReadmeText()
        {
            // Readme文本在C#中直接定义，避免Python代码字符串中的三引号问题
            // 由于文本很长，这里只包含关键信息，详细内容可以后续补充
            return @"组件名称: Load Dataset
功能: 加载scikit-learn内置数据集，供测试和学习使用

═══════════════════════════════════════════════════════════════
可用数据集详细列表
═══════════════════════════════════════════════════════════════

【分类数据集 - Classification Datasets】

1. iris（鸢尾花数据集）⭐ 推荐入门
   • 数据集名称: ""iris""
   • 样本数: 150
   • 特征数: 4（sepal_length, sepal_width, petal_length, petal_width）
   • 类别数: 3（setosa, versicolor, virginica）
   • 特点: 经典入门数据集，数据平衡，无缺失值
   • 适用场景: 分类算法学习、数据可视化、特征工程练习
   • 推荐算法: KNN, Decision Tree, Random Forest, SVM

2. wine（葡萄酒数据集）
   • 数据集名称: ""wine""
   • 样本数: 178
   • 特征数: 13（酒精、苹果酸、灰分等化学成分）
   • 类别数: 3（三种葡萄酒类型）
   • 特点: 多特征数据集，适合特征选择练习
   • 适用场景: 特征选择、降维分析、多分类问题
   • 推荐算法: Random Forest, SVM, Logistic Regression

3. breast_cancer（乳腺癌数据集）
   • 数据集名称: ""breast_cancer""
   • 样本数: 569
   • 特征数: 30（细胞核特征：半径、纹理、周长等）
   • 类别数: 2（良性/恶性）
   • 特点: 二分类问题，特征较多，适合特征重要性分析
   • 适用场景: 二分类学习、特征重要性分析、医疗数据分析
   • 推荐算法: Random Forest, SVM, Logistic Regression, Naive Bayes

4. digits（手写数字数据集）
   • 数据集名称: ""digits""
   • 样本数: 1797
   • 特征数: 64（8x8像素图像）
   • 类别数: 10（数字0-9）
   • 特点: 图像数据，特征维度较高，适合降维和特征提取
   • 适用场景: 图像分类、降维分析、多分类问题
   • 推荐算法: KNN, SVM, Random Forest, Neural Network

【回归数据集 - Regression Datasets】

5. diabetes（糖尿病数据集）
   • 数据集名称: ""diabetes""
   • 样本数: 442
   • 特征数: 10（年龄、性别、BMI、血压等生理指标）
   • 目标: 连续值（疾病进展指标）
   • 特点: 中等规模，特征已标准化
   • 适用场景: 回归算法学习、特征重要性分析
   • 推荐算法: Linear Regression, Ridge, Lasso, Random Forest Regressor

6. california_housing（加州房价数据集）⭐ 推荐
   • 数据集名称: ""california_housing""
   • 样本数: 20640
   • 特征数: 8（经度、纬度、房龄、房间数等）
   • 目标: 连续值（房价中位数）
   • 特点: 大规模数据集，适合性能测试，包含地理信息
   • 适用场景: 大规模回归、特征工程、模型性能评估
   • 推荐算法: Random Forest Regressor, Gradient Boosting, Linear Regression
   • 注意: 数据量较大，处理时间可能较长

7. linnerud（Linnerud数据集）
   • 数据集名称: ""linnerud""
   • 样本数: 20
   • 特征数: 3（运动指标）
   • 目标数: 3（生理指标）- 多输出回归
   • 特点: 小样本，多输出回归问题
   • 适用场景: 多输出回归学习、小样本分析
   • 推荐算法: Multi-output Regression

【生成数据集 - Synthetic Datasets】（用于测试和演示）

8. make_classification（生成分类数据集）
   • 数据集名称: ""make_classification""
   • 样本数: 100（可配置）
   • 特征数: 4（可配置）
   • 类别数: 2（可配置）
   • 特点: 可控制难度，适合算法测试
   • 适用场景: 算法对比测试、快速原型开发
   • 推荐算法: 所有分类算法

9. make_regression（生成回归数据集）
   • 数据集名称: ""make_regression""
   • 样本数: 100（可配置）
   • 特征数: 4（可配置）
   • 特点: 线性关系，可控制噪声水平
   • 适用场景: 回归算法测试、快速验证
   • 推荐算法: 所有回归算法

10. make_blobs（生成聚类数据集）
    • 数据集名称: ""make_blobs""
    • 样本数: 100（可配置）
    • 特征数: 2（可配置）
    • 聚类数: 3（可配置）
    • 特点: 明显的聚类结构，适合聚类算法演示
    • 适用场景: 聚类算法学习、可视化演示
    • 推荐算法: K-Means, DBSCAN, Agglomerative Clustering

═══════════════════════════════════════════════════════════════
输入参数详解
═══════════════════════════════════════════════════════════════

Dataset Name (数据集名称) - Text类型，默认""iris""
   • 可选值: 
     - 分类: ""iris"", ""wine"", ""breast_cancer"", ""digits""
     - 回归: ""diabetes"", ""california_housing"", ""linnerud""
     - 生成: ""make_classification"", ""make_regression"", ""make_blobs""
   • 格式: 字符串，不区分大小写
   • 示例: ""iris"", ""Iris"", ""IRIS"" 都可以
   • 如果输入未知名称，会显示错误信息并列出所有可用数据集

═══════════════════════════════════════════════════════════════
输出参数详解
═══════════════════════════════════════════════════════════════

1. Data (数据) - Tree结构
   • 数据类型: Grasshopper Tree结构
   • 数据结构: 
     - 每个分支代表一行数据（一个样本）
     - 分支内的元素是该行的所有列值（特征值 + target值）
     - 分支路径为 {0}, {1}, {2}...（行索引）
   • 列结构: 
     - 前N列: 特征列（feature columns）
     - 最后一列: target列（标签列）
   • 示例（iris数据集）:
     {0} → [""5.1"", ""3.5"", ""1.4"", ""0.2"", ""0""]  (4个特征 + 1个target)
     {1} → [""4.9"", ""3.0"", ""1.4"", ""0.2"", ""0""]
   • 连接建议:
     → Create Dataset的X输入（特征数据）
     → Calculate Statistics的Data输入（统计分析）
     → Calculate Correlation的Data输入（相关性分析）

2. Labels (列名) - Tree结构
   • 数据类型: Tree结构，单个分支
   • 数据结构: 单个分支 {0}，包含所有列名
   • 内容: 所有特征列名 + ""target""
   • 示例（iris数据集）:
     {0} → [""sepal_length"", ""sepal_width"", ""petal_length"", ""petal_width"", ""target""]
   • 用途: 
     - 了解数据集的列名结构
     - 在Grasshopper中作为列表使用
     - 与其他数据源进行列名对比

3. Info (信息) - Text类型
   • 内容: 数据集的基本信息
   • 包含: 数据集描述、样本数、特征数、类别数（如果是分类）
   • 格式: 可读的文本格式
   • 示例: ""鸢尾花数据集（分类）：150个样本，4个特征，3个类别, 行数: 150, 列数: 5""

═══════════════════════════════════════════════════════════════
典型工作流程
═══════════════════════════════════════════════════════════════

【分类任务流程】
Load Dataset (iris) 
  → Create Dataset (X=Data的前4列, Labels=target列)
  → Split Dataset (训练集/测试集分割)
  → Train Classifier (选择算法: KNN, Random Forest等)
  → Evaluate Classification (评估模型)
  → Predict Classifier (预测新数据)

【回归任务流程】
Load Dataset (diabetes)
  → Create Dataset (X=Data的前10列, Labels=target列)
  → Split Dataset
  → Train Regressor (选择算法: Linear Regression, Random Forest等)
  → Evaluate Regression (评估模型)
  → Predict Regressor (预测新数据)

【聚类任务流程】
Load Dataset (make_blobs)
  → Create Dataset (X=Data的前2列, 不需要Labels)
  → Train Cluster (选择算法: K-Means, DBSCAN等)
  → Evaluate Clustering (评估聚类结果)
  → Predict Cluster (预测新数据的聚类)

【数据分析流程】
Load Dataset (wine)
  → Calculate Statistics (统计分析)
  → Calculate Correlation (相关性分析)
  → Create Dataset (数据预处理)

═══════════════════════════════════════════════════════════════
数据集选择建议
═══════════════════════════════════════════════════════════════

【初学者推荐】
• iris - 最经典的入门数据集，数据简单清晰，适合学习基本概念
• diabetes - 中等规模，适合学习回归算法

【进阶学习】
• wine - 多特征分类，适合学习特征工程
• breast_cancer - 二分类问题，适合学习模型评估
• california_housing - 大规模数据，适合学习性能优化

【算法测试】
• make_classification - 快速生成测试数据
• make_regression - 快速生成回归测试数据
• make_blobs - 快速生成聚类测试数据

【特定场景】
• digits - 图像分类、降维分析
• linnerud - 多输出回归问题
• california_housing - 大规模数据处理

═══════════════════════════════════════════════════════════════
注意事项
═══════════════════════════════════════════════════════════════

1. 所有数据集都包含target列（标签列）
   • 分类数据集: target是类别编号（0, 1, 2...）
   • 回归数据集: target是连续数值

2. 数据格式
   • 所有数据都是数值型（字符串会被转换为数值）
   • 无缺失值（NaN）
   • 特征列在前，target列在最后

3. 大数据集处理
   • california_housing (20640样本) 处理时间较长
   • digits (1797样本) 特征维度较高（64维）
   • 建议先用小数据集测试工作流程

4. 生成数据集
   • make_* 数据集使用固定随机种子，结果可重复
   • 适合快速测试和算法对比
   • 不适合实际应用场景

5. 数据分离
   • 使用Create Dataset组件时，需要手动分离特征和标签
   • X输入: Data的前N列（特征列）
   • Labels输入: Data的最后一列（target列）

═══════════════════════════════════════════════════════════════
快速参考表
═══════════════════════════════════════════════════════════════

数据集名称          | 类型    | 样本数 | 特征数 | 类别/目标数 | 推荐用途
-------------------|---------|--------|--------|-------------|------------------
iris               | 分类    | 150    | 4      | 3           | 入门学习 ⭐
wine               | 分类    | 178    | 13     | 3           | 特征工程
breast_cancer      | 分类    | 569    | 30     | 2           | 二分类学习
digits             | 分类    | 1797   | 64     | 10          | 图像分类
diabetes           | 回归    | 442    | 10     | 1           | 回归学习
california_housing | 回归    | 20640  | 8      | 1           | 大规模回归 ⭐
linnerud           | 多输出  | 20     | 3      | 3           | 多输出回归
make_classification| 分类    | 100    | 4      | 2           | 快速测试
make_regression    | 回归    | 100    | 4      | 1           | 快速测试
make_blobs         | 聚类    | 100    | 2      | 3           | 聚类演示

═══════════════════════════════════════════════════════════════";
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

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(LoadDatasetComponent));
        public override Guid ComponentGuid => new Guid("E3F4A5B6-C7D8-9012-EF01-234567890124");
    }
}
