using System;
using System.IO;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.ModelTraining
{
    public class TrainLassoRegressionComponent : GH_Component
    {
        public TrainLassoRegressionComponent()
          : base("Lasso Regression", "Lasso",
              "训练Lasso回归器",
              "SimpleML", "05 Algorithm")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Alpha", "A", "正则化强度，默认1.0", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("Fit Intercept", "FI", "是否拟合截距，默认True", GH_ParamAccess.item, true);
            pManager.AddIntegerParameter("Max Iter", "MI", "最大迭代次数，默认1000", GH_ParamAccess.item, 1000);
            pManager.AddNumberParameter("Tol", "T", "停止训练的容差，默认1e-4", GH_ParamAccess.item, 0.0001);
            pManager.AddTextParameter("Selection", "S", "变量选择策略，默认'cyclic'（可选：cyclic, random）", GH_ParamAccess.item, "cyclic");
            pManager.AddIntegerParameter("Random State", "RS", "随机种子，默认None（0表示None，用于random选择）", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("Warm Start", "WS", "是否使用热启动，默认False", GH_ParamAccess.item, false);
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
            int maxIter = 1000;
            double tol = 0.0001;
            string selection = "cyclic";
            int randomState = 0;
            bool warmStart = false;

            DA.GetData(0, ref alpha);
            DA.GetData(1, ref fitIntercept);
            DA.GetData(2, ref maxIter);
            DA.GetData(3, ref tol);
            DA.GetData(4, ref selection);
            DA.GetData(5, ref randomState);
            DA.GetData(6, ref warmStart);

            try
            {
                // 构建算法参数JSON字符串
                System.Text.StringBuilder jsonBuilder = new System.Text.StringBuilder();
                jsonBuilder.Append("{");
                jsonBuilder.Append($@"""algorithm"": ""lasso_regression""");

                if (Math.Abs(alpha - 1.0) > 0.001)
                    jsonBuilder.Append($@", ""alpha"": {alpha.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

                if (!fitIntercept)
                    jsonBuilder.Append(@", ""fit_intercept"": false");

                if (maxIter != 1000)
                    jsonBuilder.Append($@", ""max_iter"": {maxIter}");

                if (Math.Abs(tol - 0.0001) > 0.00001)
                    jsonBuilder.Append($@", ""tol"": {tol.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

                if (selection != "cyclic")
                    jsonBuilder.Append($@", ""selection"": ""{selection.Replace("\"", "\\\"")}""");

                if (randomState > 0)
                    jsonBuilder.Append($@", ""random_state"": {randomState}");

                if (warmStart)
                    jsonBuilder.Append(@", ""warm_start"": true");

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
            return @"组件名称: Train Lasso Regression
功能: 配置Lasso回归器参数（不进行实际训练）

═══════════════════════════════════════════════════════════════
重要说明:
═══════════════════════════════════════════════════════════════

⚠️ 此组件只负责配置算法参数，不进行实际训练！
实际训练需要将 Algorithm Params 输出连接到 Train Regressor 组件的 Algorithm 输入。

工作流程:
1. 使用本组件配置Lasso回归器参数
2. 将 Algorithm Params 输出连接到 Train Regressor 的 Algorithm 输入
3. 将 Dataset 连接到 Train Regressor 的 Dataset 输入
4. Train Regressor 完成实际训练并输出模型

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. Alpha (正则化强度) - Float类型，默认1.0
   • 取值范围: 0.001 到 100.0 之间的正数
   • 说明: L1正则化的强度（控制过拟合和特征选择）
   • 建议:
     - 0.01-0.1: 弱正则化（保留更多特征）
     - 0.1-1.0: 中等正则化（默认1.0，适合大多数情况）
     - 1.0-10.0: 强正则化（特征选择，稀疏模型）
     - 10.0-100.0: 极强正则化（只保留最重要的特征）
   • 注意: 
     - Alpha越大，正则化越强，更多特征系数变为0（特征选择）
     - Lasso可以进行特征选择，将不重要特征的系数设为0

2. Fit Intercept (拟合截距) - Boolean类型，默认True
   • 取值范围: True / False
   • 说明: 是否计算截距项
   • 建议: 大多数情况下使用True（默认）

3. Max Iter (最大迭代次数) - Integer类型，默认1000
   • 取值范围: 100 到 10000 之间的正整数
   • 说明: 优化算法的最大迭代次数
   • 建议:
     - 1000: 默认值，适合大多数情况
     - 2000-5000: 复杂问题或未收敛时
   • 注意: 如果未收敛，可以增加此值

4. Tol (容差) - Float类型，默认1e-4
   • 取值范围: 1e-6 到 1e-2 之间的正数
   • 说明: 停止训练的容差
   • 建议: 大多数情况下使用默认值1e-4

5. Selection (变量选择策略) - Text类型，默认'cyclic'
   • 可选值: 'cyclic', 'random'
   • 说明: 每次迭代更新特征的策略
   • 建议:
     - 'cyclic': 循环更新（默认，推荐）
     - 'random': 随机更新（可能更快收敛）
   • 注意: 'random'需要设置Random State

6. Random State (随机种子) - Integer类型，默认None
   • 取值范围: 任意整数，或None
   • 说明: 控制随机性（仅用于selection='random'）
   • 建议: 调试时使用固定值（如42）

7. Warm Start (热启动) - Boolean类型，默认False
   • 取值范围: True / False
   • 说明: 是否使用前一次的结果作为初始值
   • 建议: 大多数情况下使用False（默认）

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Algorithm Params (算法参数) - Text/JSON类型，必需
   • 数据类型: JSON格式字符串
   • 内容: 包含算法标识符和所有配置参数的字典
   • 格式示例: {""algorithm"": ""lasso"", ""alpha"": 1.0}
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
  → Train Lasso Regression → Algorithm Params → Train Regressor (Algorithm输入)
  → Train Regressor → Model

特征选择流程:
Train Lasso Regression (Alpha=10.0) → Algorithm Params → Train Regressor
(使用强正则化进行特征选择，不重要特征的系数会变为0)

═══════════════════════════════════════════════════════════════
算法特性:
═══════════════════════════════════════════════════════════════

适用场景:
• 需要特征选择（自动选择重要特征）
• 特征数量很多，需要稀疏模型
• 处理多重共线性
• 需要防止过拟合

优点:
• 可以进行特征选择（L1正则化）
• 产生稀疏模型（许多系数为0）
• 防止过拟合
• 处理多重共线性
• 模型更简单，易于解释

缺点:
• 可能过度选择特征（只保留一个相关特征）
• 需要调优Alpha参数
• 对异常值敏感
• 假设线性关系

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. ⚠️ 此组件不进行实际训练，只配置参数
2. 必须将 Algorithm Params 连接到 Train Regressor 才能完成训练
3. Alpha是最重要的参数，控制特征选择的强度
4. 建议在Create Dataset中标准化特征
5. Lasso可以进行特征选择，将不重要特征的系数设为0
6. 如果多个特征高度相关，Lasso可能只选择其中一个
7. 如果需要保留所有相关特征，考虑使用Ridge回归
8. 对于特征选择任务，Lasso比Ridge更合适";
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(TrainLassoRegressionComponent));
        public override Guid ComponentGuid => new Guid("A1B2C3D4-E5F6-7890-4567-890123456780");
    }
}
