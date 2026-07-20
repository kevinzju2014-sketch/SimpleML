using System;
using System.IO;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

using SimpleML.Localization;
namespace SimpleML.Components.ModelTraining
{
    public class TrainDecisionTreeClassifierComponent : GH_Component
    {
        public TrainDecisionTreeClassifierComponent()
          : base(L.Name("TrainDecisionTreeClassifierComponent"), L.Nick("TrainDecisionTreeClassifierComponent"), L.Desc("TrainDecisionTreeClassifierComponent"),
              "SimpleML", "05 Algorithm")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.quarternary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Criterion", "C", "划分标准，默认'gini'（可选：gini, entropy, log_loss）", GH_ParamAccess.item, "gini");
            pManager.AddIntegerParameter("Max Depth", "MD", "最大深度，默认None（0表示None）", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Min Samples Split", "MSS", "最小分割样本数，默认2", GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("Min Samples Leaf", "MSL", "最小叶子样本数，默认1", GH_ParamAccess.item, 1);
            pManager.AddTextParameter("Max Features", "MF", "最大特征数，默认None（可选：None, sqrt, log2或整数）", GH_ParamAccess.item, "None");
            pManager.AddIntegerParameter("Random State", "RS", "随机种子，默认None（0表示None）", GH_ParamAccess.item, 0);
            pManager.AddTextParameter("Class Weight", "CW", "类别权重，默认None（可选：None, balanced）", GH_ParamAccess.item, "None");
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Algorithm Params", "AP", "算法参数配置（JSON格式），连接到Train Classifier的Algorithm输入", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "算法说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string criterion = "gini";
            int maxDepth = 0;
            int minSamplesSplit = 2;
            int minSamplesLeaf = 1;
            string maxFeatures = "None";
            int randomState = 0;
            string classWeight = "None";

            DA.GetData(0, ref criterion);
            DA.GetData(1, ref maxDepth);
            DA.GetData(2, ref minSamplesSplit);
            DA.GetData(3, ref minSamplesLeaf);
            DA.GetData(4, ref maxFeatures);
            DA.GetData(5, ref randomState);
            DA.GetData(6, ref classWeight);

            try
            {
                // 构建算法参数JSON字符串
                System.Text.StringBuilder jsonBuilder = new System.Text.StringBuilder();
                jsonBuilder.Append("{");
                jsonBuilder.Append($@"""algorithm"": ""decision_tree""");

                if (criterion != "gini")
                    jsonBuilder.Append($@", ""criterion"": ""{criterion.Replace("\"", "\\\"")}""");

                if (maxDepth > 0)
                    jsonBuilder.Append($@", ""max_depth"": {maxDepth}");

                if (minSamplesSplit != 2)
                    jsonBuilder.Append($@", ""min_samples_split"": {minSamplesSplit}");

                if (minSamplesLeaf != 1)
                    jsonBuilder.Append($@", ""min_samples_leaf"": {minSamplesLeaf}");

                if (maxFeatures.ToLower() != "none")
                {
                    if (maxFeatures.ToLower() == "sqrt" || maxFeatures.ToLower() == "log2")
                        jsonBuilder.Append($@", ""max_features"": ""{maxFeatures.Replace("\"", "\\\"")}""");
                    else if (int.TryParse(maxFeatures, out int maxFeatInt))
                        jsonBuilder.Append($@", ""max_features"": {maxFeatInt}");
                    else
                        jsonBuilder.Append($@", ""max_features"": null");
                }

                if (randomState > 0)
                    jsonBuilder.Append($@", ""random_state"": {randomState}");

                if (classWeight.ToLower() != "none")
                {
                    string cwValue = classWeight.ToLower().Replace("\"", "\\\"");
                    jsonBuilder.Append($@", ""class_weight"": ""{cwValue}""");
                }

                jsonBuilder.Append("}");
                string algorithmParamsJson = jsonBuilder.ToString();
                
                DA.SetData(0, algorithmParamsJson);
                
                // Readme输出
                string readme = GetReadmeText();
                DA.SetData(1, readme);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, L.T("err.exec_failed", ex.Message));
                RhinoApp.WriteLine($"SimpleML错误: {ex}");
            }
        }

        private string GetReadmeText()
        {
            return @"组件名称: Train Decision Tree Classifier

功能: 配置决策树分类器参数（不进行实际训练）

═══════════════════════════════════════════════════════════════
重要说明:
═══════════════════════════════════════════════════════════════

⚠️ 此组件只负责配置算法参数，不进行实际训练！
实际训练需要将 Algorithm Params 输出连接到 Train Classifier 组件的 Algorithm 输入。

工作流程:
1. 使用本组件配置决策树分类器参数
2. 将 Algorithm Params 输出连接到 Train Classifier 的 Algorithm 输入
3. 将 Dataset 连接到 Train Classifier 的 Dataset 输入
4. Train Classifier 完成实际训练并输出模型

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. Criterion (划分标准) - Text类型，默认'gini'
功能: 配置决策树分类器参数（不进行实际训练）

═══════════════════════════════════════════════════════════════
重要说明:
═══════════════════════════════════════════════════════════════

⚠️ 此组件只负责配置算法参数，不进行实际训练！
实际训练需要将 Algorithm Params 输出连接到 Train Classifier 组件的 Algorithm 输入。

工作流程:
1. 使用本组件配置决策树分类器参数
2. 将 Algorithm Params 输出连接到 Train Classifier 的 Algorithm 输入
3. 将 Dataset 连接到 Train Classifier 的 Dataset 输入
4. Train Classifier 完成实际训练并输出模型

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. Criterion (划分标准) - Text类型，默认'gini'
   • 可选值: 'gini', 'entropy', 'log_loss'
   • 说明: 用于衡量分割质量的函数
   • 建议:
     - 'gini': 基尼不纯度（默认，计算速度快，推荐）
     - 'entropy': 信息增益（可能在某些情况下效果更好）
     - 'log_loss': 对数损失（与entropy类似）
   • 注意: 大多数情况下使用'gini'即可

2. Max Depth (最大深度) - Integer类型，默认None（0表示None）
   • 取值范围: 0（表示None/无限制）或正整数
   • 说明: 树的最大深度，控制过拟合
   • 建议:
     - 0/None: 不限制深度（可能过拟合）
     - 3-5: 浅树，防止过拟合（小数据集）
     - 5-10: 中等深度（中等数据集，最常用）
     - 10-20: 深树（大数据集）
   • 注意: 深度越大，模型越复杂，可能过拟合

3. Min Samples Split (最小分割样本数) - Integer类型，默认2
   • 取值范围: 2 到 样本数 之间的正整数
   • 说明: 内部节点再划分所需的最小样本数
   • 建议:
     - 2: 默认值，适合大多数情况
     - 5-10: 防止过拟合（小数据集）
     - 20-50: 强正则化（大数据集）
   • 注意: 值越大，树越简单，可能欠拟合

4. Min Samples Leaf (最小叶子样本数) - Integer类型，默认1
   • 取值范围: 1 到 样本数 之间的正整数
   • 说明: 叶子节点所需的最小样本数
   • 建议:
     - 1: 默认值，适合大多数情况
     - 2-5: 防止过拟合（小数据集）
     - 10-20: 强正则化（大数据集）
   • 注意: 值越大，树越简单

5. Min Weight Fraction Leaf (最小叶子权重比例) - Float类型，默认0.0
   • 取值范围: 0.0 到 0.5 之间的小数
   • 说明: 叶子节点所需的最小权重比例
   • 建议: 大多数情况下使用默认值0.0

6. Max Features (最大特征数) - Text类型，默认None
   • 可选值: 'sqrt', 'log2', 'None' 或整数
   • 说明: 每次分割时考虑的最大特征数
   • 建议:
     - None: 使用所有特征（默认）
     - 'sqrt': 平方根（推荐，增加随机性）
     - 'log2': 对数
     - 整数: 指定具体数量
   • 注意: 这是随机性的来源之一

7. Random State (随机种子) - Integer类型，默认None
   • 取值范围: 任意整数，或None
   • 说明: 控制随机性，确保结果可重复
   • 建议: 调试时使用固定值（如42）

8. Max Leaf Nodes (最大叶子节点数) - Integer类型，默认None
   • 取值范围: 正整数，或None
   • 说明: 限制叶子节点的最大数量
   • 建议: 大多数情况下使用None，或与Max Depth配合使用

9. Class Weight (类别权重) - Text类型，默认None
   • 可选值: None, 'balanced'
   • 说明: 处理类别不平衡问题
   • 建议:
     - None: 类别平衡时使用
     - 'balanced': 自动平衡类别权重（推荐用于不平衡数据）

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Algorithm Params (算法参数) - Text/JSON类型，必需
   • 数据类型: JSON格式字符串
   • 内容: 包含算法标识符和所有配置参数的字典
   • 格式示例: {""algorithm"": ""decision_tree"", ""max_depth"": 10}
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
  → Train Decision Tree Classifier → Algorithm Params → Train Classifier (Algorithm输入)
  → Train Classifier → Model

防止过拟合:
Train Decision Tree Classifier (Max Depth=5, Min Samples Split=10) → Algorithm Params → Train Classifier

═══════════════════════════════════════════════════════════════
算法特性:
═══════════════════════════════════════════════════════════════

适用场景:
• 需要可解释的模型
• 非线性分类问题
• 特征重要性分析
• 快速训练和预测

优点:
• 易于理解和解释（可视化决策过程）
• 不需要特征缩放
• 可以处理非线性关系
• 可以处理混合数据类型
• 提供特征重要性

缺点:
• 容易过拟合（需要限制深度）
• 对数据变化敏感（不稳定）
• 可能产生偏向于多值特征的树
• 不适合处理连续目标变量

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. ⚠️ 此组件不进行实际训练，只配置参数
2. 必须将 Algorithm Params 连接到 Train Classifier 才能完成训练
3. ⚠️ 决策树容易过拟合，建议限制Max Depth
4. 建议同时设置Max Depth和Min Samples Split来防止过拟合
5. 不需要特征缩放（与SVM、KNN不同）
6. 对于类别不平衡数据，建议使用Class Weight='balanced'
7. 单个决策树可能不稳定，考虑使用Random Forest（多个决策树的集成）";
        }

        private string GetMyMLPath()
        {
            return PathResolver.GetMyMLPath();
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(TrainDecisionTreeClassifierComponent));
        public override Guid ComponentGuid => new Guid("A5B6C7D8-E9F0-1234-8901-234567890124");
    }
}
