using System;
using System.IO;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.ModelTraining
{
    public class TrainKNNRegressorComponent : GH_Component
    {
        public TrainKNNRegressorComponent()
          : base("K近邻回归参数 KNN Regressor", "KNN回归",
              "训练KNN回归器",
              "SimpleML", "05 Algorithm")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("N Neighbors", "N", "邻居数（k值），默认5", GH_ParamAccess.item, 5);
            pManager.AddTextParameter("Weights", "W", "权重函数，默认'uniform'（可选：uniform, distance）", GH_ParamAccess.item, "uniform");
            pManager.AddTextParameter("Algorithm", "A", "最近邻算法，默认'auto'（可选：auto, ball_tree, kd_tree, brute）", GH_ParamAccess.item, "auto");
            pManager.AddIntegerParameter("Leaf Size", "LS", "叶子节点大小，默认30", GH_ParamAccess.item, 30);
            pManager.AddIntegerParameter("P", "P", "闵可夫斯基距离幂参数，默认2（1=曼哈顿，2=欧氏）", GH_ParamAccess.item, 2);
            pManager.AddTextParameter("Metric", "M", "距离度量，默认'minkowski'（可选：minkowski, euclidean, manhattan等）", GH_ParamAccess.item, "minkowski");
            pManager.AddIntegerParameter("N Jobs", "NJ", "并行任务数，默认None（-1=所有CPU，0=None）", GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Algorithm Params", "AP", "算法参数配置（JSON格式），连接到Train Regressor的Algorithm输入", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "算法说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int nNeighbors = 5;
            string weights = "uniform";
            string algorithm = "auto";
            int leafSize = 30;
            int p = 2;
            string metric = "minkowski";
            int nJobs = 0;

            DA.GetData(0, ref nNeighbors);
            DA.GetData(1, ref weights);
            DA.GetData(2, ref algorithm);
            DA.GetData(3, ref leafSize);
            DA.GetData(4, ref p);
            DA.GetData(5, ref metric);
            DA.GetData(6, ref nJobs);

            try
            {
                // 构建算法参数JSON字符串
                System.Text.StringBuilder jsonBuilder = new System.Text.StringBuilder();
                jsonBuilder.Append("{");
                jsonBuilder.Append($@"""algorithm"": ""knn_regressor""");
                jsonBuilder.Append($@", ""n_neighbors"": {nNeighbors}");

                if (weights != "uniform")
                    jsonBuilder.Append($@", ""weights"": ""{weights.Replace("\"", "\\\"")}""");

                if (algorithm != "auto")
                    jsonBuilder.Append($@", ""algorithm_type"": ""{algorithm.Replace("\"", "\\\"")}""");

                if (leafSize != 30)
                    jsonBuilder.Append($@", ""leaf_size"": {leafSize}");

                if (p != 2)
                    jsonBuilder.Append($@", ""p"": {p}");

                if (metric != "minkowski")
                    jsonBuilder.Append($@", ""metric"": ""{metric.Replace("\"", "\\\"")}""");

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
            return @"组件名称: Train KNN Regressor
功能: 配置KNN（K近邻）回归器参数（不进行实际训练）

═══════════════════════════════════════════════════════════════
重要说明:
═══════════════════════════════════════════════════════════════

⚠️ 此组件只负责配置算法参数，不进行实际训练！
实际训练需要将 Algorithm Params 输出连接到 Train Regressor 组件的 Algorithm 输入。

工作流程:
1. 使用本组件配置KNN回归器参数
2. 将 Algorithm Params 输出连接到 Train Regressor 的 Algorithm 输入
3. 将 Dataset 连接到 Train Regressor 的 Dataset 输入
4. Train Regressor 完成实际训练并输出模型

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. N Neighbors (邻居数) - Integer类型，默认5
   • 取值范围: 1 到 样本数 之间的正整数
   • 说明: 用于预测的最近邻样本数量（k值）
   • 建议:
     - 3-5: 小数据集（<100样本）
     - 5-10: 中等数据集（100-1000样本，最常用）
     - 10-20: 大数据集（>1000样本）
   • 注意: k值太小，容易过拟合；k值太大，可能欠拟合

2. Weights (权重函数) - Text类型，默认'uniform'
   • 可选值: 'uniform', 'distance'
   • 说明: 邻居样本的权重计算方式
   • 建议:
     - 'uniform': 所有邻居权重相等（默认，简单快速）
     - 'distance': 距离越近权重越大（通常效果更好）
   • 注意: 'distance'通常能获得更好的回归效果

3. Algorithm (最近邻算法) - Text类型，默认'auto'
   • 可选值: 'auto', 'ball_tree', 'kd_tree', 'brute'
   • 说明: 计算最近邻的算法
   • 建议: 大多数情况下使用'auto'（默认，推荐）

4. Leaf Size (叶子节点大小) - Integer类型，默认30
   • 取值范围: 1 到 样本数 之间的正整数
   • 说明: 用于ball_tree和kd_tree的叶子节点大小
   • 建议: 大多数情况下使用默认值30

5. P (闵可夫斯基距离幂参数) - Integer类型，默认2
   • 取值范围: 1 或 2
   • 说明: 闵可夫斯基距离的幂参数
   • 建议:
     - 2: 欧氏距离（默认，最常用）
     - 1: 曼哈顿距离

6. Metric (距离度量) - Text类型，默认'minkowski'
   • 可选值: 'minkowski', 'euclidean', 'manhattan', 'chebyshev'等
   • 说明: 用于计算距离的度量方法
   • 建议: 大多数情况下使用默认值'minkowski'

7. N Jobs (并行任务数) - Integer类型，默认None
   • 取值范围: -1（使用所有CPU）或正整数
   • 说明: 用于计算的并行任务数
   • 建议:
     - None: 使用单核（默认）
     - -1: 使用所有可用CPU核心（大数据集时）

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Algorithm Params (算法参数) - Text/JSON类型，必需
   • 数据类型: JSON格式字符串
   • 内容: 包含算法标识符和所有配置参数的字典
   • 格式示例: {""algorithm"": ""knn"", ""n_neighbors"": 5}
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
  → Train KNN Regressor → Algorithm Params → Train Regressor (Algorithm输入)
  → Train Regressor → Model

═══════════════════════════════════════════════════════════════
算法特性:
═══════════════════════════════════════════════════════════════

适用场景:
• 非线性回归问题
• 数据具有局部结构
• 需要简单直观的模型
• 小到中等规模数据集

优点:
• 算法简单，易于理解
• 不需要训练过程（惰性学习）
• 可以处理非线性问题
• 对异常值相对鲁棒

缺点:
• 预测速度慢（需要计算所有距离）
• 对特征缩放敏感（建议先标准化）
• 内存占用大（需要存储所有训练数据）
• 高维数据效果差（维度灾难）

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. ⚠️ 此组件不进行实际训练，只配置参数
2. 必须将 Algorithm Params 连接到 Train Regressor 才能完成训练
3. ⚠️ KNN对特征缩放敏感，建议在Create Dataset中启用Normalize
4. N Neighbors是最重要的参数，需要仔细选择
5. 大数据集预测时间较长，考虑使用更快的算法
6. 高维数据（>50维）效果可能较差";
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(TrainKNNRegressorComponent));
        public override Guid ComponentGuid => new Guid("B2C3D4E5-F6A7-8901-5678-901234567891");
    }
}
