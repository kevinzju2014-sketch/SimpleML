using System;
using System.Text;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SimpleML.Core;
using Rhino;

using SimpleML.Localization;
namespace SimpleML.Components.ModelEvaluation
{
    public class FeatureImportanceComponent : GH_Component
    {
        public FeatureImportanceComponent()
          : base(L.Name("FeatureImportanceComponent"), L.Nick("FeatureImportanceComponent"), L.Desc("FeatureImportanceComponent"),
              "SimpleML", "07 Evaluation")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "训练好的模型", GH_ParamAccess.item);
            pManager.AddTextParameter("Feature Names", "FN", "特征名列表（可选）", GH_ParamAccess.list);
        }

        public override void CreateAttributes()
        {
            base.CreateAttributes();
            if (Params.Input.Count >= 2)
                Params.Input[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Importance", "I", "按重要性排序的 [名称, 值] 树", GH_ParamAccess.tree);
            pManager.AddTextParameter("Explanation", "E", "通俗解读", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object modelObj = null;
            var names = new System.Collections.Generic.List<string>();
            if (!DA.GetData(0, ref modelObj)) return;
            DA.GetDataList(1, names);

            try
            {
                string mymlPath = PathResolver.GetMyMLPath();
                if (string.IsNullOrEmpty(mymlPath))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "未找到 myML 路径");
                    return;
                }

                string modelStr = modelObj?.ToString() ?? "";
                string namesPy = "None";
                if (names != null && names.Count > 0)
                {
                    var parts = new System.Collections.Generic.List<string>();
                    foreach (var n in names)
                        parts.Add("'" + (n ?? "").Replace("'", "\\'") + "'");
                    namesPy = "[" + string.Join(",", parts) + "]";
                }

                var sb = new StringBuilder();
                sb.Append(PythonBridge.BuildBootstrap(mymlPath));
                sb.AppendLine("import pickle, base64");
                sb.AppendLine("from components.explain_components import calculate_feature_importance");
                sb.AppendLine("model = pickle.loads(base64.b64decode(r'''" + modelStr + "'''))");
                sb.AppendLine("feature_names = " + namesPy);
                sb.AppendLine("tree_json, explanation, readme = calculate_feature_importance(model, feature_names)");
                sb.AppendLine("print('OUTPUT_0:' + tree_json)");
                sb.AppendLine("print('OUTPUT_1:' + explanation.replace('\\n','\\\\n'))");
                sb.AppendLine("print('OUTPUT_2:' + readme.replace('\\n','\\\\n'))");

                string output = PythonScriptExecutor.ExecuteCode(sb.ToString());
                string treeJson = PythonBridge.ExtractValue(output, "OUTPUT_0:");
                string explanation = PythonBridge.ExtractValue(output, "OUTPUT_1:").Replace("\\n", "\n");
                string readme = PythonBridge.ExtractValue(output, "OUTPUT_2:").Replace("\\n", "\n");

                DA.SetDataTree(0, TreeConverter.ConvertJsonToTree(treeJson));
                DA.SetData(1, explanation);
                DA.SetData(2, readme);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                RhinoApp.WriteLine("SimpleML FeatureImportance: " + ex);
            }
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(FeatureImportanceComponent));
        public override Guid ComponentGuid => new Guid("C2D3E4F5-A6B7-8901-CDEF-313233343536");
    }
}
