using System;
using System.IO;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.ModelTraining
{
    public class TrainRandomForestClassifierComponent : GH_Component
    {
        public TrainRandomForestClassifierComponent()
          : base("Random Forest Classifier", "RF",
              "训练随机森林分类器",
              "SimpleML", "05 Algorithm")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("N Estimators", "N", "树的数量，默认100", GH_ParamAccess.item, 100);
            pManager.AddIntegerParameter("Max Depth", "D", "最大深度，默认None（0表示None）", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Min Samples Split", "MSS", "最小分割样本数，默认2", GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("Min Samples Leaf", "MSL", "最小叶子样本数，默认1", GH_ParamAccess.item, 1);
            pManager.AddTextParameter("Max Features", "MF", "最大特征数，默认'sqrt'（可选：'sqrt', 'log2', 'None'或整数）", GH_ParamAccess.item, "sqrt");
            pManager.AddBooleanParameter("Bootstrap", "B", "是否使用Bootstrap采样，默认True", GH_ParamAccess.item, true);
            pManager.AddIntegerParameter("Random State", "RS", "随机种子，默认None（0表示None）", GH_ParamAccess.item, 0);
            pManager.AddTextParameter("Class Weight", "CW", "类别权重，默认None（可选：None, 'balanced', 'balanced_subsample'）", GH_ParamAccess.item, "None");
            pManager.AddTextParameter("Criterion", "C", "划分标准，默认'gini'（可选：'gini', 'entropy', 'log_loss'）", GH_ParamAccess.item, "gini");
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Algorithm Params", "AP", "算法参数配置（JSON格式），连接到Train Classifier的Algorithm输入", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "算法说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int nEstimators = 100;
            int maxDepth = 0;
            int minSamplesSplit = 2;
            int minSamplesLeaf = 1;
            string maxFeatures = "sqrt";
            bool bootstrap = true;
            int randomState = 0;
            string classWeight = "None";
            string criterion = "gini";

            DA.GetData(0, ref nEstimators);
            DA.GetData(1, ref maxDepth);
            DA.GetData(2, ref minSamplesSplit);
            DA.GetData(3, ref minSamplesLeaf);
            DA.GetData(4, ref maxFeatures);
            DA.GetData(5, ref bootstrap);
            DA.GetData(6, ref randomState);
            DA.GetData(7, ref classWeight);
            DA.GetData(8, ref criterion);

            try
            {
                // 构建算法参数JSON字符串
                System.Text.StringBuilder jsonBuilder = new System.Text.StringBuilder();
                jsonBuilder.Append("{");
                jsonBuilder.Append($@"""algorithm"": ""random_forest""");
                jsonBuilder.Append($@", ""n_estimators"": {nEstimators}");

                // 添加可选参数
                if (maxDepth > 0)
                    jsonBuilder.Append($@", ""max_depth"": {maxDepth}");

                if (minSamplesSplit != 2)
                    jsonBuilder.Append($@", ""min_samples_split"": {minSamplesSplit}");

                if (minSamplesLeaf != 1)
                    jsonBuilder.Append($@", ""min_samples_leaf"": {minSamplesLeaf}");

                if (maxFeatures != "sqrt")
                {
                    if (maxFeatures.ToLower() == "none")
                        jsonBuilder.Append(@", ""max_features"": null");
                    else if (int.TryParse(maxFeatures, out int maxFeatInt))
                        jsonBuilder.Append($@", ""max_features"": {maxFeatInt}");
                    else
                        jsonBuilder.Append($@", ""max_features"": ""{maxFeatures.Replace("\"", "\\\"")}""");
                }

                if (!bootstrap)
                    jsonBuilder.Append(@", ""bootstrap"": false");

                if (randomState > 0)
                    jsonBuilder.Append($@", ""random_state"": {randomState}");

                if (classWeight.ToLower() != "none")
                {
                    string cwValue = classWeight.ToLower().Replace("\"", "\\\"");
                    jsonBuilder.Append($@", ""class_weight"": ""{cwValue}""");
                }

                if (criterion != "gini")
                {
                    string critValue = criterion.Replace("\"", "\\\"");
                    jsonBuilder.Append($@", ""criterion"": ""{critValue}""");
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
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"执行失败: {ex.Message}");
                RhinoApp.WriteLine($"SimpleML错误: {ex}");
            }
        }

        private string GetMyMLPath()
        {
            string envPath = Environment.GetEnvironmentVariable("SIMPLEML_PATH");
            if (!string.IsNullOrEmpty(envPath) && Directory.Exists(envPath))
                return envPath;

            string defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Grasshopper", "UserObjects", "SimpleML", "myML");
            if (Directory.Exists(defaultPath))
                return defaultPath;

            string ghaPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string ghaDir = Path.GetDirectoryName(ghaPath);
            string relativePath = Path.Combine(ghaDir, "myML");
            if (Directory.Exists(relativePath))
                return relativePath;

            return null;
        }

        private string GetReadmeText()
        {
            return @"组件名称: Train Random Forest Classifier
功能: 配置随机森林分类器参数（不进行实际训练）

═══════════════════════════════════════════════════════════════
重要说明:
═══════════════════════════════════════════════════════════════

⚠️ 此组件只负责配置算法参数，不进行实际训练！
实际训练需要将 Algorithm Params 输出连接到 Train Classifier 组件的 Algorithm 输入。

工作流程:
1. 使用本组件配置随机森林分类器参数
2. 将 Algorithm Params 输出连接到 Train Classifier 的 Algorithm 输入
3. 将 Dataset 连接到 Train Classifier 的 Dataset 输入
4. Train Classifier 完成实际训练并输出模型

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. N Estimators (树的数量) - Integer类型，默认100
   • 取值范围: 1 到 1000 之间的正整数
   • 说明: 随机森林中决策树的数量
   • 建议:
     - 小数据集（<1000样本）: 50-100
     - 中等数据集（1000-10000样本）: 100-200
     - 大数据集（>10000样本）: 200-500
   • 注意: 树越多，训练时间越长，但通常效果更好

2. Max Depth (最大深度) - Integer类型，默认None（0表示None）
   • 取值范围: 0（表示None/无限制）或正整数
   • 说明: 树的最大深度，控制过拟合
   • 建议:
     - 0/None: 不限制深度（默认，适合大多数情况）
     - 3-10: 限制深度，防止过拟合（小数据集）
     - 10-20: 中等深度（中等数据集）
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

5. Max Features (最大特征数) - Text类型，默认'sqrt'
   • 可选值: 'sqrt', 'log2', 'None' 或整数
   • 说明: 每次分割时考虑的最大特征数
   • 建议:
     - 'sqrt': 平方根（默认，推荐）
     - 'log2': 对数
     - 'None': 使用所有特征
     - 整数: 指定具体数量
   • 注意: 这是随机性的来源之一

6. Bootstrap (Bootstrap采样) - Boolean类型，默认True
   • 取值范围: True / False
   • 说明: 是否使用bootstrap采样
   • 建议: 大多数情况下使用True（默认）

7. Random State (随机种子) - Integer类型，默认None（0表示None）
   • 取值范围: 0（表示None）或正整数
   • 说明: 控制随机性，确保结果可重复
   • 建议: 调试时使用固定值（如42），生产环境可使用0/None

8. Class Weight (类别权重) - Text类型，默认None
   • 可选值: None, 'balanced', 'balanced_subsample'
   • 说明: 处理类别不平衡问题
   • 建议:
     - None: 类别平衡时使用
     - 'balanced': 自动平衡类别权重（推荐用于不平衡数据）
     - 'balanced_subsample': 在每个bootstrap样本中平衡权重

9. Criterion (划分标准) - Text类型，默认'gini'
   • 可选值: 'gini', 'entropy', 'log_loss'
   • 说明: 用于衡量分割质量的函数
   • 建议:
     - 'gini': 默认，计算速度快
     - 'entropy': 信息增益，可能在某些情况下效果更好
     - 'log_loss': 对数损失

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Algorithm Params (算法参数) - Text/JSON类型，必需
   • 数据类型: JSON格式字符串
   • 内容: 包含算法标识符和所有配置参数的字典
   • 格式示例: {""algorithm"": ""random_forest"", ""n_estimators"": 100, ""max_depth"": 10}
   • 连接建议:
     → Train Classifier的Algorithm输入（必需）
   • 说明: 这是实际训练所需的参数配置

═══════════════════════════════════════════════════════════════
典型工作流程:
═══════════════════════════════════════════════════════════════

完整训练流程:
Create Dataset → Split Dataset
  → Train Dataset → Train Classifier (Dataset输入)
  → Train Random Forest Classifier → Algorithm Params → Train Classifier (Algorithm输入)
  → Train Classifier → Model

参数调优流程:
Train Random Forest Classifier (参数A) → Algorithm Params A → Train Classifier → Model A
Train Random Forest Classifier (参数B) → Algorithm Params B → Train Classifier → Model B
(比较 Model A 和 Model B 的性能)

═══════════════════════════════════════════════════════════════
算法特性:
═══════════════════════════════════════════════════════════════

适用场景:
• 非线性分类问题
• 多分类任务
• 需要特征重要性分析
• 对过拟合有较好抵抗能力

优点:
• 准确率高
• 可以处理非线性关系
• 提供特征重要性
• 对缺失值不敏感

缺点:
• 训练时间较长（树数量多时）
• 模型解释性较差
• 内存占用较大

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. ⚠️ 此组件不进行实际训练，只配置参数
2. 必须将 Algorithm Params 连接到 Train Classifier 才能完成训练
3. 参数设置需要根据数据规模和特征数量调整
4. 建议先使用默认参数，然后根据结果调优
5. 对于类别不平衡数据，建议使用 Class Weight='balanced'
6. 树数量越多，训练时间越长，但通常效果更好
7. 限制 Max Depth 可以防止过拟合，但可能欠拟合";
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(TrainRandomForestClassifierComponent));
        public override Guid ComponentGuid => new Guid("E1F2A3B4-C5D6-7890-4567-890123456789");
    }
}
