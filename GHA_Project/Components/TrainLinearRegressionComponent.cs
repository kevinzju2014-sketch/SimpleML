using System;
using System.IO;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.ModelTraining
{
    public class TrainLinearRegressionComponent : GH_Component
    {
        public TrainLinearRegressionComponent()
          : base("线性回归参数 Linear Regression", "线性回归",
              "训练线性回归器",
              "SimpleML", "05 Algorithm")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Fit Intercept", "FI", "是否拟合截距，默认True", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Normalize", "N", "是否标准化，默认False（已弃用，建议在Create Dataset中处理）", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Copy X", "CX", "是否复制X，默认True", GH_ParamAccess.item, true);
            pManager.AddIntegerParameter("N Jobs", "NJ", "并行任务数，默认None（-1=所有CPU，0=None）", GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Algorithm Params", "AP", "算法参数配置（JSON格式），连接到Train Regressor的Algorithm输入", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "算法说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool fitIntercept = true;
            bool normalize = false;
            bool copyX = true;
            int nJobs = 0;

            DA.GetData(0, ref fitIntercept);
            DA.GetData(1, ref normalize);
            DA.GetData(2, ref copyX);
            DA.GetData(3, ref nJobs);

            try
            {
                // 构建算法参数JSON字符串
                System.Text.StringBuilder jsonBuilder = new System.Text.StringBuilder();
                jsonBuilder.Append("{");
                jsonBuilder.Append($@"""algorithm"": ""linear_regression""");

                if (!fitIntercept)
                    jsonBuilder.Append(@", ""fit_intercept"": false");

                if (normalize)
                    jsonBuilder.Append(@", ""normalize"": true");

                if (!copyX)
                    jsonBuilder.Append(@", ""copy_X"": false");

                if (nJobs != 0)
                {
                    if (nJobs == -1)
                        jsonBuilder.Append(@", ""n_jobs"": -1");
                    else
                        jsonBuilder.Append($@", ""n_jobs"": {nJobs}");
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

        private string GetReadmeText()
        {
            return @"组件名称: Train Linear Regression
功能: 配置线性回归器参数（不进行实际训练）

═══════════════════════════════════════════════════════════════
重要说明:
═══════════════════════════════════════════════════════════════

⚠️ 此组件只负责配置算法参数，不进行实际训练！
实际训练需要将 Algorithm Params 输出连接到 Train Regressor 组件的 Algorithm 输入。

工作流程:
1. 使用本组件配置线性回归器参数
2. 将 Algorithm Params 输出连接到 Train Regressor 的 Algorithm 输入
3. 将 Dataset 连接到 Train Regressor 的 Dataset 输入
4. Train Regressor 完成实际训练并输出模型

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. Fit Intercept (拟合截距) - Boolean类型，默认True
   • 取值范围: True / False
   • 说明: 是否计算截距项（y = ax + b中的b）
   • 建议:
     - True: 默认，适合大多数情况（推荐）
     - False: 数据已经中心化时使用
   • 注意: 大多数情况下应该使用True

2. Normalize (标准化) - Boolean类型，默认False
   • 取值范围: True / False
   • 说明: 是否标准化特征（已弃用，建议在Create Dataset中处理）
   • 建议: 使用False，在Create Dataset组件中启用Normalize
   • 注意: 此参数已弃用，建议在数据预处理阶段标准化

3. Copy X (复制X) - Boolean类型，默认True
   • 取值范围: True / False
   • 说明: 是否复制特征数据
   • 建议: 大多数情况下使用默认值True

4. N Jobs (并行任务数) - Integer类型，默认None
   • 取值范围: -1（使用所有CPU）或正整数
   • 说明: 用于计算的并行任务数
   • 建议:
     - None: 使用单核（默认）
     - -1: 使用所有可用CPU核心（大数据集时）
     - 正整数: 指定CPU核心数
   • 注意: 线性回归通常计算很快，并行化收益有限

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Algorithm Params (算法参数) - Text/JSON类型，必需
   • 数据类型: JSON格式字符串
   • 内容: 包含算法标识符和所有配置参数的字典
   • 格式示例: {""algorithm"": ""linear_regression"", ""fit_intercept"": true}
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
  → Train Linear Regression → Algorithm Params → Train Regressor (Algorithm输入)
  → Train Regressor → Model

简单快速训练:
Create Dataset → Split Dataset
  → Train Dataset → Train Regressor (Algorithm='linear_regression')
  → Train Regressor → Model

═══════════════════════════════════════════════════════════════
算法特性:
═══════════════════════════════════════════════════════════════

适用场景:
• 特征与目标变量之间存在线性关系
• 需要可解释的模型
• 快速训练和预测
• 作为基准模型

优点:
• 算法简单，易于理解和解释
• 训练速度快
• 不需要调优参数
• 对过拟合有较好的抵抗能力（简单模型）
• 可以提供特征系数（特征重要性）

缺点:
• 只能处理线性关系
• 对异常值敏感
• 假设特征之间相互独立
• 可能欠拟合（如果关系是非线性的）

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. ⚠️ 此组件不进行实际训练，只配置参数
2. 必须将 Algorithm Params 连接到 Train Regressor 才能完成训练
3. 线性回归假设特征与目标变量之间存在线性关系
4. 如果关系是非线性的，考虑使用多项式特征或其他算法
5. 建议在Create Dataset中标准化特征，而不是使用此组件的Normalize参数
6. 对异常值敏感，建议在Create Dataset中启用Remove Outliers
7. 如果特征之间存在多重共线性，考虑使用Ridge或Lasso回归";
        }

        private string GetMyMLPath()
        {
            return PathResolver.GetMyMLPath();
        }


        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(TrainLinearRegressionComponent));
        public override Guid ComponentGuid => new Guid("E9F0A1B2-C3D4-5678-2345-678901234568");
    }
}
