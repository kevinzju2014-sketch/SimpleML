"""
Grasshopper ML Plugin - Components Module
Grasshopper 组件模块

使用说明：
1. 通用训练组件：使用 train_classifier, train_regressor, train_cluster
2. 算法特定电池：使用 components.algorithms 模块中的函数
   例如：from components.algorithms import train_random_forest_regressor
"""

# 文件IO组件
from .file_io_components import (
    read_csv,
    read_excel,
    dataframe_to_numpy,
    extract_features_and_target,
    get_data_info
)

# 统计分析组件
from .statistics_components import (
    calculate_statistics,
    calculate_correlation,
    get_data_summary,
    describe_features
)

# 数据集组件
from .dataset_components import (
    Dataset,
    create_dataset,
    deconstruct_dataset,
    get_dataset_info,
    split_dataset
)

# 数据预处理组件
from .data_components import (
    prepare_data,
    split_data,
    normalize_data,
    handle_missing_data
)

# 模型训练组件
from .train_components import (
    train_classifier,
    train_regressor,
    train_cluster,
    get_available_algorithms
)

# 模型预测组件
from .predict_components import (
    predict,
    predict_proba,
    predict_with_model_manager
)

# 模型评估组件
from .evaluate_components import (
    evaluate_classification,
    evaluate_regression,
    evaluate_clustering
)

# 模型IO组件
from .model_io_components import (
    save_model,
    save_model_with_manager,
    load_model,
    load_model_as_manager,
    load_model_for_prediction,
    list_models
)

__all__ = [
    # 文件IO
    'read_csv',
    'read_excel',
    'dataframe_to_numpy',
    'extract_features_and_target',
    'get_data_info',
    # 数据集组件
    'Dataset',
    'create_dataset',
    'deconstruct_dataset',
    'get_dataset_info',
    'split_dataset',
    # 统计分析
    'calculate_statistics',
    'calculate_correlation',
    'get_data_summary',
    'describe_features',
    # 数据预处理
    'prepare_data',
    'split_data',
    'normalize_data',
    'handle_missing_data',
    # 模型训练
    'train_classifier',
    'train_regressor',
    'train_cluster',
    'get_available_algorithms',
    # 模型预测
    'predict',
    'predict_proba',
    'predict_with_model_manager',
    # 模型评估
    'evaluate_classification',
    'evaluate_regression',
    'evaluate_clustering',
    # 模型IO
    'save_model',
    'save_model_with_manager',
    'load_model',
    'load_model_as_manager',
    'load_model_for_prediction',
    'list_models'
]
