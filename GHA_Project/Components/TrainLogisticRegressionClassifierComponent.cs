using System;
using System.IO;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.ModelTraining
{
    public class TrainLogisticRegressionClassifierComponent : GH_Component
    {
        public TrainLogisticRegressionClassifierComponent()
          : base("逻辑回归分类参数 Logistic Regression", "逻辑回归",
              "训练逻辑回归分类器",
              "SimpleML", "05 Algorithm")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.quarternary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Penalty", "P", "正则化类型，默认'l2'（可选：l1, l2, elasticnet, none）", GH_ParamAccess.item, "l2");
            pManager.AddNumberParameter("C", "C", "正则化强度的倒数，默认1.0", GH_ParamAccess.item, 1.0);
            pManager.AddTextParameter("Solver", "S", "求解器，默认'lbfgs'（可选：lbfgs, liblinear, newton-cg, sag, saga）", GH_ParamAccess.item, "lbfgs");
            pManager.AddNumberParameter("Max Iter", "MI", "最大迭代次数，默认100", GH_ParamAccess.item, 100);
            pManager.AddNumberParameter("Tol", "T", "停止训练的容差，默认1e-4", GH_ParamAccess.item, 0.0001);
            pManager.AddBooleanParameter("Fit Intercept", "FI", "是否拟合截距，默认True", GH_ParamAccess.item, true);
            pManager.AddTextParameter("Class Weight", "CW", "类别权重，默认None（可选：None, 'balanced'）", GH_ParamAccess.item, "None");
            pManager.AddIntegerParameter("Random State", "RS", "随机种子，默认None（0表示None）", GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Algorithm Params", "AP", "算法参数配置（JSON格式），连接到Train Classifier的Algorithm输入", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "算法说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string penalty = "l2";
            double c = 1.0;
            string solver = "lbfgs";
            double maxIter = 100;
            double tol = 0.0001;
            bool fitIntercept = true;
            string classWeight = "None";
            int randomState = 0;

            DA.GetData(0, ref penalty);
            DA.GetData(1, ref c);
            DA.GetData(2, ref solver);
            DA.GetData(3, ref maxIter);
            DA.GetData(4, ref tol);
            DA.GetData(5, ref fitIntercept);
            DA.GetData(6, ref classWeight);
            DA.GetData(7, ref randomState);

            try
            {
                // 构建算法参数JSON字符串
                System.Text.StringBuilder jsonBuilder = new System.Text.StringBuilder();
                jsonBuilder.Append("{");
                jsonBuilder.Append($@"""algorithm"": ""logistic_regression""");

                if (penalty != "l2")
                    jsonBuilder.Append($@", ""penalty"": ""{penalty.Replace("\"", "\\\"")}""");

                if (Math.Abs(c - 1.0) > 0.001)
                    jsonBuilder.Append($@", ""C"": {c.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

                if (solver != "lbfgs")
                    jsonBuilder.Append($@", ""solver"": ""{solver.Replace("\"", "\\\"")}""");

                if (maxIter != 100)
                    jsonBuilder.Append($@", ""max_iter"": {(int)maxIter}");

                if (Math.Abs(tol - 0.0001) > 0.00001)
                    jsonBuilder.Append($@", ""tol"": {tol.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

                if (!fitIntercept)
                    jsonBuilder.Append(@", ""fit_intercept"": false");

                if (classWeight.ToLower() != "none")
                {
                    string cwValue = classWeight.ToLower().Replace("\"", "\\\"");
                    jsonBuilder.Append($@", ""class_weight"": ""{cwValue}""");
                }

                if (randomState > 0)
                    jsonBuilder.Append($@", ""random_state"": {randomState}");

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
            return @"组件名称: Train Logistic Regression Classifier
功能: 配置逻辑回归分类器参数（不进行实际训练）

═══════════════════════════════════════════════════════════════
重要说明:
═══════════════════════════════════════════════════════════════

⚠️ 此组件只负责配置算法参数，不进行实际训练！
实际训练需要将 Algorithm Params 输出连接到 Train Classifier 组件的 Algorithm 输入。

工作流程:
1. 使用本组件配置逻辑回归分类器参数
2. 将 Algorithm Params 输出连接到 Train Classifier 的 Algorithm 输入
3. 将 Dataset 连接到 Train Classifier 的 Dataset 输入
4. Train Classifier 完成实际训练并输出模型

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. Penalty (正则化类型) - Text类型，默认'l2'
   • 可选值: 'l1', 'l2', 'elasticnet', 'None'
   • 说明: 用于防止过拟合的正则化方法
   • 建议:
     - 'l2': L2正则化（默认，最常用，推荐）
     - 'l1': L1正则化（特征选择，稀疏模型）
     - 'elasticnet': L1和L2的组合
     - 'None': 无正则化（可能过拟合）
   • 注意: 不同solver支持不同的penalty类型

2. C (惩罚系数倒数) - Float类型，默认1.0
   • 取值范围: 0.001 到 1000.0 之间的正数
   • 说明: 正则化强度的倒数（C越大，正则化越弱）
   • 建议:
     - 0.1-1.0: 强正则化（小数据集，防止过拟合）
     - 1.0: 默认值，适合大多数情况
     - 1.0-10.0: 弱正则化（大数据集）
   • 注意: C值越小，正则化越强，模型越简单

3. Solver (优化算法) - Text类型，默认'lbfgs'
   • 可选值: 'lbfgs', 'liblinear', 'newton-cg', 'sag', 'saga'
   • 说明: 用于优化的算法
   • 建议:
     - 'lbfgs': 默认，适合小到中等数据集（推荐）
     - 'liblinear': 小数据集，支持l1和l2
     - 'newton-cg': 中等数据集，需要大量内存
     - 'sag': 大数据集，快速收敛
     - 'saga': 大数据集，支持所有penalty类型
   • 注意: 不同solver支持不同的penalty和multi_class组合

4. Max Iter (最大迭代次数) - Integer类型，默认100
   • 取值范围: 50 到 1000 之间的正整数
   • 说明: 优化算法的最大迭代次数
   • 建议:
     - 100: 默认值，适合大多数情况
     - 200-500: 复杂问题或未收敛时
   • 注意: 如果未收敛，可以增加此值

5. Tol (容差) - Float类型，默认1e-4
   • 取值范围: 1e-6 到 1e-2 之间的正数
   • 说明: 停止训练的容差
   • 建议: 大多数情况下使用默认值1e-4

6. Multi Class (多分类策略) - Text类型，默认'auto'
   • 可选值: 'ovr', 'multinomial', 'auto'
   • 说明: 多分类问题的处理策略
   • 建议:
     - 'auto': 自动选择（默认，推荐）
     - 'ovr': 一对多（One-vs-Rest）
     - 'multinomial': 多项逻辑回归（需要特定solver）
   • 注意: 二分类问题此参数无效

7. Random State (随机种子) - Integer类型，默认None
   • 取值范围: 任意整数，或None
   • 说明: 控制随机性，确保结果可重复
   • 建议: 调试时使用固定值（如42）

8. L1 Ratio (Elastic-Net混合参数) - Float类型，默认None
   • 取值范围: 0.0 到 1.0 之间的小数
   • 说明: Elastic-Net正则化的混合参数（仅用于penalty='elasticnet'）
   • 建议:
     - 0.5: L1和L2各占一半
     - 0.0-0.5: 偏向L2
     - 0.5-1.0: 偏向L1
   • 注意: 仅当Penalty='elasticnet'时有效

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
   • 格式示例: {""algorithm"": ""logistic_regression"", ""penalty"": ""l2"", ""C"": 1.0}
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
  → Train Logistic Regression Classifier → Algorithm Params → Train Classifier (Algorithm输入)
  → Train Classifier → Model

参数调优流程:
Train Logistic Regression Classifier (C=0.1) → Algorithm Params A → Train Classifier → Model A
Train Logistic Regression Classifier (C=1.0) → Algorithm Params B → Train Classifier → Model B
Train Logistic Regression Classifier (C=10.0) → Algorithm Params C → Train Classifier → Model C
(比较不同C值的模型性能)

═══════════════════════════════════════════════════════════════
算法特性:
═══════════════════════════════════════════════════════════════

适用场景:
• 二分类和多分类问题
• 需要概率输出
• 需要可解释的模型
• 线性或近似线性关系

优点:
• 算法简单，易于理解和解释
• 训练速度快
• 提供概率输出
• 对过拟合有较好的抵抗能力（使用正则化）
• 可以处理多分类问题

缺点:
• 假设特征与目标变量之间存在线性关系
• 对异常值敏感
• 需要特征缩放（建议标准化）
• 可能欠拟合（如果关系是非线性的）

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. ⚠️ 此组件不进行实际训练，只配置参数
2. 必须将 Algorithm Params 连接到 Train Classifier 才能完成训练
3. ⚠️ 逻辑回归对特征缩放敏感，建议在Create Dataset中启用Normalize
4. C值是最重要的参数，需要仔细调优
5. 不同solver支持不同的penalty和multi_class组合，注意兼容性
6. 对于类别不平衡数据，建议使用Class Weight='balanced'
7. 如果关系是非线性的，考虑使用多项式特征或其他算法";
        }

        private string GetMyMLPath()
        {
            return PathResolver.GetMyMLPath();
        }


        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(TrainLogisticRegressionClassifierComponent));
        public override Guid ComponentGuid => new Guid("E3F4A5B6-C7D8-9012-6789-012345678902");
    }
}
