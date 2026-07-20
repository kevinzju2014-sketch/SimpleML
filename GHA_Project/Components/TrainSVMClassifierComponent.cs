using System;
using System.IO;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

using SimpleML.Localization;
namespace SimpleML.Components.ModelTraining
{
    public class TrainSVMClassifierComponent : GH_Component
    {
        public TrainSVMClassifierComponent()
          : base(L.Name("TrainSVMClassifierComponent"), L.Nick("TrainSVMClassifierComponent"), L.Desc("TrainSVMClassifierComponent"),
              "SimpleML", "05 Algorithm")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.quarternary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Kernel", "K", "核函数类型，默认'rbf'（可选：linear, poly, sigmoid, rbf, precomputed）", GH_ParamAccess.item, "rbf");
            pManager.AddNumberParameter("C", "C", "惩罚系数，默认1.0", GH_ParamAccess.item, 1.0);
            pManager.AddTextParameter("Gamma", "G", "核函数系数，默认'scale'（可选：scale, auto或浮点数）", GH_ParamAccess.item, "scale");
            pManager.AddIntegerParameter("Degree", "D", "多项式核的度数，默认3（仅用于poly核）", GH_ParamAccess.item, 3);
            pManager.AddNumberParameter("Coef0", "C0", "核函数中的独立项，默认0.0（用于poly和sigmoid核）", GH_ParamAccess.item, 0.0);
            pManager.AddBooleanParameter("Probability", "P", "是否启用概率估计，默认False", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Random State", "RS", "随机种子，默认None（0表示None）", GH_ParamAccess.item, 0);
            pManager.AddTextParameter("Class Weight", "CW", "类别权重，默认None（可选：None, 'balanced'）", GH_ParamAccess.item, "None");
            pManager.AddNumberParameter("Tol", "T", "停止训练的容差，默认1e-3", GH_ParamAccess.item, 0.001);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Algorithm Params", "AP", "算法参数配置（JSON格式），连接到Train Classifier的Algorithm输入", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "算法说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string kernel = "rbf";
            double c = 1.0;
            string gamma = "scale";
            int degree = 3;
            double coef0 = 0.0;
            bool probability = false;
            int randomState = 0;
            string classWeight = "None";
            double tol = 0.001;

            DA.GetData(0, ref kernel);
            DA.GetData(1, ref c);
            DA.GetData(2, ref gamma);
            DA.GetData(3, ref degree);
            DA.GetData(4, ref coef0);
            DA.GetData(5, ref probability);
            DA.GetData(6, ref randomState);
            DA.GetData(7, ref classWeight);
            DA.GetData(8, ref tol);

            try
            {
                // 构建算法参数JSON字符串
                System.Text.StringBuilder jsonBuilder = new System.Text.StringBuilder();
                jsonBuilder.Append("{");
                jsonBuilder.Append($@"""algorithm"": ""svm""");
                jsonBuilder.Append($@", ""kernel"": ""{kernel.Replace("\"", "\\\"")}""");
                jsonBuilder.Append($@", ""C"": {c.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

                // 处理gamma（可能是字符串或数字）
                if (gamma.ToLower() == "scale" || gamma.ToLower() == "auto")
                    jsonBuilder.Append($@", ""gamma"": ""{gamma.Replace("\"", "\\\"")}""");
                else if (double.TryParse(gamma, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double gammaVal))
                    jsonBuilder.Append($@", ""gamma"": {gammaVal.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                else
                    jsonBuilder.Append($@", ""gamma"": ""{gamma.Replace("\"", "\\\"")}""");

                if (degree != 3)
                    jsonBuilder.Append($@", ""degree"": {degree}");

                if (coef0 != 0.0)
                    jsonBuilder.Append($@", ""coef0"": {coef0.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

                if (probability)
                    jsonBuilder.Append(@", ""probability"": true");

                if (randomState > 0)
                    jsonBuilder.Append($@", ""random_state"": {randomState}");

                if (classWeight.ToLower() != "none")
                {
                    string cwValue = classWeight.ToLower().Replace("\"", "\\\"");
                    jsonBuilder.Append($@", ""class_weight"": ""{cwValue}""");
                }

                if (Math.Abs(tol - 0.001) > 0.0001)
                    jsonBuilder.Append($@", ""tol"": {tol.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

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
            return @"组件名称: Train SVM Classifier
功能: 配置SVM（支持向量机）分类器参数（不进行实际训练）

═══════════════════════════════════════════════════════════════
重要说明:
═══════════════════════════════════════════════════════════════

⚠️ 此组件只负责配置算法参数，不进行实际训练！
实际训练需要将 Algorithm Params 输出连接到 Train Classifier 组件的 Algorithm 输入。

工作流程:
1. 使用本组件配置SVM分类器参数
2. 将 Algorithm Params 输出连接到 Train Classifier 的 Algorithm 输入
3. 将 Dataset 连接到 Train Classifier 的 Dataset 输入
4. Train Classifier 完成实际训练并输出模型

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. Kernel (核函数类型) - Text类型，默认'rbf'
   • 可选值: 'linear', 'poly', 'sigmoid', 'rbf', 'precomputed'
   • 说明:
     - 'rbf': 径向基函数（默认，最常用，适合非线性问题）
     - 'linear': 线性核（适合线性可分问题，速度快）
     - 'poly': 多项式核（适合非线性问题，可调degree）
     - 'sigmoid': Sigmoid核（类似神经网络，较少使用）
     - 'precomputed': 预计算核矩阵（高级用法）
   • 建议:
     - 大多数情况: 'rbf'（默认）
     - 线性可分数据: 'linear'（更快）
     - 需要多项式特征: 'poly'

2. C (惩罚系数) - Float类型，默认1.0
   • 取值范围: 0.01 到 1000.0 之间的正数
   • 说明: 控制对误分类的惩罚程度
   • 建议:
     - 0.1-1.0: 软间隔，允许更多误分类（默认1.0）
     - 1.0-10.0: 中等惩罚（常用范围）
     - 10.0-100.0: 强惩罚，更严格的分类边界
   • 注意: C越大，模型越复杂，可能过拟合

3. Gamma (核函数系数) - Text类型，默认'scale'
   • 可选值: 'scale', 'auto' 或浮点数（如0.001, 0.01, 0.1, 1.0）
   • 说明: 控制单个样本的影响范围（仅用于rbf、poly、sigmoid核）
   • 建议:
     - 'scale': 自动计算（默认，推荐）
     - 'auto': 使用1/n_features
     - 小值（0.001-0.01）: 大影响范围，平滑决策边界
     - 大值（0.1-1.0）: 小影响范围，复杂决策边界
   • 注意: Gamma越大，模型越复杂，可能过拟合

4. Degree (多项式度数) - Integer类型，默认3
   • 取值范围: 1 到 10 之间的正整数
   • 说明: 多项式核的度数（仅用于poly核）
   • 建议:
     - 3: 默认值，适合大多数情况
     - 2-4: 低度数，简单模型
     - 5-7: 高度数，复杂模型
   • 注意: 仅当Kernel='poly'时有效

5. Coef0 (独立项) - Float类型，默认0.0
   • 取值范围: 任意浮点数
   • 说明: 核函数中的独立项（仅用于poly和sigmoid核）
   • 建议: 大多数情况下使用默认值0.0

6. Probability (概率估计) - Boolean类型，默认False
   • 取值范围: True / False
   • 说明: 是否启用概率估计（predict_proba方法）
   • 建议:
     - False: 不需要概率时（默认，更快）
     - True: 需要概率输出时（如Predict Classifier的Probabilities输出）

7. Random State (随机种子) - Integer类型，默认None
   • 取值范围: 任意整数，或None
   • 说明: 控制随机性，确保结果可重复
   • 建议: 调试时使用固定值（如42）

8. Class Weight (类别权重) - Text类型，默认None
   • 可选值: None, 'balanced'
   • 说明: 处理类别不平衡问题
   • 建议:
     - None: 类别平衡时使用
     - 'balanced': 自动平衡类别权重（推荐用于不平衡数据）

9. Tol (容差) - Float类型，默认1e-3
   • 取值范围: 1e-5 到 1e-1 之间的正数
   • 说明: 停止训练的容差
   • 建议: 大多数情况下使用默认值1e-3

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Algorithm Params (算法参数) - Text/JSON类型，必需
   • 数据类型: JSON格式字符串
   • 内容: 包含算法标识符和所有配置参数的字典
   • 格式示例: {""algorithm"": ""svm"", ""kernel"": ""rbf"", ""C"": 1.0}
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
  → Train SVM Classifier → Algorithm Params → Train Classifier (Algorithm输入)
  → Train Classifier → Model

参数调优流程:
Train SVM Classifier (C=1.0) → Algorithm Params A → Train Classifier → Model A
Train SVM Classifier (C=10.0) → Algorithm Params B → Train Classifier → Model B
(比较 Model A 和 Model B 的性能)

═══════════════════════════════════════════════════════════════
算法特性:
═══════════════════════════════════════════════════════════════

适用场景:
• 小到中等规模数据集（<10000样本）
• 非线性分类问题（使用rbf核）
• 高维特征空间
• 需要清晰决策边界

优点:
• 在高维空间中表现优秀
• 内存效率高（只使用支持向量）
• 可以使用不同的核函数处理非线性问题
• 对过拟合有较好的抵抗能力

缺点:
• 大数据集训练时间较长
• 对特征缩放敏感（建议先标准化）
• 参数调优较复杂（C和gamma）
• 难以解释（黑盒模型）

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. ⚠️ 此组件不进行实际训练，只配置参数
2. 必须将 Algorithm Params 连接到 Train Classifier 才能完成训练
3. ⚠️ SVM对特征缩放敏感，建议在Create Dataset中启用Normalize
4. 大数据集（>10000样本）训练时间可能很长
5. C和gamma是重要参数，需要仔细调优
6. 使用rbf核时，建议先使用默认参数，然后根据结果调优
7. 对于线性可分数据，使用linear核会更快";
        }

        private string GetMyMLPath()
        {
            return PathResolver.GetMyMLPath();
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(TrainSVMClassifierComponent));
        public override Guid ComponentGuid => new Guid("A9B0C1D2-E3F4-5678-2345-678901234568");
    }
}
