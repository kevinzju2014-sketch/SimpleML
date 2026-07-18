using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.ModelPrediction
{
    /// <summary>
    /// 统一预测：自动识别分类/回归/聚类；Dataset 或 X 均可。
    /// </summary>
    public class PredictAutoComponent : GH_Component
    {
        public PredictAutoComponent()
          : base("预测 Predict", "预测",
              "统一预测：自动识别模型类型。可接 Dataset 或 X。",
              "SimpleML", "06 Prediction")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "训练好的模型", GH_ParamAccess.item);
            pManager.AddGenericParameter("Dataset", "DS", "待预测数据集（优先）", GH_ParamAccess.item);
            pManager.AddGenericParameter("X", "X", "特征 Tree（Dataset 为空时使用）", GH_ParamAccess.tree);
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
            pManager.AddGenericParameter("Predictions", "P", "预测结果", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Probabilities", "Prob", "分类概率（若可用）", GH_ParamAccess.tree);
            pManager.AddTextParameter("Task", "T", "检测到的任务类型", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "RM", "说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object modelObj = null;
            object datasetObj = null;
            GH_Structure<IGH_Goo> xTree = null;
            if (!DA.GetData(0, ref modelObj)) return;
            bool hasDs = DA.GetData(1, ref datasetObj) && datasetObj != null && !string.IsNullOrEmpty(datasetObj.ToString());
            bool hasX = DA.GetDataTree(2, out xTree) && xTree != null && xTree.DataCount > 0;
            if (!hasDs && !hasX)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "请提供 Dataset 或 X");
                return;
            }

            try
            {
                string mymlPath = PathResolver.GetMyMLPath();
                if (string.IsNullOrEmpty(mymlPath))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "未找到包路径。请打开「安装指南」或运行安装脚本。");
                    return;
                }

                var sb = new StringBuilder();
                sb.Append(PythonBridge.BuildBootstrap(mymlPath));
                sb.AppendLine("import pickle, base64, json");
                sb.AppendLine("from components.ux_helpers import predict_auto");
                sb.AppendLine("model = pickle.loads(base64.b64decode(r'''" + (modelObj?.ToString() ?? "") + "'''))");
                if (hasDs)
                {
                    sb.AppendLine("dataset = pickle.loads(base64.b64decode(r'''" + datasetObj + "'''))");
                    sb.AppendLine("preds, proba, readme, task = predict_auto(model, dataset=dataset)");
                }
                else
                {
                    string xPy = TreeToPythonLiteral(xTree);
                    sb.AppendLine("X = " + xPy);
                    sb.AppendLine("preds, proba, readme, task = predict_auto(model, X=X)");
                }
                sb.AppendLine("def _to_tree(v):");
                sb.AppendLine("    import numpy as np");
                sb.AppendLine("    if v is None: return []");
                sb.AppendLine("    a = np.array(v)");
                sb.AppendLine("    if a.size == 0: return []");
                sb.AppendLine("    if a.ndim == 0: return [[str(a.item())]]");
                sb.AppendLine("    if a.ndim == 1: return [[str(x)] for x in a.tolist()]");
                sb.AppendLine("    return [[str(c) for c in row] for row in a.tolist()]");
                sb.AppendLine("print('OUTPUT_0:' + json.dumps(_to_tree(preds), ensure_ascii=False, separators=(',',':')))");
                sb.AppendLine("print('OUTPUT_1:' + json.dumps(_to_tree(proba) if proba is not None else [], ensure_ascii=False, separators=(',',':')))");
                sb.AppendLine("print('OUTPUT_2:' + str(task))");
                sb.AppendLine("print('OUTPUT_3:' + str(readme).replace('\\n','\\\\n'))");

                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "预测运行中…若较久可增大 SIMPLEML_TIMEOUT_MS");
                string output = PythonScriptExecutor.ExecuteCode(sb.ToString());
                DA.SetDataTree(0, TreeConverter.ConvertJsonToTree(PythonBridge.ExtractValue(output, "OUTPUT_0:")));
                DA.SetDataTree(1, TreeConverter.ConvertJsonToTree(PythonBridge.ExtractValue(output, "OUTPUT_1:")));
                DA.SetData(2, PythonBridge.ExtractValue(output, "OUTPUT_2:"));
                DA.SetData(3, PythonBridge.ExtractValue(output, "OUTPUT_3:").Replace("\\n", "\n"));
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                RhinoApp.WriteLine("SimpleML Predict: " + ex);
            }
        }

        private static string TreeToPythonLiteral(GH_Structure<IGH_Goo> tree)
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

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(PredictAutoComponent));
        public override Guid ComponentGuid => new Guid("E5F6A7B8-C9D0-1234-EF01-515253545556");
    }
}
