using System;
using System.Text;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.ModelEvaluation
{
    public class SilhouetteScoreComponent : GH_Component
    {
        public SilhouetteScoreComponent()
          : base("轮廓系数 Silhouette Score", "轮廓系数",
              "计算聚类轮廓系数，评价簇的紧凑度与分离度",
              "SimpleML", "07 Evaluation")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Dataset", "DS", "特征数据集（或配合 Labels）", GH_ParamAccess.item);
            pManager.AddGenericParameter("Model", "M", "聚类模型（可选，用于自动预测标签）", GH_ParamAccess.item);
            pManager.AddGenericParameter("Labels", "L", "聚类标签（可选；若提供则优先使用）", GH_ParamAccess.tree);
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
            pManager.AddNumberParameter("Score", "S", "轮廓系数", GH_ParamAccess.item);
            pManager.AddTextParameter("Explanation", "E", "通俗解读", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object datasetObj = null;
            object modelObj = null;
            Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> labelsTree = null;

            if (!DA.GetData(0, ref datasetObj))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "必须提供 Dataset");
                return;
            }
            DA.GetData(1, ref modelObj);
            bool hasLabels = DA.GetDataTree(2, out labelsTree) && labelsTree != null && labelsTree.DataCount > 0;

            try
            {
                string mymlPath = PathResolver.GetMyMLPath();
                if (string.IsNullOrEmpty(mymlPath))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "未找到 myML 路径");
                    return;
                }

                string datasetStr = datasetObj?.ToString() ?? "";
                string modelStr = modelObj?.ToString() ?? "";

                var labelList = new System.Collections.Generic.List<string>();
                if (hasLabels)
                {
                    foreach (var goo in labelsTree.AllData(true))
                    {
                        if (goo == null) continue;
                        double num;
                        if (goo.CastTo(out num))
                            labelList.Add(num.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        else
                            labelList.Add(goo.ToString());
                    }
                }

                string labelsPy = "None";
                if (labelList.Count > 0)
                    labelsPy = "[" + string.Join(",", labelList) + "]";

                var sb = new StringBuilder();
                sb.Append(PythonBridge.BuildBootstrap(mymlPath));
                sb.AppendLine("import pickle, base64");
                sb.AppendLine("from components.explain_components import calculate_silhouette");
                sb.AppendLine("dataset = pickle.loads(base64.b64decode(r'''" + datasetStr + "'''))");
                if (!string.IsNullOrEmpty(modelStr))
                    sb.AppendLine("model = pickle.loads(base64.b64decode(r'''" + modelStr + "'''))");
                else
                    sb.AppendLine("model = None");
                sb.AppendLine("labels = " + labelsPy);
                sb.AppendLine("score, explanation, readme = calculate_silhouette(dataset=dataset, model=model, labels=labels)");
                sb.AppendLine("print('OUTPUT_0:' + str(score))");
                sb.AppendLine("print('OUTPUT_1:' + explanation.replace('\\n','\\\\n'))");
                sb.AppendLine("print('OUTPUT_2:' + readme.replace('\\n','\\\\n'))");

                string output = PythonScriptExecutor.ExecuteCode(sb.ToString());
                string scoreStr = PythonBridge.ExtractValue(output, "OUTPUT_0:");
                string explanation = PythonBridge.ExtractValue(output, "OUTPUT_1:").Replace("\\n", "\n");
                string readme = PythonBridge.ExtractValue(output, "OUTPUT_2:").Replace("\\n", "\n");

                double score;
                double.TryParse(scoreStr, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out score);

                DA.SetData(0, score);
                DA.SetData(1, explanation);
                DA.SetData(2, readme);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                RhinoApp.WriteLine("SimpleML Silhouette: " + ex);
            }
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(SilhouetteScoreComponent));
        public override Guid ComponentGuid => new Guid("D3E4F5A6-B7C8-9012-DEF0-414243444546");
    }
}
