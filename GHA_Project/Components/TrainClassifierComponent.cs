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
    public class TrainClassifierComponent : GH_Component
    {
        public TrainClassifierComponent()
          : base("训练分类器 Train Classifier", "训练分类器",
              "训练分类器（通用）",
              "SimpleML", "04 Model")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Dataset", "DS", "Dataset对象（必需），Dataset应包含X和y，以及X_names和y_names", GH_ParamAccess.item);
            pManager.AddTextParameter("Algorithm", "A", "算法参数配置（Text/JSON），可以来自算法特定训练组件的Algorithm Params输出，或直接使用算法名称字符串", GH_ParamAccess.item, "random_forest");
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "训练好的模型对象（Generic类型）", GH_ParamAccess.item);
            pManager.AddGenericParameter("Model Info", "MI", "模型训练信息（Tree结构，包含算法、特征数量、类别数量等详细信息）", GH_ParamAccess.tree);
            pManager.AddTextParameter("Readme", "R", "组件使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object datasetObj = null;
            string algorithm = "random_forest";
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
                // 使用repr()函数或手动转义，确保JSON字符串能正确传递
                string algorithmPythonStr = ConvertToPythonString(algorithm);
                
                // X Names和Y Names现在从Dataset中获取，不再需要外部输入

                // 构建Python代码，使用字符串拼接避免大括号转义问题
                StringBuilder pythonCodeBuilder = new StringBuilder();
                pythonCodeBuilder.AppendLine("# -*- coding: utf-8 -*-");
                pythonCodeBuilder.AppendLine("import sys");
                pythonCodeBuilder.AppendLine("import os");
                pythonCodeBuilder.AppendLine("import json");
                pythonCodeBuilder.AppendLine("import site");
                pythonCodeBuilder.AppendLine("import io");
                pythonCodeBuilder.AppendLine("import pickle");
                pythonCodeBuilder.AppendLine("import base64");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 设置标准输出编码为UTF-8，避免中文输出错误");
                pythonCodeBuilder.AppendLine("if sys.stdout.encoding != 'utf-8':");
                pythonCodeBuilder.AppendLine("    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')");
                pythonCodeBuilder.AppendLine("if sys.stderr.encoding != 'utf-8':");
                pythonCodeBuilder.AppendLine("    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine($"# 添加项目路径");
                pythonCodeBuilder.AppendLine($"sys.path.insert(0, r'{mymlPath}')");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 确保Rhino Python的site-packages在路径中");
                pythonCodeBuilder.AppendLine("try:");
                pythonCodeBuilder.AppendLine("    site_packages = site.getsitepackages()");
                pythonCodeBuilder.AppendLine("    for sp in site_packages:");
                pythonCodeBuilder.AppendLine("        if sp not in sys.path:");
                pythonCodeBuilder.AppendLine("            sys.path.insert(0, sp)");
                pythonCodeBuilder.AppendLine("    ");
                pythonCodeBuilder.AppendLine("    rhino_site_envs = str(next((p for root in [__import__('pathlib').Path.home()/'.rhinocode', __import__('pathlib').Path.home()/'Library'/'Application Support'/'McNeel'/'Rhinoceros'/'.rhinocode'] if root.exists() for p in root.glob('py*-rh*/site-envs') if p.is_dir()), __import__('pathlib').Path.home()/'.rhinocode'/'site-envs'))");
                pythonCodeBuilder.AppendLine("    if os.path.exists(rhino_site_envs):");
                pythonCodeBuilder.AppendLine("        for item in os.listdir(rhino_site_envs):");
                pythonCodeBuilder.AppendLine("            env_path = os.path.join(rhino_site_envs, item)");
                pythonCodeBuilder.AppendLine("            if os.path.isdir(env_path):");
                pythonCodeBuilder.AppendLine("                if env_path not in sys.path:");
                pythonCodeBuilder.AppendLine("                    sys.path.insert(0, env_path)");
                pythonCodeBuilder.AppendLine("                site_pkg = os.path.join(env_path, 'Lib', 'site-packages')");
                pythonCodeBuilder.AppendLine("                if os.path.exists(site_pkg) and site_pkg not in sys.path:");
                pythonCodeBuilder.AppendLine("                    sys.path.insert(0, site_pkg)");
                pythonCodeBuilder.AppendLine("except Exception as e:");
                pythonCodeBuilder.AppendLine("    pass");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("from components.train_components import train_classifier");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 反序列化Dataset");
                pythonCodeBuilder.AppendLine("try:");
                pythonCodeBuilder.AppendLine($"    dataset_bytes = base64.b64decode(r'''{datasetStr}''')");
                pythonCodeBuilder.AppendLine("    dataset = pickle.loads(dataset_bytes)");
                pythonCodeBuilder.AppendLine("except Exception as e:");
                pythonCodeBuilder.AppendLine("    import sys");
                pythonCodeBuilder.AppendLine("    import traceback");
                pythonCodeBuilder.AppendLine("    error_msg = f'ERROR:无效的Dataset对象: {str(e)}\\n{traceback.format_exc()}'");
                pythonCodeBuilder.AppendLine("    print(error_msg, file=sys.stderr)");
                pythonCodeBuilder.AppendLine("    print(error_msg)");
                pythonCodeBuilder.AppendLine("    exit(1)");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 调用训练函数");
                pythonCodeBuilder.AppendLine("try:");
                pythonCodeBuilder.AppendLine("    # algorithm参数直接传递，Python端会处理JSON解析");
                pythonCodeBuilder.Append("    algorithm_param = ");
                pythonCodeBuilder.AppendLine(algorithmPythonStr);
                pythonCodeBuilder.AppendLine("    result = train_classifier(dataset=dataset, algorithm=algorithm_param, X_names=None, y_names=None)");
                pythonCodeBuilder.AppendLine("    ");
                pythonCodeBuilder.AppendLine("    # 处理返回值（应该是(model, readme, model_info)元组）");
                pythonCodeBuilder.AppendLine("    if isinstance(result, tuple) and len(result) == 3:");
                pythonCodeBuilder.AppendLine("        model, readme_value, model_info_value = result");
                pythonCodeBuilder.AppendLine("        # 确保readme是字符串");
                pythonCodeBuilder.AppendLine("        if isinstance(readme_value, str):");
                pythonCodeBuilder.AppendLine("            readme = readme_value");
                pythonCodeBuilder.AppendLine("        else:");
                pythonCodeBuilder.AppendLine("            # 如果不是字符串，转换为字符串或使用默认值");
                pythonCodeBuilder.AppendLine("            readme = f'已使用算法训练分类模型'");
                pythonCodeBuilder.AppendLine("        # 确保model_info是字符串");
                pythonCodeBuilder.AppendLine("        if isinstance(model_info_value, str):");
                pythonCodeBuilder.AppendLine("            model_info = model_info_value");
                pythonCodeBuilder.AppendLine("        else:");
                pythonCodeBuilder.AppendLine("            model_info = '模型训练信息不可用'");
                pythonCodeBuilder.AppendLine("    elif isinstance(result, tuple) and len(result) == 2:");
                pythonCodeBuilder.AppendLine("        model, readme_value = result");
                pythonCodeBuilder.AppendLine("        if isinstance(readme_value, str):");
                pythonCodeBuilder.AppendLine("            readme = readme_value");
                pythonCodeBuilder.AppendLine("        else:");
                pythonCodeBuilder.AppendLine("            readme = f'已使用算法训练分类模型'");
                pythonCodeBuilder.AppendLine("        model_info = '模型训练信息不可用'");
                pythonCodeBuilder.AppendLine("    elif isinstance(result, tuple) and len(result) == 1:");
                pythonCodeBuilder.AppendLine("        # 如果只返回了模型");
                pythonCodeBuilder.AppendLine("        model = result[0]");
                pythonCodeBuilder.AppendLine("        readme = f'已使用算法训练分类模型'");
                pythonCodeBuilder.AppendLine("        model_info = '模型训练信息不可用'");
                pythonCodeBuilder.AppendLine("    else:");
                pythonCodeBuilder.AppendLine("        # 如果返回值不是元组，假设只返回了模型");
                pythonCodeBuilder.AppendLine("        model = result");
                pythonCodeBuilder.AppendLine("        readme = f'已使用算法训练分类模型'");
                pythonCodeBuilder.AppendLine("        model_info = '模型训练信息不可用'");
                pythonCodeBuilder.AppendLine("except Exception as e:");
                pythonCodeBuilder.AppendLine("    import traceback");
                pythonCodeBuilder.AppendLine("    import sys");
                pythonCodeBuilder.AppendLine("    error_msg = f'ERROR:训练失败: {str(e)}\\n{traceback.format_exc()}'");
                pythonCodeBuilder.AppendLine("    print(error_msg, file=sys.stderr)");
                pythonCodeBuilder.AppendLine("    print(error_msg)  # 同时输出到stdout以便提取");
                pythonCodeBuilder.AppendLine("    exit(1)");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 序列化模型");
                pythonCodeBuilder.AppendLine("model_bytes = pickle.dumps(model)");
                pythonCodeBuilder.AppendLine("model_str = base64.b64encode(model_bytes).decode('utf-8')");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("import json");
                pythonCodeBuilder.AppendLine("print('OUTPUT_0:' + model_str)");
                pythonCodeBuilder.AppendLine("print('OUTPUT_1:' + str(readme))");
                pythonCodeBuilder.AppendLine("print('OUTPUT_2:' + model_info)  # model_info已经是JSON格式");
                
                string pythonCode = pythonCodeBuilder.ToString();

                string output = PythonScriptExecutor.ExecuteCode(pythonCode);
                string modelStr = ExtractValue(output, "OUTPUT_0:");
                string pythonReadme = ExtractValue(output, "OUTPUT_1:");
                string modelInfoJson = ExtractValue(output, "OUTPUT_2:");
                
                // 将Model Info JSON转换为Tree结构
                GH_Structure<GH_String> modelInfoTree = TreeConverter.ConvertJsonToTree(modelInfoJson);
                
                // 生成详细的Readme
                string readme = $@"组件名称: Train Classifier
功能: 训练分类模型（通用训练组件，支持所有分类算法）

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
     - 标签y必须是分类标签（整数或字符串类别）
     - 建议使用训练集，不要使用测试集训练

2. Algorithm (算法配置) - Text/JSON类型，默认'random_forest'
   • 数据类型: 文本字符串或JSON格式
   • 两种使用方式:
     
     方式1: 使用算法特定训练组件（推荐）
     • 连接: ← Train Random Forest Classifier的Algorithm Params输出
     • 连接: ← Train SVM Classifier的Algorithm Params输出
     • 连接: ← Train KNN Classifier的Algorithm Params输出
     • 优点: 可以详细配置算法参数
     
     方式2: 直接使用算法名称字符串
     • 可选值: 
       - 'random_forest': 随机森林分类器（默认）
       - 'svm': 支持向量机分类器
       - 'knn': K近邻分类器
       - 'logistic_regression': 逻辑回归分类器
       - 'naive_bayes': 朴素贝叶斯分类器
       - 'decision_tree': 决策树分类器
     • 格式: 直接输入算法名称字符串，如 ""random_forest""
     • 优点: 快速使用，使用默认参数
   
   • JSON格式示例（来自算法特定组件）:
     {{""algorithm"": ""random_forest"", ""n_estimators"": 100, ""max_depth"": 10}}

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
   • 说明: 指定dataset中哪些列作为标签（y）
   • 使用方式:
     - 如果dataset包含所有列的数据，使用此参数指定哪些列是标签
     - 可以是列名（字符串）或列索引（整数，作为字符串输入）
     - 例如: [""label""] 或 [""3""]
   • 注意事项:
     - 如果不提供，将使用dataset原有的y数据
     - 如果提供了Y Names，将从dataset.X中提取对应的列作为标签

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Model (模型) - Generic类型，必需
   • 数据类型: 训练好的分类模型对象（Base64编码的pickle对象）
   • 用途: 用于预测和评估
   • 连接建议:
     → Predict Classifier的Model输入（进行预测）
     → Evaluate Classification的Model输入（评估模型）
     → Save Model的Model输入（保存模型）
   • 说明: 模型已训练完成，可以直接用于预测新数据

═══════════════════════════════════════════════════════════════
典型工作流程:
═══════════════════════════════════════════════════════════════

方式1: 使用算法特定组件（推荐）
Create Dataset → Split Dataset
  → Train Dataset → Train Random Forest Classifier → Algorithm Params
  → Algorithm Params → Train Classifier → Model
  → Model → Predict Classifier / Evaluate Classification

方式2: 直接使用算法名称
Create Dataset → Split Dataset
  → Train Dataset → Train Classifier (Algorithm='random_forest') → Model
  → Model → Predict Classifier / Evaluate Classification

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. Dataset必须是分类数据集（标签是类别，不是连续值）
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

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(TrainClassifierComponent));
        public override Guid ComponentGuid => new Guid("E1F2A3B4-C5D6-7890-4567-890123456780");
    }
}
