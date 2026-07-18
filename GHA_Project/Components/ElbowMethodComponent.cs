using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.Visualization
{
    /// <summary>
    /// Elbow Method Component
    /// Elbow方法组件 - 用于确定最佳聚类数
    /// </summary>
    public class ElbowMethodComponent : GH_Component
    {
        public ElbowMethodComponent()
          : base("肘部法则 Elbow Method", "肘部法则",
              "使用Elbow方法确定最佳聚类数",
              "SimpleML", "08 Visualization")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("X", "X", "特征数据（Tree结构）", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Max Clusters", "MC", "最大聚类数，默认10", GH_ParamAccess.item, 10);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("N Clusters", "NC", "聚类数列表", GH_ParamAccess.list);
            pManager.AddNumberParameter("Scores", "S", "对应的SSE得分列表", GH_ParamAccess.list);
            pManager.AddTextParameter("Readme", "R", "组件使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_Goo> xTree = new GH_Structure<IGH_Goo>();
            int maxClusters = 10;

            if (!DA.GetDataTree(0, out xTree) || xTree == null || xTree.PathCount == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "必须提供X输入（特征数据）");
                return;
            }
            DA.GetData(1, ref maxClusters);

            if (maxClusters < 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "最大聚类数必须至少为2");
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

                string xListStr = ConvertTreeToPythonList(xTree);

                System.Text.StringBuilder pythonCodeBuilder = new System.Text.StringBuilder();
                pythonCodeBuilder.AppendLine("# -*- coding: utf-8 -*-");
                pythonCodeBuilder.AppendLine("import sys");
                pythonCodeBuilder.AppendLine("import os");
                pythonCodeBuilder.AppendLine("import json");
                pythonCodeBuilder.AppendLine("import site");
                pythonCodeBuilder.AppendLine("import io");
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
                pythonCodeBuilder.AppendLine("from components.visualization_components import calculate_elbow_score");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 处理X输入");
                pythonCodeBuilder.Append("X = ");
                pythonCodeBuilder.Append(xListStr);
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine($"# 计算Elbow得分");
                pythonCodeBuilder.AppendLine($"n_clusters_list, scores, readme = calculate_elbow_score(X, max_clusters={maxClusters})");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("print('OUTPUT_0:' + json.dumps(n_clusters_list, ensure_ascii=False))");
                pythonCodeBuilder.AppendLine("print('OUTPUT_1:' + json.dumps(scores, ensure_ascii=False))");
                pythonCodeBuilder.AppendLine("print('OUTPUT_2:' + readme)");

                string pythonCode = pythonCodeBuilder.ToString();
                string output = PythonScriptExecutor.ExecuteCode(pythonCode);

                string nClustersJson = ExtractValue(output, "OUTPUT_0:");
                string scoresJson = ExtractValue(output, "OUTPUT_1:");
                string readme = ExtractValue(output, "OUTPUT_2:");

                // 解析输出
                List<int> nClusters = new List<int>();
                List<double> scores = new List<double>();

                try
                {
                    // 手动解析JSON数组
                    var clustersArray = ParseJsonIntArray(nClustersJson);
                    nClusters.AddRange(clustersArray);
                }
                catch { }

                try
                {
                    // 手动解析JSON数组
                    var scoresArray = ParseJsonDoubleArray(scoresJson);
                    scores.AddRange(scoresArray);
                }
                catch { }

                DA.SetDataList(0, nClusters);
                DA.SetDataList(1, scores);
                DA.SetData(2, readme);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"执行失败: {ex.Message}");
                RhinoApp.WriteLine($"SimpleML错误: {ex}");
            }
        }

        private string ConvertTreeToPythonList(GH_Structure<IGH_Goo> tree)
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
                    if (double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double num))
                        sb.Append(num.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    else
                        sb.Append("\"").Append(value.Replace("\\", "\\\\").Replace("\"", "\\\"")).Append("\"");
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

        private List<int> ParseJsonIntArray(string jsonData)
        {
            List<int> result = new List<int>();
            if (string.IsNullOrEmpty(jsonData)) return result;
            
            string trimmed = jsonData.Trim();
            if (trimmed.StartsWith("["))
                trimmed = trimmed.Substring(1);
            if (trimmed.EndsWith("]"))
                trimmed = trimmed.Substring(0, trimmed.Length - 1);
            trimmed = trimmed.Trim();
            
            if (string.IsNullOrEmpty(trimmed)) return result;
            
            string[] parts = trimmed.Split(',');
            foreach (var part in parts)
            {
                string val = part.Trim();
                if (int.TryParse(val, out int intVal))
                {
                    result.Add(intVal);
                }
            }
            
            return result;
        }

        private List<double> ParseJsonDoubleArray(string jsonData)
        {
            List<double> result = new List<double>();
            if (string.IsNullOrEmpty(jsonData)) return result;
            
            string trimmed = jsonData.Trim();
            if (trimmed.StartsWith("["))
                trimmed = trimmed.Substring(1);
            if (trimmed.EndsWith("]"))
                trimmed = trimmed.Substring(0, trimmed.Length - 1);
            trimmed = trimmed.Trim();
            
            if (string.IsNullOrEmpty(trimmed)) return result;
            
            string[] parts = trimmed.Split(',');
            foreach (var part in parts)
            {
                string val = part.Trim();
                if (double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double doubleVal))
                {
                    result.Add(doubleVal);
                }
            }
            
            return result;
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(ElbowMethodComponent));
        public override Guid ComponentGuid => new Guid("F5A6B7C8-D9E0-1234-ABCD-EF1234567895");
    }
}
