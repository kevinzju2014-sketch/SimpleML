using System;
using System.IO;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

using SimpleML.Localization;
namespace SimpleML.Components.ModelTraining
{
    public class TrainDBSCANComponent : GH_Component
    {
        public TrainDBSCANComponent()
          : base(L.Name("TrainDBSCANComponent"), L.Nick("TrainDBSCANComponent"), L.Desc("TrainDBSCANComponent"),
              "SimpleML", "05 Algorithm")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.quarternary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Eps", "E", "邻域半径，默认0.5", GH_ParamAccess.item, 0.5);
            pManager.AddIntegerParameter("Min Samples", "MS", "最小样本数，默认5", GH_ParamAccess.item, 5);
            pManager.AddTextParameter("Metric", "M", "距离度量，默认'euclidean'（可选：euclidean, manhattan, cosine, minkowski等）", GH_ParamAccess.item, "euclidean");
            pManager.AddTextParameter("Algorithm", "A", "最近邻算法，默认'auto'（可选：auto, ball_tree, kd_tree, brute）", GH_ParamAccess.item, "auto");
            pManager.AddIntegerParameter("Leaf Size", "LS", "叶子节点大小，默认30", GH_ParamAccess.item, 30);
            pManager.AddNumberParameter("P", "P", "闵可夫斯基距离幂参数，默认2（仅用于minkowski距离）", GH_ParamAccess.item, 2.0);
            pManager.AddIntegerParameter("N Jobs", "NJ", "并行任务数，默认None（-1=所有CPU，0=None）", GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Algorithm Params", "AP", "算法参数配置（JSON格式），连接到Train Cluster的Algorithm输入", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "算法说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double eps = 0.5;
            int minSamples = 5;
            string metric = "euclidean";
            string algorithm = "auto";
            int leafSize = 30;
            double p = 2.0;
            int nJobs = 0;

            DA.GetData(0, ref eps);
            DA.GetData(1, ref minSamples);
            DA.GetData(2, ref metric);
            DA.GetData(3, ref algorithm);
            DA.GetData(4, ref leafSize);
            DA.GetData(5, ref p);
            DA.GetData(6, ref nJobs);

            try
            {
                // 构建算法参数JSON字符串
                System.Text.StringBuilder jsonBuilder = new System.Text.StringBuilder();
                jsonBuilder.Append("{");
                jsonBuilder.Append($@"""algorithm"": ""dbscan""");

                if (Math.Abs(eps - 0.5) > 0.001)
                    jsonBuilder.Append($@", ""eps"": {eps.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

                if (minSamples != 5)
                    jsonBuilder.Append($@", ""min_samples"": {minSamples}");

                if (metric != "euclidean")
                    jsonBuilder.Append($@", ""metric"": ""{metric.Replace("\"", "\\\"")}""");

                if (algorithm != "auto")
                    jsonBuilder.Append($@", ""algorithm"": ""{algorithm.Replace("\"", "\\\"")}""");

                if (leafSize != 30)
                    jsonBuilder.Append($@", ""leaf_size"": {leafSize}");

                if (Math.Abs(p - 2.0) > 0.001)
                    jsonBuilder.Append($@", ""p"": {p.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

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
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, L.T("err.exec_failed", ex.Message));
                RhinoApp.WriteLine($"SimpleML错误: {ex}");
            }
        }

        private string GetReadmeText()
        {
            return @"组件名称: Train DBSCAN
功能: 配置DBSCAN聚类算法参数（不进行实际训练）

═══════════════════════════════════════════════════════════════
重要说明:
═══════════════════════════════════════════════════════════════

⚠️ 此组件只负责配置算法参数，不进行实际训练！
实际训练需要将 Algorithm Params 输出连接到 Train Cluster 组件的 Algorithm 输入。

工作流程:
1. 使用本组件配置DBSCAN聚类参数
2. 将 Algorithm Params 输出连接到 Train Cluster 的 Algorithm 输入
3. 将 Dataset 连接到 Train Cluster 的 Dataset 输入
4. Train Cluster 完成实际训练并输出模型

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. Eps (邻域半径) - Float类型，默认0.5
   • 取值范围: 0.01 到 10.0 之间的正数
   • 说明: 两个样本被认为是邻居的最大距离
   • 建议:
     - 0.1-0.5: 紧密聚类（小数据集）
     - 0.5-1.0: 中等聚类（最常用）
     - 1.0-5.0: 松散聚类（大数据集）
   • 注意: 
     - 这是DBSCAN最重要的参数
     - 需要根据数据密度和特征尺度调整
     - 建议先标准化数据，然后尝试0.5左右的值

2. Min Samples (最小样本数) - Integer类型，默认5
   • 取值范围: 2 到 样本数 之间的正整数
   • 说明: 形成一个核心点所需的最小邻居数
   • 建议:
     - 3-5: 小数据集（<100样本）
     - 5: 默认值，适合大多数情况
     - 5-10: 中等数据集（100-1000样本）
     - 10-20: 大数据集（>1000样本）
   • 注意: 
     - Min Samples越大，形成的聚类越少，噪声点越多
     - 通常设置为特征数量的2倍

3. Metric (距离度量) - Text类型，默认'euclidean'
   • 可选值: 'euclidean', 'manhattan', 'cosine', 'minkowski'等
   • 说明: 用于计算距离的度量方法
   • 建议:
     - 'euclidean': 欧氏距离（默认，最常用）
     - 'manhattan': 曼哈顿距离（高维数据）
     - 'cosine': 余弦距离（文本数据，高维数据）
   • 注意: 大多数情况下使用默认值'euclidean'即可

4. Algorithm (最近邻算法) - Text类型，默认'auto'
   • 可选值: 'auto', 'ball_tree', 'kd_tree', 'brute'
   • 说明: 计算最近邻的算法
   • 建议:
     - 'auto': 自动选择（默认，推荐）
     - 'ball_tree': 高维数据（>20维）
     - 'kd_tree': 低维数据（<20维）
     - 'brute': 小数据集
   • 注意: 大多数情况下使用'auto'即可

5. Leaf Size (叶子节点大小) - Integer类型，默认30
   • 取值范围: 1 到 样本数 之间的正整数
   • 说明: 用于ball_tree和kd_tree的叶子节点大小
   • 建议: 大多数情况下使用默认值30

6. P (闵可夫斯基距离幂参数) - Float类型，默认2
   • 取值范围: 1.0 到 10.0 之间的正数
   • 说明: 闵可夫斯基距离的幂参数（仅用于minkowski距离）
   • 建议:
     - 2.0: 欧氏距离（默认）
     - 1.0: 曼哈顿距离
   • 注意: 仅当Metric='minkowski'时有效

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
   • 格式示例: {""algorithm"": ""dbscan"", ""eps"": 0.5, ""min_samples"": 5}
   • 连接建议:
     → Train Cluster的Algorithm输入（必需）
   • 说明: 这是实际训练所需的参数配置

2. Readme (算法说明) - Text类型
   • 内容: 算法说明、参数配置、预期结果等信息
   • 用途: 了解算法特性和参数设置

═══════════════════════════════════════════════════════════════
典型工作流程:
═══════════════════════════════════════════════════════════════

完整训练流程:
Create Dataset → Train Cluster (Dataset输入)
  → Train DBSCAN → Algorithm Params → Train Cluster (Algorithm输入)
  → Train Cluster → Model

参数调优流程:
Train DBSCAN (Eps=0.3) → Algorithm Params A → Train Cluster → Evaluate Clustering
Train DBSCAN (Eps=0.5) → Algorithm Params B → Train Cluster → Evaluate Clustering
Train DBSCAN (Eps=0.7) → Algorithm Params C → Train Cluster → Evaluate Clustering
(比较不同Eps值的聚类结果)

═══════════════════════════════════════════════════════════════
算法特性:
═══════════════════════════════════════════════════════════════

适用场景:
• 非球形聚类
• 聚类数量未知
• 需要识别噪声点（异常值）
• 聚类密度不均匀

优点:
• 不需要预先指定聚类数量
• 可以发现任意形状的聚类
• 可以识别噪声点（异常值）
• 对初始值不敏感

缺点:
• 对Eps和Min Samples参数敏感
• 难以处理密度差异很大的聚类
• 高维数据效果较差
• 可能无法预测新数据（某些实现）

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. ⚠️ 此组件不进行实际训练，只配置参数
2. 必须将 Algorithm Params 连接到 Train Cluster 才能完成训练
3. ⚠️ Eps和Min Samples是最重要的参数，需要仔细调优
4. 建议在Create Dataset中标准化特征，确保距离计算合理
5. 参数选择对结果影响很大，建议尝试多个参数组合
6. 可以使用k-距离图（k-distance graph）帮助选择Eps值
7. 对于高维数据，考虑使用降维或使用cosine距离";
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(TrainDBSCANComponent));
        public override Guid ComponentGuid => new Guid("C3D4E5F6-A7B8-9012-6789-012345678902");
    }
}
