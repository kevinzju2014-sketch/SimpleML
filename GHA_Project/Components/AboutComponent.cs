using System;
using Grasshopper.Kernel;
using SimpleML.Core;

namespace SimpleML.Components.About
{
    /// <summary>
    /// About Component
    /// 关于组件 - 显示作者和插件信息
    /// </summary>
    public class AboutComponent : GH_Component
    {
        public AboutComponent()
          : base("关于 About", "关于",
              "显示SimpleML插件的作者信息和版本信息",
              "SimpleML", "09 Help")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // 无输入参数
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Plugin Name", "PN", "插件名称", GH_ParamAccess.item);
            pManager.AddTextParameter("Version", "V", "版本号", GH_ParamAccess.item);
            pManager.AddTextParameter("Author", "A", "作者信息", GH_ParamAccess.item);
            pManager.AddTextParameter("Description", "D", "插件描述", GH_ParamAccess.item);
            pManager.AddTextParameter("Contact", "C", "联系方式", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData(0, "SimpleML");
            DA.SetData(1, "1.2.0");
            DA.SetData(2, "参数化凯通学/B站/犀流堂/小红书同名账号");
            DA.SetData(3, "基于scikit-learn的Grasshopper机器学习插件（兼容 Rhino 7 及以上、Windows 与 macOS），提供完整的机器学习工作流程。");
            DA.SetData(4, "邮箱：zhao_guijia@outlook.com\nB站主页：https://space.bilibili.com/387841705?spm_id_from=333.1007.0.0\n小红书主页：https://xhslink.com/m/8TiSSoH3TuS\n商务合作请联系邮箱，承接教学/程序定制/3d打印开发等业务");
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(AboutComponent));
        public override Guid ComponentGuid => new Guid("F1A2B3C4-D5E6-7890-ABCD-EF1234567890");
    }
}
