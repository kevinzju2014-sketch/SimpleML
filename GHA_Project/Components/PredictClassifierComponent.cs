using System;
using System.IO;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.ModelPrediction
{
    public class PredictClassifierComponent : GH_Component
    {
        public PredictClassifierComponent()
          : base("预测分类 Predict Classifier", "预测分类",
              "使用训练好的分类模型进行预测",
              "SimpleML", "06 Prediction")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "训练好的分类模型对象（Generic类型）", GH_ParamAccess.item);
            pManager.AddGenericParameter("X", "X", "待预测的特征数据（Tree结构）", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Predictions", "P", "预测结果（Tree结构，每个分支包含一个预测类别）", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Probabilities", "Prob", "预测概率（Tree结构，每个分支包含各类别的概率）", GH_ParamAccess.tree);
            pManager.AddTextParameter("Readme", "R", "组件使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object modelObj = null;
            GH_Structure<IGH_Goo> xTree = null;

            if (!DA.GetData(0, ref modelObj)) return;
            if (!DA.GetDataTree(1, out xTree) || xTree == null || xTree.PathCount == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "必须提供X输入（特征数据）");
                return;
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

                string modelStr = modelObj?.ToString() ?? "";
                string xListStr = ConvertTreeToPythonList(xTree);

                System.Text.StringBuilder pythonCodeBuilder = new System.Text.StringBuilder();
                pythonCodeBuilder.AppendLine("# -*- coding: utf-8 -*-");
                pythonCodeBuilder.AppendLine("import sys");
                pythonCodeBuilder.AppendLine("import os");
                pythonCodeBuilder.AppendLine("import json");
                pythonCodeBuilder.AppendLine("import site");
                pythonCodeBuilder.AppendLine("import io");
                pythonCodeBuilder.AppendLine("import pickle");
                pythonCodeBuilder.AppendLine("import base64");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 设置标准输出编码为UTF-8");
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
                pythonCodeBuilder.AppendLine("except Exception:");
                pythonCodeBuilder.AppendLine("    pass");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("from components.predict_components import predict_classifier");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 反序列化模型");
                pythonCodeBuilder.AppendLine("try:");
                pythonCodeBuilder.Append("    model_bytes = base64.b64decode(r'''");
                pythonCodeBuilder.Append(modelStr);
                pythonCodeBuilder.AppendLine("''')");
                pythonCodeBuilder.AppendLine("    model = pickle.loads(model_bytes)");
                pythonCodeBuilder.AppendLine("except Exception as e:");
                pythonCodeBuilder.AppendLine("    print('ERROR:无效的模型对象')");
                pythonCodeBuilder.AppendLine("    exit(1)");
                pythonCodeBuilder.AppendLine();
                
                // 处理 X 输入（Tree结构）
                pythonCodeBuilder.AppendLine("# 处理X输入（Tree结构）");
                pythonCodeBuilder.Append("X = ");
                pythonCodeBuilder.Append(xListStr);
                pythonCodeBuilder.AppendLine();
                
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 进行预测");
                pythonCodeBuilder.AppendLine("predictions, probabilities, readme = predict_classifier(model, X=X, dataset=None)");
                
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 转换预测结果为Tree结构格式");
                pythonCodeBuilder.AppendLine("import numpy as np");
                pythonCodeBuilder.AppendLine("if hasattr(predictions, 'tolist'):");
                pythonCodeBuilder.AppendLine("    pred_list = predictions.tolist()");
                pythonCodeBuilder.AppendLine("else:");
                pythonCodeBuilder.AppendLine("    pred_list = list(predictions)");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 确保每个预测值都在单独的分支中");
                pythonCodeBuilder.AppendLine("if len(pred_list) > 0 and isinstance(pred_list[0], (int, float, str)):");
                pythonCodeBuilder.AppendLine("    pred_tree = [[str(val)] for val in pred_list]");
                pythonCodeBuilder.AppendLine("else:");
                pythonCodeBuilder.AppendLine("    pred_tree = [[str(val)] for val in pred_list]");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 转换概率为Tree结构格式");
                pythonCodeBuilder.AppendLine("if probabilities is not None:");
                pythonCodeBuilder.AppendLine("    prob_tree = probabilities");
                pythonCodeBuilder.AppendLine("else:");
                pythonCodeBuilder.AppendLine("    prob_tree = []");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("print('OUTPUT_0:' + json.dumps(pred_tree, ensure_ascii=False))");
                pythonCodeBuilder.AppendLine("print('OUTPUT_1:' + json.dumps(prob_tree, ensure_ascii=False))");
                pythonCodeBuilder.AppendLine("print('OUTPUT_2:' + readme)");
                
                string pythonCode = pythonCodeBuilder.ToString();

                string output = PythonScriptExecutor.ExecuteCode(pythonCode);
                
                // 解析输出
                string predJson = ExtractValue(output, "OUTPUT_0:");
                string probJson = ExtractValue(output, "OUTPUT_1:");

                GH_Structure<GH_String> predTree = TreeConverter.ConvertJsonToTree(predJson);
                GH_Structure<GH_String> probTree = TreeConverter.ConvertJsonToTree(probJson);

                DA.SetDataTree(0, predTree);
                DA.SetDataTree(1, probTree);
                
                // Readme输出
                string readme = @"组件名称: Predict Classifier
功能: 使用训练好的分类模型进行预测

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. Model (模型) - Generic类型，必需
   • 数据类型: 训练好的分类模型对象（Base64编码的pickle对象）
   • 数据结构: 包含训练好的分类模型的所有信息
   • 连接建议:
     ← Train Classifier的Model输出（最常用）
     ← Load Model的Model输出（加载已保存的模型）
   • 注意事项: 
     - 必须是有效的分类模型对象
     - 模型必须已经训练完成
     - 模型类型必须与预测任务匹配（分类模型）

2. Dataset (数据集) - Generic类型，必需
   • 数据类型: Dataset对象（Base64编码的pickle对象）
   • 数据结构: 包含特征数据（X）的Dataset对象
   • 说明: 
     - 组件将从Dataset中自动提取X（特征数据）进行预测
     - Dataset必须包含与训练时相同数量和顺序的特征
   • 示例:
     - Split Dataset的Test Dataset输出
     - Create Dataset的输出（新数据预测）
   • 连接建议:
     ← Split Dataset (Test Dataset) 输出
     ← Create Dataset 输出（新数据预测）
   • 注意事项: 
     - Dataset中的特征数量必须与训练时的特征数量一致
     - 特征顺序必须与训练时一致
     - 数据格式必须与训练时一致（如是否标准化）

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Predictions (预测结果) - Tree结构
   • 数据类型: Grasshopper Tree结构
   • 数据结构: 
     - 每个分支包含一个预测类别（单元素分支）
     - 分支路径为 {0}, {1}, {2}...（样本索引）
   • 示例:
     {0} → [""setosa""]
     {1} → [""setosa""]
     {2} → [""versicolor""]
   • 说明: 预测值是类别标签（分类任务的输出）
   • 连接建议:
     → Write CSV/Excel的Data输入（保存预测结果）
     → Evaluate Classification的Predictions输入（评估预测结果）
     → 其他Grasshopper组件（进一步处理）

2. Probabilities (预测概率) - Tree结构
   • 数据类型: Grasshopper Tree结构
   • 数据结构: 
     - 每个分支包含各类别的预测概率（多元素分支）
     - 分支路径为 {0}, {1}, {2}...（样本索引）
   • 示例:
     {0} → [""0.95"", ""0.04"", ""0.01""]  (setosa, versicolor, virginica的概率)
   • 说明: 
     - 概率值在0到1之间，所有类别概率之和为1
     - 仅当模型支持概率预测时才有值（如Random Forest, Logistic Regression）
     - 某些模型（如SVM）可能不支持概率预测
   • 连接建议:
     → 用于分析预测的置信度
     → 用于选择高置信度的预测结果
   • 注意: 如果模型不支持概率预测，此输出将为空

═══════════════════════════════════════════════════════════════
典型工作流程:
═══════════════════════════════════════════════════════════════

测试集预测:
Split Dataset (Test Dataset) → Predict Classifier (Dataset) → Predictions

新数据预测:
Read CSV/Excel → Create Dataset → Predict Classifier (Dataset) → Predictions

完整评估流程:
Split Dataset (Test Dataset) → Predict Classifier (Dataset) → Predictions
Split Dataset (Test Dataset) → Deconstruct Dataset → Labels → Evaluate Classification (Y True)

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. Model必须是分类模型，不能是回归或聚类模型
2. Data Set中的特征数量必须与训练时一致
3. 如果训练时进行了数据预处理（如标准化），预测时也需要相同的预处理
4. 预测结果是类别标签，不是连续数值
5. Probabilities输出仅当模型支持概率预测时才有值
6. 预测结果可以用于评估、保存或进一步分析";
                
                DA.SetData(2, readme);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"执行失败: {ex.Message}");
                RhinoApp.WriteLine($"SimpleML错误: {ex}");
            }
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
                    
                    // 优先检查是否为数值类型
                    if (item is Grasshopper.Kernel.Types.GH_Number num)
                    {
                        sb.Append(num.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    }
                    else if (item is Grasshopper.Kernel.Types.GH_Integer intVal)
                    {
                        sb.Append(intVal.Value);
                    }
                    else
                    {
                        string value = item.ToString();
                        // 尝试转换为数字
                        if (double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double numVal))
                        {
                            sb.Append(numVal.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            // 如果不是数字，检查是否是有效的特征值
                            // 如果值太长（可能是序列化数据），给出错误提示
                            if (value.Length > 100)
                            {
                                throw new Exception($"输入数据包含无效值（可能是序列化对象）。请确保X输入是特征数据（Tree结构），而不是Dataset对象或其他序列化数据。");
                            }
                            // 尝试作为字符串处理（可能是类别标签）
                            sb.Append("\"").Append(value.Replace("\\", "\\\\").Replace("\"", "\\\"")).Append("\"");
                        }
                    }
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

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(PredictClassifierComponent));
        public override Guid ComponentGuid => new Guid("B5C6D7E8-F9A0-1234-8901-234567890135");
    }
}
