using System;
using System.IO;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.ModelTraining
{
    public class TrainNaiveBayesClassifierComponent : GH_Component
    {
        public TrainNaiveBayesClassifierComponent()
          : base("朴素贝叶斯分类参数 Naive Bayes", "朴素贝叶斯",
              "训练朴素贝叶斯分类器",
              "SimpleML", "05 Algorithm")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Type", "T", "贝叶斯类型，默认'GaussianNB'（可选：GaussianNB, MultinomialNB, BernoulliNB, ComplementNB, CategoricalNB）", GH_ParamAccess.item, "GaussianNB");
            pManager.AddNumberParameter("Var Smoothing", "VS", "方差平滑参数，默认1e-9（仅用于GaussianNB）", GH_ParamAccess.item, 0.000000001);
            pManager.AddNumberParameter("Alpha", "A", "平滑参数，默认1.0（用于MultinomialNB和BernoulliNB）", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("Fit Prior", "FP", "是否学习类别先验概率，默认True（用于MultinomialNB和BernoulliNB）", GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Algorithm Params", "AP", "算法参数配置（JSON格式），连接到Train Classifier的Algorithm输入", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "算法说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string type = "GaussianNB";
            double varSmoothing = 0.000000001;
            double alpha = 1.0;
            bool fitPrior = true;

            DA.GetData(0, ref type);
            DA.GetData(1, ref varSmoothing);
            DA.GetData(2, ref alpha);
            DA.GetData(3, ref fitPrior);

            try
            {
                // 构建算法参数JSON字符串
                System.Text.StringBuilder jsonBuilder = new System.Text.StringBuilder();
                jsonBuilder.Append("{");
                jsonBuilder.Append($@"""algorithm"": ""naive_bayes""");
                jsonBuilder.Append($@", ""type"": ""{type.Replace("\"", "\\\"")}""");

                if (type == "GaussianNB" && Math.Abs(varSmoothing - 0.000000001) > 0.0000000001)
                    jsonBuilder.Append($@", ""var_smoothing"": {varSmoothing.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

                if ((type == "MultinomialNB" || type == "BernoulliNB") && Math.Abs(alpha - 1.0) > 0.001)
                    jsonBuilder.Append($@", ""alpha"": {alpha.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

                if ((type == "MultinomialNB" || type == "BernoulliNB") && !fitPrior)
                    jsonBuilder.Append(@", ""fit_prior"": false");

                jsonBuilder.Append("}");
                string algorithmParamsJson = jsonBuilder.ToString();
                
                DA.SetData(0, algorithmParamsJson);
                
                // Readme输出
                string readme = GetReadmeText();
                DA.SetData(1, readme);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"执行失败: {ex.Message}");
                RhinoApp.WriteLine($"SimpleML错误: {ex}");
            }
        }

        private string GetReadmeText()
        {
            return @"组件名称: Train Naive Bayes Classifier
功能: 配置朴素贝叶斯分类器参数（不进行实际训练）

═══════════════════════════════════════════════════════════════
重要说明:
═══════════════════════════════════════════════════════════════

⚠️ 此组件只负责配置算法参数，不进行实际训练！
实际训练需要将 Algorithm Params 输出连接到 Train Classifier 组件的 Algorithm 输入。

工作流程:
1. 使用本组件配置朴素贝叶斯分类器参数
2. 将 Algorithm Params 输出连接到 Train Classifier 的 Algorithm 输入
3. 将 Dataset 连接到 Train Classifier 的 Dataset 输入
4. Train Classifier 完成实际训练并输出模型

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. Type (贝叶斯类型) - Text类型，默认'GaussianNB'
   • 可选值: 'GaussianNB', 'MultinomialNB', 'BernoulliNB', 'ComplementNB', 'CategoricalNB'
   • 说明: 不同类型的朴素贝叶斯分类器
   • 建议:
     - 'GaussianNB': 高斯朴素贝叶斯（默认，连续特征，最常用）
     - 'MultinomialNB': 多项式朴素贝叶斯（计数数据，如文本分类）
     - 'BernoulliNB': 伯努利朴素贝叶斯（二值特征）
     - 'ComplementNB': 补充朴素贝叶斯（不平衡文本数据）
     - 'CategoricalNB': 分类朴素贝叶斯（分类特征）
   • 注意: 根据特征类型选择合适的类型

2. Var Smoothing (方差平滑参数) - Float类型，默认1e-9
   • 取值范围: 1e-10 到 1e-6 之间的正数
   • 说明: 方差平滑参数（仅用于GaussianNB）
   • 建议: 大多数情况下使用默认值1e-9
   • 注意: 仅当Type='GaussianNB'时有效

3. Alpha (平滑参数) - Float类型，默认1.0
   • 取值范围: 0.1 到 10.0 之间的正数
   • 说明: 拉普拉斯平滑参数（用于MultinomialNB和BernoulliNB）
   • 建议:
     - 1.0: 默认值，适合大多数情况
     - 0.1-0.5: 弱平滑（大数据集）
     - 1.0-5.0: 强平滑（小数据集，防止过拟合）
   • 注意: 仅当Type='MultinomialNB'或'BernoulliNB'时有效

4. Fit Prior (学习先验概率) - Boolean类型，默认True
   • 取值范围: True / False
   • 说明: 是否学习类别先验概率
   • 建议: 大多数情况下使用True（默认）
   • 注意: 仅当Type='MultinomialNB'或'BernoulliNB'时有效

5. Class Prior (类别先验概率) - Text类型，默认None
   • 可选值: None 或数组字符串
   • 说明: 类别的先验概率（如果提供，将覆盖Fit Prior）
   • 建议: 大多数情况下使用None（自动学习）
   • 注意: 高级用法，通常不需要设置

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Algorithm Params (算法参数) - Text/JSON类型，必需
   • 数据类型: JSON格式字符串
   • 内容: 包含算法标识符和所有配置参数的字典
   • 格式示例: {""algorithm"": ""naive_bayes"", ""type"": ""GaussianNB""}
   • 连接建议:
     → Train Classifier的Algorithm输入（必需）
   • 说明: 这是实际训练所需的参数配置

2. Readme (算法说明) - Text类型
   • 内容: 算法说明、参数配置、预期结果等信息
   • 用途: 了解算法特性和参数设置

═══════════════════════════════════════════════════════════════
典型工作流程:
═══════════════════════════════════════════════════════════════

完整训练流程:
Create Dataset → Split Dataset
  → Train Dataset → Train Classifier (Dataset输入)
  → Train Naive Bayes Classifier → Algorithm Params → Train Classifier (Algorithm输入)
  → Train Classifier → Model

文本分类流程:
Create Dataset (文本特征) → Train Naive Bayes Classifier (Type='MultinomialNB') → Algorithm Params → Train Classifier

═══════════════════════════════════════════════════════════════
算法特性:
═══════════════════════════════════════════════════════════════

适用场景:
• 文本分类（MultinomialNB）
• 小数据集
• 需要快速训练和预测
• 特征相互独立（朴素假设）

优点:
• 训练速度快
• 预测速度快
• 对小数据集效果好
• 可以处理多分类问题
• 对缺失值不敏感

缺点:
• 假设特征相互独立（朴素假设，可能不现实）
• 可能欠拟合（如果特征相关）
• 对特征分布有假设（如GaussianNB假设正态分布）

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. ⚠️ 此组件不进行实际训练，只配置参数
2. 必须将 Algorithm Params 连接到 Train Classifier 才能完成训练
3. ⚠️ 根据特征类型选择合适的Type（连续特征用GaussianNB，计数数据用MultinomialNB）
4. 朴素贝叶斯假设特征相互独立，如果特征相关，效果可能较差
5. 对于文本分类，MultinomialNB是很好的选择
6. 训练和预测速度都很快，适合实时应用";
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(TrainNaiveBayesClassifierComponent));
        public override Guid ComponentGuid => new Guid("B6C7D8E9-F0A1-2345-9012-345678901235");
    }
}
