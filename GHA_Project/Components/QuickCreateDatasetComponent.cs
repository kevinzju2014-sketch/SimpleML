using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SimpleML.Core;
using Rhino;

using SimpleML.Localization;
namespace SimpleML.Components.DatasetManagement
{
    /// <summary>
    /// 简洁创建数据集：仅 X / y / 列名。
    /// </summary>
    public class QuickCreateDatasetComponent : GH_Component
    {
        public QuickCreateDatasetComponent()
          : base(L.Name("QuickCreateDatasetComponent"), L.Nick("QuickCreateDatasetComponent"), L.Desc("QuickCreateDatasetComponent"),
              "SimpleML", "03 Dataset")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("X", "X", "特征 Tree", GH_ParamAccess.tree);
            pManager.AddGenericParameter("y", "y", "标签 Tree（可选）", GH_ParamAccess.tree);
            pManager.AddTextParameter("X Names", "XN", "特征名（可选）", GH_ParamAccess.list);
        }

        public override void CreateAttributes()
        {
            base.CreateAttributes();
            if (Params.Input.Count >= 3)
            {
                Params.Input[1].Optional = true;
                Params.Input[2].Optional = true;
            }
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Dataset", "DS", "数据集", GH_ParamAccess.item);
            pManager.AddTextParameter("Info", "I", "信息", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "RM", "说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_Goo> xTree = null;
            GH_Structure<IGH_Goo> yTree = null;
            var names = new List<string>();
            if (!DA.GetDataTree(0, out xTree) || xTree == null || xTree.DataCount == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "必须提供 X");
                return;
            }
            bool hasY = DA.GetDataTree(1, out yTree) && yTree != null && yTree.DataCount > 0;
            DA.GetDataList(2, names);

            try
            {
                string mymlPath = PathResolver.GetMyMLPath();
                if (string.IsNullOrEmpty(mymlPath))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "未找到包路径。请打开「安装指南」。");
                    return;
                }

                string xPy = TreeToPython(xTree);
                string yPy = hasY ? TreeToPython(yTree) : "None";
                string namesPy = "None";
                if (names != null && names.Count > 0)
                {
                    var parts = new List<string>();
                    foreach (var n in names) parts.Add("'" + (n ?? "").Replace("'", "\\'") + "'");
                    namesPy = "[" + string.Join(",", parts) + "]";
                }

                var sb = new StringBuilder();
                sb.Append(PythonBridge.BuildBootstrap(mymlPath));
                sb.AppendLine("import pickle, base64");
                sb.AppendLine("from components.dataset_components import create_dataset");
                sb.AppendLine("X = " + xPy);
                sb.AppendLine("y = " + yPy);
                sb.AppendLine("X_names = " + namesPy);
                sb.AppendLine("ds = create_dataset(X, y, X_names=X_names)");
                sb.AppendLine("info = f'shape={ds.X.shape}, labels={ds.has_labels()}'");
                sb.AppendLine("print('OUTPUT_0:' + base64.b64encode(pickle.dumps(ds)).decode('utf-8'))");
                sb.AppendLine("print('OUTPUT_1:' + info)");
                sb.AppendLine("print('OUTPUT_2:' + '快速数据集已创建。可连接分割数据集或智能训练。')");

                string output = PythonScriptExecutor.ExecuteCode(sb.ToString());
                DA.SetData(0, PythonBridge.ExtractValue(output, "OUTPUT_0:"));
                DA.SetData(1, PythonBridge.ExtractValue(output, "OUTPUT_1:"));
                DA.SetData(2, PythonBridge.ExtractValue(output, "OUTPUT_2:"));
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                RhinoApp.WriteLine("SimpleML QuickDataset: " + ex);
            }
        }

        private static string TreeToPython(GH_Structure<IGH_Goo> tree)
        {
            var rows = new List<string>();
            foreach (GH_Path path in tree.Paths)
            {
                var cells = new List<string>();
                foreach (IGH_Goo goo in tree.get_Branch(path))
                {
                    if (goo == null) { cells.Add("0"); continue; }
                    double num;
                    if (goo.CastTo(out num))
                        cells.Add(num.ToString(CultureInfo.InvariantCulture));
                    else
                        cells.Add("'" + (goo.ToString() ?? "").Replace("'", "\\'") + "'");
                }
                rows.Add("[" + string.Join(",", cells) + "]");
            }
            return "[" + string.Join(",", rows) + "]";
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(QuickCreateDatasetComponent));
        public override Guid ComponentGuid => new Guid("B8C9D0E1-F2A3-4567-1234-818283848586");
    }
}
