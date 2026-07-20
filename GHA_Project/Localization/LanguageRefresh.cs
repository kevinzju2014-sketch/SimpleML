using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Grasshopper;
using Grasshopper.Kernel;

namespace SimpleML.Localization
{
    /// <summary>
    /// Force-refresh SimpleML component + parameter labels on open canvases.
    /// </summary>
    public static class LanguageRefresh
    {
        private static bool _idleHooked;
        private static bool _hooksInstalled;
        private static int _generation;
        private static EventHandler _idleHandler;

        private sealed class ParamOrigin
        {
            public string EnName;
            public string EnNick;
        }

        private static readonly ConditionalWeakTable<IGH_Param, ParamOrigin> Origins =
            new ConditionalWeakTable<IGH_Param, ParamOrigin>();

        public static void EnsureHooks()
        {
            if (_hooksInstalled)
                return;
            _hooksInstalled = true;
            try
            {
                var server = Instances.DocumentServer;
                if (server == null)
                {
                    _hooksInstalled = false;
                    return;
                }
                server.DocumentAdded += OnDocumentAdded;
            }
            catch
            {
                _hooksInstalled = false;
            }
        }

        private static void OnDocumentAdded(GH_DocumentServer sender, GH_Document doc)
        {
            try
            {
                if (doc == null) return;
                ApplyToDocument(doc);
                try { doc.NewSolution(false); } catch { }
                try { Instances.RedrawCanvas(); } catch { }
            }
            catch { }
        }

        public static void ApplySoon()
        {
            EnsureHooks();
            _generation++;
            int gen = _generation;
            if (_idleHandler == null)
            {
                _idleHandler = (s, e) =>
                {
                    try { Rhino.RhinoApp.Idle -= _idleHandler; } catch { }
                    _idleHooked = false;
                    ApplyToOpenDocuments();
                    if (gen != _generation)
                        ApplySoon();
                };
            }
            if (_idleHooked)
                return;
            _idleHooked = true;
            try { Rhino.RhinoApp.Idle += _idleHandler; }
            catch
            {
                _idleHooked = false;
                ApplyToOpenDocuments();
            }
        }

        public static int ApplyNow()
        {
            EnsureHooks();
            _generation++;
            try
            {
                if (_idleHooked && _idleHandler != null)
                    Rhino.RhinoApp.Idle -= _idleHandler;
            }
            catch { }
            _idleHooked = false;
            int n = ApplyToOpenDocuments();
            // Second pass after layout: GH sometimes restores nicknames mid-event.
            try { Rhino.RhinoApp.Idle += SecondPassOnce; } catch { }
            return n;
        }

        private static void SecondPassOnce(object sender, EventArgs e)
        {
            try { Rhino.RhinoApp.Idle -= SecondPassOnce; } catch { }
            try { ApplyToOpenDocuments(); } catch { }
        }

        public static int ApplyToOpenDocuments()
        {
            int n = 0;
            try
            {
                var server = Instances.DocumentServer;
                if (server == null)
                    return 0;
                for (int i = 0; i < server.DocumentCount; i++)
                {
                    GH_Document doc = server[i];
                    if (doc == null) continue;
                    n += ApplyToDocument(doc);
                    try { doc.NewSolution(false); } catch { }
                }
            }
            catch { }

            try { Instances.RedrawCanvas(); } catch { }
            try
            {
                var canvas = Instances.ActiveCanvas;
                if (canvas != null)
                {
                    canvas.Refresh();
                    canvas.Invalidate();
                }
            }
            catch { }
            return n;
        }

        public static bool IsSimpleMLComponent(IGH_DocumentObject obj)
        {
            if (!(obj is GH_Component))
                return false;
            Type t = obj.GetType();
            if (t.Assembly == typeof(LanguageRefresh).Assembly)
                return true;
            string ns = t.Namespace ?? "";
            if (ns.StartsWith("SimpleML", StringComparison.Ordinal))
                return true;
            string an = t.Assembly.GetName().Name ?? "";
            return an.IndexOf("SimpleML", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static int ApplyToDocument(GH_Document doc)
        {
            int n = 0;
            if (doc == null)
                return 0;

            foreach (IGH_DocumentObject obj in doc.Objects)
            {
                if (!(obj is GH_Component c))
                    continue;
                if (!IsSimpleMLComponent(c))
                    continue;

                string cls = c.GetType().Name;
                try
                {
                    string newName = L.Name(cls);
                    string newNick = L.Nick(cls);
                    string newDesc = L.Desc(cls);
                    // Skip broken locale keys (missing entries return the key path).
                    if (newName != null && !newName.StartsWith("comp.", StringComparison.Ordinal))
                        c.Name = newName;
                    if (newNick != null && !newNick.StartsWith("comp.", StringComparison.Ordinal))
                        c.NickName = newNick;
                    if (newDesc != null && !newDesc.StartsWith("comp.", StringComparison.Ordinal))
                        c.Description = newDesc;

                    LocalizeParameters(c);

                    try { c.OnObjectChanged(GH_ObjectEventType.NickName); } catch { }

                    if (c.Attributes != null)
                    {
                        c.Attributes.ExpireLayout();
                        c.Attributes.PerformLayout();
                    }
                    c.OnDisplayExpired(true);

                    if (!string.Equals(cls, "LanguageSwitchComponent", StringComparison.Ordinal))
                        c.ExpireSolution(true);
                    n++;
                }
                catch { }
            }
            return n;
        }

        public static void LocalizeParameters(GH_Component c)
        {
            if (c?.Params == null)
                return;
            bool zh = UiLanguage.IsChinese;
            foreach (IGH_Param p in c.Params.Input)
                LocalizeOne(p, zh);
            foreach (IGH_Param p in c.Params.Output)
                LocalizeOne(p, zh);
        }

        private static ParamOrigin CaptureOrigin(IGH_Param p)
        {
            if (Origins.TryGetValue(p, out ParamOrigin existing))
                return existing;

            string name = p.Name ?? "";
            string nick = p.NickName ?? "";

            // Resolve to English name via table (works even if already Chinese).
            string enName = name;
            string enNick = nick;
            if (ParamTable.TryGet(name, out string eN, out _, out string eK, out _, out _, out _)
                || ParamTable.TryGet(nick, out eN, out _, out eK, out _, out _, out _))
            {
                enName = eN;
                enNick = eK;
            }

            var origin = new ParamOrigin { EnName = enName, EnNick = enNick };
            Origins.Add(p, origin);
            return origin;
        }

        private static void LocalizeOne(IGH_Param p, bool zh)
        {
            if (p == null)
                return;
            try
            {
                ParamOrigin origin = CaptureOrigin(p);
                if (!ParamTable.TryGet(origin.EnName, out string enName, out string zhName, out string enNick, out string zhNick, out string enDesc, out string zhDesc)
                    && !ParamTable.TryGet(origin.EnNick, out enName, out zhName, out enNick, out zhNick, out enDesc, out zhDesc)
                    && !ParamTable.TryGet(p.Name ?? "", out enName, out zhName, out enNick, out zhNick, out enDesc, out zhDesc)
                    && !ParamTable.TryGet(p.NickName ?? "", out enName, out zhName, out enNick, out zhNick, out enDesc, out zhDesc))
                {
                    return;
                }

                // Keep origin stable.
                origin.EnName = enName;
                origin.EnNick = enNick;

                p.Name = zh ? zhName : enName;
                p.NickName = zh ? zhNick : enNick;
                if (!string.IsNullOrEmpty(enDesc) || !string.IsNullOrEmpty(zhDesc))
                    p.Description = zh ? zhDesc : enDesc;

                try { p.OnObjectChanged(GH_ObjectEventType.NickName); } catch { }
                try { p.OnDisplayExpired(true); } catch { }
            }
            catch { }
        }
    }

    public static class ParamTable
    {
        private struct Entry
        {
            public string EnName, ZhName, EnNick, ZhNick, EnDesc, ZhDesc;
            public Entry(string enN, string zhN, string enNick, string zhNick, string enD, string zhD)
            {
                EnName = enN; ZhName = zhN; EnNick = enNick; ZhNick = zhNick; EnDesc = enD; ZhDesc = zhD;
            }
        }

        private static readonly Dictionary<string, Entry> Map =
            new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase)
            {
                { "Affinity", new Entry("Affinity", "亲和度", "A", "A", "", "") },
                { "亲和度", new Entry("Affinity", "亲和度", "A", "A", "", "") },
                { "Algorithm", new Entry("Algorithm", "算法", "A", "算法", "", "") },
                { "算法", new Entry("Algorithm", "算法", "A", "算法", "", "") },
                { "Algorithm Params", new Entry("Algorithm Params", "算法参数", "AP", "参数", "", "") },
                { "算法参数", new Entry("Algorithm Params", "算法参数", "AP", "参数", "", "") },
                { "Alpha", new Entry("Alpha", "Alpha", "A", "A", "", "") },
                { "Analysis", new Entry("Analysis", "分析", "A", "A", "", "") },
                { "分析", new Entry("Analysis", "分析", "A", "A", "", "") },
                { "Author", new Entry("Author", "作者", "A", "作者", "", "") },
                { "作者", new Entry("Author", "作者", "A", "作者", "", "") },
                { "AutoFix", new Entry("AutoFix", "自动修复", "Fix", "修复", "", "") },
                { "自动修复", new Entry("AutoFix", "自动修复", "Fix", "修复", "", "") },
                { "Bootstrap", new Entry("Bootstrap", "Bootstrap", "B", "B", "", "") },
                { "C", new Entry("C", "C", "C", "C", "", "") },
                { "Chinese", new Entry("Chinese", "中文", "ZH", "中文", "", "") },
                { "中文", new Entry("Chinese", "中文", "ZH", "中文", "", "") },
                { "Class Weight", new Entry("Class Weight", "类别权重", "CW", "CW", "", "") },
                { "类别权重", new Entry("Class Weight", "类别权重", "CW", "CW", "", "") },
                { "Coef0", new Entry("Coef0", "Coef0", "C0", "C0", "", "") },
                { "Color Map", new Entry("Color Map", "颜色映射", "CM", "色表", "", "") },
                { "颜色映射", new Entry("Color Map", "颜色映射", "CM", "色表", "", "") },
                { "Colored Geometry", new Entry("Colored Geometry", "着色几何", "CG", "着色", "", "") },
                { "着色几何", new Entry("Colored Geometry", "着色几何", "CG", "着色", "", "") },
                { "Colors", new Entry("Colors", "颜色", "C", "颜色", "", "") },
                { "颜色", new Entry("Colors", "颜色", "C", "颜色", "", "") },
                { "Compute Distances", new Entry("Compute Distances", "计算距离", "CD", "CD", "", "") },
                { "计算距离", new Entry("Compute Distances", "计算距离", "CD", "CD", "", "") },
                { "Compute Full Tree", new Entry("Compute Full Tree", "完整树", "CFT", "CFT", "", "") },
                { "完整树", new Entry("Compute Full Tree", "完整树", "CFT", "CFT", "", "") },
                { "Confusion Matrix", new Entry("Confusion Matrix", "混淆矩阵", "CM", "混淆", "", "") },
                { "混淆矩阵", new Entry("Confusion Matrix", "混淆矩阵", "CM", "混淆", "", "") },
                { "Contact", new Entry("Contact", "联系方式", "C", "联系", "", "") },
                { "联系方式", new Entry("Contact", "联系方式", "C", "联系", "", "") },
                { "Copy X", new Entry("Copy X", "复制X", "CX", "CX", "", "") },
                { "复制X", new Entry("Copy X", "复制X", "CX", "CX", "", "") },
                { "Correlation Matrix", new Entry("Correlation Matrix", "相关矩阵", "CM", "相关", "", "") },
                { "相关矩阵", new Entry("Correlation Matrix", "相关矩阵", "CM", "相关", "", "") },
                { "Criterion", new Entry("Criterion", "划分标准", "C", "C", "", "") },
                { "划分标准", new Entry("Criterion", "划分标准", "C", "C", "", "") },
                { "Data", new Entry("Data", "数据", "D", "数据", "", "") },
                { "数据", new Entry("Data", "数据", "D", "数据", "", "") },
                { "Dataset", new Entry("Dataset", "数据集", "DS", "数据", "", "") },
                { "数据集", new Entry("Dataset", "数据集", "DS", "数据", "", "") },
                { "Dataset Name", new Entry("Dataset Name", "数据集名", "DN", "名", "", "") },
                { "数据集名", new Entry("Dataset Name", "数据集名", "DN", "名", "", "") },
                { "Degree", new Entry("Degree", "度数", "D", "D", "", "") },
                { "度数", new Entry("Degree", "度数", "D", "D", "", "") },
                { "Description", new Entry("Description", "描述", "Desc", "描述", "", "") },
                { "描述", new Entry("Description", "描述", "Desc", "描述", "", "") },
                { "Dimensions", new Entry("Dimensions", "维度", "D", "D", "", "") },
                { "维度", new Entry("Dimensions", "维度", "D", "D", "", "") },
                { "Distance Threshold", new Entry("Distance Threshold", "距离阈值", "DT", "DT", "", "") },
                { "距离阈值", new Entry("Distance Threshold", "距离阈值", "DT", "DT", "", "") },
                { "Encoding", new Entry("Encoding", "编码", "E", "E", "", "") },
                { "编码", new Entry("Encoding", "编码", "E", "E", "", "") },
                { "Eps", new Entry("Eps", "Eps", "E", "E", "", "") },
                { "Epsilon", new Entry("Epsilon", "Epsilon", "E", "E", "", "") },
                { "Explained Variance", new Entry("Explained Variance", "解释方差", "EV", "EV", "", "") },
                { "解释方差", new Entry("Explained Variance", "解释方差", "EV", "EV", "", "") },
                { "Explanation", new Entry("Explanation", "解释", "E", "解释", "", "") },
                { "解释", new Entry("Explanation", "解释", "E", "解释", "", "") },
                { "Feature Index", new Entry("Feature Index", "特征索引", "FI", "FI", "", "") },
                { "特征索引", new Entry("Feature Index", "特征索引", "FI", "FI", "", "") },
                { "Feature Names", new Entry("Feature Names", "特征名", "FN", "列名", "", "") },
                { "特征名", new Entry("Feature Names", "特征名", "FN", "列名", "", "") },
                { "Features", new Entry("Features", "特征", "X", "X", "", "") },
                { "特征", new Entry("Features", "特征", "X", "X", "", "") },
                { "Filepath", new Entry("Filepath", "文件路径", "F", "路径", "", "") },
                { "文件路径", new Entry("Filepath", "文件路径", "F", "路径", "", "") },
                { "Fit Intercept", new Entry("Fit Intercept", "拟合截距", "FI", "FI", "", "") },
                { "拟合截距", new Entry("Fit Intercept", "拟合截距", "FI", "FI", "", "") },
                { "Fit Line", new Entry("Fit Line", "拟合线", "FL", "拟合", "", "") },
                { "拟合线", new Entry("Fit Line", "拟合线", "FL", "拟合", "", "") },
                { "Fit Prior", new Entry("Fit Prior", "学习先验", "FP", "FP", "", "") },
                { "学习先验", new Entry("Fit Prior", "学习先验", "FP", "FP", "", "") },
                { "Gamma", new Entry("Gamma", "Gamma", "G", "G", "", "") },
                { "Geometry", new Entry("Geometry", "几何", "G", "几何", "", "") },
                { "几何", new Entry("Geometry", "几何", "G", "几何", "", "") },
                { "Handle Missing", new Entry("Handle Missing", "处理缺失", "HM", "HM", "", "") },
                { "处理缺失", new Entry("Handle Missing", "处理缺失", "HM", "HM", "", "") },
                { "Header", new Entry("Header", "表头", "H", "H", "", "") },
                { "表头", new Entry("Header", "表头", "H", "H", "", "") },
                { "Health Status", new Entry("Health Status", "体检状态", "S", "体检", "", "") },
                { "体检状态", new Entry("Health Status", "体检状态", "S", "体检", "", "") },
                { "Hint", new Entry("Hint", "提示", "H", "提示", "", "") },
                { "提示", new Entry("Hint", "提示", "H", "提示", "", "") },
                { "Importance", new Entry("Importance", "重要性", "I", "重要", "", "") },
                { "重要性", new Entry("Importance", "重要性", "I", "重要", "", "") },
                { "Index", new Entry("Index", "索引", "I", "I", "", "") },
                { "索引", new Entry("Index", "索引", "I", "I", "", "") },
                { "Info", new Entry("Info", "信息", "I", "I", "", "") },
                { "信息", new Entry("Info", "信息", "I", "I", "", "") },
                { "Init", new Entry("Init", "初始化", "I", "I", "", "") },
                { "初始化", new Entry("Init", "初始化", "I", "I", "", "") },
                { "Installation Steps", new Entry("Installation Steps", "安装步骤", "IS", "步骤", "", "") },
                { "安装步骤", new Entry("Installation Steps", "安装步骤", "IS", "步骤", "", "") },
                { "Is Chinese", new Entry("Is Chinese", "是否中文", "ZH", "中文?", "", "") },
                { "是否中文", new Entry("Is Chinese", "是否中文", "ZH", "中文?", "", "") },
                { "K", new Entry("K", "K", "K", "K", "", "") },
                { "Kernel", new Entry("Kernel", "核函数", "K", "K", "", "") },
                { "核函数", new Entry("Kernel", "核函数", "K", "K", "", "") },
                { "Labels", new Entry("Labels", "标签", "L", "标签", "", "") },
                { "标签", new Entry("Labels", "标签", "L", "标签", "", "") },
                { "Language", new Entry("Language", "语言", "L", "语言", "", "") },
                { "语言", new Entry("Language", "语言", "L", "语言", "", "") },
                { "Leaf Size", new Entry("Leaf Size", "叶子大小", "LS", "LS", "", "") },
                { "叶子大小", new Entry("Leaf Size", "叶子大小", "LS", "LS", "", "") },
                { "Linkage", new Entry("Linkage", "连接方式", "L", "L", "", "") },
                { "连接方式", new Entry("Linkage", "连接方式", "L", "L", "", "") },
                { "Max Clusters", new Entry("Max Clusters", "最大簇数", "MC", "MC", "", "") },
                { "最大簇数", new Entry("Max Clusters", "最大簇数", "MC", "MC", "", "") },
                { "Max Depth", new Entry("Max Depth", "最大深度", "D", "D", "", "") },
                { "最大深度", new Entry("Max Depth", "最大深度", "D", "D", "", "") },
                { "Max Features", new Entry("Max Features", "最大特征数", "MF", "MF", "", "") },
                { "最大特征数", new Entry("Max Features", "最大特征数", "MF", "MF", "", "") },
                { "Max Iter", new Entry("Max Iter", "最大迭代", "MI", "MI", "", "") },
                { "最大迭代", new Entry("Max Iter", "最大迭代", "MI", "MI", "", "") },
                { "Method", new Entry("Method", "方法", "M", "M", "", "") },
                { "方法", new Entry("Method", "方法", "M", "M", "", "") },
                { "Metric", new Entry("Metric", "距离度量", "M", "M", "", "") },
                { "距离度量", new Entry("Metric", "距离度量", "M", "M", "", "") },
                { "Metrics", new Entry("Metrics", "指标", "M", "指标", "", "") },
                { "指标", new Entry("Metrics", "指标", "M", "指标", "", "") },
                { "Metrics JSON", new Entry("Metrics JSON", "指标JSON", "MJ", "JSON", "", "") },
                { "指标JSON", new Entry("Metrics JSON", "指标JSON", "MJ", "JSON", "", "") },
                { "Min Samples", new Entry("Min Samples", "最小样本", "MS", "MS", "", "") },
                { "最小样本", new Entry("Min Samples", "最小样本", "MS", "MS", "", "") },
                { "Min Samples Leaf", new Entry("Min Samples Leaf", "最小叶子样本", "MSL", "MSL", "", "") },
                { "最小叶子样本", new Entry("Min Samples Leaf", "最小叶子样本", "MSL", "MSL", "", "") },
                { "Min Samples Split", new Entry("Min Samples Split", "最小分割样本", "MSS", "MSS", "", "") },
                { "最小分割样本", new Entry("Min Samples Split", "最小分割样本", "MSS", "MSS", "", "") },
                { "Missing Strategy", new Entry("Missing Strategy", "缺失策略", "MS", "MS", "", "") },
                { "缺失策略", new Entry("Missing Strategy", "缺失策略", "MS", "MS", "", "") },
                { "Model", new Entry("Model", "模型", "M", "模型", "", "") },
                { "模型", new Entry("Model", "模型", "M", "模型", "", "") },
                { "Model Card", new Entry("Model Card", "模型卡片", "Card", "卡片", "", "") },
                { "模型卡片", new Entry("Model Card", "模型卡片", "Card", "卡片", "", "") },
                { "Model Info", new Entry("Model Info", "模型信息", "MI", "信息", "", "") },
                { "模型信息", new Entry("Model Info", "模型信息", "MI", "信息", "", "") },
                { "N Clusters", new Entry("N Clusters", "簇数", "NC", "簇数", "", "") },
                { "簇数", new Entry("N Clusters", "簇数", "NC", "簇数", "", "") },
                { "N Estimators", new Entry("N Estimators", "树数量", "N", "N", "", "") },
                { "树数量", new Entry("N Estimators", "树数量", "N", "N", "", "") },
                { "N Init", new Entry("N Init", "初始化次数", "NI", "NI", "", "") },
                { "初始化次数", new Entry("N Init", "初始化次数", "NI", "NI", "", "") },
                { "N Jobs", new Entry("N Jobs", "并行数", "NJ", "NJ", "", "") },
                { "并行数", new Entry("N Jobs", "并行数", "NJ", "NJ", "", "") },
                { "N Neighbors", new Entry("N Neighbors", "邻居数", "N", "N", "", "") },
                { "邻居数", new Entry("N Neighbors", "邻居数", "N", "N", "", "") },
                { "Next Steps", new Entry("Next Steps", "下一步", "Next", "下一步", "", "") },
                { "下一步", new Entry("Next Steps", "下一步", "Next", "下一步", "", "") },
                { "Normalize", new Entry("Normalize", "标准化", "N", "N", "", "") },
                { "标准化", new Entry("Normalize", "标准化", "N", "N", "", "") },
                { "Normalize Method", new Entry("Normalize Method", "标准化方法", "NM", "NM", "", "") },
                { "标准化方法", new Entry("Normalize Method", "标准化方法", "NM", "NM", "", "") },
                { "P", new Entry("P", "P", "P", "P", "", "") },
                { "Package Root", new Entry("Package Root", "包路径", "Path", "路径", "", "") },
                { "包路径", new Entry("Package Root", "包路径", "Path", "路径", "", "") },
                { "Penalty", new Entry("Penalty", "正则", "P", "P", "", "") },
                { "正则", new Entry("Penalty", "正则", "P", "P", "", "") },
                { "Plugin Installation", new Entry("Plugin Installation", "插件安装", "PI", "安装", "", "") },
                { "插件安装", new Entry("Plugin Installation", "插件安装", "PI", "安装", "", "") },
                { "Plugin Name", new Entry("Plugin Name", "插件名", "PN", "名称", "", "") },
                { "插件名", new Entry("Plugin Name", "插件名", "PN", "名称", "", "") },
                { "Points", new Entry("Points", "点", "P", "点", "", "") },
                { "点", new Entry("Points", "点", "P", "点", "", "") },
                { "Predictions", new Entry("Predictions", "预测", "P", "预测", "", "") },
                { "预测", new Entry("Predictions", "预测", "P", "预测", "", "") },
                { "Probabilities", new Entry("Probabilities", "概率", "Prob", "概率", "", "") },
                { "概率", new Entry("Probabilities", "概率", "Prob", "概率", "", "") },
                { "Probability", new Entry("Probability", "概率估计", "P", "P", "", "") },
                { "概率估计", new Entry("Probability", "概率估计", "P", "P", "", "") },
                { "Python", new Entry("Python", "Python路径", "Py", "Py", "", "") },
                { "Python路径", new Entry("Python", "Python路径", "Py", "Py", "", "") },
                { "Python Requirements", new Entry("Python Requirements", "Python依赖", "PR", "依赖", "", "") },
                { "Python依赖", new Entry("Python Requirements", "Python依赖", "PR", "依赖", "", "") },
                { "Random State", new Entry("Random State", "随机种子", "RS", "种子", "", "") },
                { "随机种子", new Entry("Random State", "随机种子", "RS", "种子", "", "") },
                { "Readme", new Entry("Readme", "说明", "R", "说明", "", "") },
                { "说明", new Entry("Readme", "说明", "R", "说明", "", "") },
                { "Recipe", new Entry("Recipe", "配方", "Recipe", "配方", "", "") },
                { "配方", new Entry("Recipe", "配方", "Recipe", "配方", "", "") },
                { "Remove Outliers", new Entry("Remove Outliers", "移除异常", "RO", "RO", "", "") },
                { "移除异常", new Entry("Remove Outliers", "移除异常", "RO", "RO", "", "") },
                { "Report", new Entry("Report", "报告", "Rep", "报告", "", "") },
                { "报告", new Entry("Report", "报告", "Rep", "报告", "", "") },
                { "Reset", new Entry("Reset", "重置", "R", "重置", "", "") },
                { "重置", new Entry("Reset", "重置", "R", "重置", "", "") },
                { "Run", new Entry("Run", "运行", "Run", "运行", "", "") },
                { "运行", new Entry("Run", "运行", "Run", "运行", "", "") },
                { "Run Health Check", new Entry("Run Health Check", "运行体检", "HC", "体检", "", "") },
                { "运行体检", new Entry("Run Health Check", "运行体检", "HC", "体检", "", "") },
                { "Save", new Entry("Save", "保存", "S", "保存", "", "") },
                { "保存", new Entry("Save", "保存", "S", "保存", "", "") },
                { "Saved Path", new Entry("Saved Path", "保存路径", "SP", "路径", "", "") },
                { "保存路径", new Entry("Saved Path", "保存路径", "SP", "路径", "", "") },
                { "Scatter Points", new Entry("Scatter Points", "散点", "SP", "散点", "", "") },
                { "散点", new Entry("Scatter Points", "散点", "SP", "散点", "", "") },
                { "Score", new Entry("Score", "得分", "S", "得分", "", "") },
                { "得分", new Entry("Score", "得分", "S", "得分", "", "") },
                { "Scores", new Entry("Scores", "得分列表", "S", "得分", "", "") },
                { "得分列表", new Entry("Scores", "得分列表", "S", "得分", "", "") },
                { "Selection", new Entry("Selection", "选择策略", "S", "S", "", "") },
                { "选择策略", new Entry("Selection", "选择策略", "S", "S", "", "") },
                { "Separator", new Entry("Separator", "分隔符", "S", "S", "", "") },
                { "分隔符", new Entry("Separator", "分隔符", "S", "S", "", "") },
                { "Sheet Name", new Entry("Sheet Name", "工作表", "S", "S", "", "") },
                { "工作表", new Entry("Sheet Name", "工作表", "S", "S", "", "") },
                { "Show Fit Line", new Entry("Show Fit Line", "显示拟合线", "SFL", "拟合线", "", "") },
                { "显示拟合线", new Entry("Show Fit Line", "显示拟合线", "SFL", "拟合线", "", "") },
                { "Solver", new Entry("Solver", "求解器", "S", "S", "", "") },
                { "求解器", new Entry("Solver", "求解器", "S", "S", "", "") },
                { "Statistics", new Entry("Statistics", "统计", "S", "统计", "", "") },
                { "统计", new Entry("Statistics", "统计", "S", "统计", "", "") },
                { "Status", new Entry("Status", "状态", "S", "状态", "", "") },
                { "状态", new Entry("Status", "状态", "S", "状态", "", "") },
                { "Summary", new Entry("Summary", "摘要", "Sum", "摘要", "", "") },
                { "摘要", new Entry("Summary", "摘要", "Sum", "摘要", "", "") },
                { "Target", new Entry("Target", "目标", "y", "y", "", "") },
                { "目标", new Entry("Target", "目标", "y", "y", "", "") },
                { "Task", new Entry("Task", "任务", "T", "任务", "", "") },
                { "任务", new Entry("Task", "任务", "T", "任务", "", "") },
                { "Test Dataset", new Entry("Test Dataset", "测试数据集", "TestDS", "测试", "", "") },
                { "测试数据集", new Entry("Test Dataset", "测试数据集", "TestDS", "测试", "", "") },
                { "Test Size", new Entry("Test Size", "测试比例", "TS", "比例", "", "") },
                { "测试比例", new Entry("Test Size", "测试比例", "TS", "比例", "", "") },
                { "Tip", new Entry("Tip", "提示", "Tip", "提示", "", "") },
                { "Tol", new Entry("Tol", "容差", "T", "T", "", "") },
                { "容差", new Entry("Tol", "容差", "T", "T", "", "") },
                { "Train Dataset", new Entry("Train Dataset", "训练数据集", "TrainDS", "训练", "", "") },
                { "训练数据集", new Entry("Train Dataset", "训练数据集", "TrainDS", "训练", "", "") },
                { "Troubleshooting", new Entry("Troubleshooting", "故障排查", "T", "排查", "", "") },
                { "故障排查", new Entry("Troubleshooting", "故障排查", "T", "排查", "", "") },
                { "Type", new Entry("Type", "类型", "T", "T", "", "") },
                { "类型", new Entry("Type", "类型", "T", "T", "", "") },
                { "Var Smoothing", new Entry("Var Smoothing", "方差平滑", "VS", "VS", "", "") },
                { "方差平滑", new Entry("Var Smoothing", "方差平滑", "VS", "VS", "", "") },
                { "Verdict", new Entry("Verdict", "结论", "V", "结论", "", "") },
                { "结论", new Entry("Verdict", "结论", "V", "结论", "", "") },
                { "Version", new Entry("Version", "版本", "V", "版本", "", "") },
                { "版本", new Entry("Version", "版本", "V", "版本", "", "") },
                { "Warm Start", new Entry("Warm Start", "热启动", "WS", "WS", "", "") },
                { "热启动", new Entry("Warm Start", "热启动", "WS", "WS", "", "") },
                { "Weights", new Entry("Weights", "权重", "W", "W", "", "") },
                { "权重", new Entry("Weights", "权重", "W", "W", "", "") },
                { "Write", new Entry("Write", "写入", "W", "写入", "", "") },
                { "写入", new Entry("Write", "写入", "W", "写入", "", "") },
                { "X", new Entry("X", "X", "X", "X", "", "") },
                { "X Names", new Entry("X Names", "特征名", "XN", "XN", "", "") },
                { "Y Pred", new Entry("Y Pred", "预测值", "YP", "预测", "", "") },
                { "预测值", new Entry("Y Pred", "预测值", "YP", "预测", "", "") },
                { "Y True", new Entry("Y True", "真实值", "YT", "真值", "", "") },
                { "真实值", new Entry("Y True", "真实值", "YT", "真值", "", "") },
                { "y", new Entry("y", "y", "y", "y", "", "") },
                { "y Names", new Entry("y Names", "标签名", "yN", "yN", "", "") },
                { "标签名", new Entry("y Names", "标签名", "yN", "yN", "", "") },
                { "AP", new Entry("Algorithm Params", "算法参数", "AP", "参数", "", "") },
                { "参数", new Entry("Algorithm Params", "算法参数", "AP", "参数", "", "") },
                { "Fix", new Entry("AutoFix", "自动修复", "Fix", "修复", "", "") },
                { "修复", new Entry("AutoFix", "自动修复", "Fix", "修复", "", "") },
                { "B", new Entry("Bootstrap", "Bootstrap", "B", "B", "", "") },
                { "CW", new Entry("Class Weight", "类别权重", "CW", "CW", "", "") },
                { "C0", new Entry("Coef0", "Coef0", "C0", "C0", "", "") },
                { "CG", new Entry("Colored Geometry", "着色几何", "CG", "着色", "", "") },
                { "着色", new Entry("Colored Geometry", "着色几何", "CG", "着色", "", "") },
                { "CD", new Entry("Compute Distances", "计算距离", "CD", "CD", "", "") },
                { "CFT", new Entry("Compute Full Tree", "完整树", "CFT", "CFT", "", "") },
                { "CX", new Entry("Copy X", "复制X", "CX", "CX", "", "") },
                { "DS", new Entry("Dataset", "数据集", "DS", "数据", "", "") },
                { "DN", new Entry("Dataset Name", "数据集名", "DN", "名", "", "") },
                { "名", new Entry("Dataset Name", "数据集名", "DN", "名", "", "") },
                { "Desc", new Entry("Description", "描述", "Desc", "描述", "", "") },
                { "DT", new Entry("Distance Threshold", "距离阈值", "DT", "DT", "", "") },
                { "EV", new Entry("Explained Variance", "解释方差", "EV", "EV", "", "") },
                { "FN", new Entry("Feature Names", "特征名", "FN", "列名", "", "") },
                { "列名", new Entry("Feature Names", "特征名", "FN", "列名", "", "") },
                { "F", new Entry("Filepath", "文件路径", "F", "路径", "", "") },
                { "路径", new Entry("Filepath", "文件路径", "F", "路径", "", "") },
                { "FL", new Entry("Fit Line", "拟合线", "FL", "拟合", "", "") },
                { "拟合", new Entry("Fit Line", "拟合线", "FL", "拟合", "", "") },
                { "FP", new Entry("Fit Prior", "学习先验", "FP", "FP", "", "") },
                { "HM", new Entry("Handle Missing", "处理缺失", "HM", "HM", "", "") },
                { "IS", new Entry("Installation Steps", "安装步骤", "IS", "步骤", "", "") },
                { "步骤", new Entry("Installation Steps", "安装步骤", "IS", "步骤", "", "") },
                { "LS", new Entry("Leaf Size", "叶子大小", "LS", "LS", "", "") },
                { "MC", new Entry("Max Clusters", "最大簇数", "MC", "MC", "", "") },
                { "MF", new Entry("Max Features", "最大特征数", "MF", "MF", "", "") },
                { "MJ", new Entry("Metrics JSON", "指标JSON", "MJ", "JSON", "", "") },
                { "JSON", new Entry("Metrics JSON", "指标JSON", "MJ", "JSON", "", "") },
                { "MSL", new Entry("Min Samples Leaf", "最小叶子样本", "MSL", "MSL", "", "") },
                { "MSS", new Entry("Min Samples Split", "最小分割样本", "MSS", "MSS", "", "") },
                { "Card", new Entry("Model Card", "模型卡片", "Card", "卡片", "", "") },
                { "卡片", new Entry("Model Card", "模型卡片", "Card", "卡片", "", "") },
                { "NC", new Entry("N Clusters", "簇数", "NC", "簇数", "", "") },
                { "NI", new Entry("N Init", "初始化次数", "NI", "NI", "", "") },
                { "NJ", new Entry("N Jobs", "并行数", "NJ", "NJ", "", "") },
                { "Next", new Entry("Next Steps", "下一步", "Next", "下一步", "", "") },
                { "NM", new Entry("Normalize Method", "标准化方法", "NM", "NM", "", "") },
                { "Path", new Entry("Package Root", "包路径", "Path", "路径", "", "") },
                { "PI", new Entry("Plugin Installation", "插件安装", "PI", "安装", "", "") },
                { "安装", new Entry("Plugin Installation", "插件安装", "PI", "安装", "", "") },
                { "PN", new Entry("Plugin Name", "插件名", "PN", "名称", "", "") },
                { "名称", new Entry("Plugin Name", "插件名", "PN", "名称", "", "") },
                { "Prob", new Entry("Probabilities", "概率", "Prob", "概率", "", "") },
                { "Py", new Entry("Python", "Python路径", "Py", "Py", "", "") },
                { "PR", new Entry("Python Requirements", "Python依赖", "PR", "依赖", "", "") },
                { "依赖", new Entry("Python Requirements", "Python依赖", "PR", "依赖", "", "") },
                { "RS", new Entry("Random State", "随机种子", "RS", "种子", "", "") },
                { "种子", new Entry("Random State", "随机种子", "RS", "种子", "", "") },
                { "RO", new Entry("Remove Outliers", "移除异常", "RO", "RO", "", "") },
                { "Rep", new Entry("Report", "报告", "Rep", "报告", "", "") },
                { "HC", new Entry("Run Health Check", "运行体检", "HC", "体检", "", "") },
                { "体检", new Entry("Run Health Check", "运行体检", "HC", "体检", "", "") },
                { "SFL", new Entry("Show Fit Line", "显示拟合线", "SFL", "拟合线", "", "") },
                { "Sum", new Entry("Summary", "摘要", "Sum", "摘要", "", "") },
                { "TestDS", new Entry("Test Dataset", "测试数据集", "TestDS", "测试", "", "") },
                { "测试", new Entry("Test Dataset", "测试数据集", "TestDS", "测试", "", "") },
                { "TS", new Entry("Test Size", "测试比例", "TS", "比例", "", "") },
                { "比例", new Entry("Test Size", "测试比例", "TS", "比例", "", "") },
                { "TrainDS", new Entry("Train Dataset", "训练数据集", "TrainDS", "训练", "", "") },
                { "训练", new Entry("Train Dataset", "训练数据集", "TrainDS", "训练", "", "") },
                { "VS", new Entry("Var Smoothing", "方差平滑", "VS", "VS", "", "") },
                { "WS", new Entry("Warm Start", "热启动", "WS", "WS", "", "") },
                { "XN", new Entry("X Names", "特征名", "XN", "XN", "", "") },
                { "YP", new Entry("Y Pred", "预测值", "YP", "预测", "", "") },
                { "YT", new Entry("Y True", "真实值", "YT", "真值", "", "") },
                { "真值", new Entry("Y True", "真实值", "YT", "真值", "", "") },
                { "yN", new Entry("y Names", "标签名", "yN", "yN", "", "") },
                { "Train", new Entry("Train Dataset", "训练集", "Train", "训练", "", "") },
                { "Test", new Entry("Test Dataset", "测试集", "Test", "测试", "", "") },
                { "训练集", new Entry("Train Dataset", "训练集", "Train", "训练", "", "") },
                { "测试集", new Entry("Test Dataset", "测试集", "Test", "测试", "", "") },

            };

        public static bool TryGet(
            string key,
            out string enName, out string zhName,
            out string enNick, out string zhNick,
            out string enDesc, out string zhDesc)
        {
            if (!string.IsNullOrEmpty(key) && Map.TryGetValue(key, out Entry e))
            {
                enName = e.EnName; zhName = e.ZhName;
                enNick = e.EnNick; zhNick = e.ZhNick;
                enDesc = e.EnDesc; zhDesc = e.ZhDesc;
                return true;
            }
            enName = zhName = enNick = zhNick = enDesc = zhDesc = null;
            return false;
        }
    }
}
