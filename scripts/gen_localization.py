# -*- coding: utf-8 -*-
"""Generate localization catalog + patch component constructors."""
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / "GHA_Project"
COMP = ROOT / "Components"
LOC = ROOT / "Localization"
LOC.mkdir(exist_ok=True)

META = {
    "AboutComponent": ("About", "About", "Show SimpleML author and version information"),
    "BeginnerWizardComponent": ("Beginner Wizard", "Wizard", "Beginner recipes for classification / regression / clustering, with optional health check"),
    "CalculateCorrelationComponent": ("Calculate Correlation", "Correlation", "Compute a correlation matrix for the input data"),
    "CalculateStatisticsComponent": ("Calculate Statistics", "Statistics", "Compute statistics (mean, std, min, max, etc.)"),
    "CancelPythonSessionComponent": ("Reset Python", "Reset", "Terminate and reset the persistent SimpleML Python process"),
    "CreateDatasetComponent": ("Create Dataset", "CreateDS", "Create a Dataset object that wraps features and labels"),
    "DeconstructDatasetComponent": ("Deconstruct Dataset", "Deconstruct", "Deconstruct a Dataset into X and y"),
    "DescribeFeaturesComponent": ("Describe Features", "Describe", "Describe features (count, mean, std, min, 25%, 50%, 75%, max)"),
    "ElbowMethodComponent": ("Elbow Method", "Elbow", "Use the elbow method to choose a good cluster count K"),
    "EvaluateAutoComponent": ("Evaluate", "Evaluate", "Unified evaluation with auto task detection and a Verdict sentence"),
    "EvaluateClassificationComponent": ("Evaluate Classification", "Eval Clf", "Evaluate a classification model"),
    "EvaluateClusteringComponent": ("Evaluate Clustering", "Eval Clus", "Evaluate a clustering model"),
    "EvaluateRegressionComponent": ("Evaluate Regression", "Eval Reg", "Evaluate a regression model"),
    "FeatureImportanceComponent": ("Feature Importance", "Importance", "Explain relative feature contribution (feature_importances_ or |coef_|)"),
    "HealthCheckComponent": ("Health Check", "Health", "Check the SimpleML environment; optional AutoFix pip install"),
    "InstallationGuideComponent": ("Installation Guide", "Install", "Cross-platform install guide for Rhino 7+ (Windows / macOS)"),
    "LoadDatasetComponent": ("Load Dataset", "LoadDS", "Load a scikit-learn sample dataset and output a Dataset"),
    "LoadModelComponent": ("Load Model", "Load", "Load a saved model from disk"),
    "PredictAutoComponent": ("Predict", "Predict", "Unified prediction with auto model-type detection; accepts Dataset or X"),
    "PredictClassifierComponent": ("Predict Classifier", "Pred Clf", "Predict with a trained classification model"),
    "PredictClusterComponent": ("Predict Cluster", "Pred Clus", "Assign cluster labels with a trained clustering model"),
    "PredictRegressorComponent": ("Predict Regressor", "Pred Reg", "Predict with a trained regression model"),
    "QuickClusterColorComponent": ("Quick Cluster Color", "Color", "Color points from a clustering model or labels for quick demos"),
    "QuickCreateDatasetComponent": ("Quick Dataset", "QuickDS", "Simple mode: X and optional y. Use Create Dataset for advanced prep"),
    "ReadCSVComponent": ("Read CSV", "CSV In", "Read a CSV file and return data, column names, and shape"),
    "ReadExcelComponent": ("Read Excel", "Excel In", "Read an Excel file and return data, column names, and shape"),
    "ReduceDimensionsComponent": ("Reduce Dimensions", "PCA", "Reduce high-dimensional data to 2D/3D for visualization"),
    "SaveModelComponent": ("Save Model", "Save", "Save a trained model to disk"),
    "SilhouetteScoreComponent": ("Silhouette Score", "Silhouette", "Score clustering compactness and separation"),
    "SmartTrainComponent": ("Smart Train", "Smart", "One-click train: Dataset → Model, with next steps and model card"),
    "SplitDatasetComponent": ("Split Dataset", "Split", "Split a Dataset into train and test sets"),
    "TrainAgglomerativeClusteringComponent": ("Agglomerative Params", "Agglomerative", "Agglomerative clustering algorithm parameters"),
    "TrainClassifierComponent": ("Train Classifier", "Train Clf", "Train a classifier (generic)"),
    "TrainClusterComponent": ("Train Cluster", "Train Clus", "Train a clustering model (generic)"),
    "TrainDBSCANComponent": ("DBSCAN Params", "DBSCAN", "DBSCAN clustering algorithm parameters"),
    "TrainDecisionTreeClassifierComponent": ("Decision Tree Params", "Decision Tree", "Decision tree classifier parameters"),
    "TrainKMeansComponent": ("K-Means Params", "K-Means", "K-Means clustering algorithm parameters"),
    "TrainKNNClassifierComponent": ("KNN Classifier Params", "KNN Clf", "KNN classifier parameters"),
    "TrainKNNRegressorComponent": ("KNN Regressor Params", "KNN Reg", "KNN regressor parameters"),
    "TrainLassoRegressionComponent": ("Lasso Params", "Lasso", "Lasso regression parameters"),
    "TrainLinearRegressionComponent": ("Linear Regression Params", "Linear", "Linear regression parameters"),
    "TrainLogisticRegressionClassifierComponent": ("Logistic Regression Params", "Logistic", "Logistic regression classifier parameters"),
    "TrainNaiveBayesClassifierComponent": ("Naive Bayes Params", "Naive Bayes", "Naive Bayes classifier parameters"),
    "TrainRandomForestClassifierComponent": ("Random Forest Clf Params", "RF Clf", "Random forest classifier parameters"),
    "TrainRandomForestRegressorComponent": ("Random Forest Reg Params", "RF Reg", "Random forest regressor parameters"),
    "TrainRegressorComponent": ("Train Regressor", "Train Reg", "Train a regressor (generic)"),
    "TrainRidgeRegressionComponent": ("Ridge Params", "Ridge", "Ridge regression parameters"),
    "TrainSVMClassifierComponent": ("SVM Classifier Params", "SVM", "SVM classifier parameters"),
    "TrainSVRComponent": ("SVR Params", "SVR", "Support vector regressor parameters"),
    "VisualizeClassificationLabelsComponent": ("Visualize Classification", "Viz Clf", "Color geometry by classification labels"),
    "VisualizeClusterLabelsComponent": ("Visualize Clustering", "Viz Clus", "Color geometry by cluster labels"),
    "VisualizeRegressionComponent": ("Visualize Regression", "Viz Reg", "Scatter predicted vs actual values with a fit line"),
    "WriteCSVComponent": ("Write CSV", "CSV Out", "Write data to a CSV file"),
    "WriteExcelComponent": ("Write Excel", "Excel Out", "Write data to an Excel file"),
    "LanguageSwitchComponent": ("Language", "Lang", "Switch English / Chinese. Use Boolean Chinese=true/false, or right-click this component."),
}

ZH = {
    "AboutComponent": ("关于", "关于", "显示 SimpleML 插件的作者信息和版本信息"),
    "BeginnerWizardComponent": ("新手向导", "向导", "输出分类/回归/聚类完整接线配方，并可选运行体检"),
    "CalculateCorrelationComponent": ("计算相关性", "相关性", "计算数据的相关性矩阵"),
    "CalculateStatisticsComponent": ("计算统计", "统计", "计算数据的统计信息（均值、标准差、最小值、最大值等）"),
    "CancelPythonSessionComponent": ("重置 Python 会话", "重置会话", "终止并重置 SimpleML 常驻 Python 进程。训练卡住或超时后可使用"),
    "CreateDatasetComponent": ("创建数据集", "创建数据集", "创建数据集对象，封装特征数据和标签"),
    "DeconstructDatasetComponent": ("解构数据集", "解构数据集", "解构数据集对象，提取 X 和 y"),
    "DescribeFeaturesComponent": ("描述特征", "描述特征", "描述特征（count, mean, std, min, 25%, 50%, 75%, max）"),
    "ElbowMethodComponent": ("肘部法则", "肘部法则", "使用 Elbow 方法确定最佳聚类数"),
    "EvaluateAutoComponent": ("评估", "评估", "统一评估：自动识别分类/回归/聚类，输出结论句 Verdict"),
    "EvaluateClassificationComponent": ("评估分类", "评估分类", "评估分类模型"),
    "EvaluateClusteringComponent": ("评估聚类", "评估聚类", "评估聚类模型"),
    "EvaluateRegressionComponent": ("评估回归", "评估回归", "评估回归模型"),
    "FeatureImportanceComponent": ("特征重要性", "特征重要性", "解释模型中各特征的相对贡献（feature_importances_ 或 |coef_|）"),
    "HealthCheckComponent": ("环境体检", "体检", "检查 SimpleML 运行环境；失败时给出下一步。可选 AutoFix 自动 pip 安装依赖"),
    "InstallationGuideComponent": ("安装指南", "安装指南", "SimpleML 跨版本安装指南（Rhino 7 及以上，Windows 与 macOS）"),
    "LoadDatasetComponent": ("加载示例数据集", "加载数据集", "加载 scikit-learn 示例数据，并直接输出 Dataset（可接智能训练）"),
    "LoadModelComponent": ("加载模型", "加载模型", "从磁盘加载已保存的模型"),
    "PredictAutoComponent": ("预测", "预测", "统一预测：自动识别模型类型。可接 Dataset 或 X"),
    "PredictClassifierComponent": ("预测分类", "预测分类", "使用训练好的分类模型进行预测"),
    "PredictClusterComponent": ("预测聚类", "预测聚类", "使用训练好的聚类模型进行预测（分配聚类标签）"),
    "PredictRegressorComponent": ("预测回归", "预测回归", "使用训练好的回归模型进行预测"),
    "QuickClusterColorComponent": ("一键聚类上色", "聚类上色", "输入点与聚类模型（或标签），输出着色点。适合快速演示"),
    "QuickCreateDatasetComponent": ("快速创建数据集", "快速数据集", "简洁模式：只需 X 与可选 y，适合新手。高级预处理请用「创建数据集」"),
    "ReadCSVComponent": ("读取 CSV", "读取 CSV", "读取 CSV 文件并返回数据、列名和形状"),
    "ReadExcelComponent": ("读取 Excel", "读取 Excel", "读取 Excel 文件并返回数据、列名和形状"),
    "ReduceDimensionsComponent": ("降维", "降维", "将高维数据降维到 2D/3D 并可视化"),
    "SaveModelComponent": ("保存模型", "保存模型", "将模型保存到磁盘"),
    "SilhouetteScoreComponent": ("轮廓系数", "轮廓系数", "计算聚类轮廓系数，评价簇的紧凑度与分离度"),
    "SmartTrainComponent": ("智能训练", "智能训练", "一键训练：Dataset → Model。自动识别任务并给出下一步与模型卡片"),
    "SplitDatasetComponent": ("分割数据集", "分割数据集", "分割数据集为训练集和测试集"),
    "TrainAgglomerativeClusteringComponent": ("层次聚类参数", "层次聚类", "层次聚类算法参数"),
    "TrainClassifierComponent": ("训练分类器", "训练分类器", "训练分类器（通用）"),
    "TrainClusterComponent": ("训练聚类", "训练聚类", "训练聚类模型（通用）"),
    "TrainDBSCANComponent": ("DBSCAN 参数", "DBSCAN", "DBSCAN 聚类算法参数"),
    "TrainDecisionTreeClassifierComponent": ("决策树参数", "决策树", "决策树分类器参数"),
    "TrainKMeansComponent": ("K-Means 参数", "K-Means", "K-Means 聚类算法参数"),
    "TrainKNNClassifierComponent": ("KNN 分类参数", "KNN 分类", "KNN 分类器参数"),
    "TrainKNNRegressorComponent": ("KNN 回归参数", "KNN 回归", "KNN 回归器参数"),
    "TrainLassoRegressionComponent": ("Lasso 参数", "Lasso", "Lasso 回归参数"),
    "TrainLinearRegressionComponent": ("线性回归参数", "线性回归", "线性回归参数"),
    "TrainLogisticRegressionClassifierComponent": ("逻辑回归参数", "逻辑回归", "逻辑回归分类器参数"),
    "TrainNaiveBayesClassifierComponent": ("朴素贝叶斯参数", "朴素贝叶斯", "朴素贝叶斯分类器参数"),
    "TrainRandomForestClassifierComponent": ("随机森林分类参数", "随机森林分类", "随机森林分类器参数"),
    "TrainRandomForestRegressorComponent": ("随机森林回归参数", "随机森林回归", "随机森林回归器参数"),
    "TrainRegressorComponent": ("训练回归器", "训练回归器", "训练回归器（通用）"),
    "TrainRidgeRegressionComponent": ("岭回归参数", "岭回归", "岭回归参数"),
    "TrainSVMClassifierComponent": ("SVM 分类参数", "SVM 分类", "SVM 分类器参数"),
    "TrainSVRComponent": ("SVR 参数", "SVR", "支持向量回归参数"),
    "VisualizeClassificationLabelsComponent": ("可视化分类标签", "可视化分类", "根据分类标签给几何对象着色预览"),
    "VisualizeClusterLabelsComponent": ("可视化聚类标签", "可视化聚类", "根据聚类标签给几何对象着色预览"),
    "VisualizeRegressionComponent": ("可视化回归", "可视化回归", "显示预测值 vs 实际值散点图 + 拟合线"),
    "WriteCSVComponent": ("写入 CSV", "写入 CSV", "将数据写入 CSV 文件"),
    "WriteExcelComponent": ("写入 Excel", "写入 Excel", "将数据写入 Excel 文件"),
    "LanguageSwitchComponent": ("语言", "语言", "切换中英文。用布尔值 Chinese=true/false，或右键本组件选择。"),
}

COMMON = {
    "param.readme": ("Component usage notes", "组件使用说明"),
    "param.algorithm_readme": ("Algorithm notes", "算法说明"),
    "err.need_dataset": ("Dataset is required", "必须提供 Dataset 对象"),
    "err.need_dataset_or_x": ("Provide Dataset or X", "请提供 Dataset 或 X"),
    "err.need_test_dataset": ("Test Dataset is required", "必须提供 Test Dataset 输入"),
    "err.package_missing": (
        "Package path not found. Open Installation Guide or set SIMPLEML_PATH.",
        "未找到包路径。请打开「安装指南」或设置 SIMPLEML_PATH。",
    ),
    "err.exec_failed": ("Execution failed: {0}", "执行失败: {0}"),
    "lang.switched_en": (
        "Switched to English. Canvas labels refresh automatically.",
        "已切换为英文。画布上的组件名称会自动刷新。",
    ),
    "lang.switched_zh": (
        "Switched to Chinese. Canvas labels refresh automatically.",
        "已切换为中文。画布上的组件名称会自动刷新。",
    ),
    "lang.current": ("Current language: {0}", "当前语言: {0}"),
    "plugin.desc": (
        "Grasshopper machine-learning plugin powered by scikit-learn (Rhino 7+ / Windows / macOS).",
        "基于 scikit-learn 的 Grasshopper 机器学习插件（兼容 Rhino 7 及以上、Windows 与 macOS）。",
    ),
}


def esc(s: str) -> str:
    return (
        s.replace("\\", "\\\\")
        .replace('"', '\\"')
        .replace("\r\n", "\\n")
        .replace("\n", "\\n")
        .replace("\r", "\\n")
    )


def write_core_files():
    (LOC / "UiLanguage.cs").write_text(
        r'''using System;
using System.IO;

namespace SimpleML.Localization
{
    /// <summary>
    /// UI language preference. Default: English.
    /// Ribbon names apply at Grasshopper load; runtime texts follow the current setting.
    /// </summary>
    public static class UiLanguage
    {
        public const string English = "en";
        public const string Chinese = "zh";

        private static string _cached;

        public static string PreferencePath
        {
            get
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(appData, "Grasshopper", "SimpleML", "language.txt");
            }
        }

        public static string Current
        {
            get
            {
                if (!string.IsNullOrEmpty(_cached))
                    return _cached;

                try
                {
                    string env = Environment.GetEnvironmentVariable("SIMPLEML_LANG");
                    if (!string.IsNullOrWhiteSpace(env))
                    {
                        _cached = Normalize(env);
                        return _cached;
                    }
                }
                catch { }

                try
                {
                    string path = PreferencePath;
                    if (File.Exists(path))
                    {
                        string raw = File.ReadAllText(path).Trim();
                        _cached = Normalize(raw);
                        return _cached;
                    }
                }
                catch { }

                _cached = English;
                return _cached;
            }
        }

        public static bool IsChinese => Current == Chinese;
        public static bool IsEnglish => Current == English;

        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return English;
            value = value.Trim().ToLowerInvariant();
            if (value == "zh" || value == "zh-cn" || value == "zh_cn" || value == "cn"
                || value == "chinese" || value == "中文")
                return Chinese;
            return English;
        }

        public static void Set(string language)
        {
            string lang = Normalize(language);
            _cached = lang;
            try
            {
                Environment.SetEnvironmentVariable("SIMPLEML_LANG", lang);
                string path = PreferencePath;
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(path, lang + Environment.NewLine);
            }
            catch { }
        }

        public static void Reload()
        {
            _cached = null;
            _ = Current;
        }
    }
}
''',
        encoding="utf-8",
    )

    (LOC / "L.cs").write_text(
        r'''namespace SimpleML.Localization
{
    /// <summary>
    /// Localization helper. Default language is English.
    /// </summary>
    public static class L
    {
        public static string T(string key)
        {
            if (ComponentLocales.TryGet(key, out string en, out string zh))
                return UiLanguage.IsChinese ? zh : en;
            return key;
        }

        public static string T(string key, params object[] args)
        {
            string fmt = T(key);
            try { return string.Format(fmt, args); }
            catch { return fmt; }
        }

        public static string Comp(string componentClass, string field)
        {
            return T("comp." + componentClass + "." + field);
        }

        public static string Name(string componentClass) => Comp(componentClass, "name");
        public static string Nick(string componentClass) => Comp(componentClass, "nick");
        public static string Desc(string componentClass) => Comp(componentClass, "desc");
    }
}
''',
        encoding="utf-8",
    )

    lines = [
        "using System.Collections.Generic;",
        "",
        "namespace SimpleML.Localization",
        "{",
        "    /// <summary>Auto-generated EN/ZH string table.</summary>",
        "    public static class ComponentLocales",
        "    {",
        "        private static readonly Dictionary<string, Entry> Map = new Dictionary<string, Entry>",
        "        {",
    ]
    for k, (en, zh) in COMMON.items():
        lines.append(f'            {{ "{k}", new Entry("{esc(en)}", "{esc(zh)}") }},')
    for cls, (en_n, en_k, en_d) in META.items():
        zh_n, zh_k, zh_d = ZH[cls]
        lines.append(f'            {{ "comp.{cls}.name", new Entry("{esc(en_n)}", "{esc(zh_n)}") }},')
        lines.append(f'            {{ "comp.{cls}.nick", new Entry("{esc(en_k)}", "{esc(zh_k)}") }},')
        lines.append(f'            {{ "comp.{cls}.desc", new Entry("{esc(en_d)}", "{esc(zh_d)}") }},')
    lines += [
        "        };",
        "",
        "        private struct Entry",
        "        {",
        "            public readonly string En;",
        "            public readonly string Zh;",
        "            public Entry(string en, string zh) { En = en; Zh = zh; }",
        "        }",
        "",
        "        public static bool TryGet(string key, out string en, out string zh)",
        "        {",
        "            if (Map.TryGetValue(key, out Entry e))",
        "            {",
        "                en = e.En; zh = e.Zh; return true;",
        "            }",
        "            en = null; zh = null; return false;",
        "        }",
        "    }",
        "}",
        "",
    ]
    (LOC / "ComponentLocales.cs").write_text("\n".join(lines), encoding="utf-8")
    print("Wrote Localization/*.cs")


def patch_components():
    pat = re.compile(
        r'(public\s+\w+\s*\(\s*\)\s*\r?\n\s*:\s*base\(\s*)"[^"]*"\s*,\s*"[^"]*"\s*,\s*"[^"]*"\s*,(\s*"[^"]+"\s*,\s*"[^"]+"\s*\))',
        re.M,
    )
    patched = 0
    for f in sorted(COMP.glob("*.cs")):
        text = f.read_text(encoding="utf-8")
        mcls = re.search(r"public class (\w+)", text)
        if not mcls:
            continue
        cls = mcls.group(1)
        if cls not in META or cls == "LanguageSwitchComponent":
            continue
        new_text, n = pat.subn(
            rf'\1L.Name("{cls}"), L.Nick("{cls}"), L.Desc("{cls}"),\2',
            text,
            count=1,
        )
        if not n:
            print("SKIP base()", cls)
            continue
        if "using SimpleML.Localization;" not in new_text:
            usings = list(re.finditer(r"^using .+;\s*$", new_text, re.M))
            if usings:
                last = usings[-1]
                new_text = (
                    new_text[: last.end()]
                    + "\nusing SimpleML.Localization;"
                    + new_text[last.end() :]
                )
        f.write_text(new_text, encoding="utf-8")
        patched += 1
        print("patched", cls)
    print("patched_count", patched)


if __name__ == "__main__":
    write_core_files()
    patch_components()
