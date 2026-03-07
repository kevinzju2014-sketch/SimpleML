using System;
using System.IO;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.ModelTraining
{
    public class TrainRidgeRegressionComponent : GH_Component
    {
        public TrainRidgeRegressionComponent()
          : base("Ridge Regression", "Ridge",
              "训练岭回归器",
              "SimpleML", "05 Algorithm")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Alpha", "A", "正则化强度，默认1.0", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("Fit Intercept", "FI", "是否拟合截距，默认True", GH_ParamAccess.item, true);
            pManager.AddTextParameter("Solver", "S", "求解器，默认'auto'（可选：auto, svd, cholesky, lsqr, sparse_cg, sag, saga, lbfgs）", GH_ParamAccess.item, "auto");
            pManager.AddNumberParameter("Max Iter", "MI", "最大迭代次数，默认None（0表示None）", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Tol", "T", "停止训练的容差，默认1e-4", GH_ParamAccess.item, 0.0001);
            pManager.AddIntegerParameter("Random State", "RS", "随机种子，默认None（0表示None，用于sag和saga求解器）", GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Algorithm Params", "AP", "算法参数配置（JSON格式），连接到Train Regressor的Algorithm输入", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "算法说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double alpha = 1.0;
            bool fitIntercept = true;
            string solver = "auto";
            double maxIter = 0;
            double tol = 0.0001;
            int randomState = 0;

            DA.GetData(0, ref alpha);
            DA.GetData(1, ref fitIntercept);
            DA.GetData(2, ref solver);
            DA.GetData(3, ref maxIter);
            DA.GetData(4, ref tol);
            DA.GetData(5, ref randomState);

            try
            {
                // 构建算法参数JSON字符串
                System.Text.StringBuilder jsonBuilder = new System.Text.StringBuilder();
                jsonBuilder.Append("{");
                jsonBuilder.Append($@"""algorithm"": ""ridge_regression""");

                if (Math.Abs(alpha - 1.0) > 0.001)
                    jsonBuilder.Append($@", ""alpha"": {alpha.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

                if (!fitIntercept)
                    jsonBuilder.Append(@", ""fit_intercept"": false");

                if (solver != "auto")
                    jsonBuilder.Append($@", ""solver"": ""{solver.Replace("\"", "\\\"")}""");

                if (maxIter > 0)
                    jsonBuilder.Append($@", ""max_iter"": {(int)maxIter}");

                if (Math.Abs(tol - 0.0001) > 0.00001)
                    jsonBuilder.Append($@", ""tol"": {tol.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

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
            return @"组件名称: Train Ridge Regression
功能: 配置岭回归器参数（不进行实际训练）

═══════════════════════════════════════════════════════════════
重要说明:
═══════════════════════════════════════════════════════════════

⚠️ 此组件只负责配置算法参数，不进行实际训练！
实际训练需要将 Algorithm Params 输出连接到 Train Regressor 组件的 Algorithm 输入。

工作流程:
1. 使用本组件配置岭回归器参数
2. 将 Algorithm Params 输出连接到 Train Regressor 的 Algorithm 输入
3. 将 Dataset 连接到 Train Regressor 的 Dataset 输入
4. Train Regressor 完成实际训练并输出模型

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. Alpha (正则化强度) - Float类型，默认1.0
   • 取值范围: 0.01 到 100.0 之间的正数
   • 说明: L2正则化的强度（控制过拟合）
   • 建议:
     - 0.1-1.0: 弱正则化（大数据集，特征数量少）
     - 1.0: 默认值，适合大多数情况
     - 1.0-10.0: 中等正则化（中等数据集）
     - 10.0-100.0: 强正则化（小数据集，特征数量多，防止过拟合）
   • 注意: Alpha越大，正则化越强，模型越简单，系数越小

2. Fit Intercept (拟合截距) - Boolean类型，默认True
   • 取值范围: True / False
   • 说明: 是否计算截距项
   • 建议: 大多数情况下使用True（默认）

3. Solver (求解器) - Text类型，默认'auto'
   • 可选值: 'auto', 'svd', 'cholesky', 'lsqr', 'sparse_cg', 'sag', 'saga', 'lbfgs'
   • 说明: 用于求解的算法
   • 建议:
     - 'auto': 自动选择（默认，推荐）
     - 'svd': 奇异值分解（稳定，适合所有情况）
     - 'cholesky': Cholesky分解（快速，适合密集矩阵）
     - 'sag': 随机平均梯度（大数据集）
     - 'saga': SAG的改进版本（大数据集）
   • 注意: 不同solver适用于不同的数据规模

4. Max Iter (最大迭代次数) - Integer类型，默认None
   • 取值范围: 100 到 10000 之间的正整数，或None
   • 说明: 求解器的最大迭代次数
   • 建议:
     - None: 不限制（默认，适合大多数情况）
     - 1000-5000: 复杂问题或未收敛时
   • 注意: 仅当使用迭代求解器（如sag、saga）时有效

5. Tol (容差) - Float类型，默认1e-4
   • 取值范围: 1e-6 到 1e-2 之间的正数
   • 说明: 停止训练的容差
   • 建议: 大多数情况下使用默认值1e-4

6. Random State (随机种子) - Integer类型，默认None
   • 取值范围: 任意整数，或None
   • 说明: 控制随机性（仅用于sag和saga求解器）
   • 建议: 调试时使用固定值（如42）

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Algorithm Params (算法参数) - Text/JSON类型，必需
   • 数据类型: JSON格式字符串
   • 内容: 包含算法标识符和所有配置参数的字典
   • 格式示例: {""algorithm"": ""ridge"", ""alpha"": 1.0}
   • 连接建议:
     → Train Regressor的Algorithm输入（必需）
   • 说明: 这是实际训练所需的参数配置

2. Readme (算法说明) - Text类型
   • 内容: 算法说明、参数配置、预期结果等信息
   • 用途: 了解算法特性和参数设置

═══════════════════════════════════════════════════════════════
典型工作流程:
═══════════════════════════════════════════════════════════════

完整训练流程:
Create Dataset → Split Dataset
  → Train Dataset → Train Regressor (Dataset输入)
  → Train Ridge Regression → Algorithm Params → Train Regressor (Algorithm输入)
  → Train Regressor → Model

处理多重共线性:
Create Dataset → Train Ridge Regression (Alpha=10.0) → Algorithm Params → Train Regressor
(使用强正则化处理特征之间的多重共线性)

═══════════════════════════════════════════════════════════════
算法特性:
═══════════════════════════════════════════════════════════════

适用场景:
• 特征之间存在多重共线性
• 需要防止过拟合
• 特征数量接近或超过样本数量
• 需要稳定的回归模型

优点:
• 处理多重共线性问题
• 防止过拟合（L2正则化）
• 比普通线性回归更稳定
• 所有特征都保留（不像Lasso会特征选择）

缺点:
• 不能进行特征选择（所有特征都保留）
• 需要调优Alpha参数
• 对异常值敏感
• 假设线性关系

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. ⚠️ 此组件不进行实际训练，只配置参数
2. 必须将 Algorithm Params 连接到 Train Regressor 才能完成训练
3. Alpha是最重要的参数，需要根据数据调优
4. 建议在Create Dataset中标准化特征
5. 如果特征之间存在多重共线性，Ridge回归是很好的选择
6. 如果需要特征选择，考虑使用Lasso回归
7. Alpha值越大，系数越小，模型越简单";
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(TrainRidgeRegressionComponent));
        public override Guid ComponentGuid => new Guid("F0A1B2C3-D4E5-6789-3456-789012345679");
    }
}
