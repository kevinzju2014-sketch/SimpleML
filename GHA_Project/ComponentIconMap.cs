using System.Collections.Generic;

namespace SimpleML.Core
{
    /// <summary>
    /// 组件名称到图标文件名的映射（含文字标签图标）
    /// </summary>
    public static class ComponentIconMap
    {
        private static readonly Dictionary<string, string> _iconMap = new Dictionary<string, string>
        {
            // 数据输入组件
            { "ReadCSVComponent", "read_csv.png" },
            { "ReadExcelComponent", "read_excel.png" },
            { "WriteCSVComponent", "write_csv.png" },
            { "WriteExcelComponent", "write_excel.png" },
            { "LoadDatasetComponent", "load_dataset.png" },
            
            // 数据分析组件
            { "CalculateStatisticsComponent", "calculate_statistics.png" },
            { "CalculateCorrelationComponent", "calculate_correlation.png" },
            { "DescribeFeaturesComponent", "describe_features.png" },
            
            // 数据集组件
            { "CreateDatasetComponent", "create_dataset.png" },
            { "DeconstructDatasetComponent", "deconstruct_dataset.png" },
            { "SplitDatasetComponent", "split_data.png" },
            
            // 训练组件
            { "TrainClassifierComponent", "train_classifier.png" },
            { "TrainRegressorComponent", "train_regressor.png" },
            { "TrainClusterComponent", "train_cluster.png" },
            { "SmartTrainComponent", "smart_train.png" },
            
            // 算法特定训练组件 - 分类
            { "TrainRandomForestClassifierComponent", "random_forest_classifier.png" },
            { "TrainSVMClassifierComponent", "support_vector_machine_classifier.png" },
            { "TrainKNNClassifierComponent", "k_nearest_neighbors_classifier.png" },
            { "TrainLogisticRegressionClassifierComponent", "logistic_regression.png" },
            { "TrainNaiveBayesClassifierComponent", "naive_bayes_classifier.png" },
            { "TrainDecisionTreeClassifierComponent", "decision_tree_classifier.png" },
            
            // 算法特定训练组件 - 回归
            { "TrainRandomForestRegressorComponent", "random_forest_regressor.png" },
            { "TrainSVRComponent", "support_vector_regression.png" },
            { "TrainLinearRegressionComponent", "linear_regression.png" },
            { "TrainRidgeRegressionComponent", "ridge_regression.png" },
            { "TrainLassoRegressionComponent", "lasso_regression.png" },
            { "TrainKNNRegressorComponent", "k_nearest_neighbors_regressor.png" },
            
            // 算法特定训练组件 - 聚类
            { "TrainKMeansComponent", "k_means.png" },
            { "TrainDBSCANComponent", "density_baised_spatial_clistering_of_applications_with_noise.png" },
            { "TrainAgglomerativeClusteringComponent", "agglomerative_clustering.png" },
            
            // 预测组件
            { "PredictClassifierComponent", "predict_classifier.png" },
            { "PredictRegressorComponent", "predict_regressor.png" },
            { "PredictClusterComponent", "predict_cluster.png" },
            
            // 评估组件
            { "EvaluateClassificationComponent", "evaluate_classification.png" },
            { "EvaluateRegressionComponent", "evaluate_regression.png" },
            { "EvaluateClusteringComponent", "evaluate_clustering.png" },
            { "FeatureImportanceComponent", "feature_importance.png" },
            { "SilhouetteScoreComponent", "silhouette_score.png" },
            
            // 模型IO组件
            { "SaveModelComponent", "save_model.png" },
            { "LoadModelComponent", "load_model.png" },

            // 可视化
            { "VisualizeClusterLabelsComponent", "visualize_cluster_labels.png" },
            { "VisualizeClassificationLabelsComponent", "visualize_classification_labels.png" },
            { "VisualizeRegressionComponent", "visualize_regression.png" },
            { "ReduceDimensionsComponent", "reduce_dimensions.png" },
            { "ElbowMethodComponent", "elbow_method.png" },
            
            // 帮助
            { "AboutComponent", "about.png" },
            { "InstallationGuideComponent", "installation_guide.png" },
            { "HealthCheckComponent", "health_check.png" },
        };

        public static string GetIconFileName(string componentName)
        {
            if (_iconMap.TryGetValue(componentName, out string iconFileName))
            {
                return iconFileName;
            }
            return null;
        }
    }
}
