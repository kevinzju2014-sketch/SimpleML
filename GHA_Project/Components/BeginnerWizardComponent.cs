using System;
using System.Text;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.About
{
    /// <summary>
    /// 新手向导：输出可复制的接线配方，并可一键体检。
    /// </summary>
    public class BeginnerWizardComponent : GH_Component
    {
        public BeginnerWizardComponent()
          : base("新手向导 Beginner Wizard", "新手向导",
              "上架向新手向导：输出分类/回归/聚类完整接线配方，并可选运行体检",
              "SimpleML", "09 Help")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Task", "T", "classification / regression / clustering", GH_ParamAccess.item, "classification");
            pManager.AddBooleanParameter("Run Health Check", "HC", "同时运行环境体检", GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Recipe", "Recipe", "接线配方（接 Panel）", GH_ParamAccess.item);
            pManager.AddTextParameter("Health Status", "S", "体检状态", GH_ParamAccess.item);
            pManager.AddTextParameter("Next Steps", "Next", "下一步", GH_ParamAccess.item);
            pManager.AddTextParameter("Report", "Rep", "体检报告", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string task = "classification";
            bool runHc = true;
            DA.GetData(0, ref task);
            DA.GetData(1, ref runHc);

            try
            {
                string mymlPath = PathResolver.GetMyMLPath();
                string recipe;
                if (string.IsNullOrEmpty(mymlPath))
                {
                    recipe = "尚未找到 SimpleML 包路径。\n请先运行 install.sh / install.bat，或打开「安装指南」。\n\n"
                             + RhinoCompat.DescribeCompatibility();
                    DA.SetData(0, recipe);
                    DA.SetData(1, "FAIL");
                    DA.SetData(2, "打开「安装指南」→ 按平台复制 .gha 与 myML → 重启 Rhino → 再运行本向导");
                    DA.SetData(3, "");
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "未找到包路径");
                    return;
                }

                var sb = new StringBuilder();
                sb.Append(PythonBridge.BuildBootstrap(mymlPath));
                sb.AppendLine("from components.ux_helpers import wizard_recipe");
                sb.AppendLine("from components.health_check import run_health_check");
                sb.AppendLine("task = " + PythonBridge.ToPythonStringLiteral(task));
                sb.AppendLine("recipe = wizard_recipe(task)");
                sb.AppendLine("print('OUTPUT_0:' + recipe.replace('\\n','\\\\n'))");
                if (runHc)
                {
                    sb.AppendLine("r = run_health_check(r'''" + mymlPath.Replace("\\", "\\\\") + "''')");
                    sb.AppendLine("print('OUTPUT_1:' + r.get('status',''))");
                    sb.AppendLine("print('OUTPUT_2:' + r.get('next_steps','').replace('\\n','\\\\n'))");
                    sb.AppendLine("print('OUTPUT_3:' + r.get('report','').replace('\\n','\\\\n'))");
                }
                else
                {
                    sb.AppendLine("print('OUTPUT_1:SKIP')");
                    sb.AppendLine("print('OUTPUT_2:已跳过体检。建议首次使用时开启。')");
                    sb.AppendLine("print('OUTPUT_3:')");
                }

                string output = PythonScriptExecutor.ExecuteCode(sb.ToString());
                string recipeOut = PythonBridge.ExtractValue(output, "OUTPUT_0:").Replace("\\n", "\n");
                recipeOut = RhinoCompat.DescribeCompatibility() + "\n\n" + recipeOut;
                DA.SetData(0, recipeOut);
                DA.SetData(1, PythonBridge.ExtractValue(output, "OUTPUT_1:"));
                DA.SetData(2, PythonBridge.ExtractValue(output, "OUTPUT_2:").Replace("\\n", "\n"));
                DA.SetData(3, PythonBridge.ExtractValue(output, "OUTPUT_3:").Replace("\\n", "\n"));
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                RhinoApp.WriteLine("SimpleML Wizard: " + ex);
            }
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(BeginnerWizardComponent));
        public override Guid ComponentGuid => new Guid("A7B8C9D0-E1F2-3456-0123-717273747576");
    }
}
