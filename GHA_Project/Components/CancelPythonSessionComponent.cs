using System;
using Grasshopper.Kernel;
using SimpleML.Core;

using SimpleML.Localization;
namespace SimpleML.Components.About
{
    /// <summary>
    /// 重置常驻 Python 会话（长任务卡住时使用）。
    /// </summary>
    public class CancelPythonSessionComponent : GH_Component
    {
        public CancelPythonSessionComponent()
          : base(L.Name("CancelPythonSessionComponent"), L.Nick("CancelPythonSessionComponent"), L.Desc("CancelPythonSessionComponent"),
              "SimpleML", "09 Help")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Reset", "R", "设为 true 立即重置", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Status", "S", "结果", GH_ParamAccess.item);
            pManager.AddTextParameter("Hint", "H", "提示", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool reset = false;
            DA.GetData(0, ref reset);
            if (!reset)
            {
                DA.SetData(0, "IDLE");
                DA.SetData(1, "将 Reset 设为 true 以终止卡住的 Python 会话。也可调大 SIMPLEML_TIMEOUT_MS。");
                return;
            }

            try
            {
                // 通过反射调用内部 Reset，避免把 API 设为 public 破坏面过大；若失败则切换环境变量提示
                var t = typeof(PythonScriptExecutor);
                var m = t.GetMethod("ResetSessionPublic", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (m != null)
                {
                    m.Invoke(null, null);
                }
                else
                {
                    PythonScriptExecutor.ResetSessionPublic();
                }
                DA.SetData(0, "RESET_OK");
                DA.SetData(1, "Python 会话已重置。请重新运行训练/预测组件。");
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Python 会话已重置");
            }
            catch (Exception ex)
            {
                DA.SetData(0, "RESET_FAIL");
                DA.SetData(1, ex.Message);
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
            }
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(CancelPythonSessionComponent));
        public override Guid ComponentGuid => new Guid("D0E1F2A3-B4C5-6789-3456-A1A2A3A4A5A6");
    }
}
