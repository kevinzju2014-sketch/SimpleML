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
    /// Visualize Regression Component
    /// 回归可视化组件 - 显示预测值 vs 实际值 + 拟合线
    /// </summary>
    public class VisualizeRegressionComponent : GH_Component, IGH_PreviewObject
    {
        private List<Point3d> _scatterPoints = new List<Point3d>();
        private Polyline _fitLine = null;
        private bool _hidden = false;

        public VisualizeRegressionComponent()
          : base(L.Name("VisualizeRegressionComponent"), L.Nick("VisualizeRegressionComponent"), L.Desc("VisualizeRegressionComponent"),
              "SimpleML", "08 Visualization")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override BoundingBox ClippingBox => GetBoundingBox();
        public override bool IsPreviewCapable => true;
        public new bool Hidden { get => _hidden; set => _hidden = value; }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Y True", "YT", "实际值（Tree结构）", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Y Pred", "YP", "预测值（Tree结构）", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Show Fit Line", "SFL", "是否显示拟合线（y=x），默认True", GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Scatter Points", "SP", "散点（x=实际值，y=预测值）", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Fit Line", "FL", "拟合线（y=x）", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "组件使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_Goo> yTrueTree = new GH_Structure<IGH_Goo>();
            GH_Structure<IGH_Goo> yPredTree = new GH_Structure<IGH_Goo>();
            bool showFitLine = true;

            if (!DA.GetDataTree(0, out yTrueTree) || yTrueTree == null || yTrueTree.PathCount == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "必须提供Y True输入");
                return;
            }
            if (!DA.GetDataTree(1, out yPredTree) || yPredTree == null || yPredTree.PathCount == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "必须提供Y Pred输入");
                return;
            }
            DA.GetData(2, ref showFitLine);

            _scatterPoints.Clear();
            _fitLine = null;

            var scatterTree = new GH_Structure<GH_Point>();
            var yTrueList = new List<double>();
            var yPredList = new List<double>();

            // 提取实际值和预测值
            foreach (var path in yTrueTree.Paths)
            {
                var branch = yTrueTree[path];
                foreach (var item in branch)
                {
                    if (double.TryParse(item.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double yTrue))
                    {
                        yTrueList.Add(yTrue);
                    }
                }
            }

            foreach (var path in yPredTree.Paths)
            {
                var branch = yPredTree[path];
                foreach (var item in branch)
                {
                    if (double.TryParse(item.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double yPred))
                    {
                        yPredList.Add(yPred);
                    }
                }
            }

            if (yTrueList.Count != yPredList.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"实际值和预测值数量不匹配：{yTrueList.Count} vs {yPredList.Count}");
            }

            int minCount = Math.Min(yTrueList.Count, yPredList.Count);
            
            // 计算范围用于拟合线
            double minVal = double.MaxValue;
            double maxVal = double.MinValue;

            for (int i = 0; i < minCount; i++)
            {
                double yTrue = yTrueList[i];
                double yPred = yPredList[i];
                
                Point3d point = new Point3d(yTrue, yPred, 0);
                _scatterPoints.Add(point);
                scatterTree.Append(new GH_Point(point), new GH_Path(i));

                minVal = Math.Min(minVal, Math.Min(yTrue, yPred));
                maxVal = Math.Max(maxVal, Math.Max(yTrue, yPred));
            }

            // 创建拟合线（y=x）
            if (showFitLine && minVal < maxVal)
            {
                Point3d p1 = new Point3d(minVal, minVal, 0);
                Point3d p2 = new Point3d(maxVal, maxVal, 0);
                _fitLine = new Polyline(new Point3d[] { p1, p2 });
            }

            DA.SetDataTree(0, scatterTree);
            if (_fitLine != null)
            {
                DA.SetData(1, new GH_Curve(_fitLine.ToNurbsCurve()));
            }
            
            string readme = $"回归可视化：显示{minCount}个数据点的预测值 vs 实际值散点图";
            if (showFitLine)
            {
                readme += "，包含y=x拟合线（理想情况所有点应在这条线上）";
            }
            DA.SetData(2, readme);
        }

        private BoundingBox GetBoundingBox()
        {
            BoundingBox bbox = BoundingBox.Unset;
            foreach (var point in _scatterPoints)
            {
                bbox.Union(point);
            }
            if (_fitLine != null)
            {
                bbox.Union(_fitLine.BoundingBox);
            }
            return bbox;
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            // 不绘制网格
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            // 绘制散点
            foreach (var point in _scatterPoints)
            {
                args.Display.DrawPoint(point, Color.Blue);
            }

            // 绘制拟合线
            if (_fitLine != null)
            {
                args.Display.DrawPolyline(_fitLine, Color.Red, 2);
            }
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(VisualizeRegressionComponent));
        public override Guid ComponentGuid => new Guid("F4A5B6C7-D8E9-0123-ABCD-EF1234567894");
    }
}
