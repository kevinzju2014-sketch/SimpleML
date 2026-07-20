using System;
using System.Text;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SimpleML.Core;
using Rhino;

using SimpleML.Localization;
namespace SimpleML.Components.ModelTraining
{
    public class SmartTrainComponent : GH_Component
    {
        public SmartTrainComponent()
          : base(L.Name("SmartTrainComponent"), L.Nick("SmartTrainComponent"), L.Desc("SmartTrainComponent"),
              "SimpleML", "04 Model")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Dataset", "DS", "训练数据集", GH_ParamAccess.item);
            pManager.AddTextParameter("Task", "T", "auto / classification / regression / clustering", GH_ParamAccess.item, "auto");
            pManager.AddTextParameter("Algorithm", "A", "算法名或 JSON；auto 则自动选择", GH_ParamAccess.item, "auto");
            pManager.AddIntegerParameter("N Clusters", "K", "聚类默认簇数", GH_ParamAccess.item, 3);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "训练好的模型", GH_ParamAccess.item);
            pManager.AddGenericParameter("Model Info", "MI", "模型信息树", GH_ParamAccess.tree);
            pManager.AddTextParameter("Model Card", "Card", "一眼可读的模型卡片", GH_ParamAccess.item);
            pManager.AddTextParameter("Next Steps", "Next", "下一步该接哪个组件", GH_ParamAccess.item);
            pManager.AddTextParameter("Explanation", "E", "为何选该算法", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "RM", "使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object datasetObj = null;
            string task = "auto";
            string algorithm = "auto";
            int nClusters = 3;

            if (!DA.GetData(0, ref datasetObj))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "必须提供 Dataset（可用「加载示例数据集」）");
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
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "未找到包路径。请运行「环境体检」或「安装指南」。");
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
                sb.AppendLine("model, model_info, explanation, readme, next_steps, card = smart_train(dataset, task=task, algorithm=algorithm, n_clusters=n_clusters)");
                sb.AppendLine("model_str = base64.b64encode(pickle.dumps(model)).decode('utf-8')");
                sb.AppendLine("print('OUTPUT_0:' + model_str)");
                sb.AppendLine("print('OUTPUT_1:' + (model_info if isinstance(model_info, str) else json.dumps(model_info, ensure_ascii=False, separators=(',',':'))))");
                sb.AppendLine("print('OUTPUT_2:' + card.replace('\\n', '\\\\n'))");
                sb.AppendLine("print('OUTPUT_3:' + next_steps.replace('\\n', '\\\\n'))");
                sb.AppendLine("print('OUTPUT_4:' + explanation.replace('\\n', '\\\\n'))");
                sb.AppendLine("print('OUTPUT_5:' + readme.replace('\\n', '\\\\n'))");

                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "训练中…大数据可调大 SIMPLEML_TIMEOUT_MS；卡住可用「重置Python会话」");
                string output = PythonScriptExecutor.ExecuteCode(sb.ToString());
                string modelStr = PythonBridge.ExtractValue(output, "OUTPUT_0:");
                string modelInfo = PythonBridge.ExtractValue(output, "OUTPUT_1:");
                string card = PythonBridge.ExtractValue(output, "OUTPUT_2:").Replace("\\n", "\n");
                string next = PythonBridge.ExtractValue(output, "OUTPUT_3:").Replace("\\n", "\n");
                string explanation = PythonBridge.ExtractValue(output, "OUTPUT_4:").Replace("\\n", "\n");
                string readme = PythonBridge.ExtractValue(output, "OUTPUT_5:").Replace("\\n", "\n");

                DA.SetData(0, modelStr);
                DA.SetDataTree(1, TreeConverter.ConvertJsonToTree(modelInfo));
                DA.SetData(2, card);
                DA.SetData(3, next);
                DA.SetData(4, explanation);
                DA.SetData(5, readme);
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, next.Split('\n')[0]);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "智能训练失败: " + ex.Message + "（可试「重置Python会话」）");
                RhinoApp.WriteLine("SimpleML SmartTrain: " + ex);
            }
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(SmartTrainComponent));
        public override Guid ComponentGuid => new Guid("B1C2D3E4-F5A6-7890-BCDE-212223242526");
    }
}
