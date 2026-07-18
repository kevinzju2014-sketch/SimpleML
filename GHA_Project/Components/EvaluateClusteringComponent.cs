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
    public class EvaluateClusteringComponent : GH_Component
    {
        public EvaluateClusteringComponent()
          : base("评估聚类 Evaluate Clustering", "评估聚类",
              "评估聚类模型",
              "SimpleML", "07 Evaluation")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "训练好的聚类模型对象（Generic类型）", GH_ParamAccess.item);
            pManager.AddGenericParameter("Test Dataset", "DS", "测试数据集（Dataset对象），将从中提取特征X和真实标签y（可选）", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Metrics", "M", "评估指标（Tree结构，每个分支包含指标名称和值）", GH_ParamAccess.tree);
            pManager.AddTextParameter("Report", "R", "详细的评估报告", GH_ParamAccess.item);
            pManager.AddGenericParameter("Labels", "L", "聚类标签（Tree结构，每个分支包含一个聚类标签）", GH_ParamAccess.tree);
            pManager.AddTextParameter("Readme", "R", "组件使用说明", GH_ParamAccess.item);
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
                pythonCodeBuilder.AppendLine("    silhouette_score, davies_bouldin_score, calinski_harabasz_score");
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
                pythonCodeBuilder.AppendLine("    # 尝试获取标签，如果不存在则为None");
                pythonCodeBuilder.AppendLine("    try:");
                pythonCodeBuilder.AppendLine("        y_true = dataset.get_y() if dataset.has_labels() else None");
                pythonCodeBuilder.AppendLine("    except:");
                pythonCodeBuilder.AppendLine("        y_true = None");
                pythonCodeBuilder.AppendLine("except Exception as e:");
                pythonCodeBuilder.AppendLine("    print(f'ERROR:无效的Dataset对象: {str(e)}')");
                pythonCodeBuilder.AppendLine("    exit(1)");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 确保数据格式正确");
                pythonCodeBuilder.AppendLine("X = np.array(X)");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 获取聚类标签");
                pythonCodeBuilder.AppendLine("if hasattr(model, 'predict'):");
                pythonCodeBuilder.AppendLine("    labels = model.predict(X)");
                pythonCodeBuilder.AppendLine("elif hasattr(model, 'labels_'):");
                pythonCodeBuilder.AppendLine("    labels = model.labels_");
                pythonCodeBuilder.AppendLine("else:");
                pythonCodeBuilder.AppendLine("    labels = model.fit_predict(X)");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 计算评估指标");
                pythonCodeBuilder.AppendLine("metrics_dict = {}");
                pythonCodeBuilder.AppendLine("n_clusters = len(np.unique(labels[labels >= 0]))  # 排除噪声点（-1）");
                pythonCodeBuilder.AppendLine("metrics_dict['n_clusters'] = int(n_clusters)");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 轮廓系数");
                pythonCodeBuilder.AppendLine("try:");
                pythonCodeBuilder.AppendLine("    if n_clusters > 1:");
                pythonCodeBuilder.AppendLine("        metrics_dict['silhouette_score'] = float(silhouette_score(X, labels))");
                pythonCodeBuilder.AppendLine("    else:");
                pythonCodeBuilder.AppendLine("        metrics_dict['silhouette_score'] = None");
                pythonCodeBuilder.AppendLine("except:");
                pythonCodeBuilder.AppendLine("    metrics_dict['silhouette_score'] = None");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# Davies-Bouldin指数");
                pythonCodeBuilder.AppendLine("try:");
                pythonCodeBuilder.AppendLine("    if n_clusters > 1:");
                pythonCodeBuilder.AppendLine("        metrics_dict['davies_bouldin_score'] = float(davies_bouldin_score(X, labels))");
                pythonCodeBuilder.AppendLine("    else:");
                pythonCodeBuilder.AppendLine("        metrics_dict['davies_bouldin_score'] = None");
                pythonCodeBuilder.AppendLine("except:");
                pythonCodeBuilder.AppendLine("    metrics_dict['davies_bouldin_score'] = None");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# Calinski-Harabasz指数");
                pythonCodeBuilder.AppendLine("try:");
                pythonCodeBuilder.AppendLine("    if n_clusters > 1:");
                pythonCodeBuilder.AppendLine("        metrics_dict['calinski_harabasz_score'] = float(calinski_harabasz_score(X, labels))");
                pythonCodeBuilder.AppendLine("    else:");
                pythonCodeBuilder.AppendLine("        metrics_dict['calinski_harabasz_score'] = None");
                pythonCodeBuilder.AppendLine("except:");
                pythonCodeBuilder.AppendLine("    metrics_dict['calinski_harabasz_score'] = None");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 生成详细报告");
                pythonCodeBuilder.AppendLine("sil_score = metrics_dict.get('silhouette_score', 'N/A')");
                pythonCodeBuilder.AppendLine("db_score = metrics_dict.get('davies_bouldin_score', 'N/A')");
                pythonCodeBuilder.AppendLine("ch_score = metrics_dict.get('calinski_harabasz_score', 'N/A')");
                pythonCodeBuilder.AppendLine("report = f\"\"\"聚类模型评估报告");
                pythonCodeBuilder.AppendLine("==================");
                pythonCodeBuilder.AppendLine("聚类数量: {n_clusters}");
                pythonCodeBuilder.AppendLine("轮廓系数: {sil_score}");
                pythonCodeBuilder.AppendLine("Davies-Bouldin指数: {db_score}");
                pythonCodeBuilder.AppendLine("Calinski-Harabasz指数: {ch_score}");
                pythonCodeBuilder.AppendLine("\"\"\"");
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
                pythonCodeBuilder.AppendLine("readme = '聚类模型评估完成'");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 转换聚类标签为Tree结构格式");
                pythonCodeBuilder.AppendLine("labels_list = labels.tolist() if hasattr(labels, 'tolist') else list(labels)");
                pythonCodeBuilder.AppendLine("labels_tree = [[str(val)] for val in labels_list]");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("print('OUTPUT_0:' + json.dumps(metrics_tree, ensure_ascii=False))");
                pythonCodeBuilder.AppendLine("print('OUTPUT_1:' + report)");
                pythonCodeBuilder.AppendLine("print('OUTPUT_2:' + json.dumps(labels_tree, ensure_ascii=False))");
                pythonCodeBuilder.AppendLine("print('OUTPUT_3:' + readme)");
                
                string pythonCode = pythonCodeBuilder.ToString();

                string output = PythonScriptExecutor.ExecuteCode(pythonCode);
                
                // 解析输出
                string metricsJson = ExtractValue(output, "OUTPUT_0:");
                string report = ExtractValue(output, "OUTPUT_1:");
                string labelsJson = ExtractValue(output, "OUTPUT_2:");
                string pythonReadme = ExtractValue(output, "OUTPUT_3:");

                GH_Structure<GH_String> metricsTree = TreeConverter.ConvertJsonToTree(metricsJson);
                GH_Structure<GH_String> labelsTree = TreeConverter.ConvertJsonToTree(labelsJson);

                DA.SetDataTree(0, metricsTree);
                DA.SetData(1, report);
                DA.SetDataTree(2, labelsTree);
                
                // Readme输出
                string readme = @"组件名称: Evaluate Clustering
功能: 评估聚类模型的性能（自动预测并评估）

===================================================================
输入参数详解:
===================================================================

1. Model (模型) - Generic类型，必需
   • 数据类型: 训练好的聚类模型对象（Base64编码的pickle对象）
   • 连接建议:
     ← Train Cluster的Model输出
     ← Load Model的Model输出
   • 注意事项: 
     - 必须是有效的聚类模型对象
     - 模型必须已经训练完成

2. Test Dataset (测试数据集) - Generic类型，必需
   • 数据类型: Dataset对象（Base64编码的pickle对象）
   • 数据结构: 包含特征数据（X）和真实标签（y，可选）的Dataset对象
   • 说明: 
     - 组件将从Dataset中自动提取X（特征数据）和y（真实标签，如果存在）
     - 组件内部使用Model对X进行预测，然后进行评估
     - 如果Dataset包含标签，会计算有监督和无监督评估指标
     - 如果Dataset不包含标签，会计算无监督评估指标
   • 连接建议:
     ← Split Dataset (Test Dataset) 输出
     ← Create Dataset 输出
   • 注意事项: 
     - Dataset必须包含特征数据X
     - 如果Dataset包含标签，可以计算有监督评估指标（如调整兰德指数）
     - 如果Dataset不包含标签，会计算无监督评估指标（如轮廓系数）

===================================================================
输出参数详解:
===================================================================

1. Metrics (评估指标) - Tree结构
   • 数据类型: Grasshopper Tree结构
   • 数据结构: 
     - 每个分支包含[指标名称, 指标值]
     - 分支路径为 {0}, {1}, {2}...（指标索引）
   • 无监督指标（不需要标签）:
     - {0} → [""n_clusters"", ""3""] - 聚类数量
     - {1} → [""silhouette_score"", ""0.65""] - 轮廓系数，范围-1到1，越大越好
     - {2} → [""davies_bouldin_score"", ""0.8""] - Davies-Bouldin指数，越小越好
     - {3} → [""calinski_harabasz_score"", ""245.3""] - Calinski-Harabasz指数，越大越好
   • 有监督指标（需要标签，如果Dataset包含标签）:
     - 如果Dataset包含标签，会添加有监督指标
   • 格式: Tree结构，每个分支包含指标名称和值
   • 用途: 量化聚类性能，用于模型比较和可视化

2. Report (评估报告) - Text类型
   • 内容: 详细的聚类评估报告
   • 包含信息:
     - 各评估指标的数值
     - 聚类性能总结
     - 聚类数量和质量分析
   • 格式: 可读的文本格式
   • 用途: 详细了解模型的聚类性能

===================================================================
典型工作流程:
===================================================================

简单评估流程:
Create Dataset → Train Cluster → Model → Evaluate Clustering (Model)
Create Dataset → Evaluate Clustering (Test Dataset)

模型比较流程:
Train Cluster (Model A) → Evaluate Clustering → Metrics A
Train Cluster (Model B) → Evaluate Clustering → Metrics B
(比较 Metrics A 和 Metrics B)

===================================================================
注意事项:
===================================================================

1. Model必须是聚类模型，不能是分类或回归模型
2. Test Dataset必须包含特征数据X
3. 组件内部自动进行预测，无需手动连接Predictions
4. 组件会自动计算所有可用的评估指标（根据是否有标签选择有监督或无监督指标）
5. 轮廓系数是最常用的无监督评估指标
6. 如果Test Dataset包含标签，可以计算调整兰德指数等有监督指标
7. 评估结果可以用于选择最佳聚类数量和算法参数
8. 某些聚类算法（如DBSCAN）的评估可能需要特殊处理";
                
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
            int endIndex = output.IndexOf('\n', startIndex);
            if (endIndex == -1) endIndex = output.Length;
            return output.Substring(startIndex, endIndex - startIndex).Trim();
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(EvaluateClusteringComponent));
        public override Guid ComponentGuid => new Guid("C1D2E3F4-A5B6-7890-4567-890123456780");
    }
}
