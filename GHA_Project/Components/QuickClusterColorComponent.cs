using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.Visualization
{
    /// <summary>
    /// 点/点云 → 聚类标签 → 颜色（设计师友好一键宏）。
    /// </summary>
    public class QuickClusterColorComponent : GH_Component
    {
        public QuickClusterColorComponent()
          : base("一键聚类上色 Quick Cluster Color", "聚类上色",
              "输入点与聚类模型（或标签），输出着色点。适合快速演示。",
              "SimpleML", "08 Visualization")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "点列表", GH_ParamAccess.list);
            pManager.AddGenericParameter("Model", "M", "聚类模型（可选，若无 Labels）", GH_ParamAccess.item);
            pManager.AddGenericParameter("Labels", "L", "聚类标签（可选）", GH_ParamAccess.list);
            pManager.AddIntegerParameter("K", "K", "若需现场训练时的簇数", GH_ParamAccess.item, 3);
        }

        public override void CreateAttributes()
        {
            base.CreateAttributes();
            if (Params.Input.Count >= 4)
            {
                Params.Input[1].Optional = true;
                Params.Input[2].Optional = true;
            }
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "原点", GH_ParamAccess.list);
            pManager.AddColourParameter("Colors", "C", "按簇着色", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Labels", "L", "簇标签", GH_ParamAccess.list);
            pManager.AddGenericParameter("Model", "M", "使用的模型（若现场训练则输出）", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "RM", "说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var pts = new List<Point3d>();
            object modelObj = null;
            var labelGoos = new List<IGH_Goo>();
            int k = 3;
            if (!DA.GetDataList(0, pts) || pts.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "请提供 Points");
                return;
            }
            bool hasModel = DA.GetData(1, ref modelObj) && modelObj != null;
            DA.GetDataList(2, labelGoos);
            DA.GetData(3, ref k);

            try
            {
                string mymlPath = PathResolver.GetMyMLPath();
                if (string.IsNullOrEmpty(mymlPath))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "未找到包路径。请打开「安装指南」。");
                    return;
                }

                var xyz = new StringBuilder("[");
                for (int i = 0; i < pts.Count; i++)
                {
                    if (i > 0) xyz.Append(",");
                    xyz.AppendFormat(CultureInfo.InvariantCulture, "[{0},{1},{2}]", pts[i].X, pts[i].Y, pts[i].Z);
                }
                xyz.Append("]");

                var sb = new StringBuilder();
                sb.Append(PythonBridge.BuildBootstrap(mymlPath));
                sb.AppendLine("import pickle, base64, json");
                sb.AppendLine("import numpy as np");
                sb.AppendLine("from components.dataset_components import create_dataset");
                sb.AppendLine("from components.smart_train import smart_train");
                sb.AppendLine("from components.predict_components import predict_cluster");
                sb.AppendLine("X = np.array(" + xyz + ", dtype=float)");
                sb.AppendLine("labels = None");
                sb.AppendLine("model_b64 = ''");
                if (labelGoos != null && labelGoos.Count > 0)
                {
                    var labs = new List<string>();
                    foreach (var g in labelGoos)
                    {
                        double n; int i;
                        if (g != null && g.CastTo(out i)) labs.Add(i.ToString());
                        else if (g != null && g.CastTo(out n)) labs.Add(((int)n).ToString());
                        else labs.Add("0");
                    }
                    sb.AppendLine("labels = np.array([" + string.Join(",", labs) + "], dtype=int)");
                }
                else if (hasModel)
                {
                    sb.AppendLine("model = pickle.loads(base64.b64decode(r'''" + modelObj + "'''))");
                    sb.AppendLine("labels, _ = predict_cluster(model, X=X)");
                    sb.AppendLine("labels = np.array(labels).ravel()");
                    sb.AppendLine("model_b64 = base64.b64encode(pickle.dumps(model)).decode('utf-8')");
                }
                else
                {
                    sb.AppendLine("ds = create_dataset(X, None)");
                    sb.AppendLine($"model, *_rest = smart_train(ds, task='clustering', n_clusters={k})");
                    sb.AppendLine("labels, _ = predict_cluster(model, X=X)");
                    sb.AppendLine("labels = np.array(labels).ravel()");
                    sb.AppendLine("model_b64 = base64.b64encode(pickle.dumps(model)).decode('utf-8')");
                }
                sb.AppendLine("print('OUTPUT_0:' + json.dumps(labels.astype(int).tolist(), separators=(',',':')))");
                sb.AppendLine("print('OUTPUT_1:' + model_b64)");
                sb.AppendLine("print('OUTPUT_2:' + '一键聚类上色完成。Colors 可直接接 Custom Preview。')");

                string output = PythonScriptExecutor.ExecuteCode(sb.ToString());
                string labJson = PythonBridge.ExtractValue(output, "OUTPUT_0:");
                var labels = new List<int>();
                string t = labJson.Trim().Trim('[', ']');
                if (!string.IsNullOrEmpty(t))
                {
                    foreach (var p in t.Split(','))
                    {
                        int v; if (int.TryParse(p.Trim(), out v)) labels.Add(v);
                    }
                }

                var colors = new List<Color>();
                Color[] palette = {
                    Color.FromArgb(255, 193, 7),
                    Color.FromArgb(76, 175, 80),
                    Color.FromArgb(33, 150, 243),
                    Color.FromArgb(244, 67, 54),
                    Color.FromArgb(156, 39, 176),
                    Color.FromArgb(0, 150, 136),
                    Color.FromArgb(255, 87, 34),
                    Color.FromArgb(63, 81, 181)
                };
                for (int i = 0; i < pts.Count; i++)
                {
                    int lab = i < labels.Count ? labels[i] : 0;
                    if (lab < 0) colors.Add(Color.Gray);
                    else colors.Add(palette[Math.Abs(lab) % palette.Length]);
                }

                DA.SetDataList(0, pts);
                DA.SetDataList(1, colors);
                DA.SetDataList(2, labels);
                string modelOut = PythonBridge.ExtractValue(output, "OUTPUT_1:");
                if (!string.IsNullOrEmpty(modelOut)) DA.SetData(3, modelOut);
                else if (hasModel) DA.SetData(3, modelObj?.ToString());
                DA.SetData(4, PythonBridge.ExtractValue(output, "OUTPUT_2:"));
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                RhinoApp.WriteLine("SimpleML QuickClusterColor: " + ex);
            }
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(QuickClusterColorComponent));
        public override Guid ComponentGuid => new Guid("C9D0E1F2-A3B4-5678-2345-919293949596");
    }
}
