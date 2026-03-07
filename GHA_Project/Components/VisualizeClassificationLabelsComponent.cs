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

namespace SimpleML.Components.Visualization
{
    /// <summary>
    /// Visualize Classification Labels Component
    /// 分类标签可视化组件 - 根据分类标签给几何对象着色预览
    /// </summary>
    public class VisualizeClassificationLabelsComponent : GH_Component, IGH_PreviewObject
    {
        private List<object> _geometries = new List<object>();
        private List<string> _labels = new List<string>();
        private bool _hidden = false;

        public VisualizeClassificationLabelsComponent()
          : base("Visualize Classification Labels", "VizClass",
              "根据分类标签给几何对象着色预览",
              "SimpleML", "08 Visualization")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override BoundingBox ClippingBox => GetBoundingBox();
        public override bool IsPreviewCapable => true;
        public new bool Hidden { get => _hidden; set => _hidden = value; }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "要着色的几何对象（点、曲线、曲面等）", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Labels", "L", "分类标签（Tree结构，每个分支包含一个标签）", GH_ParamAccess.tree);
            pManager.AddColourParameter("Colors", "C", "类别颜色列表（可选，默认使用自动生成的颜色）", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Colored Geometry", "CG", "着色后的几何对象", GH_ParamAccess.tree);
            pManager.AddColourParameter("Color Map", "CM", "颜色映射表", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_GeometricGoo> geometryTree = new GH_Structure<IGH_GeometricGoo>();
            GH_Structure<IGH_Goo> labelsTree = new GH_Structure<IGH_Goo>();
            List<Color> customColors = new List<Color>();

            if (!DA.GetDataTree(0, out geometryTree)) return;
            if (!DA.GetDataTree(1, out labelsTree)) return;
            DA.GetDataList(2, customColors);

            _geometries.Clear();
            _labels.Clear();

            // 提取几何对象和标签
            var paths = geometryTree.Paths;
            var coloredGeometryTree = new GH_Structure<IGH_GeometricGoo>();
            var colorMapTree = new GH_Structure<GH_Colour>();

            // 生成颜色映射
            Dictionary<string, Color> colorMap = GenerateColorMap(labelsTree, customColors);

            int index = 0;
            foreach (var path in paths)
            {
                var branch = geometryTree[path];
                var coloredBranch = new List<IGH_GeometricGoo>();

                foreach (var geo in branch)
                {
                    if (geo != null)
                    {
                        object geometry = null;
                        if (geo is GH_Point ghPoint)
                            geometry = ghPoint.Value;
                        else if (geo is GH_Curve ghCurve)
                            geometry = ghCurve.Value;
                        else if (geo is GH_Surface ghSurface)
                            geometry = ghSurface.Value;
                        else if (geo is GH_Mesh ghMesh)
                            geometry = ghMesh.Value;
                        else if (geo is GH_Brep ghBrep)
                            geometry = ghBrep.Value;
                        
                        if (geometry != null)
                        {
                            _geometries.Add(geometry);
                            
                            // 获取对应的标签
                            string label = "Unknown";
                            if (index < labelsTree.PathCount)
                            {
                                var labelPath = labelsTree.Paths[index];
                                var labelBranch = labelsTree[labelPath];
                                if (labelBranch.Count > 0)
                                {
                                    label = labelBranch[0].ToString();
                                }
                            }
                            _labels.Add(label);

                            coloredBranch.Add(geo);
                            index++;
                        }
                    }
                }

                if (coloredBranch.Count > 0)
                {
                    coloredGeometryTree.AppendRange(coloredBranch, path);
                }
            }

            // 输出颜色映射
            int mapIndex = 0;
            foreach (var kvp in colorMap)
            {
                var path = new GH_Path(mapIndex++);
                colorMapTree.Append(new GH_Colour(kvp.Value), path);
            }

            DA.SetDataTree(0, coloredGeometryTree);
            DA.SetDataTree(1, colorMapTree);
        }

        private Dictionary<string, Color> GenerateColorMap(GH_Structure<IGH_Goo> labelsTree, List<Color> customColors)
        {
            Dictionary<string, Color> colorMap = new Dictionary<string, Color>();
            HashSet<string> uniqueLabels = new HashSet<string>();

            // 收集所有唯一的标签
            foreach (var path in labelsTree.Paths)
            {
                var branch = labelsTree[path];
                foreach (var item in branch)
                {
                    string label = item.ToString();
                    uniqueLabels.Add(label);
                }
            }

            // 生成颜色映射
            var sortedLabels = new List<string>(uniqueLabels);
            sortedLabels.Sort();

            Color[] defaultColors = new Color[]
            {
                Color.FromArgb(255, 31, 119, 180),   // 蓝色
                Color.FromArgb(255, 255, 127, 14),   // 橙色
                Color.FromArgb(255, 44, 160, 44),    // 绿色
                Color.FromArgb(255, 214, 39, 40),     // 红色
                Color.FromArgb(255, 148, 103, 189),  // 紫色
                Color.FromArgb(255, 140, 86, 75),     // 棕色
                Color.FromArgb(255, 227, 119, 194),  // 粉色
                Color.FromArgb(255, 127, 127, 127),  // 灰色
                Color.FromArgb(255, 188, 189, 34),   // 橄榄色
                Color.FromArgb(255, 23, 190, 207),   // 青色
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

        private BoundingBox GetBoundingBox()
        {
            BoundingBox bbox = BoundingBox.Unset;
            foreach (var geo in _geometries)
            {
                if (geo != null)
                {
                    if (geo is Point3d point)
                        bbox.Union(new BoundingBox(point, point));
                    else if (geo is GeometryBase geom)
                        bbox.Union(geom.GetBoundingBox(false));
                }
            }
            return bbox;
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            // 不绘制网格，只绘制线框和点
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_geometries.Count != _labels.Count) return;

            // 生成颜色映射
            Dictionary<string, Color> colorMap = new Dictionary<string, Color>();
            HashSet<string> uniqueLabels = new HashSet<string>(_labels);
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
                colorMap[sortedLabels[i]] = defaultColors[i % defaultColors.Length];
            }

            // 绘制几何对象
            for (int i = 0; i < _geometries.Count; i++)
            {
                var geo = _geometries[i];
                string label = _labels[i];
                Color color = colorMap.ContainsKey(label) ? colorMap[label] : Color.Black;

                if (geo is Point3d point)
                {
                    args.Display.DrawPoint(point, color);
                }
                else if (geo is Curve curve)
                {
                    args.Display.DrawCurve(curve, color, 2);
                }
                else if (geo is Surface surface)
                {
                    args.Display.DrawSurface(surface, color, 1);
                }
                else if (geo is Mesh mesh)
                {
                    args.Display.DrawMeshWires(mesh, color);
                }
                else if (geo is Brep brep)
                {
                    args.Display.DrawBrepWires(brep, color, 1);
                }
                else if (geo is GeometryBase geom)
                {
                    // 通用几何对象，尝试绘制线框
                    if (geom is Curve c)
                        args.Display.DrawCurve(c, color, 2);
                    else if (geom is Surface s)
                        args.Display.DrawSurface(s, color, 1);
                    else if (geom is Mesh m)
                        args.Display.DrawMeshWires(m, color);
                    else if (geom is Brep b)
                        args.Display.DrawBrepWires(b, color, 1);
                }
            }
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(VisualizeClassificationLabelsComponent));
        public override Guid ComponentGuid => new Guid("F2A3B4C5-D6E7-8901-ABCD-EF1234567892");
    }
}
