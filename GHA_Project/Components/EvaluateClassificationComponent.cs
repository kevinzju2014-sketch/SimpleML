using System;
using System.IO;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SimpleML.Core;
using Rhino;

using SimpleML.Localization;
namespace SimpleML.Components.ModelEvaluation
{
    public class EvaluateClassificationComponent : GH_Component
    {
        public EvaluateClassificationComponent()
          : base(L.Name("EvaluateClassificationComponent"), L.Nick("EvaluateClassificationComponent"), L.Desc("EvaluateClassificationComponent"),
              "SimpleML", "07 Evaluation")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "训练好的分类模型对象（Generic类型）", GH_ParamAccess.item);
            pManager.AddGenericParameter("Test Dataset", "DS", "测试数据集（Dataset对象），将从中提取特征X和真实标签y", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Metrics", "M", "评估指标（Tree结构，每个分支包含指标名称和值）", GH_ParamAccess.tree);
            pManager.AddTextParameter("Report", "Rep", "详细的评估报告", GH_ParamAccess.item);
            pManager.AddGenericParameter("Confusion Matrix", "CM", "混淆矩阵（Tree结构）", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Predictions", "P", "预测结果（Tree结构，每个分支包含一个预测类别）", GH_ParamAccess.tree);
            pManager.AddTextParameter("Readme", "RM", "组件使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object modelObj = null;
            object datasetObj = null;

            if (!DA.GetData(0, ref modelObj)) return;
            if (!DA.GetData(1, ref datasetObj))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, L.T("err.need_test_dataset"));
                return;
            }

            try
            {
                string mymlPath = GetMyMLPath();
                if (string.IsNullOrEmpty(mymlPath))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, 
                        L.T("err.package_missing"));
                    return;
                }

                string modelStr = modelObj?.ToString() ?? "";
                string datasetStr = datasetObj?.ToString() ?? "";

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
                pythonCodeBuilder.AppendLine("import numpy as np");
                pythonCodeBuilder.AppendLine("from sklearn.metrics import (");
                pythonCodeBuilder.AppendLine("    accuracy_score, precision_score, recall_score, f1_score,");
                pythonCodeBuilder.AppendLine("    confusion_matrix, classification_report");
                pythonCodeBuilder.AppendLine(")");
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
                pythonCodeBuilder.AppendLine("# 反序列化Dataset");
                pythonCodeBuilder.AppendLine("try:");
                pythonCodeBuilder.Append("    dataset_bytes = base64.b64decode(r'''");
                pythonCodeBuilder.Append(datasetStr);
                pythonCodeBuilder.AppendLine("''')");
                pythonCodeBuilder.AppendLine("    dataset = pickle.loads(dataset_bytes)");
                pythonCodeBuilder.AppendLine("    X = dataset.get_X()");
                pythonCodeBuilder.AppendLine("    y_true = dataset.get_y()");
                pythonCodeBuilder.AppendLine("except Exception as e:");
                pythonCodeBuilder.AppendLine("    print(f'ERROR:无效的Dataset对象: {str(e)}')");
                pythonCodeBuilder.AppendLine("    exit(1)");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 确保数据格式正确");
                pythonCodeBuilder.AppendLine("X = np.array(X)");
                pythonCodeBuilder.AppendLine("y_true = np.array(y_true).ravel()");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 使用模型进行预测");
                pythonCodeBuilder.AppendLine("y_pred = model.predict(X)");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 计算评估指标");
                pythonCodeBuilder.AppendLine("metrics_dict = {}");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 准确率");
                pythonCodeBuilder.AppendLine("metrics_dict['accuracy'] = float(accuracy_score(y_true, y_pred))");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 精确率");
                pythonCodeBuilder.AppendLine("try:");
                pythonCodeBuilder.AppendLine("    metrics_dict['precision'] = float(precision_score(y_true, y_pred, average='weighted', zero_division=0))");
                pythonCodeBuilder.AppendLine("except:");
                pythonCodeBuilder.AppendLine("    metrics_dict['precision'] = None");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 召回率");
                pythonCodeBuilder.AppendLine("try:");
                pythonCodeBuilder.AppendLine("    metrics_dict['recall'] = float(recall_score(y_true, y_pred, average='weighted', zero_division=0))");
                pythonCodeBuilder.AppendLine("except:");
                pythonCodeBuilder.AppendLine("    metrics_dict['recall'] = None");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# F1分数");
                pythonCodeBuilder.AppendLine("try:");
                pythonCodeBuilder.AppendLine("    metrics_dict['f1_score'] = float(f1_score(y_true, y_pred, average='weighted', zero_division=0))");
                pythonCodeBuilder.AppendLine("except:");
                pythonCodeBuilder.AppendLine("    metrics_dict['f1_score'] = None");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 混淆矩阵");
                pythonCodeBuilder.AppendLine("cm = confusion_matrix(y_true, y_pred)");
                pythonCodeBuilder.AppendLine("cm_tree = cm.tolist()");
                pythonCodeBuilder.AppendLine("metrics_dict['confusion_matrix'] = cm_tree");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 生成详细报告");
                pythonCodeBuilder.AppendLine("try:");
                pythonCodeBuilder.AppendLine("    report_base = classification_report(y_true, y_pred)");
                pythonCodeBuilder.AppendLine("    ");
                pythonCodeBuilder.AppendLine("    # 添加详细说明");
                pythonCodeBuilder.AppendLine("    explanation = '\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '===================================================================\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '指标说明:\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '===================================================================\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '• Precision (精确率/查准率):\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  含义: 在所有被模型预测为正例的样本中，实际为正例的比例\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  通俗理解: \"模型说对的，有多少真的对了\"\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  数值范围: 0-1，越高越好（>0.7通常认为较好）\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  使用场景: 当误报成本高时，需要高精确率\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '• Recall (召回率/查全率):\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  含义: 在所有实际为正例的样本中，被模型正确预测为正例的比例\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  通俗理解: \"实际为正的，模型找到了多少\"\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  数值范围: 0-1，越高越好（>0.7通常认为较好）\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  使用场景: 当漏报成本高时，需要高召回率\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '• F1-Score (F1分数):\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  含义: 精确率和召回率的调和平均数，综合评估模型性能\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  通俗理解: \"精确率和召回率的平衡值\"\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  数值范围: 0-1，越高越好（>0.7通常认为较好）\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  使用场景: 需要平衡精确率和召回率时\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '• Support (支持数):\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  含义: 每个类别在测试集中实际出现的样本数量\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  通俗理解: \"测试集中每个类别有多少个样本\"\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  用途: 了解测试集的类别分布，判断数据集是否类别不平衡\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '• Accuracy (准确率):\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  含义: 所有预测中，预测正确的比例\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  注意: 当类别不平衡时，准确率可能误导\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '• Macro Avg (宏平均):\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  含义: 所有类别指标的简单平均值，每个类别权重相同\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  使用场景: 类别重要性相同，或类别样本数量差异很大时\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '• Weighted Avg (加权平均):\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  含义: 根据每个类别的样本数量加权平均，样本多的类别权重更大\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '  使用场景: 关注整体性能，样本多的类别更重要时\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '===================================================================\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '如何解读报告:\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '===================================================================\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '1. 看整体性能: 查看 accuracy 和 macro avg / weighted avg 的 F1-score\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '2. 看各类别性能: 检查每个类别的 precision、recall、f1-score\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '3. 看类别平衡: 比较各类别的 support，如果差异很大，关注 macro avg\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '4. 根据需求选择指标:\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '   - 需要高精确率: 关注 precision（减少误报）\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '   - 需要高召回率: 关注 recall（减少漏报）\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '   - 需要平衡: 关注 f1-score\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '\\n'");
                pythonCodeBuilder.AppendLine("    explanation += '===================================================================\\n'");
                pythonCodeBuilder.AppendLine("    ");
                pythonCodeBuilder.AppendLine("    report = report_base + explanation");
                pythonCodeBuilder.AppendLine("    # 调试输出");
                pythonCodeBuilder.AppendLine("    print(f'DEBUG: report_base长度 = {len(report_base)}', file=sys.stderr)");
                pythonCodeBuilder.AppendLine("    print(f'DEBUG: explanation长度 = {len(explanation)}', file=sys.stderr)");
                pythonCodeBuilder.AppendLine("    print(f'DEBUG: report总长度 = {len(report)}', file=sys.stderr)");
                pythonCodeBuilder.AppendLine("except Exception as e:");
                pythonCodeBuilder.AppendLine("    import traceback");
                pythonCodeBuilder.AppendLine("    error_msg = traceback.format_exc()");
                pythonCodeBuilder.AppendLine("    print(f'ERROR生成报告时出错: {str(e)}', file=sys.stderr)");
                pythonCodeBuilder.AppendLine("    print(f'ERROR详细错误: {error_msg}', file=sys.stderr)");
                pythonCodeBuilder.AppendLine("    try:");
                pythonCodeBuilder.AppendLine("        report = classification_report(y_true, y_pred)");
                pythonCodeBuilder.AppendLine("    except:");
                pythonCodeBuilder.AppendLine("        report = '分类报告生成失败'");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 将metrics_dict转换为Tree结构格式");
                pythonCodeBuilder.AppendLine("# Tree结构：每个分支包含[指标名称, 指标值]");
                pythonCodeBuilder.AppendLine("metrics_tree = []");
                pythonCodeBuilder.AppendLine("for key, value in metrics_dict.items():");
                pythonCodeBuilder.AppendLine("    if key == 'confusion_matrix':");
                pythonCodeBuilder.AppendLine("        # 混淆矩阵单独处理，跳过");
                pythonCodeBuilder.AppendLine("        continue");
                pythonCodeBuilder.AppendLine("    if value is None:");
                pythonCodeBuilder.AppendLine("        metrics_tree.append([str(key), 'N/A'])");
                pythonCodeBuilder.AppendLine("    else:");
                pythonCodeBuilder.AppendLine("        metrics_tree.append([str(key), str(value)])");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("readme = '分类模型评估完成'");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 确保report是字符串类型");
                pythonCodeBuilder.AppendLine("if not isinstance(report, str):");
                pythonCodeBuilder.AppendLine("    report = str(report)");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 转换预测结果为Tree结构格式");
                pythonCodeBuilder.AppendLine("pred_list = y_pred.tolist() if hasattr(y_pred, 'tolist') else list(y_pred)");
                pythonCodeBuilder.AppendLine("pred_tree = [[str(val)] for val in pred_list]");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("print('OUTPUT_0:' + json.dumps(metrics_tree, ensure_ascii=False))");
                pythonCodeBuilder.AppendLine("print('OUTPUT_1:' + report)");
                pythonCodeBuilder.AppendLine("print('OUTPUT_2:' + json.dumps(cm_tree, ensure_ascii=False))");
                pythonCodeBuilder.AppendLine("print('OUTPUT_3:' + json.dumps(pred_tree, ensure_ascii=False))");
                pythonCodeBuilder.AppendLine("print('OUTPUT_4:' + readme)");
                
                string pythonCode = pythonCodeBuilder.ToString();

                string output = PythonScriptExecutor.ExecuteCode(pythonCode);
                
                // 解析输出
                string metricsJson = ExtractValue(output, "OUTPUT_0:");
                string report = ExtractValue(output, "OUTPUT_1:");
                string cmJson = ExtractValue(output, "OUTPUT_2:");
                string predJson = ExtractValue(output, "OUTPUT_3:");
                string pythonReadme = ExtractValue(output, "OUTPUT_4:");

                GH_Structure<GH_String> metricsTree = TreeConverter.ConvertJsonToTree(metricsJson);
                GH_Structure<GH_String> cmTree = TreeConverter.ConvertJsonToTree(cmJson);
                GH_Structure<GH_String> predTree = TreeConverter.ConvertJsonToTree(predJson);

                DA.SetDataTree(0, metricsTree);
                DA.SetData(1, report);
                DA.SetDataTree(2, cmTree);
                DA.SetDataTree(3, predTree);
                
                // Readme输出
                string readme = @"组件名称: Evaluate Classification
功能: 评估分类模型的性能（自动预测并评估）

===================================================================
输入参数详解:
===================================================================

1. Model (模型) - Generic类型，必需
   • 数据类型: 训练好的分类模型对象（Base64编码的pickle对象）
   • 连接建议:
     ← Train Classifier的Model输出
     ← Load Model的Model输出
   • 注意事项: 
     - 必须是有效的分类模型对象
     - 模型必须已经训练完成

2. Test Dataset (测试数据集) - Generic类型，必需
   • 数据类型: Dataset对象（Base64编码的pickle对象）
   • 数据结构: 包含特征数据（X）和真实标签（y）的Dataset对象
   • 说明: 
     - 组件将从Dataset中自动提取X（特征数据）和y（真实标签）
     - 组件内部使用Model对X进行预测，然后与y进行比较评估
   • 连接建议:
     ← Split Dataset (Test Dataset) 输出
   • 注意事项: 
     - Dataset必须包含特征数据X和标签数据y
     - 特征数量必须与训练时一致

===================================================================
输出参数详解:
===================================================================

1. Metrics (评估指标) - Tree结构
   • 数据类型: Grasshopper Tree结构
   • 数据结构: 
     - 每个分支包含[指标名称, 指标值]
     - 分支路径为 {0}, {1}, {2}...（指标索引）
   • 包含的指标:
     - {0} → [""accuracy"", ""0.95""] - 准确率（正确预测的比例）
     - {1} → [""precision"", ""0.94""] - 精确率（预测为正例中实际为正例的比例）
     - {2} → [""recall"", ""0.93""] - 召回率（实际正例中被正确预测的比例）
     - {3} → [""f1_score"", ""0.935""] - F1分数（精确率和召回率的调和平均）
   • 格式: Tree结构，每个分支包含指标名称和值
   • 用途: 量化模型性能，用于模型比较和可视化

2. Report (评估报告) - Text类型
   • 内容: 详细的分类评估报告
   • 包含信息:
     - 每个类别的精确率、召回率、F1分数
     - 总体准确率
   • 格式: 可读的文本格式
   • 用途: 详细了解模型在每个类别上的表现

3. Confusion Matrix (混淆矩阵) - Tree结构
   • 数据类型: Grasshopper Tree结构
   • 数据结构: 
     - 行表示真实类别
     - 列表示预测类别
     - 值表示样本数量
   • 用途: 可视化分类错误，识别易混淆的类别对

===================================================================
典型工作流程:
===================================================================

简单评估流程:
Split Dataset (Test Dataset) → Evaluate Classification (Test Dataset)
Train Classifier (Model) → Evaluate Classification (Model)

模型比较流程:
Train Classifier (Model A) → Evaluate Classification → Metrics A
Train Classifier (Model B) → Evaluate Classification → Metrics B
(比较 Metrics A 和 Metrics B)

===================================================================
注意事项:
===================================================================

1. Model必须是分类模型，不能是回归或聚类模型
2. Test Dataset必须包含特征数据X和标签数据y
3. 建议使用测试集进行评估，不要使用训练集
4. 组件内部自动进行预测，无需手动连接Predictions
5. 组件会自动计算所有评估指标，包括准确率、精确率、召回率、F1分数和混淆矩阵
6. 混淆矩阵可以帮助识别模型的错误模式
7. 评估结果可以用于模型选择和参数调优";
                
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

        private string ExtractValue(string output, string prefix)
        {
            int startIndex = output.IndexOf(prefix);
            if (startIndex == -1) return string.Empty;
            startIndex += prefix.Length;
            
            // 查找下一个OUTPUT标记（如果存在）
            int nextOutputIndex = output.IndexOf("OUTPUT_", startIndex);
            int endIndex;
            if (nextOutputIndex != -1)
            {
                // 找到下一个OUTPUT标记，提取到该标记之前（不包括换行符）
                endIndex = nextOutputIndex;
                // 移除末尾的换行符和空白字符
                while (endIndex > startIndex && char.IsWhiteSpace(output[endIndex - 1]))
                {
                    endIndex--;
                }
            }
            else
            {
                // 没有下一个OUTPUT标记，提取到末尾
                endIndex = output.Length;
            }
            
            return output.Substring(startIndex, endIndex - startIndex);
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(EvaluateClassificationComponent));
        public override Guid ComponentGuid => new Guid("C5D6E7F8-A9B0-1234-8901-234567890124");
    }
}
