using System;
using System.IO;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.DatasetManagement
{
    public class SplitDatasetComponent : GH_Component
    {
        public SplitDatasetComponent()
          : base("Split Dataset", "SplitDS",
              "分割数据集为训练集和测试集",
              "SimpleML", "03 Dataset")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Dataset", "DS", "Dataset对象", GH_ParamAccess.item);
            pManager.AddNumberParameter("Test Size", "TS", "测试集比例，默认0.2", GH_ParamAccess.item, 0.2);
            pManager.AddIntegerParameter("Random State", "RS", "随机种子，默认42", GH_ParamAccess.item, 42);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Train Dataset", "TrainDS", "训练集Dataset对象", GH_ParamAccess.item);
            pManager.AddGenericParameter("Test Dataset", "TestDS", "测试集Dataset对象", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "组件使用说明和注意事项", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object datasetObj = null;
            double testSize = 0.2;
            int randomState = 42;

            if (!DA.GetData(0, ref datasetObj)) return;
            DA.GetData(1, ref testSize);
            DA.GetData(2, ref randomState);

            try
            {
                string mymlPath = GetMyMLPath();
                if (string.IsNullOrEmpty(mymlPath))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, 
                        "未找到myML文件夹。请设置SIMPLEML_PATH环境变量。");
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

from components.dataset_components import split_dataset

# 反序列化数据集
try:
    dataset_bytes = base64.b64decode(r'''{datasetStr}''')
    dataset = pickle.loads(dataset_bytes)
except:
    print('ERROR:无效的Dataset对象')
    exit(1)

# 分割数据集
train_dataset, test_dataset = split_dataset(dataset, test_size={testSize}, random_state={randomState})

# 序列化结果
train_bytes = pickle.dumps(train_dataset)
test_bytes = pickle.dumps(test_dataset)
train_str = base64.b64encode(train_bytes).decode('utf-8')
test_str = base64.b64encode(test_bytes).decode('utf-8')

print('TRAIN:' + train_str)
print('TEST:' + test_str)
";

                string output = PythonScriptExecutor.ExecuteCode(pythonCode);
                DA.SetData(0, ExtractValue(output, "TRAIN:"));
                DA.SetData(1, ExtractValue(output, "TEST:"));
                
                // Readme输出
                string readme = @"组件名称: Split Dataset
功能: 将数据集分割为训练集和测试集，用于模型训练和评估

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. Dataset (数据集) - Generic类型，必需
   • 数据类型: Dataset对象（Base64编码的pickle对象）
   • 数据结构: 包含特征数据X和标签数据y的Dataset对象
   • 连接建议:
     ← Create Dataset的Dataset输出（最常用）
     ← Load Dataset的Dataset输出（如果已创建Dataset）
   • 注意事项: 必须是有效的Dataset对象，不能是原始Tree数据

2. Test Size (测试集比例) - Number类型，默认0.2
   • 取值范围: 0.0 到 1.0 之间的小数
   • 常用值:
     - 0.2: 20%作为测试集，80%作为训练集（最常用）
     - 0.3: 30%作为测试集，70%作为训练集
     - 0.1: 10%作为测试集，90%作为训练集（数据量很大时）
   • 建议:
     - 数据量较大（>1000样本）: 使用0.2或0.3
     - 数据量较小（<500样本）: 使用0.2，确保测试集有足够样本
     - 数据量很小（<100样本）: 考虑使用交叉验证而不是简单分割

3. Random State (随机种子) - Integer类型，默认42
   • 取值范围: 任意整数，或None（不设置）
   • 说明:
     - 设置随机种子可以确保每次分割结果相同（可重复）
     - 相同种子 + 相同数据 = 相同的训练集/测试集分割
     - None或不设置: 每次运行分割结果可能不同
   • 建议:
     - 开发调试时: 使用固定种子（如42）确保结果可重复
     - 生产环境: 可以使用None，获得不同的随机分割

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Train Dataset (训练集) - Generic类型，必需
   • 数据类型: Dataset对象
   • 用途: 用于模型训练
   • 连接建议:
     → Train Classifier的Dataset输入
     → Train Regressor的Dataset输入
     → Train Cluster的Dataset输入
   • 说明: 包含大部分数据（默认80%），用于训练模型

2. Test Dataset (测试集) - Generic类型，必需
   • 数据类型: Dataset对象
   • 用途: 用于模型评估和验证
   • 连接建议:
     → Evaluate Classification的X和Y True输入
     → Evaluate Regression的X和Y True输入
     → Predict Classifier/Regressor的X输入（用于预测）
   • 说明: 包含小部分数据（默认20%），用于评估模型性能

═══════════════════════════════════════════════════════════════
典型工作流程:
═══════════════════════════════════════════════════════════════

分类任务完整流程:
Create Dataset → Split Dataset 
  → Train Dataset → Train Classifier → Model
  → Test Dataset → Predict Classifier → Predictions
  → Test Dataset → Evaluate Classification → Metrics

回归任务完整流程:
Create Dataset → Split Dataset
  → Train Dataset → Train Regressor → Model
  → Test Dataset → Predict Regressor → Predictions
  → Test Dataset → Evaluate Regression → Metrics

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. 输入必须是有效的Dataset对象（来自Create Dataset）
2. Test Size应在0到1之间，通常使用0.2或0.3
3. Random State用于控制随机分割，相同种子会产生相同分割
4. 分割是随机的，但保持标签分布（分层采样）
5. 训练集和测试集互不重叠，确保评估结果可靠
6. 建议在训练前分割数据，避免数据泄露";
                DA.SetData(2, readme);
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

        private string ExtractValue(string output, string prefix)
        {
            int startIndex = output.IndexOf(prefix);
            if (startIndex == -1) return string.Empty;
            startIndex += prefix.Length;
            int endIndex = output.IndexOf('\n', startIndex);
            if (endIndex == -1) endIndex = output.Length;
            return output.Substring(startIndex, endIndex - startIndex).Trim();
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(SplitDatasetComponent));
        public override Guid ComponentGuid => new Guid("B8C9D0E1-F2A3-4567-1234-567890123458");
    }
}
