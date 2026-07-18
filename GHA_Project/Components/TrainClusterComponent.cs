using System;
using System.IO;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.ModelTraining
{
    public class TrainClusterComponent : GH_Component
    {
        public TrainClusterComponent()
          : base("训练聚类 Train Cluster", "训练聚类",
              "训练聚类模型（通用）",
              "SimpleML", "04 Model")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Dataset", "DS", "Dataset对象（必需）", GH_ParamAccess.item);
            pManager.AddTextParameter("Algorithm", "A", "算法参数配置（Text/JSON），可以来自算法特定训练组件的Algorithm Params输出，或直接使用算法名称字符串", GH_ParamAccess.item, "kmeans");
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "训练好的模型对象（Generic类型）", GH_ParamAccess.item);
            pManager.AddGenericParameter("Model Info", "MI", "模型训练信息（Tree结构，包含算法、特征数量、聚类数量等详细信息）", GH_ParamAccess.tree);
            pManager.AddTextParameter("Readme", "R", "组件使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object datasetObj = null;
            string algorithm = "kmeans";

            if (!DA.GetData(0, ref datasetObj))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "必须提供Dataset对象");
                return;
            }
            DA.GetData(1, ref algorithm);

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
                string escapedAlgorithm = algorithm.Replace("'", "\\'").Replace("\"", "\\\"");

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

from components.train_components import train_cluster

# 反序列化Dataset
try:
    dataset_bytes = base64.b64decode(r'''{datasetStr}''')
    dataset = pickle.loads(dataset_bytes)
except Exception as e:
    print('ERROR:无效的Dataset对象')
    exit(1)

# 调用训练函数
result = train_cluster(dataset=dataset, algorithm=r'''{escapedAlgorithm}''')
# 处理返回值（可能是2个或3个值）
if isinstance(result, tuple) and len(result) == 3:
    model, readme, model_info = result
elif isinstance(result, tuple) and len(result) == 2:
    model, readme = result
    model_info = '模型训练信息不可用'
else:
    model = result
    readme = '已使用算法训练聚类模型'
    model_info = '模型训练信息不可用'

# 序列化模型
model_bytes = pickle.dumps(model)
model_str = base64.b64encode(model_bytes).decode('utf-8')

print('OUTPUT_0:' + model_str)
print('OUTPUT_1:' + readme)
print('OUTPUT_2:' + model_info)  # model_info已经是JSON格式
";

                string output = PythonScriptExecutor.ExecuteCode(pythonCode);
                string modelStr = ExtractValue(output, "OUTPUT_0:");
                string pythonReadme = ExtractValue(output, "OUTPUT_1:");
                string modelInfoJson = ExtractValue(output, "OUTPUT_2:");
                
                // 将Model Info JSON转换为Tree结构
                GH_Structure<GH_String> modelInfoTree = TreeConverter.ConvertJsonToTree(modelInfoJson);
                
                // 生成详细的Readme
                string readme = $@"组件名称: Train Cluster
功能: 训练聚类模型（通用训练组件，支持所有聚类算法）

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. Dataset (数据集) - Generic类型，必需
   • 数据类型: Dataset对象（Base64编码的pickle对象）
   • 数据结构: 包含特征数据X的Dataset对象（聚类任务不需要标签）
   • 连接建议:
     ← Create Dataset的Dataset输出（最常用，聚类任务不需要标签）
     ← Split Dataset的Train Dataset输出（如果已分割数据）
   • 注意事项: 
     - 必须是有效的Dataset对象
     - 聚类任务不需要标签，Dataset可以没有y
     - 建议使用全部数据进行训练（聚类通常不需要测试集）

2. Algorithm (算法配置) - Text/JSON类型，默认'kmeans'
   • 数据类型: 文本字符串或JSON格式
   • 两种使用方式:
     
     方式1: 使用算法特定训练组件（推荐）
     • 连接: ← Train K-Means的Algorithm Params输出
     • 连接: ← Train DBSCAN的Algorithm Params输出
     • 连接: ← Train Agglomerative Clustering的Algorithm Params输出
     • 优点: 可以详细配置算法参数
     
     方式2: 直接使用算法名称字符串
     • 可选值: 
       - 'kmeans': K-Means聚类（默认）
       - 'dbscan': DBSCAN聚类
       - 'agglomerative': 层次聚类
     • 格式: 直接输入算法名称字符串，如 ""kmeans""
     • 优点: 快速使用，使用默认参数
   
   • JSON格式示例（来自算法特定组件）:
     {{""algorithm"": ""kmeans"", ""n_clusters"": 3}}

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Model (模型) - Generic类型，必需
   • 数据类型: 训练好的聚类模型对象（Base64编码的pickle对象）
   • 用途: 用于预测和评估
   • 连接建议:
     → Predict Cluster的Model输入（进行预测，分配聚类标签）
     → Evaluate Clustering的Model输入（评估聚类性能）
     → Save Model的Model输入（保存模型）
   • 说明: 模型已训练完成，可以直接用于预测新数据

═══════════════════════════════════════════════════════════════
典型工作流程:
═══════════════════════════════════════════════════════════════

方式1: 使用算法特定组件（推荐）
Create Dataset → Train K-Means → Algorithm Params
  → Algorithm Params → Train Cluster → Model
  → Model → Predict Cluster / Evaluate Clustering

方式2: 直接使用算法名称
Create Dataset → Train Cluster (Algorithm='kmeans') → Model
  → Model → Predict Cluster / Evaluate Clustering

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. Dataset可以是无标签的（聚类任务不需要标签）
2. 聚类任务通常使用全部数据，不需要分割训练集和测试集
3. 使用算法特定组件可以更好地控制算法参数（如聚类数量）
4. 训练完成后，模型可以用于预测和评估
5. 某些聚类算法（如DBSCAN）可能无法预测新数据

{pythonReadme}";
                
                DA.SetData(0, modelStr);
                DA.SetDataTree(1, modelInfoTree);
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
            return PathResolver.GetMyMLPath();
        }

        private string ExtractValue(string output, string prefix)
        {
            int startIndex = output.IndexOf(prefix);
            if (startIndex == -1) return string.Empty;
            startIndex += prefix.Length;
            
            // 查找下一个 OUTPUT_ 前缀来确定结束位置（用于多行JSON）
            int endIndex = output.Length;
            for (int i = startIndex; i < output.Length - 7; i++)
            {
                if (output.Substring(i, 7) == "OUTPUT_")
                {
                    endIndex = i;
                    break;
                }
            }
            
            return output.Substring(startIndex, endIndex - startIndex).Trim();
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(TrainClusterComponent));
        public override Guid ComponentGuid => new Guid("A3B4C5D6-E7F8-9012-6789-012345678902");
    }
}
