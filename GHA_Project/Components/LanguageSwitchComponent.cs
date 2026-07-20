using System;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using SimpleML.Core;
using SimpleML.Localization;

namespace SimpleML.Components.About
{
    /// <summary>
    /// Language toggle. Right-click OR Boolean Chinese=true/false.
    /// Canvas labels refresh immediately (no Rhino restart).
    /// </summary>
    public class LanguageSwitchComponent : GH_Component
    {
        private string _lastApplied;
        private bool _menuLock;
        private bool _menuChinese;

        public LanguageSwitchComponent()
          : base(L.Name(nameof(LanguageSwitchComponent)),
                 L.Nick(nameof(LanguageSwitchComponent)),
                 L.Desc(nameof(LanguageSwitchComponent)),
                 "SimpleML", "09 Help")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter(
                "Chinese", "ZH",
                "true = Chinese, false = English. Or right-click → English / 中文.",
                GH_ParamAccess.item,
                false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Is Chinese", "ZH", "True when UI language is Chinese", GH_ParamAccess.item);
            pManager.AddTextParameter("Language", "L", "Current language (en / zh)", GH_ParamAccess.item);
            pManager.AddTextParameter("Tip", "Tip", "What changed / how to use", GH_ParamAccess.item);
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);

            Menu_AppendItem(
                menu,
                "English",
                (sender, e) => ApplyFromMenu(false),
                true,
                UiLanguage.IsEnglish);

            Menu_AppendItem(
                menu,
                "中文 Chinese",
                (sender, e) => ApplyFromMenu(true),
                true,
                UiLanguage.IsChinese);
        }

        private void ApplyFromMenu(bool chinese)
        {
            _menuLock = true;
            _menuChinese = chinese;
            SetBooleanInput(chinese);
            ApplyLanguage(chinese);
            int n = LanguageRefresh.ApplyNow();
            // Defer a second pass so layout settles after GH finishes this event.
            LanguageRefresh.ApplySoon();
            ExpireSolution(true);
            try
            {
                Rhino.RhinoApp.WriteLine(
                    chinese
                        ? $"[SimpleML] 已切换为中文，已刷新 {n} 个画布组件。"
                        : $"[SimpleML] Switched to English; refreshed {n} canvas components.");
            }
            catch { }
        }

        private void SetBooleanInput(bool chinese)
        {
            try
            {
                if (Params.Input.Count > 0 && Params.Input[0] is Param_Boolean pb)
                {
                    // If something is wired into ZH, warn via Tip later; still set persistent data.
                    pb.PersistentData.Clear();
                    pb.PersistentData.Append(new GH_Boolean(chinese));
                    pb.ExpireSolution(false);
                }
            }
            catch { }
        }

        private void ApplyLanguage(bool chinese)
        {
            string want = chinese ? UiLanguage.Chinese : UiLanguage.English;
            UiLanguage.Set(want);
            _lastApplied = want;

            Name = L.Name(nameof(LanguageSwitchComponent));
            NickName = L.Nick(nameof(LanguageSwitchComponent));
            Description = L.Desc(nameof(LanguageSwitchComponent));

            if (Params.Input.Count > 0)
            {
                Params.Input[0].Name = chinese ? "中文" : "Chinese";
                Params.Input[0].NickName = "ZH";
                Params.Input[0].Description = chinese
                    ? "true = 中文，false = 英文。也可右键本组件切换。"
                    : "true = Chinese, false = English. Or right-click this component.";
            }
            if (Params.Output.Count >= 3)
            {
                Params.Output[0].Name = chinese ? "是否中文" : "Is Chinese";
                Params.Output[0].NickName = "ZH";
                Params.Output[1].Name = chinese ? "语言" : "Language";
                Params.Output[1].NickName = "L";
                Params.Output[2].Name = chinese ? "提示" : "Tip";
                Params.Output[2].NickName = "Tip";
            }

            Attributes?.ExpireLayout();
            OnDisplayExpired(true);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool chinese;
            string tip;

            if (_menuLock)
            {
                chinese = _menuChinese;
                _menuLock = false;
                tip = chinese
                    ? "已切换为中文。画布组件名/说明已刷新。若顶部 Ribbon 仍是旧语言，重新搜索组件名即可。"
                    : "Switched to English. Canvas names refreshed. If the top ribbon still shows old names, search the component again.";
            }
            else
            {
                chinese = false;
                DA.GetData(0, ref chinese);
                tip = chinese
                    ? "当前：中文。右键本组件可快速切换。"
                    : "Current: English. Right-click this component to switch quickly.";
            }

            // If a Boolean Toggle is wired, it wins over menu persistent data on the next solve.
            if (Params.Input[0].SourceCount > 0)
            {
                tip += chinese
                    ? "\n注意：ZH 口已接线，语言跟随上游 Boolean。"
                    : "\nNote: ZH input is wired; language follows the upstream Boolean.";
            }

            LanguageRefresh.EnsureHooks();

            string want = chinese ? UiLanguage.Chinese : UiLanguage.English;
            bool changed = want != UiLanguage.Current || want != _lastApplied;
            ApplyLanguage(chinese);

            // Always refresh open canvases so already-placed batteries update Name / ports.
            int n = LanguageRefresh.ApplyNow();
            LanguageRefresh.ApplySoon();
            if (changed)
            {
                tip += chinese
                    ? $"\n已刷新画布组件 {n} 个（名称与输入/输出）。Ribbon 需重启 Rhino 才更新。"
                    : $"\nRefreshed {n} canvas components (names + ports). Ribbon needs Rhino restart.";
            }
            else
            {
                tip += chinese
                    ? $"\n已同步画布标签（{n}）。"
                    : $"\nCanvas labels synced ({n}).";
            }

            DA.SetData(0, UiLanguage.IsChinese);
            DA.SetData(1, UiLanguage.Current);
            DA.SetData(2, tip);
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(LanguageSwitchComponent));
        public override Guid ComponentGuid => new Guid("A9C4E2B1-7D35-4F80-9E12-6B8A0C5D4E31");
    }
}
