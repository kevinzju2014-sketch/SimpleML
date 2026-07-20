using System;
using System.Text;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SimpleML.Core;
using Rhino;

using SimpleML.Localization;
namespace SimpleML.Components.DataInput
{
    /// <summary>
    /// 加载示例数据集，直接输出可训练的 Dataset。
    /// </summary>
    public class LoadDatasetComponent : GH_Component
    {
        public LoadDatasetComponent()
          : base(L.Name("LoadDatasetComponent"), L.Nick("LoadDatasetComponent"), L.Desc("LoadDatasetComponent"),
              "SimpleML", "01 Input")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Dataset Name", "DN",
                "iris / wine / breast_cancer / digits / diabetes / california_housing / make_classification / make_regression / make_blobs",
                GH_ParamAccess.item, "iris");
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Dataset", "DS", "可直接连接智能训练 / 分割数据集", GH_ParamAccess.item);
            pManager.AddGenericParameter("Features", "X", "特征矩阵（Tree）", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Target", "y", "标签/目标（Tree）", GH_ParamAccess.tree);
            pManager.AddTextParameter("Feature Names", "FN", "特征列名", GH_ParamAccess.list);
            pManager.AddTextParameter("Info", "I", "数据集说明", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "RM", "使用说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string datasetName = "iris";
            DA.GetData(0, ref datasetName);
            if (string.IsNullOrWhiteSpace(datasetName)) datasetName = "iris";

            try
            {
                string mymlPath = PathResolver.GetMyMLPath();
                if (string.IsNullOrEmpty(mymlPath))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                        "未找到 SimpleML 包路径。请打开「安装指南」或运行 install.sh / install.bat。");
                    return;
                }

                var sb = new StringBuilder();
                sb.Append(PythonBridge.BuildBootstrap(mymlPath));
                sb.AppendLine("import json, pickle, base64");
                sb.AppendLine("from components.dataset_loader import load_sklearn_dataset");
                sb.AppendLine("from components.dataset_components import create_dataset");
                sb.AppendLine("name = " + PythonBridge.ToPythonStringLiteral(datasetName));
                sb.AppendLine("X, y, feature_names, target_names, info = load_sklearn_dataset(name, return_X_y=True)");
                sb.AppendLine("ds = create_dataset(X, y, X_names=list(feature_names) if feature_names is not None else None)");
                sb.AppendLine("ds_b64 = base64.b64encode(pickle.dumps(ds)).decode('utf-8')");
                sb.AppendLine("X_tree = X.tolist() if hasattr(X, 'tolist') else list(X)");
                sb.AppendLine("y_tree = [[v] for v in (y.tolist() if hasattr(y, 'tolist') else list(y))]");
                sb.AppendLine("print('OUTPUT_0:' + ds_b64)");
                sb.AppendLine("print('OUTPUT_1:' + json.dumps(X_tree, ensure_ascii=False, separators=(',',':')))");
                sb.AppendLine("print('OUTPUT_2:' + json.dumps(y_tree, ensure_ascii=False, separators=(',',':')))");
                sb.AppendLine("print('OUTPUT_3:' + json.dumps(list(feature_names) if feature_names is not None else [], ensure_ascii=False))");
                sb.AppendLine("print('OUTPUT_4:' + str(info).replace('\\n','\\\\n'))");
                sb.AppendLine("print('OUTPUT_5:' + 'Dataset 已就绪：可直接连接 分割数据集 / 智能训练。')");

                string output = PythonScriptExecutor.ExecuteCode(sb.ToString());
                string ds = PythonBridge.ExtractValue(output, "OUTPUT_0:");
                string xJson = PythonBridge.ExtractValue(output, "OUTPUT_1:");
                string yJson = PythonBridge.ExtractValue(output, "OUTPUT_2:");
                string fnJson = PythonBridge.ExtractValue(output, "OUTPUT_3:");
                string info = PythonBridge.ExtractValue(output, "OUTPUT_4:").Replace("\\n", "\n");
                string readme = PythonBridge.ExtractValue(output, "OUTPUT_5:");

                DA.SetData(0, ds);
                DA.SetDataTree(1, TreeConverter.ConvertJsonToTree(xJson));
                DA.SetDataTree(2, TreeConverter.ConvertJsonToTree(yJson));

                var names = ParseJsonStringList(fnJson);
                DA.SetDataList(3, names);
                DA.SetData(4, info);
                DA.SetData(5, readme);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                RhinoApp.WriteLine("SimpleML LoadDataset: " + ex);
            }
        }

        private static System.Collections.Generic.List<string> ParseJsonStringList(string json)
        {
            var names = new System.Collections.Generic.List<string>();
            if (string.IsNullOrWhiteSpace(json)) return names;
            string t = json.Trim();
            if (t.StartsWith("[")) t = t.Substring(1);
            if (t.EndsWith("]")) t = t.Substring(0, t.Length - 1);
            foreach (string part in t.Split(','))
            {
                string s = part.Trim().Trim('"').Trim('\'');
                if (!string.IsNullOrEmpty(s)) names.Add(s);
            }
            return names;
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(LoadDatasetComponent));
        public override Guid ComponentGuid => new Guid("E3F4A5B6-C7D8-9012-EF01-234567890124");
    }
}

