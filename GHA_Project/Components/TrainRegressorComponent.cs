using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.ModelTraining
{
    public class TrainRegressorComponent : GH_Component
    {
        public TrainRegressorComponent()
          : base("训练回归器 Train Regressor", "训练回归器",
              "训练回归器（通用）",
              "SimpleML", "04 Model")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Dataset", "DS", "Dataset对象（必需），Dataset应包含X和y，以及X_names和y_names", GH_ParamAccess.item);
            pManager.AddTextParameter("Algorithm", "A", "算法参数配置（Text/JSON），可以来自算法特定训练组件的Algorithm Params输出，或直接使用算法名称字符串", GH_ParamAccess.item, "linear_regression");
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "训练好的模型对象（Generic类型）", GH_ParamAccess.item);
            pManager.AddGenericParameter("Model Info", "MI", "模型训练信息（Tree结构，包含算法、特征数量、目标值统计等详细信息）", GH_ParamAccess.tree);
            pManager.AddTextParameter("Readme", "R", "组件使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object datasetObj = null;
            string algorithm = "linear_regression";
            var xNamesList = new System.Collections.Generic.List<string>();
            var yNamesList = new System.Collections.Generic.List<string>();

            if (!DA.GetData(0, ref datasetObj))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "必须提供Dataset对象");
                return;
            }
            DA.GetData(1, ref algorithm);
            
            // X Names和Y Names现在从Dataset中获取，不再需要外部输入

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
                
                // 对于algorithm参数，需要特殊处理：将其转换为Python字符串字面量
                string algorithmPythonStr = ConvertToPythonString(algorithm);
                
                // 构建X Names和Y Names的Python列表字符串
                string xNamesStr = "None";
                if (xNamesList.Count > 0)
                {
                    var xNamesEscaped = new System.Collections.Generic.List<string>();
                    foreach (var name in xNamesList)
                    {
                        xNamesEscaped.Add($"'{name.Replace("'", "\\'")}'");
                    }
                    xNamesStr = "[" + string.Join(", ", xNamesEscaped) + "]";
                }
                
                string yNamesStr = "None";
                if (yNamesList.Count > 0)
                {
                    var yNamesEscaped = new System.Collections.Generic.List<string>();
                    foreach (var name in yNamesList)
                    {
                        yNamesEscaped.Add($"'{name.Replace("'", "\\'")}'");
                    }
                    yNamesStr = "[" + string.Join(", ", yNamesEscaped) + "]";
                }

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

from components.train_components import train_regressor

# 反序列化Dataset
try:
    dataset_bytes = base64.b64decode(r'''{datasetStr}''')
    dataset = pickle.loads(dataset_bytes)
except Exception as e:
    import sys
    import traceback
    error_msg = f'ERROR:无效的Dataset对象: {{str(e)}}\\n{{traceback.format_exc()}}'
    print(error_msg, file=sys.stderr)
    print(error_msg)
    exit(1)

# 调用训练函数（X_names和y_names现在从Dataset中获取）
algorithm_param = {algorithmPythonStr}
model, readme, model_info = train_regressor(dataset=dataset, algorithm=algorithm_param, X_names=None, y_names=None)

# 序列化模型
model_bytes = pickle.dumps(model)
model_str = base64.b64encode(model_bytes).decode('utf-8')

import json
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
                string readme = $@"组件名称: Train Regressor
功能: 训练回归模型（通用训练组件，支持所有回归算法）

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. Dataset (数据集) - Generic类型，必需
   • 数据类型: Dataset对象（Base64编码的pickle对象）
   • 数据结构: 包含特征数据X和标签数据y的Dataset对象
   • 连接建议:
     ← Split Dataset的Train Dataset输出（最常用）
     ← Create Dataset的Dataset输出（如果不分割数据）
   • 注意事项: 
     - 必须是有效的Dataset对象
     - 标签y必须是连续数值（回归任务）
     - 建议使用训练集，不要使用测试集训练

2. Algorithm (算法配置) - Text/JSON类型，默认'linear_regression'
   • 数据类型: 文本字符串或JSON格式
   • 两种使用方式:
     
     方式1: 使用算法特定训练组件（推荐）
     • 连接: ← Train Linear Regression的Algorithm Params输出
     • 连接: ← Train Ridge Regression的Algorithm Params输出
     • 连接: ← Train Lasso Regression的Algorithm Params输出
     • 连接: ← Train Random Forest Regressor的Algorithm Params输出
     • 连接: ← Train SVR的Algorithm Params输出
     • 连接: ← Train KNN Regressor的Algorithm Params输出
     • 优点: 可以详细配置算法参数
     
     方式2: 直接使用算法名称字符串
     • 可选值: 
       - 'linear_regression': 线性回归（默认）
       - 'ridge': 岭回归
       - 'lasso': Lasso回归
       - 'random_forest': 随机森林回归器
       - 'svr': 支持向量回归器
       - 'knn': K近邻回归器
     • 格式: 直接输入算法名称字符串，如 ""linear_regression""
     • 优点: 快速使用，使用默认参数
   
   • JSON格式示例（来自算法特定组件）:
     {{""algorithm"": ""linear_regression"", ""fit_intercept"": true}}

3. X Names (X列名列表) - Text类型，可选
   • 数据类型: 文本列表（List结构）
   • 说明: 指定dataset中哪些列作为特征（X）
   • 使用方式:
     - 如果dataset包含所有列的数据，使用此参数指定哪些列是特征
     - 可以是列名（字符串）或列索引（整数，作为字符串输入）
     - 例如: [""feature1"", ""feature2"", ""feature3""] 或 [""0"", ""1"", ""2""]
   • 注意事项:
     - 如果不提供，将使用dataset原有的X数据
     - 如果提供了X Names，将从dataset.X中提取对应的列

4. Y Names (y列名列表) - Text类型，可选
   • 数据类型: 文本列表（List结构）
   • 说明: 指定dataset中哪些列作为目标值（y）
   • 使用方式:
     - 如果dataset包含所有列的数据，使用此参数指定哪些列是目标值
     - 可以是列名（字符串）或列索引（整数，作为字符串输入）
     - 例如: [""target""] 或 [""3""]
   • 注意事项:
     - 如果不提供，将使用dataset原有的y数据
     - 如果提供了Y Names，将从dataset.X中提取对应的列作为目标值

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Model (模型) - Generic类型，必需
   • 数据类型: 训练好的回归模型对象（Base64编码的pickle对象）
   • 用途: 用于预测和评估
   • 连接建议:
     → Predict Regressor的Model输入（进行预测）
     → Evaluate Regression的Model输入（评估模型）
     → Save Model的Model输入（保存模型）
   • 说明: 模型已训练完成，可以直接用于预测新数据

═══════════════════════════════════════════════════════════════
典型工作流程:
═══════════════════════════════════════════════════════════════

方式1: 使用算法特定组件（推荐）
Create Dataset → Split Dataset
  → Train Dataset → Train Linear Regression → Algorithm Params
  → Algorithm Params → Train Regressor → Model
  → Model → Predict Regressor / Evaluate Regression

方式2: 直接使用算法名称
Create Dataset → Split Dataset
  → Train Dataset → Train Regressor (Algorithm='linear_regression') → Model
  → Model → Predict Regressor / Evaluate Regression

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. Dataset必须是回归数据集（标签是连续数值，不是类别）
2. 建议使用训练集训练，不要使用测试集
3. 使用算法特定组件可以更好地控制算法参数
4. 训练完成后，模型可以用于预测和评估
5. 建议先分割数据再训练，避免过拟合

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

        private string ConvertToPythonString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return "''";
            
            // 转义特殊字符，使其成为Python字符串字面量
            // 使用三重引号来避免转义问题
            // 注意：在C#字符串插值中，{需要转义为{{，但这里我们返回的字符串会被插入到Python代码中
            // 所以需要确保返回的字符串本身是正确的Python字符串字面量
            StringBuilder sb = new StringBuilder();
            sb.Append("r'''");
            foreach (char c in str)
            {
                if (c == '\\')
                    sb.Append("\\\\");
                else if (c == '\'')
                    sb.Append("\\'");
                else if (c == '\r')
                    sb.Append("\\r");
                else if (c == '\n')
                    sb.Append("\\n");
                else if (c == '\t')
                    sb.Append("\\t");
                else
                    sb.Append(c);
            }
            sb.Append("'''");
            return sb.ToString();
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

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(TrainRegressorComponent));
        public override Guid ComponentGuid => new Guid("F2A3B4C5-D6E7-8901-5678-901234567891");
    }
}
