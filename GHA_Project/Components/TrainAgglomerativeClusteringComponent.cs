using System;
using System.IO;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.ModelTraining
{
    public class TrainAgglomerativeClusteringComponent : GH_Component
    {
        public TrainAgglomerativeClusteringComponent()
          : base("层次聚类参数 Agglomerative", "层次聚类",
              "训练层次聚类",
              "SimpleML", "05 Algorithm")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("N Clusters", "NC", "聚类数量，默认3（如果设置Distance Threshold，则设为0表示None）", GH_ParamAccess.item, 3);
            pManager.AddTextParameter("Linkage", "L", "聚合方式，默认'ward'（可选：ward, complete, average, single）", GH_ParamAccess.item, "ward");
            pManager.AddTextParameter("Affinity", "A", "距离度量，默认'euclidean'（可选：euclidean, l1, l2, manhattan, cosine等）", GH_ParamAccess.item, "euclidean");
            pManager.AddBooleanParameter("Compute Full Tree", "CFT", "是否计算完整树，默认False", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Distance Threshold", "DT", "距离阈值，默认None（0表示None，如果设置，N Clusters必须为0）", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("Compute Distances", "CD", "是否计算距离，默认False", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Algorithm Params", "AP", "算法参数配置（JSON格式），连接到Train Cluster的Algorithm输入", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "算法说明", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int nClusters = 3;
            string linkage = "ward";
            string affinity = "euclidean";
            bool computeFullTree = false;
            double distanceThreshold = 0;
            bool computeDistances = false;

            DA.GetData(0, ref nClusters);
            DA.GetData(1, ref linkage);
            DA.GetData(2, ref affinity);
            DA.GetData(3, ref computeFullTree);
            DA.GetData(4, ref distanceThreshold);
            DA.GetData(5, ref computeDistances);

            try
            {
                // 构建算法参数JSON字符串
                System.Text.StringBuilder jsonBuilder = new System.Text.StringBuilder();
                jsonBuilder.Append("{");
                jsonBuilder.Append($@"""algorithm"": ""agglomerative""");

                // 处理 n_clusters 和 distance_threshold 的互斥关系
                if (distanceThreshold > 0)
                {
                    // 如果设置了 distance_threshold，n_clusters 必须为 None
                    jsonBuilder.Append($@", ""distance_threshold"": {distanceThreshold.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                }
                else if (nClusters > 0)
                {
                    jsonBuilder.Append($@", ""n_clusters"": {nClusters}");
                }

                if (linkage != "ward")
                    jsonBuilder.Append($@", ""linkage"": ""{linkage.Replace("\"", "\\\"")}""");

                if (affinity != "euclidean")
                    jsonBuilder.Append($@", ""affinity"": ""{affinity.Replace("\"", "\\\"")}""");

                if (computeFullTree)
                    jsonBuilder.Append(@", ""compute_full_tree"": true");

                if (computeDistances)
                    jsonBuilder.Append(@", ""compute_distances"": true");

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
            return @"组件名称: Train Agglomerative Clustering
功能: 配置层次聚类（Agglomerative Clustering）参数（不进行实际训练）

═══════════════════════════════════════════════════════════════
重要说明:
═══════════════════════════════════════════════════════════════

⚠️ 此组件只负责配置算法参数，不进行实际训练！
实际训练需要将 Algorithm Params 输出连接到 Train Cluster 组件的 Algorithm 输入。

工作流程:
1. 使用本组件配置层次聚类参数
2. 将 Algorithm Params 输出连接到 Train Cluster 的 Algorithm 输入
3. 将 Dataset 连接到 Train Cluster 的 Dataset 输入
4. Train Cluster 完成实际训练并输出模型

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. N Clusters (聚类数量) - Integer类型，默认3
   • 取值范围: 1 到 样本数 之间的正整数，或0（表示None）
   • 说明: 期望的聚类数量
   • 建议:
     - 3-5: 小数据集（<100样本）
     - 5-10: 中等数据集（100-1000样本）
     - 10-20: 大数据集（>1000样本）
   • 注意: 如果设置了 Distance Threshold，N Clusters 必须设为 0（None）

2. Linkage (聚合方式) - Text类型，默认'ward'
   • 可选值: 'ward', 'complete', 'average', 'single'
   • 说明: 用于计算簇之间距离的策略
   • 建议:
     - 'ward': Ward链接（默认，最小化簇内方差，适合大多数情况）
     - 'complete': 完全链接（最大距离，产生紧凑的簇）
     - 'average': 平均链接（平均距离，平衡选择）
     - 'single': 单链接（最小距离，可能产生链状簇）
   • 注意: 'ward' 只能与 'euclidean' affinity 一起使用

3. Affinity (距离度量) - Text类型，默认'euclidean'
   • 可选值: 'euclidean', 'l1', 'l2', 'manhattan', 'cosine', 'precomputed'
   • 说明: 用于计算距离的度量方法
   • 建议:
     - 'euclidean': 欧氏距离（默认，最常用）
     - 'manhattan': 曼哈顿距离（L1距离）
     - 'cosine': 余弦距离（适合高维数据或文本数据）
     - 'l1', 'l2': L1和L2距离
   • 注意: 'ward' linkage 只能与 'euclidean' affinity 一起使用

4. Compute Full Tree (计算完整树) - Boolean类型，默认False
   • 取值范围: True / False
   • 说明: 是否计算完整的层次树
   • 建议: 大多数情况下使用False（默认），除非需要完整的树结构

5. Distance Threshold (距离阈值) - Float类型，默认None（0表示None）
   • 取值范围: 正数，或0（表示None）
   • 说明: 链接距离阈值，超过此距离的簇不会被合并
   • 建议: 如果设置此参数，N Clusters 必须设为 0（None）
   • 注意: 与 N Clusters 互斥，只能设置其中一个

6. Compute Distances (计算距离) - Boolean类型，默认False
   • 取值范围: True / False
   • 说明: 是否计算每个样本到其簇中心的距离
   • 建议: 大多数情况下使用False（默认），除非需要距离信息

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Algorithm Params (算法参数) - Text/JSON类型，必需
   • 数据类型: JSON格式字符串
   • 内容: 包含算法标识符和所有配置参数的字典
   • 格式示例: {""algorithm"": ""agglomerative"", ""n_clusters"": 3, ""linkage"": ""ward""}
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
Create Dataset → Train Agglomerative Clustering → Algorithm Params → Train Cluster (Algorithm输入)
  → Train Cluster (Dataset输入) → Model

使用距离阈值:
Train Agglomerative Clustering (N Clusters=0, Distance Threshold=0.5) → Algorithm Params → Train Cluster

═══════════════════════════════════════════════════════════════
算法特性:
═══════════════════════════════════════════════════════════════

适用场景:
• 需要层次结构的聚类
• 需要可视化聚类过程（树状图）
• 不确定聚类数量
• 小到中等规模数据集

优点:
• 可以生成层次结构（树状图）
• 不需要预先指定聚类数量（使用distance_threshold时）
• 可以处理任意形状的簇
• 提供簇之间的层次关系

缺点:
• 计算复杂度高（O(n³)），不适合大数据集
• 对噪声和异常值敏感
• 一旦合并，无法撤销
• 内存占用较大

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. ⚠️ 此组件不进行实际训练，只配置参数
2. 必须将 Algorithm Params 连接到 Train Cluster 才能完成训练
3. ⚠️ N Clusters 和 Distance Threshold 互斥，只能设置其中一个
4. ⚠️ 'ward' linkage 只能与 'euclidean' affinity 一起使用
5. 层次聚类计算复杂度高，不适合大数据集（>10000样本）
6. 建议在Create Dataset中标准化特征
7. 如果数据有噪声，考虑先进行异常值检测";
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(TrainAgglomerativeClusteringComponent));
        public override Guid ComponentGuid => new Guid("D4E5F6A7-B8C9-0123-7890-123456789013");
    }
}
