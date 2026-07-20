using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Display;
using SimpleML.Core;
using Rhino;

using SimpleML.Localization;
namespace SimpleML.Components.Visualization
{
    /// <summary>
    /// Reduce Dimensions Component
    /// 降维可视化组件 - 将高维数据降维到2D/3D并可视化
    /// </summary>
    public class ReduceDimensionsComponent : GH_Component, IGH_PreviewObject
    {
        private List<Point3d> _points = new List<Point3d>();
        private List<Color> _colors = new List<Color>();
        private bool _hidden = false;

        public ReduceDimensionsComponent()
          : base(L.Name("ReduceDimensionsComponent"), L.Nick("ReduceDimensionsComponent"), L.Desc("ReduceDimensionsComponent"),
              "SimpleML", "08 Visualization")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override BoundingBox ClippingBox => GetBoundingBox();
        public override bool IsPreviewCapable => true;
        public new bool Hidden { get => _hidden; set => _hidden = value; }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("X", "X", "特征数据（Tree结构）", GH_ParamAccess.tree);
            pManager.AddTextParameter("Method", "M", "降维方法：'pca' 或 'tsne'，默认'pca'", GH_ParamAccess.item, "pca");
            pManager.AddIntegerParameter("Dimensions", "D", "降维后的维度（2或3），默认2", GH_ParamAccess.item, 2);
            pManager.AddGenericParameter("Labels", "L", "标签（可选，用于着色）", GH_ParamAccess.tree);
            pManager.AddColourParameter("Colors", "C", "标签颜色列表（可选）", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "降维后的点（2D或3D）", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Explained Variance", "EV", "解释方差（仅PCA）", GH_ParamAccess.list);
            pManager.AddTextParameter("Readme", "R", "组件使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_Goo> xTree = new GH_Structure<IGH_Goo>();
            string method = "pca";
            int dimensions = 2;
            GH_Structure<IGH_Goo> labelsTree = new GH_Structure<IGH_Goo>();
            List<Color> customColors = new List<Color>();

            if (!DA.GetDataTree(0, out xTree) || xTree == null || xTree.PathCount == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "必须提供X输入（特征数据）");
                return;
            }
            DA.GetData(1, ref method);
            DA.GetData(2, ref dimensions);
            DA.GetDataTree(3, out labelsTree);
            DA.GetDataList(4, customColors);

            if (dimensions < 2 || dimensions > 3)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "维度必须是2或3");
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

                string xListStr = ConvertTreeToPythonList(xTree);
                string labelsListStr = labelsTree.PathCount > 0 ? ConvertTreeToPythonList(labelsTree) : "None";

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
                pythonCodeBuilder.AppendLine("from components.visualization_components import reduce_dimensions");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 处理X输入");
                pythonCodeBuilder.Append("X = ");
                pythonCodeBuilder.Append(xListStr);
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("# 处理Labels输入");
                if (labelsTree.PathCount > 0)
                {
                    pythonCodeBuilder.Append("labels = ");
                    pythonCodeBuilder.Append(labelsListStr);
                    pythonCodeBuilder.AppendLine();
                }
                else
                {
                    pythonCodeBuilder.AppendLine("labels = None");
                }
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine($"# 执行降维");
                pythonCodeBuilder.AppendLine($"reduced_X, explained_variance, readme = reduce_dimensions(X, method='{method}', n_components={dimensions}, labels=labels)");
                pythonCodeBuilder.AppendLine();
                pythonCodeBuilder.AppendLine("print('OUTPUT_0:' + json.dumps(reduced_X, ensure_ascii=False))");
                pythonCodeBuilder.AppendLine("print('OUTPUT_1:' + json.dumps(explained_variance if explained_variance else [], ensure_ascii=False))");
                pythonCodeBuilder.AppendLine("print('OUTPUT_2:' + readme)");

                string pythonCode = pythonCodeBuilder.ToString();
                string output = PythonScriptExecutor.ExecuteCode(pythonCode);

                string reducedXJson = ExtractValue(output, "OUTPUT_0:");
                string explainedVarianceJson = ExtractValue(output, "OUTPUT_1:");
                string readme = ExtractValue(output, "OUTPUT_2:");

                // 解析降维后的点
                var pointsTree = ParsePointsJson(reducedXJson, dimensions);
                _points.Clear();
                _colors.Clear();

                // 生成颜色（如果有标签）
                Dictionary<string, Color> colorMap = new Dictionary<string, Color>();
                if (labelsTree.PathCount > 0)
                {
                    colorMap = GenerateColorMap(labelsTree, customColors);
                }

                int pointIndex = 0;
                foreach (var path in pointsTree.Paths)
                {
                    var branch = pointsTree[path];
                    foreach (var point in branch)
                    {
                        _points.Add(point.Value);
                        if (labelsTree.PathCount > 0 && pointIndex < labelsTree.PathCount)
                        {
                            var labelPath = labelsTree.Paths[pointIndex];
                            var labelBranch = labelsTree[labelPath];
                            if (labelBranch.Count > 0)
                            {
                                string label = labelBranch[0].ToString();
                                _colors.Add(colorMap.ContainsKey(label) ? colorMap[label] : Color.Black);
                            }
                            else
                            {
                                _colors.Add(Color.Black);
                            }
                        }
                        else
                        {
                            _colors.Add(Color.Blue);
                        }
                        pointIndex++;
                    }
                }

                // 解析解释方差
                List<double> explainedVariance = new List<double>();
                if (!string.IsNullOrEmpty(explainedVarianceJson) && explainedVarianceJson != "[]")
                {
                try
                {
                    // 手动解析JSON数组
                    var varianceArray = ParseJsonArray(explainedVarianceJson);
                    foreach (var val in varianceArray)
                    {
                        if (double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double d))
                            explainedVariance.Add(d);
                    }
                }
                catch { }
                }

                DA.SetDataTree(0, pointsTree);
                DA.SetDataList(1, explainedVariance);
                DA.SetData(2, readme);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, L.T("err.exec_failed", ex.Message));
                RhinoApp.WriteLine($"SimpleML错误: {ex}");
            }
        }

        private GH_Structure<GH_Point> ParsePointsJson(string jsonData, int dimensions)
        {
            GH_Structure<GH_Point> tree = new GH_Structure<GH_Point>();
            
            try
            {
                if (string.IsNullOrEmpty(jsonData)) return tree;
                
                // 手动解析JSON数组
                var rows = ParseJsonRows(jsonData);
                
                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    var values = ParseJsonArray(row);
                    
                    if (values.Count >= dimensions)
                    {
                        if (double.TryParse(values[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double x) &&
                            double.TryParse(values[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double y))
                        {
                            Point3d point;
                            if (dimensions == 2)
                            {
                                point = new Point3d(x, y, 0);
                            }
                            else if (values.Count >= 3 && double.TryParse(values[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double z))
                            {
                                point = new Point3d(x, y, z);
                            }
                            else
                            {
                                point = new Point3d(x, y, 0);
                            }
                            tree.Append(new GH_Point(point), new GH_Path(i));
                        }
                    }
                }
            }
            catch { }
            
            return tree;
        }

        private List<string> ParseJsonRows(string jsonData)
        {
            List<string> rows = new List<string>();
            if (string.IsNullOrEmpty(jsonData)) return rows;
            
            string trimmed = jsonData.Trim();
            if (trimmed.StartsWith("["))
                trimmed = trimmed.Substring(1);
            if (trimmed.EndsWith("]"))
                trimmed = trimmed.Substring(0, trimmed.Length - 1);
            trimmed = trimmed.Trim();
            
            if (string.IsNullOrEmpty(trimmed)) return rows;
            
            int bracketCount = 0;
            bool inString = false;
            bool escapeNext = false;
            int startIndex = 0;
            
            for (int i = 0; i < trimmed.Length; i++)
            {
                if (escapeNext)
                {
                    escapeNext = false;
                    continue;
                }
                
                char c = trimmed[i];
                if (c == '\\' && inString)
                {
                    escapeNext = true;
                    continue;
                }
                if (c == '"')
                {
                    inString = !inString;
                    continue;
                }
                
                if (!inString)
                {
                    if (c == '[')
                        bracketCount++;
                    else if (c == ']')
                        bracketCount--;
                    else if (bracketCount == 0 && c == ',')
                    {
                        string row = trimmed.Substring(startIndex, i - startIndex).Trim();
                        if (!string.IsNullOrEmpty(row))
                            rows.Add(row);
                        startIndex = i + 1;
                    }
                }
            }
            
            if (startIndex < trimmed.Length)
            {
                string row = trimmed.Substring(startIndex).Trim();
                if (!string.IsNullOrEmpty(row))
                    rows.Add(row);
            }
            
            return rows;
        }

        private List<string> ParseJsonArray(string jsonArray)
        {
            List<string> values = new List<string>();
            if (string.IsNullOrEmpty(jsonArray)) return values;
            
            bool inString = false;
            bool escapeNext = false;
            int startIndex = 0;
            
            for (int i = 0; i < jsonArray.Length; i++)
            {
                if (escapeNext)
                {
                    escapeNext = false;
                    continue;
                }
                
                char c = jsonArray[i];
                if (c == '\\' && inString)
                {
                    escapeNext = true;
                    continue;
                }
                if (c == '"')
                {
                    inString = !inString;
                    continue;
                }
                
                if (!inString && c == ',')
                {
                    string value = jsonArray.Substring(startIndex, i - startIndex).Trim();
                    if (!string.IsNullOrEmpty(value))
                        values.Add(value);
                    startIndex = i + 1;
                }
            }
            
            if (startIndex < jsonArray.Length)
            {
                string value = jsonArray.Substring(startIndex).Trim();
                if (!string.IsNullOrEmpty(value))
                    values.Add(value);
            }
            
            return values;
        }

        private Dictionary<string, Color> GenerateColorMap(GH_Structure<IGH_Goo> labelsTree, List<Color> customColors)
        {
            Dictionary<string, Color> colorMap = new Dictionary<string, Color>();
            HashSet<string> uniqueLabels = new HashSet<string>();

            foreach (var path in labelsTree.Paths)
            {
                var branch = labelsTree[path];
                foreach (var item in branch)
                {
                    uniqueLabels.Add(item.ToString());
                }
            }

            var sortedLabels = new List<string>(uniqueLabels);
            sortedLabels.Sort();

            Color[] defaultColors = new Color[]
            {
                Color.FromArgb(255, 31, 119, 180),
                Color.FromArgb(255, 255, 127, 14),
                Color.FromArgb(255, 44, 160, 44),
                Color.FromArgb(255, 214, 39, 40),
                Color.FromArgb(255, 148, 103, 189),
                Color.FromArgb(255, 140, 86, 75),
                Color.FromArgb(255, 227, 119, 194),
                Color.FromArgb(255, 127, 127, 127),
                Color.FromArgb(255, 188, 189, 34),
                Color.FromArgb(255, 23, 190, 207),
            };

            for (int i = 0; i < sortedLabels.Count; i++)
            {
                string label = sortedLabels[i];
                if (i < customColors.Count)
                {
                    colorMap[label] = customColors[i];
                }
                else
                {
                    colorMap[label] = defaultColors[i % defaultColors.Length];
                }
            }

            return colorMap;
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

        private BoundingBox GetBoundingBox()
        {
            BoundingBox bbox = BoundingBox.Unset;
            foreach (var point in _points)
            {
                bbox.Union(point);
            }
            return bbox;
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            // 不绘制网格
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_points.Count != _colors.Count) return;

            for (int i = 0; i < _points.Count; i++)
            {
                args.Display.DrawPoint(_points[i], _colors[i]);
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

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(ReduceDimensionsComponent));
        public override Guid ComponentGuid => new Guid("F3A4B5C6-D7E8-9012-ABCD-EF1234567893");
    }
}
