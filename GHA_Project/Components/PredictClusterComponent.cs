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
    public class PredictClusterComponent : GH_Component
    {
        public PredictClusterComponent()
          : base("预测聚类 Predict Cluster", "预测聚类",
              "使用训练好的聚类模型进行预测（分配聚类标签）",
              "SimpleML", "06 Prediction")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "训练好的聚类模型对象（Generic类型）", GH_ParamAccess.item);
            pManager.AddGenericParameter("X", "X", "待预测的特征数据（Tree结构）", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Labels", "L", "聚类标签（Tree结构，每个分支包含一个聚类标签）", GH_ParamAccess.tree);
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
                pythonCodeBuilder.AppendLine("from components.predict_components import predict_cluster");
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
                pythonCodeBuilder.AppendLine("# 调用预测函数");
                pythonCodeBuilder.AppendLine("labels, readme = predict_cluster(model, X=X, dataset=None)");
                
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 转换标签为Tree结构格式");
                pythonCodeBuilder.AppendLine("import numpy as np");
                pythonCodeBuilder.AppendLine("labels_list = labels.tolist() if hasattr(labels, 'tolist') else list(labels)");
                pythonCodeBuilder.AppendLine("labels_tree = [[val] for val in labels_list] if len(labels_list) > 0 and isinstance(labels_list[0], (int, float)) else labels_list");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("print('OUTPUT_0:' + json.dumps(labels_tree, ensure_ascii=False))");
                pythonCodeBuilder.AppendLine("print('OUTPUT_1:' + readme)");
                
                string pythonCode = pythonCodeBuilder.ToString();

                string output = PythonScriptExecutor.ExecuteCode(pythonCode);
                
                // 解析输出
                string labelsJson = ExtractValue(output, "OUTPUT_0:");
                string pythonReadme = ExtractValue(output, "OUTPUT_1:");

                GH_Structure<GH_String> labelsTree = TreeConverter.ConvertJsonToTree(labelsJson);

                DA.SetDataTree(0, labelsTree);
                
                // Readme输出
                string readme = @"组件名称: Predict Cluster
功能: 使用训练好的聚类模型进行预测（分配聚类标签）

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. Model (模型) - Generic类型，必需
   • 数据类型: 训练好的聚类模型对象（Base64编码的pickle对象）
   • 数据结构: 包含训练好的聚类模型的所有信息
   • 连接建议:
     ← Train Cluster的Model输出（最常用）
     ← Load Model的Model输出（加载已保存的模型）
   • 注意事项: 
     - 必须是有效的聚类模型对象
     - 模型必须已经训练完成
     - 某些聚类算法（如DBSCAN）可能无法预测新数据

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

1. Labels (聚类标签) - Tree结构
   • 数据类型: Grasshopper Tree结构
   • 数据结构: 
     - 每个分支包含一个聚类标签（单元素分支）
     - 分支路径为 {0}, {1}, {2}...（样本索引）
   • 示例:
     {0} → [""0""]  (属于聚类0)
     {1} → [""0""]  (属于聚类0)
     {2} → [""1""]  (属于聚类1)
     {3} → [""-1""] (噪声点，仅DBSCAN)
   • 说明: 
     - 标签是整数，表示样本所属的聚类
     - 对于K-Means，标签范围是0到n_clusters-1
     - 对于DBSCAN，-1表示噪声点（不属于任何聚类）
   • 连接建议:
     → Write CSV/Excel的Data输入（保存聚类结果）
     → Evaluate Clustering的Labels输入（评估聚类结果）
     → 其他Grasshopper组件（进一步处理）

═══════════════════════════════════════════════════════════════
典型工作流程:
═══════════════════════════════════════════════════════════════

聚类预测流程:
Create Dataset → Train Cluster → Model → Predict Cluster → Labels

新数据聚类:
Read CSV/Excel → Create Dataset → Predict Cluster (Dataset) → Labels

完整评估流程:
Create Dataset → Train Cluster → Model → Predict Cluster (Dataset) → Labels
Create Dataset → Evaluate Clustering

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. Model必须是聚类模型，不能是分类或回归模型
2. Data Set中的特征数量必须与训练时一致
3. 如果训练时进行了数据预处理（如标准化），预测时也需要相同的预处理
4. 某些聚类算法（如DBSCAN）可能无法预测新数据，只能对训练数据分配标签
5. 聚类标签是整数，表示样本所属的聚类
6. 对于DBSCAN，-1表示噪声点（不属于任何聚类）";
                
                DA.SetData(1, readme);
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

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(PredictClusterComponent));
        public override Guid ComponentGuid => new Guid("D7E8F9A0-B1C2-3456-0123-456789012357");
    }
}
