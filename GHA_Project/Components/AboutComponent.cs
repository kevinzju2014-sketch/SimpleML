using System;
using Grasshopper.Kernel;
using SimpleML.Core;
using SimpleML.Localization;

namespace SimpleML.Components.About
{
    /// <summary>
    /// About component — author and plugin information (EN/ZH).
    /// </summary>
    public class AboutComponent : GH_Component
    {
        public AboutComponent()
          : base(L.Name(nameof(AboutComponent)),
                 L.Nick(nameof(AboutComponent)),
                 L.Desc(nameof(AboutComponent)),
                 "SimpleML", "09 Help")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Plugin Name", "PN", "Plugin name", GH_ParamAccess.item);
            pManager.AddTextParameter("Version", "V", "Version", GH_ParamAccess.item);
            pManager.AddTextParameter("Author", "A", "Author", GH_ParamAccess.item);
            pManager.AddTextParameter("Description", "D", "Plugin description", GH_ParamAccess.item);
            pManager.AddTextParameter("Contact", "C", "Contact", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData(0, "SimpleML");
            DA.SetData(1, "1.2.0");
            if (UiLanguage.IsChinese)
            {
                DA.SetData(2, "参数化凯通学/B站/犀流堂/小红书同名账号");
                DA.SetData(3, L.T("plugin.desc") + " 提供完整的机器学习工作流程。");
                DA.SetData(4,
                    "邮箱：zhao_guijia@outlook.com\n" +
                    "B站主页：https://space.bilibili.com/387841705?spm_id_from=333.1007.0.0\n" +
                    "小红书主页：https://xhslink.com/m/8TiSSoH3TuS\n" +
                    "商务合作请联系邮箱，承接教学/程序定制/3d打印开发等业务");
            }
            else
            {
                DA.SetData(2, "Parametric Kai (Bilibili / Xiaohongshu)");
                DA.SetData(3, L.T("plugin.desc") + " Full ML workflow in Grasshopper.");
                DA.SetData(4,
                    "Email: zhao_guijia@outlook.com\n" +
                    "Bilibili: https://space.bilibili.com/387841705?spm_id_from=333.1007.0.0\n" +
                    "Xiaohongshu: https://xhslink.com/m/8TiSSoH3TuS\n" +
                    "For teaching / custom tools / 3D-print development, contact by email.");
            }
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(AboutComponent));
        public override Guid ComponentGuid => new Guid("F1A2B3C4-D5E6-7890-ABCD-EF1234567890");
    }
}
