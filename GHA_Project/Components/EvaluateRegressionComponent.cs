using System;
using System.IO;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.ModelEvaluation
{
    public class EvaluateRegressionComponent : GH_Component
    {
        public EvaluateRegressionComponent()
          : base("评估回归 Evaluate Regression", "评估回归",
              "评估回归模型",
              "SimpleML", "07 Evaluation")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "训练好的回归模型对象（Generic类型）", GH_ParamAccess.item);
            pManager.AddGenericParameter("Test Dataset", "DS", "测试数据集（Dataset对象），将从中提取特征X和真实标签y", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Metrics", "M", "评估指标（Tree结构，每个分支包含指标名称和值）", GH_ParamAccess.tree);
            pManager.AddTextParameter("Report", "Rep", "详细的评估报告", GH_ParamAccess.item);
            pManager.AddGenericParameter("Predictions", "P", "预测结果（Tree结构，每个分支包含一个预测值）", GH_ParamAccess.tree);
            pManager.AddTextParameter("Readme", "RM", "组件使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object modelObj = null;
            object datasetObj = null;

            if (!DA.GetData(0, ref modelObj)) return;
            if (!DA.GetData(1, ref datasetObj))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "必须提供Test Dataset输入");
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
                pythonCodeBuilder.AppendLine("    mean_squared_error, mean_absolute_error, r2_score, explained_variance_score");
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
                pythonCodeBuilder.AppendLine("# 确保y_true是数值类型");
                pythonCodeBuilder.AppendLine("y_true = np.array(y_true, dtype=float).ravel()");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 使用模型进行预测");
                pythonCodeBuilder.AppendLine("y_pred = model.predict(X)");
                pythonCodeBuilder.AppendLine("# 确保y_pred是数值类型");
                pythonCodeBuilder.AppendLine("y_pred = np.array(y_pred, dtype=float).ravel()");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 计算评估指标");
                pythonCodeBuilder.AppendLine("metrics_dict = {}");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 均方误差 (MSE)");
                pythonCodeBuilder.AppendLine("mse = mean_squared_error(y_true, y_pred)");
                pythonCodeBuilder.AppendLine("metrics_dict['mse'] = float(mse)");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 均方根误差 (RMSE)");
                pythonCodeBuilder.AppendLine("metrics_dict['rmse'] = float(np.sqrt(mse))");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 平均绝对误差 (MAE)");
                pythonCodeBuilder.AppendLine("metrics_dict['mae'] = float(mean_absolute_error(y_true, y_pred))");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# R²分数");
                pythonCodeBuilder.AppendLine("metrics_dict['r2_score'] = float(r2_score(y_true, y_pred))");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 解释方差");
                pythonCodeBuilder.AppendLine("metrics_dict['explained_variance'] = float(explained_variance_score(y_true, y_pred))");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 生成详细报告");
                pythonCodeBuilder.AppendLine("mse_val = metrics_dict.get('mse', 'N/A')");
                pythonCodeBuilder.AppendLine("rmse_val = metrics_dict.get('rmse', 'N/A')");
                pythonCodeBuilder.AppendLine("mae_val = metrics_dict.get('mae', 'N/A')");
                pythonCodeBuilder.AppendLine("r2_val = metrics_dict.get('r2_score', 'N/A')");
                pythonCodeBuilder.AppendLine("ev_val = metrics_dict.get('explained_variance', 'N/A')");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 格式化数值");
                pythonCodeBuilder.AppendLine("def format_val(val):");
                pythonCodeBuilder.AppendLine("    if isinstance(val, (int, float)):");
                pythonCodeBuilder.AppendLine("        return f'{val:.4f}'");
                pythonCodeBuilder.AppendLine("    return str(val)");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("report_base_lines = [");
                pythonCodeBuilder.AppendLine("    '回归模型评估报告',");
                pythonCodeBuilder.AppendLine("    '==================',");
                pythonCodeBuilder.AppendLine("    '',");
                pythonCodeBuilder.AppendLine("    f'均方误差 (MSE): {format_val(mse_val)}',");
                pythonCodeBuilder.AppendLine("    f'均方根误差 (RMSE): {format_val(rmse_val)}',");
                pythonCodeBuilder.AppendLine("    f'平均绝对误差 (MAE): {format_val(mae_val)}',");
                pythonCodeBuilder.AppendLine("    f'R² 分数: {format_val(r2_val)}',");
                pythonCodeBuilder.AppendLine("    f'解释方差: {format_val(ev_val)}',");
                pythonCodeBuilder.AppendLine("]");
                pythonCodeBuilder.AppendLine("report_base = '\\n'.join(report_base_lines)");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 添加详细说明");
                pythonCodeBuilder.AppendLine("explanation_lines = [");
                pythonCodeBuilder.AppendLine("    '',");
                pythonCodeBuilder.AppendLine("    '===================================================================',");
                pythonCodeBuilder.AppendLine("    '指标说明:',");
                pythonCodeBuilder.AppendLine("    '===================================================================',");
                pythonCodeBuilder.AppendLine("    '',");
                pythonCodeBuilder.AppendLine("    '• MSE (均方误差 - Mean Squared Error):',");
                pythonCodeBuilder.AppendLine("    '  含义: 预测值与真实值差的平方的平均值',");
                pythonCodeBuilder.AppendLine("    '  公式: MSE = Σ(预测值 - 真实值)² / n',");
                pythonCodeBuilder.AppendLine("    '  特点: 对大误差敏感（平方放大了大误差的影响）',");
                pythonCodeBuilder.AppendLine("    '  数值范围: ≥ 0，越小越好',");
                pythonCodeBuilder.AppendLine("    '  单位: 与目标变量的平方单位相同',");
                pythonCodeBuilder.AppendLine("    '',");
                pythonCodeBuilder.AppendLine("    '• RMSE (均方根误差 - Root Mean Squared Error):',");
                pythonCodeBuilder.AppendLine("    '  含义: MSE的平方根，与目标变量单位相同',");
                pythonCodeBuilder.AppendLine("    '  公式: RMSE = √MSE',");
                pythonCodeBuilder.AppendLine("    '  特点: 比MSE更直观，单位与目标变量一致',");
                pythonCodeBuilder.AppendLine("    '  数值范围: ≥ 0，越小越好',");
                pythonCodeBuilder.AppendLine("    '  单位: 与目标变量单位相同',");
                pythonCodeBuilder.AppendLine("    '  使用场景: 最常用的回归评估指标之一',");
                pythonCodeBuilder.AppendLine("    '',");
                pythonCodeBuilder.AppendLine("    '• MAE (平均绝对误差 - Mean Absolute Error):',");
                pythonCodeBuilder.AppendLine("    '  含义: 预测值与真实值差的绝对值的平均值',");
                pythonCodeBuilder.AppendLine("    '  公式: MAE = Σ|预测值 - 真实值| / n',");
                pythonCodeBuilder.AppendLine("    '  特点: 对所有误差一视同仁，不受异常值影响',");
                pythonCodeBuilder.AppendLine("    '  数值范围: ≥ 0，越小越好',");
                pythonCodeBuilder.AppendLine("    '  单位: 与目标变量单位相同',");
                pythonCodeBuilder.AppendLine("    '  使用场景: 当不希望大误差被过度放大时',");
                pythonCodeBuilder.AppendLine("    '',");
                pythonCodeBuilder.AppendLine("    '• R² (决定系数 - R-squared):',");
                pythonCodeBuilder.AppendLine("    '  含义: 模型解释的方差占总方差的比例',");
                pythonCodeBuilder.AppendLine("    '  公式: R² = 1 - (SS_res / SS_tot)',");
                pythonCodeBuilder.AppendLine("    '  特点: 无量纲，不受目标变量单位影响',");
                pythonCodeBuilder.AppendLine("    '  数值范围: -∞ 到 1',");
                pythonCodeBuilder.AppendLine("    '  - R² = 1: 完美拟合（模型完美预测）',");
                pythonCodeBuilder.AppendLine("    '  - R² = 0: 模型性能等同于简单平均值',");
                pythonCodeBuilder.AppendLine("    '  - R² < 0: 模型性能比简单平均值还差',");
                pythonCodeBuilder.AppendLine("    '  使用场景: 最常用的回归模型评估指标',");
                pythonCodeBuilder.AppendLine("    '  参考值: > 0.7 通常认为较好，> 0.9 非常好',");
                pythonCodeBuilder.AppendLine("    '',");
                pythonCodeBuilder.AppendLine("    '• Explained Variance (解释方差):',");
                pythonCodeBuilder.AppendLine("    '  含义: 模型能够解释的目标变量方差比例',");
                pythonCodeBuilder.AppendLine("    '  特点: 与R²类似，但计算方法略有不同',");
                pythonCodeBuilder.AppendLine("    '  数值范围: 0 到 1，越接近1越好',");
                pythonCodeBuilder.AppendLine("    '  使用场景: 评估模型的解释能力',");
                pythonCodeBuilder.AppendLine("    '',");
                pythonCodeBuilder.AppendLine("    '===================================================================',");
                pythonCodeBuilder.AppendLine("    '如何解读报告:',");
                pythonCodeBuilder.AppendLine("    '===================================================================',");
                pythonCodeBuilder.AppendLine("    '',");
                pythonCodeBuilder.AppendLine("    '1. 看整体拟合度:',");
                pythonCodeBuilder.AppendLine("    '   - R²值越接近1，模型拟合越好',");
                pythonCodeBuilder.AppendLine("    '   - R² > 0.7 通常认为较好',");
                pythonCodeBuilder.AppendLine("    '   - R² > 0.9 非常好',");
                pythonCodeBuilder.AppendLine("    '',");
                pythonCodeBuilder.AppendLine("    '2. 看预测误差:',");
                pythonCodeBuilder.AppendLine("    '   - RMSE和MAE越小，预测误差越小',");
                pythonCodeBuilder.AppendLine("    '   - RMSE对大误差敏感，MAE对所有误差一视同仁',");
                pythonCodeBuilder.AppendLine("    '   - 如果RMSE >> MAE，说明存在较大的预测误差',");
                pythonCodeBuilder.AppendLine("    '',");
                pythonCodeBuilder.AppendLine("    '3. 比较不同模型:',");
                pythonCodeBuilder.AppendLine("    '   - 比较R²值：越高越好',");
                pythonCodeBuilder.AppendLine("    '   - 比较RMSE/MAE：越低越好',");
                pythonCodeBuilder.AppendLine("    '   - 注意单位一致性',");
                pythonCodeBuilder.AppendLine("    '',");
                pythonCodeBuilder.AppendLine("    '4. 根据应用场景选择指标:',");
                pythonCodeBuilder.AppendLine("    '   - 关注整体拟合度: 使用R²',");
                pythonCodeBuilder.AppendLine("    '   - 关注预测误差大小: 使用RMSE或MAE',");
                pythonCodeBuilder.AppendLine("    '   - 关注异常值影响: 使用MAE（不受异常值影响）',");
                pythonCodeBuilder.AppendLine("    '',");
                pythonCodeBuilder.AppendLine("    '==================================================================='");
                pythonCodeBuilder.AppendLine("]");
                pythonCodeBuilder.AppendLine("explanation = '\\n'.join(explanation_lines)");
                pythonCodeBuilder.AppendLine("report = report_base + explanation");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 将metrics_dict转换为Tree结构格式");
                pythonCodeBuilder.AppendLine("# Tree结构：每个分支包含[指标名称, 指标值]");
                pythonCodeBuilder.AppendLine("metrics_tree = []");
                pythonCodeBuilder.AppendLine("for key, value in metrics_dict.items():");
                pythonCodeBuilder.AppendLine("    if value is None:");
                pythonCodeBuilder.AppendLine("        metrics_tree.append([str(key), 'N/A'])");
                pythonCodeBuilder.AppendLine("    else:");
                pythonCodeBuilder.AppendLine("        metrics_tree.append([str(key), str(value)])");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("readme = '回归模型评估完成'");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 转换预测结果为Tree结构格式");
                pythonCodeBuilder.AppendLine("pred_list = y_pred.tolist() if hasattr(y_pred, 'tolist') else list(y_pred)");
                pythonCodeBuilder.AppendLine("pred_tree = [[str(val)] for val in pred_list]");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("print('OUTPUT_0:' + json.dumps(metrics_tree, ensure_ascii=False))");
                pythonCodeBuilder.AppendLine("print('OUTPUT_1:' + report)");
                pythonCodeBuilder.AppendLine("print('OUTPUT_2:' + json.dumps(pred_tree, ensure_ascii=False))");
                pythonCodeBuilder.AppendLine("print('OUTPUT_3:' + readme)");
                
                string pythonCode = pythonCodeBuilder.ToString();

                string output = PythonScriptExecutor.ExecuteCode(pythonCode);
                
                // 解析输出
                string metricsJson = ExtractValue(output, "OUTPUT_0:");
                string report = ExtractValue(output, "OUTPUT_1:");
                string predJson = ExtractValue(output, "OUTPUT_2:");
                string pythonReadme = ExtractValue(output, "OUTPUT_3:");

                GH_Structure<GH_String> metricsTree = TreeConverter.ConvertJsonToTree(metricsJson);
                GH_Structure<GH_String> predTree = TreeConverter.ConvertJsonToTree(predJson);

                DA.SetDataTree(0, metricsTree);
                DA.SetData(1, report);
                DA.SetDataTree(2, predTree);
                
                // Readme输出
                string readme = @"组件名称: Evaluate Regression
功能: 评估回归模型的性能（自动预测并评估）

===================================================================
输入参数详解:
===================================================================

1. Model (模型) - Generic类型，必需
   • 数据类型: 训练好的回归模型对象（Base64编码的pickle对象）
   • 连接建议:
     ← Train Regressor的Model输出
     ← Load Model的Model输出
   • 注意事项: 
     - 必须是有效的回归模型对象
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
     - {0} → [""mse"", ""0.25""] - 均方误差（Mean Squared Error），越小越好
     - {1} → [""rmse"", ""0.5""] - 均方根误差（Root Mean Squared Error），越小越好
     - {2} → [""mae"", ""0.4""] - 平均绝对误差（Mean Absolute Error），越小越好
     - {3} → [""r2_score"", ""0.85""] - R²决定系数（R-squared），越接近1越好（范围0-1）
     - {4} → [""explained_variance"", ""0.86""] - 解释方差，越接近1越好
   • 格式: Tree结构，每个分支包含指标名称和值
   • 用途: 量化模型性能，用于模型比较和可视化

2. Report (评估报告) - Text类型
   • 内容: 详细的回归评估报告
   • 包含信息:
     - 各评估指标的数值
     - 模型性能总结
     - 预测误差分析
   • 格式: 可读的文本格式
   • 用途: 详细了解模型的回归性能

===================================================================
典型工作流程:
===================================================================

简单评估流程:
Split Dataset (Test Dataset) → Evaluate Regression (Test Dataset)
Train Regressor (Model) → Evaluate Regression (Model)

模型比较流程:
Train Regressor (Model A) → Evaluate Regression → Metrics A
Train Regressor (Model B) → Evaluate Regression → Metrics B
(比较 Metrics A 和 Metrics B)

===================================================================
注意事项:
===================================================================

1. Model必须是回归模型，不能是分类或聚类模型
2. Test Dataset必须包含特征数据X和标签数据y
3. 建议使用测试集进行评估，不要使用训练集
4. 组件内部自动进行预测，无需手动连接Predictions
5. 组件会自动计算所有评估指标，包括MSE、RMSE、MAE、R²和解释方差
6. R²值越接近1表示模型拟合越好
7. RMSE和MAE越小表示预测误差越小
8. 评估结果可以用于模型选择和参数调优";
                
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

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(EvaluateRegressionComponent));
        public override Guid ComponentGuid => new Guid("D6E7F8A9-B0C1-2345-9012-345678901235");
    }
}
