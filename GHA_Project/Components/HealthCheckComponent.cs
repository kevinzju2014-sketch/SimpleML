using System;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.About
{
    /// <summary>
    /// 环境体检：检查 Python、依赖、路径是否可用。
    /// </summary>
    public class HealthCheckComponent : GH_Component
    {
        public HealthCheckComponent()
          : base("环境体检 Health Check", "体检",
              "检查 SimpleML 运行环境（Python、依赖库、项目路径），跨平台可用",
              "SimpleML", "09 Help")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "Run", "设为 true 执行体检", GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Status", "S", "PASS 或 FAIL", GH_ParamAccess.item);
            pManager.AddTextParameter("Summary", "Sum", "简要摘要", GH_ParamAccess.item);
            pManager.AddTextParameter("Report", "R", "完整体检报告", GH_ParamAccess.item);
            pManager.AddTextParameter("Python", "Py", "检测到的 Python 路径", GH_ParamAccess.item);
            pManager.AddTextParameter("Package Root", "Path", "myML / 代码根目录", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool run = true;
            DA.GetData(0, ref run);
            if (!run)
            {
                DA.SetData(0, "SKIP");
                DA.SetData(1, "未运行（Run=false）");
                return;
            }

            try
            {
                string mymlPath = PathResolver.GetMyMLPath();
                string pythonPath = "";
                try { pythonPath = PythonScriptExecutor.GetPythonPath(); }
                catch (Exception ex) { pythonPath = "未找到: " + ex.Message; }

                if (string.IsNullOrEmpty(mymlPath))
                {
                    DA.SetData(0, "FAIL");
                    DA.SetData(1, "未找到 SimpleML Python 包根目录");
                    DA.SetData(2,
                        "未找到包含 components/ 与 core/ 的目录。\n" +
                        "请设置环境变量 SIMPLEML_PATH，或将代码放在 GHA 同级 myML 文件夹中。\n" +
                        "Python: " + pythonPath);
                    DA.SetData(3, pythonPath);
                    DA.SetData(4, "");
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "未找到 SimpleML 包路径");
                    return;
                }

                string code = PythonBridge.BuildBootstrap(mymlPath) + @"
import json
from components.health_check import run_health_check
result = run_health_check(r'''" + mymlPath.Replace("\\", "\\\\") + @"''')
print('OUTPUT_0:' + result.get('status',''))
print('OUTPUT_1:' + result.get('summary',''))
print('OUTPUT_2:' + result.get('report','').replace('\n','\\n'))
print('OUTPUT_3:' + result.get('python',''))
print('OUTPUT_4:' + (result.get('project_dir') or ''))
";
                string output = PythonScriptExecutor.ExecuteCode(code, timeout: PythonBridge.DefaultTimeoutMs);
                string status = PythonBridge.ExtractValue(output, "OUTPUT_0:");
                string summary = PythonBridge.ExtractValue(output, "OUTPUT_1:");
                string report = PythonBridge.ExtractValue(output, "OUTPUT_2:").Replace("\\n", "\n");
                string py = PythonBridge.ExtractValue(output, "OUTPUT_3:");
                string root = PythonBridge.ExtractValue(output, "OUTPUT_4:");

                string compat = RhinoCompat.DescribeCompatibility();
                if (!string.IsNullOrEmpty(report))
                    report = compat + "\n" + report;
                summary = compat + " | " + summary;

                DA.SetData(0, status);
                DA.SetData(1, summary);
                DA.SetData(2, report);
                DA.SetData(3, string.IsNullOrEmpty(py) ? pythonPath : py);
                DA.SetData(4, string.IsNullOrEmpty(root) ? mymlPath : root);

                if (status != "PASS")
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, summary);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                RhinoApp.WriteLine("SimpleML HealthCheck: " + ex);
                DA.SetData(0, "FAIL");
                DA.SetData(1, ex.Message);
                DA.SetData(2, ex.ToString());
            }
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(HealthCheckComponent));
        public override Guid ComponentGuid => new Guid("A0B1C2D3-E4F5-6789-ABCD-101112131415");
    }
}
