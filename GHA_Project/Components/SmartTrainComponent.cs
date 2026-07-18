using System;
using System.Text;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.ModelTraining
{
    /// <summary>
    /// 一键智能训练：Dataset → Model，自动推断任务与默认算法。
    /// </summary>
    public class SmartTrainComponent : GH_Component
    {
        public SmartTrainComponent()
          : base("智能训练 Smart Train", "智能训练",
              "一键训练：输入 Dataset，自动识别分类/回归/聚类并选用稳妥默认算法。适合新手。",
              "SimpleML", "04 Model")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Dataset", "DS", "训练数据集", GH_ParamAccess.item);
            pManager.AddTextParameter("Task", "T", "任务类型: auto / classification / regression / clustering", GH_ParamAccess.item, "auto");
            pManager.AddTextParameter("Algorithm", "A", "算法名或 JSON；留空/auto 则自动选择", GH_ParamAccess.item, "auto");
            pManager.AddIntegerParameter("N Clusters", "K", "聚类默认簇数（仅聚类任务）", GH_ParamAccess.item, 3);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "训练好的模型（含预处理打包）", GH_ParamAccess.item);
            pManager.AddGenericParameter("Model Info", "MI", "模型信息树", GH_ParamAccess.tree);
            pManager.AddTextParameter("Explanation", "E", "通俗说明：为何选该算法", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object datasetObj = null;
            string task = "auto";
            string algorithm = "auto";
            int nClusters = 3;

            if (!DA.GetData(0, ref datasetObj))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "必须提供 Dataset");
                return;
            }
            DA.GetData(1, ref task);
            DA.GetData(2, ref algorithm);
            DA.GetData(3, ref nClusters);

            try
            {
                string mymlPath = PathResolver.GetMyMLPath();
                if (string.IsNullOrEmpty(mymlPath))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "未找到 myML。请运行「环境体检」或设置 SIMPLEML_PATH。");
                    return;
                }

                string datasetStr = datasetObj?.ToString() ?? "";
                var sb = new StringBuilder();
                sb.Append(PythonBridge.BuildBootstrap(mymlPath));
                sb.AppendLine("import pickle, base64, json");
                sb.AppendLine("from components.smart_train import smart_train");
                sb.AppendLine("dataset = pickle.loads(base64.b64decode(r'''" + datasetStr + "'''))");
                sb.AppendLine("task = " + PythonBridge.ToPythonStringLiteral(task));
                sb.AppendLine("algorithm = " + PythonBridge.ToPythonStringLiteral(algorithm));
                sb.AppendLine($"n_clusters = {nClusters}");
                sb.AppendLine("model, model_info, explanation, readme = smart_train(dataset, task=task, algorithm=algorithm, n_clusters=n_clusters)");
                sb.AppendLine("model_str = base64.b64encode(pickle.dumps(model)).decode('utf-8')");
                sb.AppendLine("print('OUTPUT_0:' + model_str)");
                sb.AppendLine("print('OUTPUT_1:' + (model_info if isinstance(model_info, str) else json.dumps(model_info, ensure_ascii=False, separators=(',',':'))))");
                sb.AppendLine("print('OUTPUT_2:' + explanation.replace('\\n', '\\\\n'))");
                sb.AppendLine("print('OUTPUT_3:' + readme.replace('\\n', '\\\\n'))");

                string output = PythonScriptExecutor.ExecuteCode(sb.ToString());
                string modelStr = PythonBridge.ExtractValue(output, "OUTPUT_0:");
                string modelInfo = PythonBridge.ExtractValue(output, "OUTPUT_1:");
                string explanation = PythonBridge.ExtractValue(output, "OUTPUT_2:").Replace("\\n", "\n");
                string readme = PythonBridge.ExtractValue(output, "OUTPUT_3:").Replace("\\n", "\n");

                GH_Structure<GH_String> tree = TreeConverter.ConvertJsonToTree(modelInfo);

                DA.SetData(0, modelStr);
                DA.SetDataTree(1, tree);
                DA.SetData(2, explanation);
                DA.SetData(3, readme);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "智能训练失败: " + ex.Message);
                RhinoApp.WriteLine("SimpleML SmartTrain: " + ex);
            }
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(SmartTrainComponent));
        public override Guid ComponentGuid => new Guid("B1C2D3E4-F5A6-7890-BCDE-212223242526");
    }
}
