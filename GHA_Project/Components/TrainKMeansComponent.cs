using System;
using System.IO;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

using SimpleML.Localization;
namespace SimpleML.Components.ModelTraining
{
    public class TrainKMeansComponent : GH_Component
    {
        public TrainKMeansComponent()
          : base(L.Name("TrainKMeansComponent"), L.Nick("TrainKMeansComponent"), L.Desc("TrainKMeansComponent"),
              "SimpleML", "05 Algorithm")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.quarternary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("N Clusters", "NC", "聚类数量，默认3", GH_ParamAccess.item, 3);
            pManager.AddTextParameter("Init", "I", "初始化方式，默认'k-means++'（可选：k-means++, random）", GH_ParamAccess.item, "k-means++");
            pManager.AddIntegerParameter("N Init", "NI", "初始化次数，默认10", GH_ParamAccess.item, 10);
            pManager.AddIntegerParameter("Max Iter", "MI", "最大迭代次数，默认300", GH_ParamAccess.item, 300);
            pManager.AddNumberParameter("Tol", "T", "收敛容差，默认1e-4", GH_ParamAccess.item, 0.0001);
            pManager.AddIntegerParameter("Random State", "RS", "随机种子，默认None（0表示None）", GH_ParamAccess.item, 0);
            pManager.AddTextParameter("Algorithm", "A", "K-means算法，默认'lloyd'（可选：lloyd, elkan, auto）", GH_ParamAccess.item, "lloyd");
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Algorithm Params", "AP", "算法参数配置（JSON格式），连接到Train Cluster的Algorithm输入", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "算法说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int nClusters = 3;
            string init = "k-means++";
            int nInit = 10;
            int maxIter = 300;
            double tol = 0.0001;
            int randomState = 0;
            string algorithm = "lloyd";

            DA.GetData(0, ref nClusters);
            DA.GetData(1, ref init);
            DA.GetData(2, ref nInit);
            DA.GetData(3, ref maxIter);
            DA.GetData(4, ref tol);
            DA.GetData(5, ref randomState);
            DA.GetData(6, ref algorithm);

            try
            {
                // 构建算法参数JSON字符串
                System.Text.StringBuilder jsonBuilder = new System.Text.StringBuilder();
                jsonBuilder.Append("{");
                jsonBuilder.Append($@"""algorithm"": ""kmeans""");
                jsonBuilder.Append($@", ""n_clusters"": {nClusters}");

                if (init != "k-means++")
                    jsonBuilder.Append($@", ""init"": ""{init.Replace("\"", "\\\"")}""");

                if (nInit != 10)
                    jsonBuilder.Append($@", ""n_init"": {nInit}");

                if (maxIter != 300)
                    jsonBuilder.Append($@", ""max_iter"": {maxIter}");

                if (Math.Abs(tol - 0.0001) > 0.00001)
                    jsonBuilder.Append($@", ""tol"": {tol.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

                if (randomState > 0)
                    jsonBuilder.Append($@", ""random_state"": {randomState}");

                if (algorithm != "lloyd")
                    jsonBuilder.Append($@", ""algorithm"": ""{algorithm.Replace("\"", "\\\"")}""");

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
            return @"组件名称: Train K-Means
功能: 配置K-Means聚类算法参数（不进行实际训练）

═══════════════════════════════════════════════════════════════
重要说明:
═══════════════════════════════════════════════════════════════

⚠️ 此组件只负责配置算法参数，不进行实际训练！
实际训练需要将 Algorithm Params 输出连接到 Train Cluster 组件的 Algorithm 输入。

工作流程:
1. 使用本组件配置K-Means聚类参数
2. 将 Algorithm Params 输出连接到 Train Cluster 的 Algorithm 输入
3. 将 Dataset 连接到 Train Cluster 的 Dataset 输入
4. Train Cluster 完成实际训练并输出模型

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. N Clusters (聚类数量) - Integer类型，默认3
   • 取值范围: 2 到 样本数 之间的正整数
   • 说明: 要生成的聚类数量（k值）
   • 建议:
     - 2-5: 小数据集或简单聚类
     - 3-10: 中等数据集（最常用）
     - 10-50: 大数据集或复杂聚类
   • 注意: 
     - 这是K-Means最重要的参数
     - 需要根据数据特征和业务需求选择
     - 可以使用肘部法则（Elbow Method）或轮廓系数选择最佳k值

2. Init (初始化方式) - Text类型，默认'k-means++'
   • 可选值: 'k-means++', 'random'
   • 说明: 聚类中心的初始化方法
   • 建议:
     - 'k-means++': 默认，智能初始化，收敛更快（推荐）
     - 'random': 随机初始化，可能需要多次运行
   • 注意: k-means++通常效果更好，收敛更快

3. N Init (初始化次数) - Integer类型，默认10
   • 取值范围: 1 到 100 之间的正整数
   • 说明: 使用不同初始化的运行次数，选择最佳结果
   • 建议:
     - 10: 默认值，适合大多数情况
     - 1: 只运行一次（更快，但可能不是最优）
     - 20-50: 更仔细的搜索（更慢，但可能找到更好的结果）
   • 注意: 值越大，找到更好结果的可能性越高，但计算时间越长

4. Max Iter (最大迭代次数) - Integer类型，默认300
   • 取值范围: 100 到 1000 之间的正整数
   • 说明: 单次运行的最大迭代次数
   • 建议:
     - 300: 默认值，适合大多数情况
     - 100-200: 快速收敛（简单数据）
     - 500-1000: 复杂数据或需要更精确结果
   • 注意: 如果达到最大迭代次数仍未收敛，会使用当前结果

5. Tol (收敛容差) - Float类型，默认1e-4
   • 取值范围: 1e-6 到 1e-2 之间的正数
   • 说明: 判断收敛的容差
   • 建议: 大多数情况下使用默认值1e-4

6. Random State (随机种子) - Integer类型，默认None
   • 取值范围: 任意整数，或None
   • 说明: 控制随机性，确保结果可重复
   • 建议: 调试时使用固定值（如42），生产环境可使用None

7. Algorithm (K-means算法) - Text类型，默认'lloyd'
   • 可选值: 'lloyd', 'elkan', 'auto'
   • 说明: K-means算法的变体
   • 建议:
     - 'lloyd': 经典算法（默认）
     - 'elkan': 优化版本，对某些数据更快
     - 'auto': 自动选择

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Algorithm Params (算法参数) - Text/JSON类型，必需
   • 数据类型: JSON格式字符串
   • 内容: 包含算法标识符和所有配置参数的字典
   • 格式示例: {""algorithm"": ""kmeans"", ""n_clusters"": 3}
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
  → Train K-Means → Algorithm Params → Train Cluster (Algorithm输入)
  → Train Cluster → Model

选择最佳聚类数量:
Train K-Means (N Clusters=2) → Algorithm Params → Train Cluster → Evaluate Clustering
Train K-Means (N Clusters=3) → Algorithm Params → Train Cluster → Evaluate Clustering
Train K-Means (N Clusters=4) → Algorithm Params → Train Cluster → Evaluate Clustering
(比较不同k值的评估结果，选择最佳k)

═══════════════════════════════════════════════════════════════
算法特性:
═══════════════════════════════════════════════════════════════

适用场景:
• 数据具有明显的聚类结构
• 聚类形状接近球形
• 聚类大小相似
• 需要快速聚类结果

优点:
• 算法简单，易于理解
• 计算速度快
• 适合大数据集
• 结果可解释性强

缺点:
• 需要预先指定聚类数量k
• 对初始值敏感（使用k-means++可缓解）
• 假设聚类是球形的（不适合非球形聚类）
• 对异常值敏感
• 可能收敛到局部最优

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. ⚠️ 此组件不进行实际训练，只配置参数
2. 必须将 Algorithm Params 连接到 Train Cluster 才能完成训练
3. ⚠️ N Clusters是最重要的参数，需要仔细选择
4. 建议使用肘部法则或轮廓系数选择最佳k值
5. 对特征缩放敏感，建议在Create Dataset中启用Normalize
6. 使用k-means++初始化通常效果更好
7. 多次运行（N Init）可以找到更好的结果
8. 对于非球形聚类，考虑使用DBSCAN或其他算法";
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(TrainKMeansComponent));
        public override Guid ComponentGuid => new Guid("B0C1D2E3-F4A5-6789-3456-789012345679");
    }
}
