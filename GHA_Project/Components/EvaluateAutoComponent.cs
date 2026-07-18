using System;
using System.Text;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.ModelEvaluation
{
    /// <summary>
    /// 统一评估：自动识别任务类型，输出 Metrics / Report / Verdict。
    /// </summary>
    public class EvaluateAutoComponent : GH_Component
    {
        public EvaluateAutoComponent()
          : base("评估 Evaluate", "评估",
              "统一评估：自动识别分类/回归/聚类，输出结论句 Verdict",
              "SimpleML", "07 Evaluation")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "训练好的模型", GH_ParamAccess.item);
            pManager.AddGenericParameter("Test Dataset", "DS", "测试数据集（含标签更佳）", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Verdict", "V", "一句话结论", GH_ParamAccess.item);
            pManager.AddTextParameter("Report", "Rep", "详细报告", GH_ParamAccess.item);
            pManager.AddTextParameter("Metrics JSON", "MJ", "指标 JSON", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "RM", "说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object modelObj = null;
            object datasetObj = null;
            if (!DA.GetData(0, ref modelObj)) return;
            if (!DA.GetData(1, ref datasetObj))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "必须提供测试 Dataset");
                return;
            }

            try
            {
                string mymlPath = PathResolver.GetMyMLPath();
                if (string.IsNullOrEmpty(mymlPath))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "未找到包路径。请打开「安装指南」。");
                    return;
                }

                var sb = new StringBuilder();
                sb.Append(PythonBridge.BuildBootstrap(mymlPath));
                sb.AppendLine("import pickle, base64");
                sb.AppendLine("from components.ux_helpers import evaluate_auto");
                sb.AppendLine("model = pickle.loads(base64.b64decode(r'''" + (modelObj?.ToString() ?? "") + "'''))");
                sb.AppendLine("dataset = pickle.loads(base64.b64decode(r'''" + datasetObj + "'''))");
                sb.AppendLine("metrics_json, report, verdict, readme, _extra = evaluate_auto(model, dataset=dataset)");
                sb.AppendLine("print('OUTPUT_0:' + verdict.replace('\\n','\\\\n'))");
                sb.AppendLine("print('OUTPUT_1:' + report.replace('\\n','\\\\n'))");
                sb.AppendLine("print('OUTPUT_2:' + metrics_json.replace('\\n','\\\\n'))");
                sb.AppendLine("print('OUTPUT_3:' + readme.replace('\\n','\\\\n'))");

                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "评估运行中…");
                string output = PythonScriptExecutor.ExecuteCode(sb.ToString());
                string verdict = PythonBridge.ExtractValue(output, "OUTPUT_0:").Replace("\\n", "\n");
                DA.SetData(0, verdict);
                DA.SetData(1, PythonBridge.ExtractValue(output, "OUTPUT_1:").Replace("\\n", "\n"));
                DA.SetData(2, PythonBridge.ExtractValue(output, "OUTPUT_2:").Replace("\\n", "\n"));
                DA.SetData(3, PythonBridge.ExtractValue(output, "OUTPUT_3:").Replace("\\n", "\n"));
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, verdict);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                RhinoApp.WriteLine("SimpleML Evaluate: " + ex);
            }
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(EvaluateAutoComponent));
        public override Guid ComponentGuid => new Guid("F6A7B8C9-D0E1-2345-F012-616263646566");
    }
}
